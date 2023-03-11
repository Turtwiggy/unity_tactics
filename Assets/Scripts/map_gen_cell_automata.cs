using UnityEngine;

namespace Wiggy
{
  enum TILE_TYPE
  {
    WALL,
    FLOOR
  };

  // inspired by: 
  // https://bfnightly.bracketproductions.com/rustbook/chapter_27.html
  static class map_gen_cell_automata
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
              // var dir = neighbours_info[i].Item1;
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

    // Try: 
    // Split up map in to voronoi zones
    // Generate items/loot/enemies? based on zone type?
    public static void SpawnEntities()
    {
      //
    }


  }
} // namespace Wiggy