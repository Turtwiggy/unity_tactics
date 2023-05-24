using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class OverwatchSystem : ECSSystem
  {
    private GameObject vfx_overwatch;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<WantsToOverwatch>());
      ecs.SetSystemSignature<OverwatchSystem>(s);
    }

    public void Start(Wiggy.registry ecs, GameObject vfx_overwatch)
    {
      this.vfx_overwatch = vfx_overwatch;
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var action = new Overwatch();

        if (!ActionHelpers.Valid<WantsToOverwatch>(ecs, e, action))
        {
          Debug.Log("WantsToOverwatch invalid action");
          ecs.RemoveComponent<WantsToOverwatch>(e);
          continue;
        }

        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        var pos = ecs.GetComponent<GridPositionComponent>(e);

        // How do, overwatch?
        OverwatchStatus status = new();
        ecs.AddComponent(e, status);

        // Overwatch Effects
        Entities.create_effect(ecs, pos.position, vfx_overwatch, "Overwatch Effect");

        // Request is processed
        ActionHelpers.Complete<WantsToOverwatch>(ecs, e, action);
      }
    }
  }

}