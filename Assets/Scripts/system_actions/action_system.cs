using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  public class ActionSystem : ECSSystem
  {
    private map_manager map;
    private UnitSpawnSystem unit_spawn_system;
    private SelectSystem select_system;

    // animations
    private MonoBehaviour main;
    private GameObject animation_go;
    private IEnumerator animation_coroutine;
    private Vector2Int animation_final;

    public UnityEvent action_complete;

    public void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      ecs.SetSystemSignature<ActionSystem>(s);
    }

    public void Start(Wiggy.registry ecs, main main)
    {
      this.main = main;
      this.select_system = main.select_system;
      this.unit_spawn_system = main.unit_spawn_system;
      this.map = Object.FindObjectOfType<map_manager>();

      action_complete = new();
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
        ref var actions = ref ecs.GetComponent<ActionsComponent>(from_entity);
        Attack a = new();
        a.from = from;
        a.to = to;
        actions.requested.Add(a);
      }
      else if (!to_contains_obstacle)
      {
        ref var actions = ref ecs.GetComponent<ActionsComponent>(from_entity);
        Move m = new();
        m.from = from;
        m.to = to;
        actions.requested.Add(m);
      }
    }

    public void RequestActionFromUI<T>(Wiggy.registry ecs) where T : Action, new()
    {
      if (!select_system.HasAnySelected())
      {
        Debug.Log("no unit selected!");
        return;
      }
      Entity selected = select_system.GetSelected();
      ref var actions = ref ecs.GetComponent<ActionsComponent>(selected);

      // warning: entity might not have these
      // really should implement a try_get or something
      ref var ammo = ref ecs.GetComponent<AmmoComponent>(selected);
      ref var health = ref ecs.GetComponent<HealthComponent>(selected);

      T temp = new();
      Debug.Log("Action requested: " + temp.GetType());

      // if (temp.GetType() == typeof(Move))
      //   actions.requested.Add(new Move());

      // if (temp.GetType() == typeof(Attack))
      //   actions.requested.Add(new Attack());

      if (temp.GetType() == typeof(Reload))
      {
        actions.requested.Add(new Reload(ammo));
      }
      if (temp.GetType() == typeof(Overwatch))
      {
        actions.requested.Add(new Overwatch());
      }
      if (temp.GetType() == typeof(Heal))
      {
        actions.requested.Add(new Heal(health));
      }
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities)
      {
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        ref var requested = ref actions.requested;
        ref var done = ref actions.done;

        while (requested.Count > 0)
        {
          var action = requested[0];
          requested.RemoveAt(0);

          //
          // Prevent actions that are already done
          //
          for (int i = 0; i < done.Count; i++)
          {
            if (done[i].GetType() == action.GetType())
            {
              Debug.Log("Action already complete");
              return;
            }
          }

          //
          // Move Action
          //
          if (action.GetType() == typeof(Move))
          {
            Debug.Log("moving..!");
            Move a = (Move)action;

            // Generate path
            var cells = map_manager.GameToAStar(map.obstacle_map, map.width, map.height);
            var path = a_star.generate_direct(cells, a.from, a.to, map.width);

            if (path == null)
              Debug.Log("no path...");
            else
            {
              // Update representation
              var go = unit_spawn_system.units[a.from].Data;
              unit_spawn_system.units[a.to].Set(go);
              unit_spawn_system.units[a.from].Reset();

              // Update component data
              ref var grid_pos = ref ecs.GetComponent<GridPositionComponent>(go);
              grid_pos.position = Grid.IndexToPos(a.to, map.width, map.height);

              // Start animation
              if (animation_coroutine != null)
              {
                Debug.Log("Stopping coroutine");
                main.StopCoroutine(animation_coroutine);

                // Finish moving animation
                animation_go.transform.localPosition = Grid.GridSpaceToWorldSpace(animation_final, map.size);
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
              main.StartCoroutine(animation_coroutine);
            }

            Debug.Log("Move action done");
          }
          //
          // Attack Action
          //
          else if (action.GetType() == typeof(Attack))
          {
            Attack a = (Attack)action;
            Debug.Log(string.Format("Request to attack from:{0} to:{1}", a.from, a.to));
          }
          //
          // Overwatch action
          //
          else if (action.GetType() == typeof(Overwatch))
            Debug.Log("TODO: overwatch request");
          //
          // Reload action
          //
          else if (action.GetType() == typeof(Reload))
            Debug.Log("TODO: reload request");

          done.Add(action);
          action_complete.Invoke();
        }
      }
    }
  }
}