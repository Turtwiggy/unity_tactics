using UnityEngine;

namespace Wiggy
{
  enum PHASE
  {
    SELECT,
    CONFIRM,
  };

  [System.Serializable]
  class main : MonoBehaviour
  {
    // Systems
    input_handler input_handler;
    camera_handler camera_handler;
    map_manager map_manager;
    unit_move unit_move;
    unit_select unit_select;

    [SerializeField]
    PHASE selection_phase = PHASE.SELECT;

    private bool busy = false;

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

    void Update()
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
        UnitAct(camera_handler.grid_index);

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

    private bool IsCharacter(int index)
    {
      var go = map_manager.gos[index];
      if (go == null)
        return false;
      return go.GetComponent<character_stats>() != null;
    }

    private void Attack(int from, int to)
    {
      var atk = map_manager.gos[from].GetComponent<character_stats>();
      var def = map_manager.gos[to].GetComponent<character_stats>();

      DamageEvent e = new DamageEvent();
      e.amount = atk.RndDamage();
      e.attackers_type = atk.weakness;

      // check if the defender is in cover
      var covered = map_manager.high_cover_spots[to].covered_by.Count > 0;
      if (covered)
      {
        Debug.Log("attacked unit was in cover...!");
        e.amount /= 2;
      }
      Debug.Log("dealt damage: " + e.amount);

      def.TakeDamage(e, () =>
      {
        // check if the defender was DESTROYED
        Destroy(def.gameObject);
        map_manager.gos[to] = null;
        map_manager.cells[to].path_cost = 0;

        // give the attacker XP
        atk.GiveXP();
      });
    }

    private async void UnitAct(Vector2Int xy)
    {
      busy = true;

      int width = camera_handler.grid_width;
      int size = camera_handler.grid_size;

      // select...
      if (selection_phase == PHASE.SELECT)
      {
        unit_select.Select(xy, width);
        if (unit_select.from_index != -1)
          selection_phase = PHASE.CONFIRM;
      }
      // confirm...
      else
      {
        var from = unit_select.from_index;
        var to = Grid.GetIndex(xy, width);

        if (from == to)
          return;

        if (IsCharacter(to))
          Attack(from, to);
        else
          await unit_move.Move(map_manager, from, to, width, size);

        unit_select.ClearSelection();
        selection_phase = PHASE.SELECT;
        Debug.Log("done...");
      }

      busy = false;
    }
  }

  // Dumb AI: 
  // AI takes the most immediately reasonable action with no history.
  // Get nearest player
  // Move cell closest to player
  // Attack it
  // Simulate thinking time?
}