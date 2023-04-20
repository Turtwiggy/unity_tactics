using UnityEngine;

namespace Wiggy
{
  public class main : MonoBehaviour
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
    public input_handler input;
    public camera_handler camera;
    public map_manager map;
    public main_ui ui;

    // ecs-based systems
    public Wiggy.registry ecs;
    public ActionSystem action_system;
    public AiSystem ai_system;
    public EndTurnSystem end_turn_system;
    public ExtractionSystem extraction_system;
    public FovSystem fov_system;
    public InstantiateSystem instantiate_system;
    public SelectSystem select_system;
    public UnitSpawnSystem unit_spawn_system;

    void Start()
    {
      // Register all components
      ecs = new();
      ecs.RegisterComponent<InstantiatedComponent>();
      ecs.RegisterComponent<ToBeInstantiatedComponent>();
      ecs.RegisterComponent<GridPositionComponent>();
      // combat
      ecs.RegisterComponent<ActionsComponent>();
      ecs.RegisterComponent<AmmoComponent>();
      ecs.RegisterComponent<HealthComponent>();
      // entity tags
      ecs.RegisterComponent<CursorComponent>();
      ecs.RegisterComponent<PlayerComponent>();
      // AI
      ecs.RegisterComponent<DefaultBrainComponent>();

      action_system = ecs.RegisterSystem<ActionSystem>();
      ai_system = ecs.RegisterSystem<AiSystem>();
      end_turn_system = ecs.RegisterSystem<EndTurnSystem>();
      extraction_system = ecs.RegisterSystem<ExtractionSystem>();
      fov_system = ecs.RegisterSystem<FovSystem>();
      instantiate_system = ecs.RegisterSystem<InstantiateSystem>();
      select_system = ecs.RegisterSystem<SelectSystem>();
      unit_spawn_system = ecs.RegisterSystem<UnitSpawnSystem>();

      action_system.SetSignature(ecs);
      ai_system.SetSignature(ecs);
      end_turn_system.SetSignature(ecs);
      extraction_system.SetSignature(ecs);
      fov_system.SetSignature(ecs);
      instantiate_system.SetSignature(ecs);
      select_system.SetSignature(ecs);
      unit_spawn_system.SetSignature(ecs);

      map = FindObjectOfType<map_manager>();
      input = FindObjectOfType<input_handler>();
      camera = FindObjectOfType<camera_handler>();
      ui = FindObjectOfType<main_ui>();

      // HACK
      fov_pos = map.srt_spots[0];

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

      action_system.Start(ecs, this);
      ai_system.Start(ecs);
      end_turn_system.Start(ecs);
      extraction_system.Start(ecs);
      fov_system.Start(ecs, fov_data);
      instantiate_system.Start(ecs, map);
      select_system.Start(ecs, unit_spawn_system, selected_cursor_prefab);
      unit_spawn_system.Start(ecs, us_data);

      ui.DoStart(this);
    }

    void Update()
    {
      // Camera
      camera.HandleCursorOnGrid();
      camera.HandleCameraZoom();

      // Input
      if (input.a_input)
      {
        if (!select_system.HasAnySelected())
          select_system.Select();
        else
        {
          // Here, these are actions the user takes because
          // they've interacted with something on the map.
          //
          // An example of an action outside of scope here might be reload.
          // The user wouldnt be able to click anything on the map to make them reload.
          //
          var from = select_system.GetSelected();
          var to = Grid.GetIndex(camera.grid_index, map.width);
          action_system.RequestActionFromMap(ecs, from, to);
        }
      }
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

      // Systems that update every frame
      action_system.Update(ecs);
      extraction_system.Update(ecs, map.ext_spots);
      instantiate_system.Update(ecs);
      select_system.Update(ecs);

      // UI
      ui.DoUpdate();
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