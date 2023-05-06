using UnityEngine;

namespace Wiggy
{
  public static class ActionHelpers
  {
    public static bool Valid<R>(Wiggy.registry ecs, Entity e, Action a)
      where R : Request
    {
      ref var actions = ref ecs.GetComponent<ActionsComponent>(e);

      // no duplicate types
      for (int i = 0; i < actions.done.Count; i++)
      {
        var action = actions.done[i];
        if (action.GetType() == a.GetType())
          return false;
      }

      return actions.done.Count < actions.allowed_actions_per_turn;
    }

    public static void Complete<R>(Wiggy.registry ecs, Entity e, Action a)
      where R : Request, new()
    {
      if (Valid<R>(ecs, e, a))
      {
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        actions.done.Add(a);
        Debug.Log($"Request valid: {a.GetType()}");
      }
      else
        Debug.Log($"Request invalid, removed request: {a.GetType()}");

      ecs.RemoveComponent<R>(e);
    }
  }
}