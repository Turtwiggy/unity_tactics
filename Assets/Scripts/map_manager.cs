using UnityEngine;

namespace Wiggy
{
  class map_manager : MonoBehaviour
  {
    public camera_handler camera_handler;

    public GameObject charcter_holder;

    private GameObject[] map;
    private int x_max;

    void Start()
    {
      camera_handler = FindObjectOfType<camera_handler>();

      int grid_size = camera_handler.grid_size;
      int grid_width = camera_handler.grid_width;
      int grid_height = camera_handler.grid_height;
      x_max = camera_handler.grid_width;
      map = new GameObject[grid_width * grid_height];

      foreach (Transform t in charcter_holder.transform)
      {
        var position = t.position;
        var grid = Grid.WorldSpaceToGridSpace(position, grid_size, grid_width);
        SetAt(grid.x, grid.y, t.gameObject);
      }
    }

    public GameObject GetAt(int x, int y)
    {
      return map[Grid.GetIndex(x, y, x_max)];
    }

    public void SetAt(int x, int y, GameObject go)
    {
      // Definitely a better way to move an objects position from a->b
      for (int i = 0; i < map.Length; i++)
        if (map[i] == go)
          map[i] = null;

      map[Grid.GetIndex(x, y, x_max)] = go;
    }

  }

} // namespace Wiggy