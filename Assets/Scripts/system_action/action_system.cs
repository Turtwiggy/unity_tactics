using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class ActionSystem : ECSSystem
  {
    private map_manager map;
    private UnitSpawnSystem unit_spawn_system;
    private SelectSystem select_system;
    private camera_handler camera;

    public bool action_selected_from_ui;
    public Action action_selected;

    // Move Action specific visuals
    private GameObject[] instantiated_path_visuals;
    private Vector2Int[] path_from_ui;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      ecs.SetSystemSignature<ActionSystem>(s);
    }

    public void Start(Wiggy.registry ecs, main main, GameObject move_prefab)
    {
      map = GameObject.FindObjectOfType<map_manager>();
      this.select_system = main.select_system;
      this.unit_spawn_system = main.unit_spawn_system;
      this.camera = main.camerah;

      GameObject cursor_parent = new GameObject("Cursor Parent");
      instantiated_path_visuals = new GameObject[map.width + map.height];
      for (int i = 0; i < map.width + map.height; i++)
      {
        var go = GameObject.Instantiate(move_prefab, Vector3.zero, move_prefab.transform.rotation, cursor_parent.transform);
        instantiated_path_visuals[i] = go;
        instantiated_path_visuals[i].SetActive(false);
      }
    }

    public void RequestMapInteraction(Wiggy.registry ecs, Entity from_entity)
    {
      var pos = ecs.GetComponent<GridPositionComponent>(from_entity).position;
      var from = Grid.GetIndex(pos, map.width);
      var full_to = Grid.GetIndex(camera.grid_index, map.width);

      if (from == full_to)
      {
        Debug.Log("from == to; no action to take");
        return;
      }

      // Have you got an action selected from the UI?
      if (!action_selected_from_ui)
      {
        Debug.Log("Select an action from the ui!");
        return;
      }

      // Check we're only moving players around
      var ents = map.entity_map[from].entities;
      var player = map_manager.GetFirst<PlayerComponent>(ecs, ents);
      if (!player.IsSet)
        return;

      //
      // Actions that can resolve by clicking the map
      //

      // Does destination contain an obstacle?
      // WARNING: obstacle_map is the *starting* representation
      bool to_contains_obstacle = map.obstacle_map[full_to].entities.Contains(EntityType.tile_type_wall);

      // Does destination contain something?
      // bool to_contains_something = map.entity_map[full_to].entities.Count > 0;

      var a = action_selected;
      var e = from_entity;

      // Not-immediate (map input)
      if (a.GetType() == typeof(Move))
      {
        if (path_from_ui == null)
          return;

        // Move is validated by the MoveActionVisuals
        ecs.AddComponent<WantsToMove>(e, new() { path = path_from_ui });

        path_from_ui = null;
      }
      else if (a.GetType() == typeof(Attack))
      {
        if (to_contains_obstacle)
          return;

        // Target is validated by the monitor_combat_system
        ecs.AddComponent<WantsToAttack>(e, new() { map_idx = full_to });
      }
      else if (a.GetType() == typeof(Grenade))
      {
        // Validate grenade throw position
        DexterityComponent dex_backup = default;
        ref var dex = ref ecs.TryGetComponent(e, ref dex_backup, out var has_dex);
        if (!has_dex)
          return;

        var grenade_request_pos = Grid.IndexToPos(full_to, map.width, map.height);
        var grenade_dst_from_player = Vector2Int.Distance(pos, grenade_request_pos);
        if (grenade_dst_from_player > dex.amount)
          return;

        Debug.Log("haha you can grenade anywhere on the map this is a bug");
        ecs.AddComponent<WantsToGrenade>(e, new() { index = full_to });
      }

      ClearInteraction();
    }

    public void AIRequestAttackAction(Wiggy.registry ecs, Entity e, int map_idx)
    {
      ecs.AddComponent<WantsToAttack>(e, new() { map_idx = map_idx });
    }

    public void AIRequestMoveAction(Wiggy.registry ecs, astar_cell[] astar, Entity e, int to)
    {
      var pos = ecs.GetComponent<GridPositionComponent>(e).position;
      var from = Grid.GetIndex(pos, map.width);

      // work out a path
      var path = a_star.generate_direct(astar, from, to, map.width);
      if (path == null)
        return; // no path
      var converted_path = a_star.convert_to_points(path).ToArray();

      ecs.AddComponent<WantsToMove>(e, new() { path = converted_path });
    }

    public void RequestActionFromUI<T>(Wiggy.registry ecs) where T : Action, new()
    {
      if (!select_system.HasAnySelected())
        return;

      if (action_selected_from_ui)
      {
        Debug.Log("ui action is already selected...");
        return;
      }

      // Set the action as queued
      QueueInteraction(new T());

      //
      // Actions that can resolve immediately
      //

      var a = action_selected;
      var e = select_system.GetSelected();
      Debug.Log("ui action requested: " + a.GetType());

      if (RequestActionIfImmediate(ecs, a, e))
        ClearInteraction(); // no further user input
    }

    public void QueueInteraction(Action a)
    {
      action_selected = a;
      action_selected_from_ui = true;
    }

    public void ClearInteraction()
    {
      action_selected = null;
      action_selected_from_ui = false;
    }

    public bool RequestActionIfImmediate(Wiggy.registry ecs, Action a, Entity e)
    {
      if (a.GetType() == typeof(Reload))
      {
        ecs.AddComponent<WantsToReload>(e, new());
        return true;
      }
      else if (a.GetType() == typeof(Overwatch))
      {
        ecs.AddComponent<WantsToOverwatch>(e, new());
        return true;
      }
      else if (a.GetType() == typeof(Heal))
      {
        ecs.AddComponent<WantsToHeal>(e, new());
        return true;
      }
      return false;
    }

    public void Update(Wiggy.registry ecs, astar_cell[] astar)
    {
      if (action_selected != null && action_selected.GetType() == typeof(Move) && select_system.HasAnySelected())
        MoveActionVisuals(ecs, astar);
    }

    private void MoveActionVisuals(Wiggy.registry ecs, astar_cell[] astar)
    {
      // Turn them all off
      for (int i = 0; i < instantiated_path_visuals.Length; i++)
        instantiated_path_visuals[i].SetActive(false);

      var e = select_system.GetSelected();
      var from_pos = ecs.GetComponent<GridPositionComponent>(e).position;
      var to_pos = camera.grid_index;

      DexterityComponent dex_backup = default;
      ref var dex = ref ecs.TryGetComponent(e, ref dex_backup, out var has_dex);
      if (!has_dex)
        return;

      var from_idx = Grid.GetIndex(from_pos, map.width);
      var to_idx = Grid.GetIndex(to_pos, map.width);

      // work out a path
      {
        var path = a_star.generate_direct(astar, from_idx, to_idx, map.width);
        if (path == null)
          return; // no path
        path = path.Take(dex.amount).ToArray(); // limit by dex
        path_from_ui = a_star.convert_to_points(path).ToArray();
      }

      // Display a cursor at each of the points
      for (int i = 0; i < path_from_ui.Length; i++)
      {
        if (i > dex.amount)
          break; // cant move anymore!
        instantiated_path_visuals[i].SetActive(true);
        var p = path_from_ui[i];
        var pos = Grid.GridSpaceToWorldSpace(p, 1); // 1 seems wrong to hard code here
        instantiated_path_visuals[i].transform.position = pos;
      }
    }
  }
}