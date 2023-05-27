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
    private UnitSpawnSystem unit_spawn_system;

    // animations
    private MonoBehaviour main;
    private GameObject animation_go;
    private IEnumerator animation_coroutine;
    private Vector2Int animation_final;

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
      this.unit_spawn_system = main.unit_spawn_system;
      this.something_moved = new();
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
        var position = ecs.GetComponent<GridPositionComponent>(e);

        // Process request
        int from = Grid.GetIndex(position.position, map.width);

        // Check this unit does not have the overwatch status
        // (otherwise, you'd be immobalized)

        OverwatchStatus overwatch_default = default;
        ref var overwatch = ref ecs.TryGetComponent(e, ref overwatch_default, out var has_status);
        if (has_status)
        {
          Debug.Log("you cant move, you have overwatch status!");
          ecs.RemoveComponent<WantsToMove>(e);
          continue;
        }

        Debug.Log("Moving..");
        MoveActionLogic(ecs, from, request.path.ToArray());
        Debug.Log("Move action done.");

        // Request is processed
        ActionHelpers.Complete<WantsToMove>(ecs, e, action);
      }
    }

    private void MoveActionLogic(Wiggy.registry ecs, int from, Vector2Int[] path)
    {
      if (path == null || path.Length <= 1)
      {
        Debug.Log("no path...");
        return;
      }
      var final = path[^1];
      var to = Grid.GetIndex(final, map.width);

      if (unit_spawn_system.units[to].IsSet)
      {
        Debug.Log($"EID: {unit_spawn_system.units[from].Data.id} tried to move to already full spot");
        return;
      }

      if (!unit_spawn_system.units[from].IsSet)
      {
        Debug.Log("Entity tried to move 'from' a spot that doesnt contain an entity");
        return;
      }

      // Update representation
      var go = unit_spawn_system.units[from].Data;
      unit_spawn_system.units[to].Set(go);
      unit_spawn_system.units[from].Reset();

      // Update component data
      ref var grid_pos = ref ecs.GetComponent<GridPositionComponent>(go);
      grid_pos.position = Grid.IndexToPos(to, map.width, map.height);

      // Send event
      MoveInformation move_info = new();
      move_info.path = path;
      move_info.e = go;
      something_moved.Invoke(move_info);

      // Start animation
      if (animation_coroutine != null)
      {
        Debug.Log("Stopping existing coroutine");
        main.StopCoroutine(animation_coroutine);

        // Finish moving animation
        animation_go.transform.localPosition = Grid.GridSpaceToWorldSpace(animation_final, map.size);
      }

      Debug.Log("Starting coroutine");
      var instance = ecs.GetComponent<InstantiatedComponent>(go);
      animation_go = instance.instance;
      animation_final = path[^1];
      animation_coroutine = Animate.AlongPath(animation_go, path, map.size);
      main.StartCoroutine(animation_coroutine);
    }
  }
}