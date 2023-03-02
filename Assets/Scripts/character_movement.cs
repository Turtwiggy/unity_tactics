using UnityEngine;
using UnityEngine.InputSystem;

namespace Wiggy
{
  class character_movement : MonoBehaviour
  {
    public camera_handler camera_handler;
    public map_manager map_manager;

    public GameObject selected;
    public GameObject cursor_selected;
    public GameObject cursor_available_prefab;

    private void Start()
    {
      camera_handler = FindObjectOfType<camera_handler>();
      map_manager = FindObjectOfType<map_manager>();
    }

    private void Update()
    {
      Vector2Int grid = camera_handler.grid_index;

      if (Mouse.current.leftButton.wasPressedThisFrame)
      {
        GameObject map_obj = map_manager.GetAt(grid.x, grid.y);
        if (map_obj != null)
        {
          Debug.Log("go: " + map_obj.transform.name);
          Debug.Log("gi: " + grid);
          selected = map_obj;

          var world_space = Grid.GridSpaceToWorldSpace(grid, camera_handler.grid_size);
          cursor_selected.transform.position = world_space;
        }
      }

      // if something is selected, show the cursor
      cursor_selected.SetActive(selected != null);

      if (Mouse.current.rightButton.wasPressedThisFrame)
      {
        if (!selected)
          return; // selected is null

        if (map_manager.GetAt(grid.x, grid.y) != null)
          return; // space is taken

        var world_space = Grid.GridSpaceToWorldSpace(grid, camera_handler.grid_size);
        selected.transform.position = world_space;

        map_manager.SetAt(grid.x, grid.y, selected);
        selected = null;
      }
    }
  }
} // namespace Wiggy