using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Wiggy
{
  class character_movement : MonoBehaviour
  {
    public camera_handler camera_handler;
    public map_manager map_manager;

    public int from_index = -1;
    public GameObject cursor_selected;
    public GameObject cursor_available_prefab;
    private List<GameObject> instantiated_cursor_available = new();

    private System.Action<Vector2Int> lmb_clicked;
    private System.Action<Vector2Int> rmb_clicked;

    private void Start()
    {
      camera_handler = FindObjectOfType<camera_handler>();
      map_manager = FindObjectOfType<map_manager>();

      lmb_clicked += (vec) => { HandleSelect(vec); };
      rmb_clicked += (vec) => { HandleMove(vec); };
    }

    private void HandleSelect(Vector2Int xy)
    {
      int index = Grid.GetIndex(xy, camera_handler.grid_width);
      var cell = map_manager.cells[index];
      var go = map_manager.gos[index];

      if (go != null)
      {
        var world_space = Grid.GridSpaceToWorldSpace(cell.pos, camera_handler.grid_size);
        cursor_selected.transform.position = world_space;

        from_index = index;
      }
      else
        from_index = -1;
    }

    private void HandleMove(Vector2Int new_xy)
    {
      if (from_index == -1)
        return; // from space must be selected

      int to_index = Grid.GetIndex(new_xy, camera_handler.grid_width);

      //
      // validate "to"
      //
      {
        var to_go = map_manager.gos[to_index];

        // "to" must be empty
        if (to_go != null)
          return;
      }

      //
      // update "to"
      //
      map_manager.gos[to_index] = map_manager.gos[from_index];
      map_manager.cells[to_index].pos = new_xy;

      //
      // clear "from"
      //
      map_manager.gos[from_index] = null;
      map_manager.cells[from_index].pos = new Vector2Int(-1, -1);

      // update position in world
      var cell = map_manager.cells[to_index];
      var go = map_manager.gos[to_index];
      var world_space = Grid.GridSpaceToWorldSpace(cell.pos, camera_handler.grid_size);
      go.transform.position = world_space;

      // finished with selected object
      from_index = -1;
    }

    private void Update()
    {
      // if something is selected, show the cursor
      cursor_selected.SetActive(from_index != -1);

      if (Mouse.current.leftButton.wasPressedThisFrame)
      {
        Vector2Int grid = camera_handler.grid_index;
        lmb_clicked.Invoke(grid);
      }

      if (Mouse.current.rightButton.wasPressedThisFrame)
      {
        Vector2Int grid = camera_handler.grid_index;
        rmb_clicked.Invoke(grid);
      }

      if (Keyboard.current.enterKey.wasPressedThisFrame && from_index != -1)
      {
        // from: the lmb clicked cell
        var from = map_manager.cells[from_index];

        // to: the currently hovered cell
        var to = map_manager.cells[Grid.GetIndex(camera_handler.grid_index, camera_handler.grid_width)];

        var paths = a_star.generate_direct(map_manager.cells, from, to, camera_handler.grid_width);

        // TODO: improve this
        foreach (GameObject go in instantiated_cursor_available)
          Destroy(go);

        for (int i = 0; i < paths.Length; i++)
        {
          var pos = Grid.GridSpaceToWorldSpace(paths[i].pos, camera_handler.grid_size);
          var cursor_available = Instantiate(cursor_available_prefab);
          cursor_available.transform.position = pos;
          cursor_available.SetActive(true);
          instantiated_cursor_available.Add(cursor_available);
        }

      }
    }
  }
} // namespace Wiggy