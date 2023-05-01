using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class OverwatchSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<WantsToOverwatch>());
      ecs.SetSystemSignature<OverwatchSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {

    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var action = new Overwatch();

        if (!ActionHelpers.Valid<WantsToOverwatch>(ecs, e, action))
          continue;

        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);

        // How do, overwatch?
        OverwatchStatus status = new();
        ecs.AddComponent(e, status);

        // Request is processed
        ActionHelpers.Complete<WantsToOverwatch>(ecs, e, action);
      }
    }
  }

}