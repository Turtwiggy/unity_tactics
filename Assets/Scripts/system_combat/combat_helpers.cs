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
    public static int SpotQuality(Wiggy.registry ecs, map_manager map, UnitSpawnSystem spawner, Entity e, Vector2Int spot)
    {
      var team = ecs.GetComponent<TeamComponent>(e);

      int quality = 0;
      bool spot_in_cover = SpotInCover(map, spot);
      if (spot_in_cover)
        quality += 5;

      // Warning: this loops through all the spawned entities which is bad
      foreach (var other in spawner.entities)
      {
        if (e.id == other.id)
          continue; // dont compare self

        // Defensive questions

        // Would the spot be moving in to a vulnerable position?
        var other_pos = ecs.GetComponent<GridPositionComponent>(other);
        var other_team = ecs.GetComponent<TeamComponent>(other);
        bool spot_is_flanked = SpotIsFlanked(map, spot, other_pos.position);
        if (spot_is_flanked)
        {
          if (team.team != other_team.team)
            quality -= 5;
          else
            quality += 2;
        }
      }

      return quality;
    }

  }
}