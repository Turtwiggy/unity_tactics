using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  [System.Serializable]
  class main : MonoBehaviour
  {
    public GameObject player_prefab;
    public GameObject enemy_prefab;

    // x * y representation
    public GameObject[] units;

    input_handler input;
    camera_handler camera;
    map_manager map;
    unit_select unit_select;
    unit_act act;

    bool something_is_acting = false;

    void Start()
    {
      input = FindObjectOfType<input_handler>();
      camera = FindObjectOfType<camera_handler>();
      map = FindObjectOfType<map_manager>();
      unit_select = FindObjectOfType<unit_select>();
      act = new GameObject("unit_act").AddComponent<unit_act>();

      act.DoStart();
      camera.DoStart();
      unit_select.DoStart();

      // TODO: map_gen_items_and_enemies
      var voronoi_map = map.voronoi_map;
      var voronoi_zones = map.voronoi_zones;

      // set players at start spots
      units = new GameObject[map.width * map.height];
      unit_manager.create_unit(units, player_prefab, map.srt_spots[0], map, "Wiggy");
      unit_manager.create_unit(units, player_prefab, map.srt_spots[1], map, "Wallace");
      unit_manager.create_unit(units, player_prefab, map.srt_spots[2], map, "Sherbert");
      unit_manager.create_unit(units, player_prefab, map.srt_spots[3], map, "Grunbo");

      // TODO: gameover if all players on exit spots
      var ext_spots = map.ext_spots;

      act.attack_event.AddListener((e) => AttackEvent(e));
      act.move_event.AddListener((e) => MoveEvent(e));
      // act.move_event.AddListener((e) => objective_manager.UnitMovedEvent(e));
    }

    private void AttackEvent(AttackEvent e)
    {
      Debug.Log("Attack something?");
    }

    private async void MoveEvent(MoveEvent e)
    {
      Debug.Log("Move to somewhere?");

      // Slowly show animation
      var cells = map_manager.GameToAStar(map.obstacle_map, map.width, map.height);
      var path = a_star.generate_direct(cells, e.from, e.to, map.width);
      if (path == null)
      {
        Debug.Log("no path...");
        something_is_acting = false;
        return;
      }
      // DisplayPathUI(path, size);

      // Immediately update representation
      var go = units[e.from];
      units[e.from] = null;
      units[e.to] = go;

      var path_vec2s = new Vector2Int[path.Length];
      for (int i = 0; i < path.Length; i++)
      {
        astar_cell p = path[i];
        path_vec2s[i] = p.pos;
      }

      await Animate.AlongPath(go, path_vec2s, map.size);
      something_is_acting = false;
    }

    void Update()
    {
      camera.HandleCursorOnGrid();
      camera.HandleCameraZoom();
      unit_select.UpdateUI(map.size);

      if (input.a_input && !something_is_acting)
      {
        something_is_acting = true;
        act.Act(units, unit_select, camera, map);
      }

      if (input.b_input)
        unit_select.ClearSelection();
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