using System.Threading.Tasks;
using UnityEngine;

namespace Wiggy
{
  [System.Serializable]
  class main : MonoBehaviour
  {
    // Systems
    input_handler input_handler;
    camera_handler camera_handler;
    map_manager map_manager;
    unit_move unit_move;
    unit_select unit_select;

    private bool busy = false;
    private GameObject debug_go_0;
    private GameObject debug_go_1;

    void Start()
    {
      input_handler = FindObjectOfType<input_handler>();
      camera_handler = FindObjectOfType<camera_handler>();
      map_manager = FindObjectOfType<map_manager>();
      unit_move = FindObjectOfType<unit_move>();
      unit_select = FindObjectOfType<unit_select>();

      // units.player_prefab = player_prefab;
      // units.enemy_prefab = enemy_prefab;
      // units.CreateUnit(grid, new coord(0, 1), "Wiggy", Team.PLAYER);
      // units.CreateUnit(grid, new coord(0, 2), "Wallace", Team.PLAYER);
      // units.CreateUnit(grid, new coord(2, 3), "Elite Goblin", Team.ENEMY);
      // units.CreateUnit(grid, new coord(3, 3), "Goblin", Team.ENEMY);
    }

    async void Update()
    {
      camera_handler.HandleCursorOnGrid();
      camera_handler.HandleCameraZoom();
      unit_select.UpdateUI(camera_handler.grid_size);

      // CURSOR IN WORLDSPACE??
      // var x = input_handler.l_analogue.x;
      // if (x > 0 && x < 1.0f)
      //   select_pos.x += 1;
      // if (x < 0 && x > -1.0f)
      //   select_pos.x -= 1;
      // var y = input_handler.l_analogue.y;
      // if (y > 0 && y < 1.0f)
      //   select_pos.y += 1;
      // if (y < 0 && y > -1.0f)
      //   select_pos.y -= 1;

      if (input_handler.a_input && !busy)
      {
        busy = true;
        await UnitAct(camera_handler.grid_index);
        busy = false;
        Debug.Log("done act");
      }

      if (input_handler.b_input)
        unit_select.ClearSelection();

#if DEBUG
      if (debug_go_0 != null && debug_go_1 != null)
        Debug.DrawLine(debug_go_0.transform.position, debug_go_1.transform.position, Color.red);
#endif
    }

    void LateUpdate()
    {
      float delta = Time.deltaTime;
      input_handler.DoLateUpdate();
      camera_handler.HandleCameraMovement(delta, input_handler.l_analogue);
      camera_handler.HandleCameraLookAt();
    }

    private async Task UnitAct(Vector2Int xy)
    {
      int x_max = camera_handler.grid_width;
      int size = camera_handler.grid_size;

      //
      // Must select a unit
      //
      bool unit_selected = unit_select.from_index != -1;
      if (!unit_selected)
      {
        unit_select.Select(xy, x_max);
        return;
      }

      //
      // Now there's a unit selected.
      // What did the user select next?
      //
      var from = unit_select.from_index;
      var to = Grid.GetIndex(xy, x_max);

      // the same tile?
      if (from == to)
        return;

      // a unit?
      bool selected_a_unit = unit_attack.IsCharacter(map_manager, to);
      if (selected_a_unit)
      {
        Debug.Log("attacking...");
        debug_go_0 = map_manager.gos[from];
        debug_go_1 = map_manager.gos[to];
        unit_attack.Attack(map_manager, from, to, x_max);
      }

      // a different tile?
      else
        await unit_move.Move(map_manager, from, to, x_max, size);

      //
      // Assume action was successful, and clear the selected tile
      //
      unit_select.ClearSelection();
    }
  }

  // Dumb AI: 
  // AI takes the most immediately reasonable action with no history.
  // Get nearest player
  // Move cell closest to player
  // Attack it
  // Simulate thinking time?
}