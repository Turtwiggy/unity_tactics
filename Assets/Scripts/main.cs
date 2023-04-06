using UnityEngine;

namespace Wiggy
{
  [System.Serializable]
  class main : MonoBehaviour
  {
    [Header("Entities")]
    public GameObject player_prefab;
    public GameObject enemy_prefab;
    public GameObject selected_cursor_prefab;

    [Header("FOV")]
    private int fov_max_distance = 10;
    public GameObject fov_holder;
    public GameObject fov_cursor_prefab;
    public GameObject fov_grid_prefab;
    private Vector2Int fov_pos = new(0, 0);

    // unity-based systems
    input_handler input;
    camera_handler camera;
    map_manager map;
    main_ui ui;

    // ecs-based systems
    Wiggy.registry ecs;
    ActionSystem action_system;
    ExtractionSystem extraction_system;
    FovSystem fov_system;
    InstantiateSystem instantiate_system;
    SelectSystem select_system;
    UnitSpawnSystem units_system;

    void Start()
    {
      // Register all components
      ecs = new();
      ecs.RegisterComponent<PlayerComponent>();
      ecs.RegisterComponent<CursorComponent>();
      ecs.RegisterComponent<GridPositionComponent>();
      ecs.RegisterComponent<ToBeInstantiatedComponent>();
      ecs.RegisterComponent<InstantiatedComponent>();

      action_system = ecs.RegisterSystem<ActionSystem>();
      extraction_system = ecs.RegisterSystem<ExtractionSystem>();
      fov_system = ecs.RegisterSystem<FovSystem>();
      instantiate_system = ecs.RegisterSystem<InstantiateSystem>();
      select_system = ecs.RegisterSystem<SelectSystem>();
      units_system = ecs.RegisterSystem<UnitSpawnSystem>();

      action_system.SetSignature(ecs);
      extraction_system.SetSignature(ecs);
      fov_system.SetSignature(ecs);
      instantiate_system.SetSignature(ecs);
      select_system.SetSignature(ecs);
      units_system.SetSignature(ecs);

      map = FindObjectOfType<map_manager>();
      input = FindObjectOfType<input_handler>();
      camera = FindObjectOfType<camera_handler>();
      ui = FindObjectOfType<main_ui>();

      // HACK
      fov_pos = map.srt_spots[0];

      // unity-based systems

      // map.seed = 0;
      // map.zone_seed = 0;
      // map.GenerateMap();

      // ecs-based systems

      FovSystem.FovSystemInit fov_data = new()
      {
        fov_pos = fov_pos,
        fov_holder = fov_holder,
        fov_cursor_prefab = fov_cursor_prefab,
        fov_grid_prefab = fov_grid_prefab,
        max_dst = fov_max_distance
      };

      UnitSpawnSystem.UnitSpawnSystemInit us_data = new()
      {
        player_prefab = player_prefab,
        enemy_prefab = enemy_prefab
      };

      action_system.Start(ecs);
      extraction_system.Start(ecs);
      fov_system.Start(ecs, map, fov_data);
      instantiate_system.Start(ecs, map);
      select_system.Start(ecs, selected_cursor_prefab);
      units_system.Start(ecs, us_data);
    }

    void Update()
    {
      // Camera
      camera.HandleCursorOnGrid();
      camera.HandleCameraZoom();

      // Input
      if (input.a_input)
        select_system.Select();
      if (input.b_input)
        select_system.ClearSelect();
      if (input.d_pad_u)
        fov_pos.y += 1;
      if (input.d_pad_d)
        fov_pos.y -= 1;
      if (input.d_pad_l)
        fov_pos.x -= 1;
      if (input.d_pad_r)
        fov_pos.x += 1;
      if (input.d_pad_d || input.d_pad_u || input.d_pad_l || input.d_pad_r)
        fov_system.Update(ecs, fov_pos);

      // Systems
      extraction_system.Update(ecs, map.ext_spots);
      instantiate_system.Update(ecs);
      select_system.Update(ecs);

      // UI
      main_ui_data ui_data = new();
      ui_data.ready_for_extraction = extraction_system.ready_for_extraction;
      ui.DoUpdate(ui_data);
    }

    void LateUpdate()
    {
      float delta = Time.deltaTime;
      input.DoLateUpdate();
      camera.HandleCameraMovement(delta, input.l_analogue);
      camera.HandleCameraLookAt();
    }
  }
}