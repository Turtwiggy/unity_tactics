using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  public static class CombatHelpers
  {
    public static square_direction WhichQuadrantIsAttacker(Vector2Int def_cell, Vector2Int atk_cell)
    {
      var dir = def_cell - atk_cell;
      bool on_diagonal = Mathf.Abs(dir.x) == Mathf.Abs(dir.y);
      bool north = dir.y < 0;
      bool south = dir.y > 0;
      bool east = dir.x < 0;
      bool west = dir.x > 0;
      bool y_pull = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);
      bool x_pull = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

      bool in_quad_N = y_pull && north;
      bool in_quad_S = y_pull && south;
      bool in_quad_E = x_pull && east;
      bool in_quad_W = x_pull && west;
      bool in_quad_NE = on_diagonal && north && east;
      bool in_quad_NW = on_diagonal && north && west;
      bool in_quad_SE = on_diagonal && south && east;
      bool in_quad_SW = on_diagonal && south && west;

      if (in_quad_N)
        return square_direction.N;
      if (in_quad_S)
        return square_direction.S;
      if (in_quad_E)
        return square_direction.E;
      if (in_quad_W)
        return square_direction.W;

      if (in_quad_NE)
        return square_direction.NE;
      if (in_quad_NW)
        return square_direction.NW;
      if (in_quad_SE)
        return square_direction.SE;
      if (in_quad_SW)
        return square_direction.SW;

      return default;
    }

    public static bool SpotInCover(map_manager map, Vector2Int pos)
    {
      var neighbours = a_star.square_neighbour_indicies(pos.x, pos.y, map.width, map.height);
      foreach (var (_, idx) in neighbours)
        if (map.obstacle_map[idx].entities.Contains(EntityType.tile_type_wall))
          return true;
      return false;
    }

    public static bool SpotIsFlanked(map_manager map, Vector2Int atk_pos, Vector2Int def_pos)
    {
      // Which quadrant is the attacker in?
      var attacker_quadrant = WhichQuadrantIsAttacker(def_pos, atk_pos);

      // Which directions is the defender covered from?
      List<square_direction> covered_directions = new();
      var def_surrounding_idxs = a_star.square_neighbour_indicies(def_pos.x, def_pos.y, map.width, map.height);
      foreach (var (dir, idx) in def_surrounding_idxs)
        if (map.obstacle_map[idx].entities.Contains(EntityType.tile_type_wall))
          covered_directions.Add(dir);

      // ... check line of sight
      // Note: this only matters for "high" cover 
      // or objects that shouldnt be shot through
      bool line_of_sight_blocked = false;

      var astar = map_manager.GameToAStar(map.obstacle_map, map.width, map.height);
      int from = Grid.GetIndex(atk_pos, map.width);
      int to = Grid.GetIndex(def_pos, map.width);
      var path = a_star.generate_direct_with_diagonals(astar, from, to, map.width, false);
      for (int i = 0; i < path.Length; i++)
      {
        var idx = Grid.GetIndex(path[i].pos, map.width);
        if (map.obstacle_map[idx].entities.Contains(EntityType.tile_type_wall))
        {
          line_of_sight_blocked = true;
          break;
        }
      }

      // Flanked if not covered by the attacker's quadrant
      var flanked = !covered_directions.Contains(attacker_quadrant);

      // Flanked if the attacker has line of sight on the defender
      if (line_of_sight_blocked)
        flanked = false;

      return flanked;
    }

    // The quality of a spot is determined by:
    // Am I flanked by hostiles or friendlies
    public static int SpotQuality(Wiggy.registry ecs, map_manager map, Entity e, Vector2Int spot, Entity player, astar_cell[] path)
    {
      int quality = 0;

      bool spot_in_cover = SpotInCover(map, spot);
      if (spot_in_cover)
        quality += 2;

      // Does this spot move closer to player?
      var different_pos = spot != ecs.GetComponent<GridPositionComponent>(e).position;
      if (Array.Exists(path, e => e.pos == spot) && different_pos)
        quality += 5;

      // Is a player flanking the potential spot?
      var player_pos = ecs.GetComponent<GridPositionComponent>(player).position;
      bool spot_is_flanked = SpotIsFlanked(map, player_pos, spot);
      if (spot_is_flanked)
        quality -= 1;

      return quality;
    }

    public static int CalculateDamage(Wiggy.registry ecs, map_manager map, Entity attacker, Entity defender)
    {
      // Weapon may or may not be equipped

      WeaponComponent backup = default;
      WeaponComponent weapon = ecs.TryGetComponent(attacker, ref backup);
      bool has_component = !weapon.Equals(default);
      if (has_component)
        return 0;

      int damage = 0;

      // Check range
      var atk_pos = ecs.GetComponent<GridPositionComponent>(attacker).position;
      var def_pos = ecs.GetComponent<GridPositionComponent>(defender).position;
      var dst = Mathf.Abs(Vector2Int.Distance(atk_pos, def_pos));
      var in_weapon_range = dst <= weapon.max_range && dst >= weapon.min_range;
      if (!in_weapon_range)
        damage = 0;

      // Check flanked
      var flanked = CombatHelpers.SpotIsFlanked(map, atk_pos, def_pos);
      if (flanked)
        damage *= 2;

      // Check weaknesses
      // TODO

      // Random crit amount?
      // TODO

      return damage;
    }
  }
}