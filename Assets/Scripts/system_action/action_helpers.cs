using UnityEngine;

namespace Wiggy
{
  public static class ActionHelpers
  {
    public static bool Valid<R>(Wiggy.registry ecs, Entity e, Action a)
      where R : Request, new()
    {
      ref var actions = ref ecs.GetComponent<ActionsComponent>(e);

      // no duplicate types
      for (int i = 0; i < actions.done.Count; i++)
      {
        var action = actions.done[i];
        if (action.GetType() == a.GetType())
          return false;
      }

      var valid = actions.done.Count < actions.allowed_actions_per_turn;

      // Remove request if invalid
      if (!valid)
        ecs.RemoveComponent<R>(e);

      return valid;
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
    }
  }
}