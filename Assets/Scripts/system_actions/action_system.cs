using UnityEngine;

namespace Wiggy
{
  using Entity = System.Int32;

  public class ActionSystem : ECSSystem
  {
    private map_manager map;
    private UnitSpawnSystem unit_spawn_system;

    public void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      ecs.SetSystemSignature<ActionSystem>(s);
    }

    public void Start(Wiggy.registry ecs, UnitSpawnSystem uss)
    {
      map = Object.FindObjectOfType<map_manager>();
      unit_spawn_system = uss;
    }

    public void RequestActionFromMap(Wiggy.registry ecs, int from, int to)
    {
      if (from == to)
      {
        Debug.Log("from == to; no action to take");
        return;
      }

      var units = unit_spawn_system.units;
      Optional<Entity> from_entity = units[from];
      Optional<Entity> to_entity = units[to];
      bool to_contains_unit = to_entity.IsSet;
      bool to_contains_obstacle = map.obstacle_map[to].entities.Contains(EntityType.tile_type_wall);

      if (from_entity.IsSet && to_contains_unit)
      {
        ref var actions = ref ecs.GetComponent<ActionsComponent>(from_entity.Data);
        actions.requested[0] = new Attack();
      }

      if (from_entity.IsSet && !to_contains_obstacle)
      {
        ref var actions = ref ecs.GetComponent<ActionsComponent>(from_entity.Data);
        actions.requested[0] = new Move();
      }
    }

    public void RequestActionFromUI(Wiggy.registry ecs, int from, Action a)
    {
      var units = unit_spawn_system.units;
      Optional<Entity> from_entity = units[from];

      if (!from_entity.IsSet)
      {
        Debug.Log("no unit at requested action index");
        return;
      }

      ref var actions = ref ecs.GetComponent<ActionsComponent>(from_entity.Data);

      if (a.GetType() == typeof(Overwatch))
        actions.requested[0] = new Overwatch();

      if (a.GetType() == typeof(Reload))
        actions.requested[0] = new Reload();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities)
      {
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        ref var requested = ref actions.requested;

        for (int i = 0; i < requested.Length; i++)
        {
          var action = actions.requested[i];

          if (action != null)
          {
            Debug.Log("unit wants to take an action!");
          }
        }
      }
    }
  }
}