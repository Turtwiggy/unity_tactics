using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  public struct MoveInformation
  {
    public astar_cell[] path;
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
          continue;

        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        var request = ecs.GetComponent<WantsToMove>(e);
        var position = ecs.GetComponent<GridPositionComponent>(e);

        // Process request
        int from = Grid.GetIndex(position.position, map.width);
        int to = request.to;

        // Check this unit does not have the overwatch status
        // (otherwise, you'd be immobalized)
        OverwatchStatus status = default;
        var has_status = ecs.TryGetComponent(e, ref status);
        if (has_status)
        {
          Debug.Log("you cant move, you have overwatch status!");
          continue;
        }

        Debug.Log("Moving..");
        MoveActionLogic(ecs, from, to);
        Debug.Log("Move action done.");

        // Request is processed
        ActionHelpers.Complete<WantsToMove>(ecs, e, action);
      }
    }

    private void MoveActionLogic(Wiggy.registry ecs, int from, int to)
    {
      // Generate path
      var cells = map_manager.GameToAStar(map.obstacle_map, map.width, map.height);
      var path = a_star.generate_direct(cells, from, to, map.width);

      if (path == null)
      {
        Debug.Log("no path...");
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
  }
}