using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wiggy
{
  using Entity = System.Int32;

  [System.Serializable]
  class main : MonoBehaviour
  {
    public GameObject player_prefab;
    public GameObject enemy_prefab;

    Wiggy.registry ecs;
    input_handler input;
    camera_handler camera;
    map_manager map;
    unit_select unit_select;
    unit_act act;
    scene_manager scene;

    // x * y representation
    Optional<Entity>[] units;

    List<MoveEvent> move_event_queue = new();
    List<AttackEvent> attack_event_queue = new();

    ExtractionSystem extraction_system;
    InstantiateSystem instantiate_system;

    public GameObject extraction_ui;
    public Button extraction_button;

    [SerializeField]
    private IEnumerator animation_coroutine;
    private GameObject animation_go;
    private Vector2Int animation_final;


    void Start()
    {
      // Register all components
      ecs = new();
      ecs.RegisterComponent<PlayerComponent>();
      ecs.RegisterComponent<GridPositionComponent>();
      ecs.RegisterComponent<ToBeInstantiatedComponent>();
      ecs.RegisterComponent<InstantiatedComponent>();
      instantiate_system = ecs.RegisterSystem<InstantiateSystem>();
      extraction_system = ecs.RegisterSystem<ExtractionSystem>();

      map = FindObjectOfType<map_manager>();
      input = FindObjectOfType<input_handler>();
      camera = FindObjectOfType<camera_handler>();
      unit_select = FindObjectOfType<unit_select>();
      scene = FindObjectOfType<scene_manager>();
      act = new GameObject("unit_act").AddComponent<unit_act>();

      units = new Optional<Entity>[map.width * map.height];
      for (int i = 0; i < map.width * map.height; i++)
        units[i] = new Optional<Entity>();

      instantiate_system.Start(ecs, map);
      extraction_system.Start(ecs);
      act.DoStart();
      camera.DoStart();
      unit_select.DoStart();

      // TODO: map_gen_items_and_enemies
      var voronoi_map = map.voronoi_map;
      var voronoi_zones = map.voronoi_zones;

      void create_player(Vector2Int spot, string name)
      {
        var e = ecs.Create();

        ecs.AddComponent<PlayerComponent>(e, default);

        GridPositionComponent gpc = new();
        gpc.position = spot;
        ecs.AddComponent(e, gpc);

        ToBeInstantiatedComponent tbic = new();
        tbic.prefab = player_prefab;
        tbic.name = name;
        ecs.AddComponent(e, tbic);

        var index = Grid.GetIndex(spot, map.width);
        units[index] = new Optional<Entity>(e);
      }
      // set players at start spots
      create_player(map.srt_spots[0], "Wiggy");
      create_player(map.srt_spots[1], "Wallace");
      create_player(map.srt_spots[2], "Sherbert");
      create_player(map.srt_spots[3], "Grunbo");

      act.attack_event.AddListener((e) => attack_event_queue.Add(e));
      act.move_event.AddListener((e) => move_event_queue.Add(e));
      extraction_button.onClick.AddListener(() => scene.Load());
    }

    void Update()
    {
      // Camera
      camera.HandleCursorOnGrid();
      camera.HandleCameraZoom();

      // Input
      if (input.a_input)
        act.Act(units, unit_select, camera, map);
      if (input.b_input)
        unit_select.ClearSelection();

      // Logic
      ProcessMoveQueue();
      ProcessAttackQueue();
      extraction_system.Update(ecs, map.ext_spots);
      instantiate_system.Update(ecs);

      // UI
      unit_select.UpdateSelectedCursorUI(map.size);
      extraction_ui.SetActive(extraction_system.ready_for_extraction);
    }

    void LateUpdate()
    {
      float delta = Time.deltaTime;
      input.DoLateUpdate();
      camera.HandleCameraMovement(delta, input.l_analogue);
      camera.HandleCameraLookAt();
    }

    private void ProcessMoveQueue()
    {
      if (move_event_queue.Count == 0)
        return;
      var e = move_event_queue[0];
      move_event_queue.RemoveAt(0);

      // generate would-be path...
      var cells = map_manager.GameToAStar(map.obstacle_map, map.width, map.height);
      var path = a_star.generate_direct(cells, e.from_index, e.to_index, map.width);
      if (path == null)
      {
        Debug.Log("no path...");
        return;
      }



      // Immediately update representation
      if (!units[e.from_index].IsSet || units[e.to_index].IsSet)
      {
        Debug.Log("no unit to move, or blocked destsination");
        return;
      }
      Entity go = units[e.from_index].Data;
      units[e.to_index].Set(go);
      units[e.from_index].Reset();

      // Update gridposition component data
      ref var grid_pos = ref ecs.GetComponent<GridPositionComponent>(go);
      grid_pos.position = Grid.IndexToPos(e.to_index, map.width, map.height);

      if (animation_coroutine != null)
      {
        Debug.Log("Stopping coroutine");
        StopAllCoroutines();

        // Finish moving animation
        animation_go.transform.localPosition =
          Grid.GridSpaceToWorldSpace(animation_final, map.size);
      }

      // convert astar_cells to Vector2Int[]
      var path_vec2s = new Vector2Int[path.Length];
      for (int i = 0; i < path.Length; i++)
        path_vec2s[i] = path[i].pos;

      Debug.Log("Starting coroutine");
      var instance = ecs.GetComponent<InstantiatedComponent>(go);
      animation_go = instance.instance;
      animation_final = path[^1].pos;
      animation_coroutine = Animate.AlongPath(animation_go, path_vec2s, map.size);
      StartCoroutine(animation_coroutine);
    }

    private void ProcessAttackQueue()
    {
      if (attack_event_queue.Count == 0)
        return;
      Debug.Log("pulling attack event from queue!");
      var e = attack_event_queue[0];
      attack_event_queue.RemoveAt(0);

      var from = e.from;
      var to = e.target;

      Debug.Log("TODO: a attacking b...");
    }
  }
}