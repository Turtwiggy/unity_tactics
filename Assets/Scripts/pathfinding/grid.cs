using UnityEngine;

namespace Wiggy
{
  public static class Grid
  {
    public static Vector3 GridSpaceToWorldSpace(Vector2Int grid, int size_per_grid_tile)
    {
      var world_space = new Vector3(grid.x, 0f, grid.y) * size_per_grid_tile; // bottom_left
      world_space += new Vector3(1, 0, 1) * (size_per_grid_tile * 0.5f); // center
      return world_space;
    }

    public static Vector2Int WorldSpaceToGridSpace(Vector3 point, int size_per_grid_tile, int x_max)
    {
      int grid_x = (int)(point.x / size_per_grid_tile);
      int grid_z = (int)(point.z / size_per_grid_tile);
      grid_x = Mathf.Clamp(grid_x, 0, x_max - 1);
      grid_z = Mathf.Clamp(grid_z, 0, x_max - 1);
      return new Vector2Int(grid_x, grid_z);
    }

    public static int GetIndex(Vector2Int grid, int x_max)
    {
      return x_max * grid.y + grid.x;
    }

    public static int GetIndex(int x, int y, int x_max)
    {
      return x_max * y + x;
    }

    public static Vector2Int IndexToPos(int index, int x_max, int y_max)
    {
      int x = index % x_max;
      int y = (int)(index / (float)y_max);
      return new Vector2Int(x, y);
    }

  }

}