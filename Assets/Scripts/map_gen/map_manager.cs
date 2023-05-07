using UnityEngine;
using System.Collections.Generic;

namespace Wiggy
{
  [System.Serializable]
  public class MapEntry
  {
    // 0 is bottom, n is top
    [SerializeField]
    public List<EntityType> entities = new();

    [SerializeField]
    public List<GameObject> instantiated = new();
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
    public MapEntry[] obstacle_map;
    public MapEntry[] voronoi_map;

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
    public int iterations = 5;
    public int seed = 0;
    public bool remove_isolated = true;
    public int remove_isolated_count = 1;

    [Header("Voronoi Zones")]
    public int zone_seed = 0;
    public int zone_size = 5;

    public static astar_cell[] GameToAStar(MapEntry[] map, int x_max, int y_max)
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
    public static MapEntry[] CreateBlankMap(int dim)
    {
      MapEntry[] map = new MapEntry[dim];
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

    // Given a spot on the map, generate X connected cells
    private List<Vector2Int> GenerateConnectedSpots(MapEntry[] map, Vector2Int spot, int amount)
    {
      int range = amount + 1; // arbitrarily chosen
      int index = Grid.GetIndex(spot, width);
      var astar = GameToAStar(map, width, height);
      var cells = a_star.generate_accessible_areas(astar, index, range, width, height);

      if (cells.Length < amount)
        Debug.LogError("Not enough spots for amount");

      List<Vector2Int> spots = new();
      for (int i = 0; i < amount; i++)
        spots.Add(cells[i].pos);

      return spots;
    }

    public void GenerateMap()
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

      // Generate Obstacles
      obstacle_map = map_gen_obstacles.GenerateObstacles(width, height, iterations, seed);
      if (remove_isolated)
        map_gen_obstacles.ObstaclePostProcessing(obstacle_map, remove_isolated_count, width, height);

      // Make borders of map obstacles
      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          int idx = Grid.GetIndex(x, y, width);

          if (y == 0) // bottom
          {
            obstacle_map[idx].entities.Clear();
            obstacle_map[idx].entities.Add(EntityType.tile_type_wall);
          }
          if (x == width - 1) // right
          {
            obstacle_map[idx].entities.Clear();
            obstacle_map[idx].entities.Add(EntityType.tile_type_wall);
          }

          if (x == 0) // left
          {
            obstacle_map[idx].entities.Clear();
            obstacle_map[idx].entities.Add(EntityType.tile_type_wall);
          }

          if (y == height - 1) // top
          {
            obstacle_map[idx].entities.Clear();
            obstacle_map[idx].entities.Add(EntityType.tile_type_wall);
          }
        }
      }

      // Generate Start/End Points
      int players = 4;
      var srt = map_gen_obstacles.StartPoint(obstacle_map, width, height);
      var ext = map_gen_obstacles.ExitPoint(obstacle_map, width, height);
      srt_spots = GenerateConnectedSpots(obstacle_map, srt, players);
      ext_spots = GenerateConnectedSpots(obstacle_map, ext, players);

      // Map Zones
      var poisson_points = voronoi.GeneratePoissonPoints(srt, zone_size, zone_seed, width, height, size);
      var voronoi_graph = voronoi.Generate(poisson_points, width, height, 0);
      voronoi_map = voronoi.GetVoronoiRepresentation(voronoi_graph, width, height, size);
      voronoi_zones = voronoi.GetZones(poisson_points, voronoi_map, width, height);

      // 
      // Unity side of things
      //
      void InstantiateMapEntry(MapEntry[] map, EntityType type, GameObject prefab, Transform parent)
      {
        for (int i = 0; i < map.Length; i++)
        {
          var entry = map[i];
          if (entry.entities.Contains(type))
          {
            var pos = Grid.IndexToPos(i, width, height);
            var wpos = Grid.GridSpaceToWorldSpace(pos, size);
            wpos.y += 0.5f; // hmm

            var go = Instantiate(prefab, wpos, Quaternion.identity, parent);
            go.transform.rotation = Quaternion.Euler(0, 180, 0); // 180 because the default unity cube is upsidedown
            go.transform.name = "ObjectIndex: " + i;

            // This should probably be removed for the ecs system
            map[i].instantiated.Add(go);
          }
        }
      }
      void InstantiateSpots(List<Vector2Int> spots, GameObject prefab)
      {
        for (int i = 0; i < spots.Count; i++)
        {
          var wpos = Grid.GridSpaceToWorldSpace(spots[i], size);
          var obj = Instantiate(prefab);
          obj.transform.SetPositionAndRotation(wpos, prefab.transform.rotation);
          obj.transform.parent = generated_map_holder;
        }
      }
      void InstantiateSpotsFromIdxs<T>(T[] map, List<int> idxs, GameObject prefab)
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

      InstantiateMapEntry(obstacle_map, EntityType.tile_type_wall, wall_prefab, generated_obstacle_holder);
      // InstantiateMapEntry(voronoi_map, EntityType.tile_type_wall, debug_zone_edge_prefab, map_holder.transform);
      InstantiateSpots(poisson_points, debug_zone_core_prefab);
      InstantiateSpots(srt_spots, debug_start_points_prefab);
      InstantiateSpots(ext_spots, debug_end_points_prefab);

      // Visualize the zones
      // Random.InitState(1);
      // for (int i = 0; i < voronoi_zones.Count; i++)
      // {
      //   var sr = debug_zone_edge_prefab.GetComponentInChildren<SpriteRenderer>();
      //   sr.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
      //   InstantiateSpotsFromIdxs(voronoi_map, voronoi_zones[i], debug_zone_edge_prefab);
      // }
    }
  }
} // namespace Wiggy