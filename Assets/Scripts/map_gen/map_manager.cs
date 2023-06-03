using UnityEngine;
using System.Collections.Generic;

namespace Wiggy
{
  [System.Serializable]
  public class MapRepresentation
  {
    public List<EntityType> entities = new();
  };

  [System.Serializable]
  public class MapEntry
  {
    public List<Entity> entities = new();
  };

  [System.Serializable]
  public class IndexList
  {
    [SerializeField]
    public List<int> idxs = new();
  }

  public class map_manager : MonoBehaviour
  {
    // Map representations
    public MapRepresentation[] obstacle_map;
    public MapRepresentation[] voronoi_map;
    public MapEntry[] entity_map;

    // Interesting spots
    public List<IndexList> voronoi_zones;
    public List<Vector2Int> srt_spots;
    public List<Vector2Int> ext_spots;

    public GameObject map_holder_public;
    public Transform generated_obstacle_holder;
    public Transform generated_map_holder;

    public GameObject debug_zone_core_prefab;
    public GameObject debug_zone_edge_prefab;
    public GameObject debug_start_points_prefab;
    public GameObject debug_end_points_prefab;
    public GameObject wall_prefab;

    [Header("Map Info")]
    public int width = 30;
    public int height = 30;
    public int size = 1;

    [Header("Cell Automata Obstacles")]
    public bool generate_obstacles = false;
    public int iterations = 5;
    public int seed = 0;
    public bool remove_isolated = true;
    public int remove_isolated_count = 1;

    [Header("Voronoi Zones")]
    public int zone_seed = 0;
    public int zone_size = 5;

    public static astar_cell[] GameToAStar(Wiggy.registry ecs, map_manager map)
    {
      int size = map.width * map.height;
      astar_cell[] astar = new astar_cell[size];

      for (int i = 0; i < size; i++)
      {
        astar_cell c = new();
        c.pos = Grid.IndexToPos(i, map.width, map.height);

        int cost = 1;

        // Obstacle Mask
        var obstacles = map.obstacle_map[i].entities;
        if (obstacles.Contains(EntityType.tile_type_wall))
          cost = -1; // impassable

        // Game Mask
        var entities = map.entity_map[i].entities;
        var door = GetFirst<DoorComponent>(ecs, entities);
        if (door.IsSet)
          cost = -1; // assume impassable (until doors can be open state?)

        c.path_cost = cost;
        astar[i] = c;
      }
      return astar;
    }

    public static astar_cell[] GameToAStar(MapRepresentation[] map, int x_max, int y_max)
    {
      astar_cell[] astar = new astar_cell[map.Length];

      for (int i = 0; i < map.Length; i++)
      {
        astar_cell c = new();
        c.pos = Grid.IndexToPos(i, x_max, y_max);
        c.path_cost = map[i].entities.Contains(EntityType.tile_type_wall) ? -1 : 1;
        astar[i] = c;
      }
      return astar;
    }

    // Create an X by Y map entirely of floor
    public static MapRepresentation[] CreateBlankMap(int dim)
    {
      MapRepresentation[] map = new MapRepresentation[dim];
      for (int i = 0; i < map.Length; i++)
      {
        map[i] = new()
        {
          entities = new()
        };
        map[i].entities.Add(EntityType.tile_type_floor);
      }
      return map;
    }

    public void GenerateMap(List<(int, EntityType)> loaded_map_entities = null)
    {
      // Generate Map Holder
      string holder_name = "Generated Map";
      {
        var holder = map_holder_public.transform.Find(holder_name);
        if (holder)
          DestroyImmediate(holder.gameObject);
      }
      generated_map_holder = new GameObject(holder_name).transform;
      generated_map_holder.parent = map_holder_public.transform;
      generated_obstacle_holder = new GameObject("Obstacle Holder").transform;
      generated_obstacle_holder.parent = generated_map_holder;

      // make entirely floor
      obstacle_map = CreateBlankMap(width * height);
      if (generate_obstacles)
        obstacle_map = map_gen_obstacles.GenerateObstacles(width, height, iterations, seed);
      if (remove_isolated)
        map_gen_obstacles.ObstaclePostProcessing(obstacle_map, remove_isolated_count, width, height);

      var walls = GetFilteredMapEntities(loaded_map_entities, EntityType.tile_type_wall);
      foreach (var wall in walls)
      {
        var idx = wall.Item1;
        var ent = wall.Item2;
        obstacle_map[idx].entities.Clear();
        obstacle_map[idx].entities.Add(ent);
      }

      // start spots
      var map_defined_start_spots = GetFilteredMapEntities(loaded_map_entities, EntityType.actor_player);
      foreach (var spot in map_defined_start_spots)
      {
        var pos = Grid.IndexToPos(spot.Item1, width, height);
        srt_spots.Add(pos);
      }

      // exit spots
      var map_defined_exit_spots = GetFilteredMapEntities(loaded_map_entities, EntityType.tile_type_exit);
      foreach (var spot in map_defined_exit_spots)
      {
        var pos = Grid.IndexToPos(spot.Item1, width, height);
        ext_spots.Add(pos);
      }

      // Create entity map
      entity_map = new MapEntry[width * height];
      for (int i = 0; i < entity_map.Length; i++)
        entity_map[i] = new() { entities = new() };


      // Make borders of map obstacles
      // GenerateWallBorders();

      // Generate Start/End Points
      // int players = 4;
      // var srt = map_gen_obstacles.StartPoint(obstacle_map, width, height);
      // var ext = map_gen_obstacles.ExitPoint(obstacle_map, width, height);
      // srt_spots = GenerateConnectedSpots(obstacle_map, srt, players);
      // ext_spots = GenerateConnectedSpots(obstacle_map, ext, players);

      // Map Zones
      // var poisson_points = voronoi.GeneratePoissonPoints(srt, zone_size, zone_seed, width, height, size);
      // var voronoi_graph = voronoi.Generate(poisson_points, width, height, 0);
      // voronoi_map = voronoi.GetVoronoiRepresentation(voronoi_graph, width, height, size);
      // voronoi_zones = voronoi.GetZones(poisson_points, voronoi_map, width, height);

      // 
      // Unity side of things
      //

      // InstantiateMapEntry(obstacle_map, EntityType.tile_type_wall, wall_prefab, generated_obstacle_holder);
      // InstantiateMapEntry(voronoi_map, EntityType.tile_type_wall, debug_zone_edge_prefab, map_holder.transform);
      // InstantiateSpots(poisson_points, debug_zone_core_prefab);
      DebugInstantiateSpots(srt_spots, debug_start_points_prefab);
      DebugInstantiateSpots(ext_spots, debug_end_points_prefab);

      // Visualize the zones
      // Random.InitState(1);
      // for (int i = 0; i < voronoi_zones.Count; i++)
      // {
      //   var sr = debug_zone_edge_prefab.GetComponentInChildren<SpriteRenderer>();
      //   sr.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
      //   DebugInstantiateSpotsFromIdxs(voronoi_map, voronoi_zones[i], debug_zone_edge_prefab);
      // }
    }

    public void DebugInstantiateSpots(List<Vector2Int> spots, GameObject prefab)
    {
      for (int i = 0; i < spots.Count; i++)
      {
        var wpos = Grid.GridSpaceToWorldSpace(spots[i], size);
        var obj = Instantiate(prefab);
        obj.transform.SetPositionAndRotation(wpos, prefab.transform.rotation);
        obj.transform.parent = generated_map_holder;
      }
    }

    public void DebugInstantiateSpotsFromIdxs<T>(T[] map, List<int> idxs, GameObject prefab)
    {
      for (int j = 0; j < idxs.Count; j++)
      {
        var idx = idxs[j];
        var pos = Grid.IndexToPos(idx, width, height);
        var wpos = Grid.GridSpaceToWorldSpace(pos, size);
        var obj = Instantiate(prefab);
        obj.transform.SetPositionAndRotation(wpos, prefab.transform.rotation);
        obj.transform.parent = generated_map_holder;
      }
    }

    public static List<(int, EntityType)> GetFilteredMapEntities(List<(int, EntityType)> loaded_map_entities, EntityType type)
    {
      var entities = new List<(int, EntityType)>();
      for (int i = 0; i < loaded_map_entities.Count; i++)
      {
        var ent = loaded_map_entities[i];
        if (ent.Item2 == type)
          entities.Add(ent);
      }
      return entities;
    }

    public static Optional<int> GetFirst<T>(Wiggy.registry ecs, List<Entity> entities)
    {
      for (int i = 0; i < entities.Count; i++)
      {
        var def = default(T);
        ecs.TryGetComponent(entities[i], ref def, out var has_comp);
        if (has_comp)
          return new Optional<int>(i);
      }
      return new Optional<int>();
    }
  }


} // namespace Wiggy