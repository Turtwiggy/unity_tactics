using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

    // x * y representation
    Optional<Entity>[] units;

    List<MoveEvent> move_event_queue = new();
    List<AttackEvent> attack_event_queue = new();

    ExtractionSystem extraction_system;
    InstantiateSystem instantiate_system;

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

      // gameover if all players on exit spots
      var ext_spots = map.ext_spots;

      act.attack_event.AddListener((e) => attack_event_queue.Add(e));
      act.move_event.AddListener((e) => move_event_queue.Add(e));
    }

    async void Update()
    {
      camera.HandleCursorOnGrid();
      camera.HandleCameraZoom();
      unit_select.UpdateSelectedCursorUI(map.size);

      if (input.a_input)
        act.Act(units, unit_select, camera, map);

      if (input.b_input)
        unit_select.ClearSelection();

      await ProcessMoveQueue();
      await ProcessAttackQueue();

      extraction_system.Update(ecs, map.ext_spots);
      instantiate_system.Update(ecs);
    }

    void LateUpdate()
    {
      float delta = Time.deltaTime;
      input.DoLateUpdate();
      camera.HandleCameraMovement(delta, input.l_analogue);
      camera.HandleCameraLookAt();
    }

    private async Task ProcessMoveQueue()
    {
      if (move_event_queue.Count == 0)
        return;
      Debug.Log("pulling move event from queue!");
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
      var grid_pos = ecs.GetComponent<GridPositionComponent>(go);
      grid_pos.position = Grid.IndexToPos(e.to_index, map.width, map.height);

      var grid_pos2 = ecs.GetComponent<GridPositionComponent>(go);
      Debug.Log(grid_pos2.position);

      // convert astar_cells to Vector2Int[]
      var path_vec2s = new Vector2Int[path.Length];
      for (int i = 0; i < path.Length; i++)
        path_vec2s[i] = path[i].pos;

      // DisplayPathUI(path, size);
      var instance = ecs.GetComponent<InstantiatedComponent>(go);
      await Animate.AlongPath(instance.instance, path_vec2s, map.size);
      // DeletePathUI(path);

      Debug.Log("done with event");
    }

    private async Task ProcessAttackQueue()
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