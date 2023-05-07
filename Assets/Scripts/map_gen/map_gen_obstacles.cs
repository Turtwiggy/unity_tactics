using UnityEngine;

namespace Wiggy
{
  // inspired by: 
  // https://bfnightly.bracketproductions.com/rustbook/chapter_27.html

  public static class map_gen_obstacles
  {
    public static MapEntry[] GenerateObstacles(int width, int height, int iterations, int seed)
    {
      int dim = width * height;
      MapEntry[] map = map_manager.CreateBlankMap(dim);
      UnityEngine.Random.InitState(seed);

      // Make the map completely chaos, with 55% set to floor
      for (int idx = 0; idx < dim; idx++)
      {
        var rnd = UnityEngine.Random.Range(0, 100);

        map[idx].entities.Clear();
        if (rnd > 55) { map[idx].entities.Add(EntityType.tile_type_floor); }
        else { map[idx].entities.Add(EntityType.tile_type_wall); }
      }

      // iteratively apply the cellular atomata rules
      for (int i = 0; i < iterations; i++)
      {
        var newtiles = new MapEntry[map.Length];

        for (int idx = 0; idx < dim; idx++)
        {
          newtiles[idx] = new();
          newtiles[idx].entities = new();

          var xy = Grid.IndexToPos(idx, width, height);
          var neighbours_info = a_star.square_neighbour_indicies_with_diagonals(xy.x, xy.y, width, height);

          int neighbours = 0;
          for (int n = 0; n < neighbours_info.Length; n++)
          {
            int nidx = neighbours_info[n].Item2;
            neighbours += (map[nidx].entities[0] == EntityType.tile_type_wall) ? 1 : 0;
          }

          // If there are more than 4, or zero, neighboring walls 
          // - then the tile (in newtiles) becomes a wall. 
          // Otherwise, it becomes a floor.
          if (neighbours > 4 || neighbours == 0)
            newtiles[idx].entities.Add(EntityType.tile_type_wall);
          else
            newtiles[idx].entities.Add(EntityType.tile_type_floor);
        }

        // Copy newtiles back in to map
        map = newtiles;
      }

      return map;
    }

    // Problem: <4 should not be removing some stuff
    // need to take a copy of map and not iterate over changing map
    public static void ObstaclePostProcessing(MapEntry[] map, int count, int width, int height)
    {
      for (int idx = 0; idx < map.Length; idx++)
      {
        var xy = Grid.IndexToPos(idx, width, height);
        var neighbours_info = a_star.square_neighbour_indicies_with_diagonals(xy.x, xy.y, width, height);

        int neighbours = 0;
        for (int n = 0; n < neighbours_info.Length; n++)
        {
          int nidx = neighbours_info[n].Item2;
          neighbours += (map[nidx].entities[0] == EntityType.tile_type_wall) ? 1 : 0;
        }

        // Remove any isolated cells
        if (neighbours <= count)
          map[idx].entities[0] = EntityType.tile_type_floor;
      }
    }

    public static Vector2Int StartPoint(MapEntry[] map, int width, int height)
    {
      // Find a starting point; start at the middle and walk left until we find an open tile
      var pos = new Vector2Int(width / 2, height / 2);
      int idx = Grid.GetIndex(pos, width);

      while (!map[idx].entities.Contains(EntityType.tile_type_floor))
      {
        pos.x -= 1;
        idx = Grid.GetIndex(pos, width);
      }

      // WARNING: in theory, there could be no floor tile on this row

      return pos;
    }

    public static Vector2Int ExitPoint(MapEntry[] map, int width, int height)
    {
      // Find all tiles we can reach from the starting point
      var start = StartPoint(map, width, height);
      var start_idx = Grid.GetIndex(start, width);

      // Generate DijkstraMap
      var astar = map_manager.GameToAStar(map, width, height);
      var dmap = a_star.generate_accessible_areas(astar, start_idx, int.MaxValue, width, height);

      // Get the tile furthest away from the starting point 
      (int, int) result = (start_idx, 0);
      for (int i = 0; i < dmap.Length; i++)
      {
        var visible_cell = dmap[i];
        if (visible_cell.distance > result.Item2)
          result = (Grid.GetIndex(visible_cell.pos, width), visible_cell.distance);
      }

      return Grid.IndexToPos(result.Item1, width, height);
    }

  }
} // namespace Wiggy