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
    character_movement character_movement;
    map_manager map_manager;

    [SerializeField]
    PHASE selection_phase = PHASE.SELECT;

    void Start()
    {
      input_handler = FindObjectOfType<input_handler>();
      camera_handler = FindObjectOfType<camera_handler>();
      character_movement = FindObjectOfType<character_movement>();
      map_manager = FindObjectOfType<map_manager>();

      // units.player_prefab = player_prefab;
      // units.enemy_prefab = enemy_prefab;
      // units.CreateUnit(grid, new coord(0, 1), "Wiggy", Team.PLAYER);
      // units.CreateUnit(grid, new coord(0, 2), "Wallace", Team.PLAYER);
      // units.CreateUnit(grid, new coord(2, 3), "Elite Goblin", Team.ENEMY);
      // units.CreateUnit(grid, new coord(3, 3), "Goblin", Team.ENEMY);
    }

    async void Update()
    {
      var grid_xy = camera_handler.HandleCursorOnGrid();
      camera_handler.HandleCameraZoom();

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

      if (input_handler.a_input)
      {
        var valid = grid_xy.Item1;
        var xy = grid_xy.Item2;

        // select...
        if (valid && selection_phase == PHASE.SELECT)
        {
          character_movement.SelectUnit(xy, camera_handler.grid_width, camera_handler.grid_size);
          if (character_movement.from_index != -1)
            selection_phase = PHASE.CONFIRM;
        }
        // confirm...
        else if (valid && selection_phase == PHASE.CONFIRM)
        {
          await character_movement.MoveUnit(xy, camera_handler.grid_width, camera_handler.grid_size);
          selection_phase = PHASE.SELECT;
        }
      }
    }

    void LateUpdate()
    {
      float delta = Time.deltaTime;
      input_handler.DoLateUpdate();
      camera_handler.HandleCameraMovement(delta, input_handler.l_analogue);
      camera_handler.HandleCameraLookAt();
    }

    // Dumb AI: 
    // AI takes the most immediately reasonable action with no history.
    // Get nearest player
    // Move cell closest to player
    // Attack it
    // Simulate thinking time?
  }
}