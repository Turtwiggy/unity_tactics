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
    public camera_handler camera_handler;
    public GameObject charcter_holder;
    public GameObject object_holder;
    public GameObject cover_spot_prefab;
    public GameObject cover_spot_holder;

    // >:(
    private int x_max;
    public cell[] cells { get; private set; }
    public GameObject[] gos { get; private set; }
    public high_cover_spot[] high_cover_spots { get; private set; }

    void Start()
    {
      camera_handler = FindObjectOfType<camera_handler>();

      int grid_size = camera_handler.grid_size;
      int grid_width = camera_handler.grid_width;
      int grid_height = camera_handler.grid_height;
      int grid_dim = grid_width * grid_height;
      x_max = camera_handler.grid_width;

      cells = new cell[grid_dim];
      gos = new GameObject[grid_dim];
      high_cover_spots = new high_cover_spot[grid_dim];

      // instantiate grid

      for (int x = 0; x < grid_width; x++)
      {
        for (int y = 0; y < grid_height; y++)
        {
          var pos = new Vector2Int(x, y);
          var index = Grid.GetIndex(pos, grid_width);
          cells[index] = new();
          cells[index].pos = pos;
          cells[index].path_cost = 0;

          high_cover_spots[index] = new();
        }
      }

      //
      // populate grid gameobjects from world
      //
      foreach (Transform t in charcter_holder.transform)
      {
        var position = t.position;
        var grid = Grid.WorldSpaceToGridSpace(position, grid_size, grid_width);
        var index = Grid.GetIndex(grid, x_max);

        cells[index].pos = grid;
        cells[index].path_cost = -1; // impassable
        gos[index] = t.gameObject;

        // make it a character
        t.gameObject.AddComponent<character_stats>();
      }
      foreach (Transform t in object_holder.transform)
      {
        var position = t.position;
        var grid = Grid.WorldSpaceToGridSpace(position, grid_size, grid_width);
        var index = Grid.GetIndex(grid, x_max);

        cells[index].pos = grid;
        cells[index].path_cost = -1; // impassable
        gos[index] = t.gameObject;
      }

      // assume each spot next to an obstacle 
      // (that isnt blocked) is a high cover spot
      foreach (Transform t in object_holder.transform)
      {
        var position = t.position;
        var grid = Grid.WorldSpaceToGridSpace(position, grid_size, grid_width);
        var neighbour_idxs = a_star.square_neighbour_indicies(grid.x, grid.y, camera_handler.grid_width, camera_handler.grid_width);
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
      //   var pos = Grid.IndexToPos(index, camera_handler.grid_width, camera_handler.grid_height);
      //   var wpos = Grid.GridSpaceToWorldSpace(pos, camera_handler.grid_size);
      //   hcs.instantiated_prefab = Instantiate(cover_spot_prefab, wpos, Quaternion.identity, cover_spot_holder.transform);
      // }

    }
  }
} // namespace Wiggy