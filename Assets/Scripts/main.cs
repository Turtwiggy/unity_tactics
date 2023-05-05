using UnityEngine;
using UnityEngine.EventSystems;

namespace Wiggy
{
  public class main : MonoBehaviour
  {
    [Header("Entities")]
    public GameObject player_prefab;
    public GameObject enemy_prefab;
    public GameObject selected_cursor_prefab;

    // unity-based systems
    [HideInInspector] public input_handler input;
    [HideInInspector] public camera_handler camera;
    [HideInInspector] public map_manager map;
    [HideInInspector] public map_visual_manager mvm;
    [HideInInspector] public main_ui ui;

    // ecs-based systems
    public Wiggy.registry ecs;
    public ActionSystem action_system;
    public AiSystem ai_system;
    public CombatSystem combat_system;
    public EndTurnSystem end_turn_system;
    public ExtractionSystem extraction_system;
    public HealSystem heal_system;
    public InstantiateSystem instantiate_system;
    public MonitorCombatEventsSystem monitor_combat_events_system;
    public MonitorOverwatchSystem monitor_overwatch_system;
    public MoveSystem move_system;
    public OverwatchSystem overwatch_system;
    public ReloadSystem reload_system;
    public SelectSystem select_system;
    public UnitSpawnSystem unit_spawn_system;

    public void RegisterComponents(Wiggy.registry ecs)
    {
      ecs.RegisterComponent<InstantiatedComponent>();
      ecs.RegisterComponent<ToBeInstantiatedComponent>();
      // actions
      ecs.RegisterComponent<ActionsComponent>();
      // movement
      ecs.RegisterComponent<GridPositionComponent>();
      // ai
      ecs.RegisterComponent<AIMoveConsiderationComponent>();
      // events
      ecs.RegisterComponent<AttackEvent>();
      // combat
      ecs.RegisterComponent<AmmoComponent>();
      ecs.RegisterComponent<HealthComponent>();
      ecs.RegisterComponent<TargetsComponent>();
      ecs.RegisterComponent<WeaponComponent>();
      ecs.RegisterComponent<TeamComponent>();
      ecs.RegisterComponent<OverwatchStatus>();
      // entity tags
      ecs.RegisterComponent<CursorComponent>();
      ecs.RegisterComponent<PlayerComponent>();
      // AI
      ecs.RegisterComponent<DefaultBrainComponent>();
      // Requests
      ecs.RegisterComponent<WantsToAttack>();
      ecs.RegisterComponent<WantsToHeal>();
      ecs.RegisterComponent<WantsToMove>();
      ecs.RegisterComponent<WantsToOverwatch>();
      ecs.RegisterComponent<WantsToReload>();
    }
    public void RegisterSystems(Wiggy.registry ecs)
    {
      action_system = ecs.RegisterSystem<ActionSystem>();
      ai_system = ecs.RegisterSystem<AiSystem>();
      combat_system = ecs.RegisterSystem<CombatSystem>();
      end_turn_system = ecs.RegisterSystem<EndTurnSystem>();
      extraction_system = ecs.RegisterSystem<ExtractionSystem>();
      heal_system = ecs.RegisterSystem<HealSystem>();
      instantiate_system = ecs.RegisterSystem<InstantiateSystem>();
      monitor_combat_events_system = ecs.RegisterSystem<MonitorCombatEventsSystem>();
      monitor_overwatch_system = ecs.RegisterSystem<MonitorOverwatchSystem>();
      move_system = ecs.RegisterSystem<MoveSystem>();
      overwatch_system = ecs.RegisterSystem<OverwatchSystem>();
      reload_system = ecs.RegisterSystem<ReloadSystem>();
      select_system = ecs.RegisterSystem<SelectSystem>();
      unit_spawn_system = ecs.RegisterSystem<UnitSpawnSystem>();
    }
    public void RegisterSystemSignatures(Wiggy.registry ecs)
    {
      action_system.SetSignature(ecs);
      ai_system.SetSignature(ecs);
      combat_system.SetSignature(ecs);
      end_turn_system.SetSignature(ecs);
      extraction_system.SetSignature(ecs);
      heal_system.SetSignature(ecs);
      instantiate_system.SetSignature(ecs);
      monitor_combat_events_system.SetSignature(ecs);
      monitor_overwatch_system.SetSignature(ecs);
      move_system.SetSignature(ecs);
      overwatch_system.SetSignature(ecs);
      reload_system.SetSignature(ecs);
      select_system.SetSignature(ecs);
      unit_spawn_system.SetSignature(ecs);
    }

    void Start()
    {
      ecs = new();
      RegisterComponents(ecs);
      RegisterSystems(ecs);
      RegisterSystemSignatures(ecs);

      map = FindObjectOfType<map_manager>();
      input = FindObjectOfType<input_handler>();
      camera = FindObjectOfType<camera_handler>();
      ui = FindObjectOfType<main_ui>();
      mvm = FindObjectOfType<map_visual_manager>();

      // map.seed = 0;
      // map.zone_seed = 0;
      // map.GenerateMap();

      // ecs-based systems

      UnitSpawnSystem.UnitSpawnSystemInit uss_data = new()
      {
        player_prefab = player_prefab,
        enemy_prefab = enemy_prefab
      };

      action_system.Start(ecs, this);
      ai_system.Start(ecs, unit_spawn_system);
      combat_system.Start(ecs);
      end_turn_system.Start(ecs);
      extraction_system.Start(ecs);
      heal_system.Start(ecs);
      instantiate_system.Start(ecs, map);
      move_system.Start(ecs, this);
      monitor_combat_events_system.Start(ecs);
      monitor_overwatch_system.Start(ecs, move_system);
      overwatch_system.Start(ecs);
      reload_system.Start(ecs);
      select_system.Start(ecs, unit_spawn_system, selected_cursor_prefab);
      unit_spawn_system.Start(ecs, uss_data);

      mvm.DoStart();
      mvm.RefreshVisuals();
      ui.DoStart(this);
    }

    void Update()
    {
      // Camera
      camera.HandleCursorOnGrid();
      camera.HandleCameraZoom();

      // Input
      var cursor_over_ui = EventSystem.current.IsPointerOverGameObject();
      if (!cursor_over_ui && input.a_input)
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

      // Systems that update every frame
      action_system.Update(ecs);
      combat_system.Update(ecs);
      extraction_system.Update(ecs, map.ext_spots);
      heal_system.Update(ecs);
      instantiate_system.Update(ecs);
      move_system.Update(ecs);
      overwatch_system.Update(ecs);
      monitor_combat_events_system.Update(ecs);
      monitor_overwatch_system.Update(ecs); // dep: overwatch_system
      reload_system.Update(ecs);
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