using UnityEngine;
using System.Collections.Generic;

namespace Wiggy
{
  [System.Serializable]
  public class high_cover_spot
  {
    public GameObject instantiated_prefab;

    [SerializeField]
    public HashSet<square_direction> covered_by = new();
  };

  public class map_manager : MonoBehaviour
  {
    public GameObject charcter_holder;
    public GameObject obstacle_holder;
    public GameObject debug_spot_prefab;
    public GameObject cover_spot_holder;

    public GameObject map_holder_public;
    public GameObject wall_prefab;
    public GameObject floor_prefab;

    // >:(
    public cell[] cells { get; private set; }
    public GameObject[] gos { get; private set; }
    public high_cover_spot[] high_cover_spots { get; private set; }
    public objective_hold_zone[] objective_spots { get; private set; }

    // map gen info
    public int width = 30;
    public int height = 30;
    public int size = 1;
    public int iterations = 5;
    public int seed = 0;

    public void GenerateMap()
    {
      string holder_name = "Generated Map";
      { // Existing Map Holder
        var holder = map_holder_public.transform.Find(holder_name);
        if (holder)
          DestroyImmediate(holder.gameObject);
      }

      // Warning: this would overwrite any editor edits
      foreach (Transform t in obstacle_holder.transform)
        DestroyImmediate(t.gameObject);

      // New Map Holder
      var map_holder = new GameObject(holder_name).transform;
      map_holder.parent = map_holder_public.transform;

      var map = map_gen_cell_automata.Generate(width, height, iterations, seed);
      var srt = map_gen_cell_automata.StartPoint(map, width, height);
      var end = map_gen_cell_automata.ExitPoint(map, width, height);

      // Visualize the map
      for (int i = 0; i < map.Length; i++)
      {
        var xy = Grid.IndexToPos(i, width, height);
        var pos = Grid.GridSpaceToWorldSpace(xy, size);

        if (map[i] == TILE_TYPE.WALL)
        {
          pos.y += 0.5f;
          Instantiate(wall_prefab, pos, Quaternion.identity, obstacle_holder.transform);
        }
        else if (map[i] == TILE_TYPE.FLOOR)
        {
          // Use one global floor
          // Instantiate(floor_prefab, pos, Quaternion.Euler(Vector3.right * 90), map_holder);
        }
      }

      // DEBUG START AND EXIT POINTS
      {
        var index = Grid.GetIndex(srt, width);
        var pos = Grid.IndexToPos(index, width, height);
        var wpos = Grid.GridSpaceToWorldSpace(pos, size);
        Instantiate(debug_spot_prefab, wpos, Quaternion.identity, map_holder);
      }
      {
        var index = Grid.GetIndex(end, width);
        var pos = Grid.IndexToPos(index, width, height);
        var wpos = Grid.GridSpaceToWorldSpace(pos, size);
        Instantiate(debug_spot_prefab, wpos, Quaternion.identity, map_holder);
      }
    }

    public static cell[] GeneratedToGame(TILE_TYPE[] gen, int x_max, int y_max)
    {
      cell[] map = new cell[gen.Length];

      for (int i = 0; i < gen.Length; i++)
      {
        cell c = new();
        c.pos = Grid.IndexToPos(i, x_max, y_max);
        c.path_cost = gen[i] == TILE_TYPE.WALL ? -1 : 1;
        map[i] = c;
      }

      return map;
    }

    public void DoStart()
    {
      int x_max = width;
      int dim = width * height;
      cells = new cell[dim];
      gos = new GameObject[dim];
      high_cover_spots = new high_cover_spot[dim];
      objective_spots = new objective_hold_zone[dim];

      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          var pos = new Vector2Int(x, y);
          var index = Grid.GetIndex(pos, width);
          cells[index] = new() { pos = pos };
          high_cover_spots[index] = new();
        }
      }

      //
      // populate grid gameobjects from world
      //
      foreach (Transform t in charcter_holder.transform)
      {
        var position = t.position;
        var grid = Grid.WorldSpaceToGridSpace(position, size, x_max);
        var index = Grid.GetIndex(grid, x_max);

        cells[index].pos = grid;
        cells[index].path_cost = -1; // impassable
        gos[index] = t.gameObject;

        // make it a character
        t.gameObject.AddComponent<character_stats>();
      }
      foreach (Transform t in obstacle_holder.transform)
      {
        var position = t.position;
        var grid = Grid.WorldSpaceToGridSpace(position, size, x_max);
        var index = Grid.GetIndex(grid, x_max);

        cells[index].pos = grid;
        cells[index].path_cost = -1; // impassable
        gos[index] = t.gameObject;
      }

      // assume each spot next to an obstacle 
      // (that isnt blocked) is a high cover spot
      foreach (Transform t in obstacle_holder.transform)
      {
        var position = t.position;
        var grid = Grid.WorldSpaceToGridSpace(position, size, x_max);
        var neighbour_idxs = a_star.square_neighbour_indicies(grid.x, grid.y, x_max, x_max);
        for (int i = 0; i < neighbour_idxs.Length; i++)
        {
          var neighbour_idx = neighbour_idxs[i].Item2;
          if (gos[neighbour_idx] != null)
            continue; // full

          // each spot around an obstacle is a high cover spot, covered by this obstacle
          var hcs_direction = neighbour_idxs[i].Item1;
          high_cover_spots[neighbour_idx].covered_by.Add(hcs_direction.Opposite());
        }
      }

      // DEBUG: debug the cover spots
      // for (int index = 0; index < high_cover_spots.Length; index++)
      // {
      //   var hcs = high_cover_spots[index];
      //   if (hcs.covered_by.Count == 0)
      //     continue; // not a hcs
      //   var pos = Grid.IndexToPos(index, width, height);
      //   var wpos = Grid.GridSpaceToWorldSpace(pos, camera_handler.grid_size);
      //   hcs.instantiated_prefab = Instantiate(cover_spot_prefab, wpos, Quaternion.identity, cover_spot_holder.transform);
      // }
    }
  }
} // namespace Wiggy