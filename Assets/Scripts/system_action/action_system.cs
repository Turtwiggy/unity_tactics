using UnityEngine;

namespace Wiggy
{
  public class ActionSystem : ECSSystem
  {
    private map_manager map;
    private UnitSpawnSystem unit_spawn_system;
    private SelectSystem select_system;

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
    }

    public void RequestActionFromMap(Wiggy.registry ecs, Entity from_entity, int to)
    {
      var pos = ecs.GetComponent<GridPositionComponent>(from_entity).position;
      var from = Grid.GetIndex(pos, map.width);

      if (from == to)
      {
        Debug.Log("from == to; no action to take");
        return;
      }

      var units = unit_spawn_system.units;
      Optional<Entity> to_entity = units[to];
      bool to_contains_unit = to_entity.IsSet;
      bool to_contains_obstacle = map.obstacle_map[to].entities.Contains(EntityType.tile_type_wall);

      if (to_contains_unit)
      {
        ref var targets = ref ecs.GetComponent<TargetsComponent>(from_entity);
        targets.targets.Clear();
        targets.targets.Add(to_entity.Data);

        RequestAction(ecs, new Attack(), from_entity);
      }

      else if (!to_contains_obstacle)
        RequestAction(ecs, new Move(), from_entity, to);
    }

    public void RequestActionFromUI<T>(Wiggy.registry ecs) where T : Action, new()
    {
      if (!select_system.HasAnySelected())
      {
        Debug.Log("no unit selected!");
        return;
      }

      Entity selected = select_system.GetSelected();

      T temp = new();

      if (temp.GetType() == typeof(Attack)) // force use map
        return;

      if (temp.GetType() == typeof(Move)) // force use map
        return;

      RequestAction(ecs, temp, selected);
    }

    public static void RequestAction(Wiggy.registry ecs, Action a, Entity e, int to = default)
    {
      Debug.Log("Action requested: " + a.GetType());

      if (a.GetType() == typeof(Attack))
        ecs.AddComponent<WantsToAttack>(e, new());

      else if (a.GetType() == typeof(Move))
        ecs.AddComponent<WantsToMove>(e, new() { to = to });

      else if (a.GetType() == typeof(Reload))
        ecs.AddComponent<WantsToReload>(e, new());

      else if (a.GetType() == typeof(Overwatch))
        ecs.AddComponent<WantsToOverwatch>(e, new());

      else if (a.GetType() == typeof(Heal))
        ecs.AddComponent<WantsToHeal>(e, new());

      else
        Debug.LogError("Action requested that is not implemented");
    }

    public void Update(Wiggy.registry ecs)
    {

    }
  }
}