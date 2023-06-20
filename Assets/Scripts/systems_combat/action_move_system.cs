using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  public struct MoveInformation
  {
    public Vector2Int[] path;
    public Entity e;
  }

  public class MoveSystem : ECSSystem
  {
    public UnityEvent<MoveInformation> something_moved;
    private map_manager map;
    private SelectSystem select_system;

    // animations
    private MonoBehaviour main;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<WantsToMove>());
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      ecs.SetSystemSignature<MoveSystem>(s);
    }

    public void Start(Wiggy.registry ecs, main main)
    {
      this.main = main;
      this.map = Object.FindObjectOfType<map_manager>();
      this.something_moved = new();
      this.select_system = main.select_system;
    }

    private bool ValidateRequest(Wiggy.registry ecs, Entity e, WantsToMove request)
    {
      // Does the request contain a path?
      if (request.path == null || request.path.Length <= 1)
      {
        Debug.Log("no path...");
        return false;
      }

      var position = ecs.GetComponent<GridPositionComponent>(e);
      int from = Grid.GetIndex(position.position, map.width);
      var final = request.path[^1];
      var to = Grid.GetIndex(final, map.width);
      var to_ents = map.entity_map[to].entities;
      var from_ents = map.entity_map[from].entities;

      // Does the "to" spot contain a humanoid?
      {
        bool to_contains_humanoid = false;
        foreach (var to_ent in to_ents)
        {
          HumanoidComponent humanoid_default = default;
          ecs.TryGetComponent(to_ent, ref humanoid_default, out var is_humanoid);
          if (is_humanoid)
            to_contains_humanoid = true;
        }
        if (to_contains_humanoid)
        {
          Debug.Log($"EID: {e.id} tried to move to spot containing a humanoid");
          return false;
        }
      }

      // Does the "from" spot contain the correct entity?
      {
        var floor_index = select_system.GetFloorIndex(ecs, e);
        if (!floor_index.IsSet)
          return false;
      }
      return true;
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var action = new Move();

        if (!ActionHelpers.Valid<WantsToMove>(ecs, e, action))
        {
          Debug.Log("WantsToMove invalid action");
          ecs.RemoveComponent<WantsToMove>(e);
          continue;
        }

        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        var request = ecs.GetComponent<WantsToMove>(e);

        if (!ValidateRequest(ecs, e, request))
        {
          Debug.Log("WantsToMove invalid request - not using action");
          ecs.RemoveComponent<WantsToMove>(e);
          continue;
        }

        // If this unit had the overwatch status, remove it
        OverwatchStatus overwatch_default = default;
        ref var overwatch = ref ecs.TryGetComponent(e, ref overwatch_default, out var has_status);
        if (has_status)
        {
          Debug.Log("Unit Moving; removing overwatch status");
          ecs.RemoveComponent<OverwatchStatus>(e);
        }

        Debug.Log("Moving..");
        MoveActionLogic(ecs, e, request.path.ToArray());
        Debug.Log("Move action done.");

        // Request is processed
        ActionHelpers.Complete<WantsToMove>(ecs, e, action);
      }
    }

    private void MoveActionLogic(Wiggy.registry ecs, Entity e, Vector2Int[] path)
    {
      var position = ecs.GetComponent<GridPositionComponent>(e);
      int from = Grid.GetIndex(position.position, map.width);
      var final = path[^1];
      var to = Grid.GetIndex(final, map.width);
      var floor_index = select_system.GetFloorIndex(ecs, e);

      // Update representation
      map.entity_map[from].entities.RemoveAt(floor_index.Data);
      map.entity_map[to].entities.Add(e);

      // Update component data
      ref var grid_pos = ref ecs.GetComponent<GridPositionComponent>(e);
      grid_pos.position = Grid.IndexToPos(to, map.width, map.height);

      // Send event
      MoveInformation move_info = new();
      move_info.path = path;
      move_info.e = e;
      something_moved.Invoke(move_info);

      // Start animation
      // if (animation_coroutine != null)
      // {
      //   Debug.Log("Stopping existing coroutine");
      //   main.StopCoroutine(animation_coroutine);
      //   if (animation_go != null) // Finish moving animation
      //     animation_go.transform.localPosition = Grid.GridSpaceToWorldSpace(animation_final, map.size);
      // }

      Debug.Log("Starting coroutine");
      var instance = ecs.GetComponent<InstantiatedComponent>(e);
      // animation_go = instance.instance;
      // animation_final = path[^1];
      var animation_coroutine = Animate.AlongPath(instance.instance, path, map.size);
      main.StartCoroutine(animation_coroutine);
    }
  }
}