using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wiggy
{
  public class main : MonoBehaviour
  {
    [Header("Entities")]
    public GameObject player_prefab;
    public GameObject enemy_prefab;
    public GameObject barrel_prefab;
    public GameObject trap_prefab;
    public GameObject selected_cursor_prefab;
    public GameObject move_prefab;
    public Texture2D map_texture;

    // unity-based systems
    [HideInInspector] public input_handler input;
    [HideInInspector] public camera_handler camera;
    [HideInInspector] public map_manager map;
    [HideInInspector] public map_visual_manager mvm;
    [HideInInspector] public main_ui ui;
    [HideInInspector] public scene_manager scene_manager;

    // ecs-based systems
    public Wiggy.registry ecs;
    public ActionSystem action_system;
    public AiSystem ai_system;
    public CombatSystem combat_system;
    public EndTurnSystem end_turn_system;
    public ExtractionSystem extraction_system;
    public GrenadeSystem grenade_system;
    public HealSystem heal_system;
    public InstantiateSystem instantiate_system;
    public IsDeadSystem is_dead_system;
    public MonitorCombatEventsSystem monitor_combat_events_system;
    public MonitorOverwatchSystem monitor_overwatch_system;
    public MonitorParticleEffectSystem monitor_particle_effect_system;
    public MoveSystem move_system;
    public OverwatchSystem overwatch_system;
    public ReloadSystem reload_system;
    public SelectSystem select_system;
    public UnitSpawnSystem unit_spawn_system;

    private GameObject vfx_death;
    private GameObject vfx_grenade;
    private GameObject vfx_heal;
    private GameObject vfx_overwatch;
    private GameObject vfx_reload;
    private GameObject vfx_take_damage;

    public void RegisterComponents(Wiggy.registry ecs)
    {
      ecs.RegisterComponent<InstantiatedComponent>();
      ecs.RegisterComponent<ToBeInstantiatedComponent>();
      // actions
      ecs.RegisterComponent<ActionsComponent>();
      // movement
      ecs.RegisterComponent<GridPositionComponent>();
      // stats
      ecs.RegisterComponent<DexterityComponent>();
      // ai
      ecs.RegisterComponent<AIMoveConsiderationComponent>();
      // events
      ecs.RegisterComponent<AttackEvent>();
      ecs.RegisterComponent<ExplodesOnDeath>();
      // combat
      ecs.RegisterComponent<AmmoComponent>();
      ecs.RegisterComponent<HealthComponent>();
      ecs.RegisterComponent<TargetsComponent>();
      ecs.RegisterComponent<WeaponComponent>();
      ecs.RegisterComponent<TeamComponent>();
      ecs.RegisterComponent<OverwatchStatus>();
      ecs.RegisterComponent<IsDeadComponent>();
      // entity tags
      ecs.RegisterComponent<BarrelComponent>();
      ecs.RegisterComponent<CursorComponent>();
      ecs.RegisterComponent<PlayerComponent>();
      ecs.RegisterComponent<ParticleEffectComponent>();
      ecs.RegisterComponent<TrapComponent>();
      // AI
      ecs.RegisterComponent<DefaultBrainComponent>();
      // Requests
      ecs.RegisterComponent<WantsToAttack>();
      ecs.RegisterComponent<WantsToGrenade>();
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
      grenade_system = ecs.RegisterSystem<GrenadeSystem>();
      heal_system = ecs.RegisterSystem<HealSystem>();
      instantiate_system = ecs.RegisterSystem<InstantiateSystem>();
      is_dead_system = ecs.RegisterSystem<IsDeadSystem>();
      monitor_combat_events_system = ecs.RegisterSystem<MonitorCombatEventsSystem>();
      monitor_overwatch_system = ecs.RegisterSystem<MonitorOverwatchSystem>();
      monitor_particle_effect_system = ecs.RegisterSystem<MonitorParticleEffectSystem>();
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
      grenade_system.SetSignature(ecs);
      heal_system.SetSignature(ecs);
      instantiate_system.SetSignature(ecs);
      is_dead_system.SetSignature(ecs);
      monitor_combat_events_system.SetSignature(ecs);
      monitor_overwatch_system.SetSignature(ecs);
      monitor_particle_effect_system.SetSignature(ecs);
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

      camera = FindObjectOfType<camera_handler>();
      map = FindObjectOfType<map_manager>();
      ui = FindObjectOfType<main_ui>();
      mvm = FindObjectOfType<map_visual_manager>();
      input = new GameObject("input_handler").AddComponent<input_handler>();
      scene_manager = new GameObject("scene_manager").AddComponent<scene_manager>();

      var texture_map_entities = LoadMapFromTexture();
      map.seed = 0;
      map.zone_seed = 0;
      map.GenerateMap(texture_map_entities);

      // ecs-based systems

      UnitSpawnSystem.UnitSpawnSystemInit uss_data = new()
      {
        player_prefab = player_prefab,
        enemy_prefab = enemy_prefab,
        barrel_prefab = barrel_prefab,
        trap_prefab = trap_prefab,
        entities = texture_map_entities
      };

      // load resources
      vfx_death = Resources.Load("Prefabs/Death") as GameObject;
      vfx_grenade = Resources.Load("Prefabs/Grenade") as GameObject;
      vfx_heal = Resources.Load("Prefabs/Heal") as GameObject;
      vfx_overwatch = Resources.Load("Prefabs/Overwatch") as GameObject;
      vfx_reload = Resources.Load("Prefabs/Reload") as GameObject;
      vfx_take_damage = Resources.Load("Prefabs/TakeDamage") as GameObject;

      action_system.Start(ecs, this, move_prefab);
      ai_system.Start(ecs, unit_spawn_system, action_system);
      combat_system.Start(ecs);
      end_turn_system.Start(ecs);
      extraction_system.Start(ecs);
      grenade_system.Start(ecs, unit_spawn_system, vfx_grenade);
      heal_system.Start(ecs, vfx_heal);
      instantiate_system.Start(ecs, map);
      is_dead_system.Start(ecs, unit_spawn_system, vfx_death);
      move_system.Start(ecs, this);
      monitor_combat_events_system.Start(ecs, vfx_take_damage);
      monitor_overwatch_system.Start(ecs, move_system);
      monitor_particle_effect_system.Start(ecs);
      overwatch_system.Start(ecs, vfx_overwatch);
      reload_system.Start(ecs, vfx_reload);
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
          action_system.RequestMapInteraction(ecs, from);
        }
      }
      if (input.b_input)
      {
        select_system.ClearSelect();
        action_system.ClearInteraction();
      }

      // Systems that update every frame
      action_system.Update(ecs);
      combat_system.Update(ecs);
      extraction_system.Update(ecs, map.ext_spots);
      grenade_system.Update(ecs);
      heal_system.Update(ecs);
      instantiate_system.Update(ecs);
      move_system.Update(ecs);
      overwatch_system.Update(ecs);
      monitor_combat_events_system.Update(ecs);
      monitor_overwatch_system.Update(ecs); // dep: overwatch_system
      monitor_particle_effect_system.Update(ecs);
      reload_system.Update(ecs);
      select_system.Update(ecs);

      // kill entities last
      is_dead_system.Update(ecs);

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

    List<(int, EntityType)> LoadMapFromTexture()
    {
      ColorUtility.TryParseHtmlString("#ffffff", out var player_colour);
      ColorUtility.TryParseHtmlString("#222034", out var wall_colour);
      ColorUtility.TryParseHtmlString("#6abe30", out var cover_colour);
      ColorUtility.TryParseHtmlString("#5fcde4", out var barrel_colour);
      ColorUtility.TryParseHtmlString("#ac3232", out var enemy_colour);
      ColorUtility.TryParseHtmlString("#d77bba", out var exit_colour);
      ColorUtility.TryParseHtmlString("#fbf236", out var door_colour);
      ColorUtility.TryParseHtmlString("#639bff", out var trap_colour);

      List<(Color, EntityType)> colour_to_entity_type_association = new()
      {
        new(player_colour, EntityType.actor_player),
        new(wall_colour, EntityType.tile_type_wall),
        new(cover_colour, EntityType.tile_type_wall),
        new(barrel_colour, EntityType.actor_barrel),
        new(enemy_colour, EntityType.actor_enemy),
        new(exit_colour, EntityType.tile_type_exit),
        new(door_colour, EntityType.tile_type_door),
        new(trap_colour, EntityType.tile_type_trap)
      };

      List<(int, EntityType)> entities = new();

      for (int x = 0; x < map.width; x++)
      {
        for (int y = 0; y < map.height; y++)
        {
          var pixel = map_texture.GetPixel(x, y);
          if (pixel.a == 0.0f)
            continue;

          for (int i = 0; i < colour_to_entity_type_association.Count; i++)
          {
            var e = colour_to_entity_type_association[i];
            if (e.Item1 == pixel)
            {
              var entity = e.Item2;
              var pos = new Vector2Int(x, y);
              var idx = Grid.GetIndex(pos, map.width);
              entities.Add((idx, entity));

              break;
            }
          }
        }
      }

      return entities;
    }
  }
}