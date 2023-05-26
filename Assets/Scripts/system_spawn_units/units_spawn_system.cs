using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  public class UnitSpawnSystem : ECSSystem
  {
    public struct UnitSpawnSystemInit
    {
      public GameObject player_prefab;
      public GameObject enemy_prefab;
      public GameObject barrel_prefab;
      public GameObject trap_prefab;
      public List<(int, EntityType)> entities;
    }

    // x * y representation
    public Optional<Entity>[] units { get; set; }

    public map_manager map;
    private UnitSpawnSystemInit data;

    public Entity CreatePlayer(Wiggy.registry ecs, Vector2Int gpos, string name)
    {
      var idx = Grid.GetIndex(gpos, map.width);
      var pos = Grid.IndexToPos(idx, map.width, map.height);
      var unit = Entities.create_player(ecs, pos, name, new Optional<GameObject>(data.player_prefab));
      units[idx].Set(unit);
      return units[idx].Data;
    }

    public Entity CreateEnemy(Wiggy.registry ecs, Vector2Int gpos, string name)
    {
      var idx = Grid.GetIndex(gpos, map.width);
      var pos = Grid.IndexToPos(idx, map.width, map.height);
      var unit = Entities.create_enemy(ecs, pos, name, new Optional<GameObject>(data.enemy_prefab));
      units[idx].Set(unit);
      return units[idx].Data;
    }

    public Entity CreateBarrel(Wiggy.registry ecs, Vector2Int gpos, string name)
    {
      var idx = Grid.GetIndex(gpos, map.width);
      var pos = Grid.IndexToPos(idx, map.width, map.height);
      var unit = Entities.create_barrel(ecs, pos, name, new Optional<GameObject>(data.barrel_prefab));
      units[idx].Set(unit);
      return units[idx].Data;
    }

    public Entity CreateTrap(Wiggy.registry ecs, Vector2Int gpos, string name)
    {
      var idx = Grid.GetIndex(gpos, map.width);
      var pos = Grid.IndexToPos(idx, map.width, map.height);
      var unit = Entities.create_trap(ecs, pos, name, new Optional<GameObject>(data.trap_prefab));
      units[idx].Set(unit);
      return units[idx].Data;
    }

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      s.Set(ecs.GetComponentType<TeamComponent>());
      ecs.SetSystemSignature<UnitSpawnSystem>(s);
    }

    public void Start(Wiggy.registry ecs, UnitSpawnSystemInit data)
    {
      this.data = data;
      map = Object.FindObjectOfType<map_manager>();

      units = new Optional<Entity>[map.width * map.height];
      for (int i = 0; i < map.width * map.height; i++)
        units[i] = new Optional<Entity>();

      // Spawn players
      var players_to_spawn = map_manager.GetFilteredMapEntities(data.entities, EntityType.actor_player);
      foreach (var player in players_to_spawn)
      {
        var spot = Grid.IndexToPos(player.Item1, map.width, map.height);
        CreatePlayer(ecs, spot, "Player");
      }

      // Spawn enemies
      var enemies_to_spawn = map_manager.GetFilteredMapEntities(data.entities, EntityType.actor_enemy);
      foreach (var enemy in enemies_to_spawn)
      {
        var spot = Grid.IndexToPos(enemy.Item1, map.width, map.height);
        CreateEnemy(ecs, spot, "Enemy");
      }

      // Spawn explosive barrels
      var barrels_to_spawn = map_manager.GetFilteredMapEntities(data.entities, EntityType.actor_barrel);
      foreach (var barrel in barrels_to_spawn)
      {
        var spot = Grid.IndexToPos(barrel.Item1, map.width, map.height);
        CreateBarrel(ecs, spot, "Explosive Barrel");
      }

      // Spawn traps
      var trap_to_spawn = map_manager.GetFilteredMapEntities(data.entities, EntityType.tile_type_trap);
      foreach (var trap in trap_to_spawn)
      {
        var spot = Grid.IndexToPos(trap.Item1, map.width, map.height);
        CreateTrap(ecs, spot, "Trap");
      }

      // set players at start spots
      // CreatePlayer(ecs, map.srt_spots[0], "Wiggy", new Optional<GameObject>(data.player_prefab));
      // CreatePlayer(ecs, map.srt_spots[1], "Wallace", new Optional<GameObject>(data.player_prefab));
      // CreatePlayer(ecs, map.srt_spots[2], "Sherbert", new Optional<GameObject>(data.player_prefab));
      // CreatePlayer(ecs, map.srt_spots[3], "Grunbo", new Optional<GameObject>(data.player_prefab));

      // TODO: map_gen_items_and_enemies
      var voronoi_map = map.voronoi_map;
      var voronoi_zones = map.voronoi_zones;

      int n = 0;

      foreach (IndexList zone_idxs in voronoi_zones)
      {
        if (n >= 1)
          break;

        foreach (int idx in zone_idxs.idxs)
        {
          if (n >= 1)
            break;

          // validation checks

          // unit in zone?
          if (units[idx].IsSet)
          {
            Debug.Log("Skipping tile; unit already existed");
            break;
          }

          // In obstacle?
          if (map.obstacle_map[idx].entities.Contains(EntityType.tile_type_wall))
          {
            Debug.Log("Would spawn in obstacle... skipping");
            continue;
          }

          var pos = Grid.IndexToPos(idx, map.width, map.height);
          // CreateEnemy(ecs, pos, "Random Enemy", new Optional<GameObject>(data.enemy_prefab));
          n++;
          break;
        }
      }
    }

    public void Update()
    {

    }
  }
}