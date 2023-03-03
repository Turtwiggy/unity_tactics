using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wiggy
{
  class map_manager : MonoBehaviour
  {
    public camera_handler camera_handler;
    public GameObject charcter_holder;
    public GameObject object_holder;

    // >:(
    private int x_max;
    public cell[] cells { get; private set; }
    public GameObject[] gos { get; private set; }

    void Start()
    {
      camera_handler = FindObjectOfType<camera_handler>();

      int grid_size = camera_handler.grid_size;
      int grid_width = camera_handler.grid_width;
      int grid_height = camera_handler.grid_height;
      x_max = camera_handler.grid_width;

      cells = new cell[grid_width * grid_height];
      for (int x = 0; x < grid_width; x++)
      {
        for (int y = 0; y < grid_height; y++)
        {
          var pos = new Vector2Int(x, y);
          var index = Grid.GetIndex(pos, grid_width);
          cells[index] = new();
          cells[index].pos = pos;
        }
      }

      //
      // populate grid gameobjects from world
      //

      for (int i = 0; i < cells.Length; i++)

        gos = new GameObject[grid_width * grid_height];

      foreach (Transform t in charcter_holder.transform)
      {
        var position = t.position;
        var grid = Grid.WorldSpaceToGridSpace(position, grid_size, grid_width);
        var index = Grid.GetIndex(grid, x_max);

        cells[index].pos = grid;
        cells[index].path_cost = -1; // impassable
        gos[index] = t.gameObject;
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
    }
  }
} // namespace Wiggy