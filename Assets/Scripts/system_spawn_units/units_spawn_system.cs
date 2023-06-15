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
      public GameObject keycard_prefab;
      public GameObject wall_prefab;
      public GameObject door_prefab;
      public GameObject exit_prefab;
      public List<(int, EntityType)> entities;
    }

    private UnitSpawnSystemInit data;

    public static Entity Create(Wiggy.registry ecs, map_manager map, EntityType type, Vector2Int gpos, string name, Optional<GameObject> go, Optional<GameObject> parent)
    {
      var idx = Grid.GetIndex(gpos, map.width);
      var pos = Grid.IndexToPos(idx, map.width, map.height);

      Entity u = default;
      switch (type)
      {
        case EntityType.actor_player:
          u = Entities.create_player(ecs, pos, name, go, parent);
          break;
        case EntityType.tile_type_door:
          u = Entities.create_door(ecs, pos, name, go, parent);
          break;
        case EntityType.actor_enemy:
          u = Entities.create_enemy(ecs, pos, name, go, parent);
          break;
        case EntityType.actor_barrel:
          u = Entities.create_barrel(ecs, pos, name, go, parent);
          break;
        case EntityType.tile_type_wall:
          u = Entities.create_wall(ecs, pos, name, go, parent);
          break;
        case EntityType.tile_type_trap:
          u = Entities.create_trap(ecs, pos, name, go, parent);
          break;
        case EntityType.keycard:
          u = Entities.create_keycard(ecs, pos, name, go, parent);
          break;
        case EntityType.tile_type_exit:
          u = Entities.create_exit(ecs, pos, name, go, parent);
          break;
        default:
          Debug.LogError("Failed to create entity");
          break;
      }

      map.entity_map[idx].entities.Add(u);
      return u;
    }

    public void Spawn(Wiggy.registry ecs, map_manager map, EntityType type, string name, Optional<GameObject> prefab, Optional<GameObject> parent)
    {
      var spawn = map_manager.GetFilteredMapEntities(data.entities, type);
      for (int i = 0; i < spawn.Count; i++)
      {
        (int, EntityType) s = spawn[i];
        var spot = Grid.IndexToPos(s.Item1, map.width, map.height);
        Create(ecs, map, type, spot, name, prefab, parent);
      }
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
      var map = Object.FindObjectOfType<map_manager>();

      GameObject parent = new("Map Parent");
      Spawn(ecs, map, EntityType.tile_type_wall, "Walls", new Optional<GameObject>(data.wall_prefab), new Optional<GameObject>(parent));
      Spawn(ecs, map, EntityType.actor_player, "Player", new Optional<GameObject>(data.player_prefab), new Optional<GameObject>(parent));
      Spawn(ecs, map, EntityType.actor_enemy, "Enemy", new Optional<GameObject>(data.enemy_prefab), new Optional<GameObject>(parent));
      Spawn(ecs, map, EntityType.actor_barrel, "Bomb", new Optional<GameObject>(data.barrel_prefab), new Optional<GameObject>(parent));
      Spawn(ecs, map, EntityType.tile_type_trap, "Trap", new Optional<GameObject>(data.trap_prefab), new Optional<GameObject>(parent));
      Spawn(ecs, map, EntityType.keycard, "Keycard", new Optional<GameObject>(data.keycard_prefab), new Optional<GameObject>(parent));
      Spawn(ecs, map, EntityType.tile_type_door, "Door", new Optional<GameObject>(data.door_prefab), new Optional<GameObject>(parent));
      Spawn(ecs, map, EntityType.tile_type_exit, "Exit", new Optional<GameObject>(data.exit_prefab), new Optional<GameObject>(parent));

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
          if (map.entity_map.Length != 0)
          {
            Debug.Log("Skipping tile; something already existed");
            break;
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