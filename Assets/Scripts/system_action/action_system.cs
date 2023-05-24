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

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      ecs.SetSystemSignature<ActionSystem>(s);
    }

    public void Start(Wiggy.registry ecs, main main)
    {
      map = Object.FindObjectOfType<map_manager>();
      this.select_system = main.select_system;
      this.unit_spawn_system = main.unit_spawn_system;
      this.camera = main.camera;
    }

    public void RequestMapInteraction(Wiggy.registry ecs, Entity from_entity)
    {
      var pos = ecs.GetComponent<GridPositionComponent>(from_entity).position;
      var from = Grid.GetIndex(pos, map.width);
      var to = Grid.GetIndex(camera.grid_index, map.width);

      if (from == to)
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

      //
      // Actions that can resolve by clicking the map
      //
      var units = unit_spawn_system.units;
      Optional<Entity> to_entity = units[to];
      bool to_contains_unit = to_entity.IsSet;
      bool to_contains_obstacle = map.obstacle_map[to].entities.Contains(EntityType.tile_type_wall);

      if (to_contains_unit)
      {
        Debug.Log("user wants to attack unit");
        ref var targets = ref ecs.GetComponent<TargetsComponent>(from_entity);
        targets.targets.Clear();
        targets.targets.Add(to_entity.Data);
      }

      if (action_selected.GetType() == typeof(Move) && to_contains_unit)
      {
        Debug.Log("cant move to another unit position");
        return; // cant move to another unit
      }

      if (action_selected.GetType() == typeof(Attack) && to_contains_obstacle)
      {
        Debug.Log("cant attack obstacles");
        return; // cant attack obstacles?
      }

      var a = action_selected;
      var e = from_entity;

      // Not-immediate (map input)
      if (a.GetType() == typeof(Move))
        RequestMoveAction(ecs, e, to);
      else if (a.GetType() == typeof(Attack))
        RequestAttackAction(ecs, e);
      else if (a.GetType() == typeof(Grenade))
        RequestGrenadeAction(ecs, e, to);

      ClearInteraction();
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

    public void RequestMoveAction(Wiggy.registry ecs, Entity e, int to)
    {
      Debug.Log($"EID: {e.id} wants to move to: {to}");
      ecs.AddComponent<WantsToMove>(e, new() { to = to });
    }

    public void RequestAttackAction(Wiggy.registry ecs, Entity e)
    {
      ecs.AddComponent<WantsToAttack>(e, new() { });
    }

    public void RequestGrenadeAction(Wiggy.registry ecs, Entity e, int to)
    {
      ecs.AddComponent<WantsToGrenade>(e, new() { index = to });
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

    public void Update(Wiggy.registry ecs)
    {

    }
  }
}