using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  [System.Serializable]
  class main : MonoBehaviour
  {
    input_handler input_handler;
    camera_handler camera_handler;
    map_manager map;
    unit_move unit_move;
    unit_select unit_select;
    objective_manager objective_manager;

    public UnityEvent<Vector2Int> unit_moved { get; private set; }

    private bool busy = false;

    void Start()
    {
      input_handler = FindObjectOfType<input_handler>();
      camera_handler = FindObjectOfType<camera_handler>();
      map = FindObjectOfType<map_manager>();
      unit_move = FindObjectOfType<unit_move>();
      unit_select = FindObjectOfType<unit_select>();
      objective_manager = FindObjectOfType<objective_manager>();

      camera_handler.DoStart();
      map.DoStart();
      unit_select.DoStart();
      objective_manager.DoStart();

      unit_moved = new();
      unit_moved.AddListener((pos) => objective_manager.UnitMovedEvent(pos));

      // units.CreateUnit(grid, new coord(0, 1), "Wiggy", Team.PLAYER);
      // units.CreateUnit(grid, new coord(0, 2), "Wallace", Team.PLAYER);
      // units.CreateUnit(grid, new coord(2, 3), "Elite Goblin", Team.ENEMY);
      // units.CreateUnit(grid, new coord(3, 3), "Goblin", Team.ENEMY);
    }

    async void Update()
    {
      camera_handler.HandleCursorOnGrid();
      camera_handler.HandleCameraZoom();
      unit_select.UpdateUI(map.size);

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
      }

      if (input_handler.b_input)
        unit_select.ClearSelection();
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
      int x_max = map.width;
      int size = map.size;

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
      bool selected_a_unit = unit_attack.IsCharacter(map, to);
      if (selected_a_unit)
        unit_attack.Attack(map, from, to, x_max);

      // a different tile?
      else
      {
        await unit_move.Move(map, from, to, x_max, size);
        unit_moved.Invoke(xy);
      }

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