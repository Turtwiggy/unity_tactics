using UnityEngine;

namespace Wiggy
{
  public enum TILE_TYPE
  {
    WALL,
    FLOOR
  };

  // inspired by: 
  // https://bfnightly.bracketproductions.com/rustbook/chapter_27.html

  // TODO mapgen idea: 
  // poisson distribute a bunch of points
  // compute the voronoi on those points to turn in to zones
  // make the different zones differen colours
  // get the floor tiles for each zone
  // spawn things in each zone!

  public static class map_gen_cell_automata
  {
    public static TILE_TYPE[] Generate(int width, int height, int iterations, int seed)
    {
      int dim = width * height;
      TILE_TYPE[] map = new TILE_TYPE[dim];
      Random.InitState(seed);

      // Make the map completely chaos, with 55% set to floor
      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          var rnd = Random.Range(0, 100);
          var idx = Grid.GetIndex(x, y, width);
          if (rnd > 55) { map[idx] = TILE_TYPE.FLOOR; }
          else { map[idx] = TILE_TYPE.WALL; }
        }
      }

      // iteratively apply the cellular atomata rules
      for (int i = 0; i < iterations; i++)
      {
        TILE_TYPE[] newtiles = (TILE_TYPE[])(map.Clone());

        for (int y = 0; y < height; y++)
        {
          for (int x = 0; x < width; x++)
          {
            var neighbours_info = a_star.square_neighbour_indicies_with_diagonals(x, y, width, height);

            int neighbours = 0;
            for (int n = 0; n < neighbours_info.Length; n++)
            {
              int nidx = neighbours_info[n].Item2;
              neighbours += map[nidx] == TILE_TYPE.WALL ? 1 : 0;
            }

            // If there are more than 4, or zero, neighboring walls 
            // - then the tile (in newtiles) becomes a wall. 
            // Otherwise, it becomes a floor.
            var idx = Grid.GetIndex(x, y, width);
            if (neighbours > 4 || neighbours == 0)
              newtiles[idx] = TILE_TYPE.WALL;
            else
              newtiles[idx] = TILE_TYPE.FLOOR;
          }
        }

        // Copy newtiles back in to map
        map = (TILE_TYPE[])newtiles.Clone();
      }

      return map;
    }

    public static Vector2Int StartPoint(TILE_TYPE[] map, int width, int height)
    {
      // Find a starting point; start at the middle and walk left until we find an open tile
      var pos = new Vector2Int(width / 2, height / 2);
      int idx = Grid.GetIndex(pos, width);

      while (map[idx] != TILE_TYPE.FLOOR)
      {
        pos.x -= 1;
        idx = Grid.GetIndex(pos, width);
      }

      // WARNING: in theory, there could be no floor tile on this row

      return pos;
    }

    public static Vector2Int ExitPoint(TILE_TYPE[] map, int width, int height)
    {
      // Find all tiles we can reach from the starting point
      var start = StartPoint(map, width, height);
      var start_idx = Grid.GetIndex(start, width);

      // Generate DijkstraMap
      var gmap = map_manager.GeneratedToGame(map, width, height);
      var dmap = a_star.generate_accessible_areas(gmap, start_idx, int.MaxValue, width, height);

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