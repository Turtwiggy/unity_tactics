using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class CombatSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<WantsToAttack>());
      s.Set(ecs.GetComponentType<TargetsComponent>());
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      ecs.SetSystemSignature<CombatSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var action = new Attack();

        if (!ActionHelpers.Valid<WantsToAttack>(ecs, e, action))
          continue;

        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);

        var targets = ecs.GetComponent<TargetsComponent>(e);
        if (targets.targets.Count == 0)
        {
          Debug.Log("No targets to attack!");
          ActionHelpers.Complete<WantsToAttack>(ecs, e, action);
          continue;
        }

        // Create event
        AttackEvent evt = new();
        evt.from = e;
        evt.to = targets.targets[0];
        Debug.Log("creating attack event...");

        // Create entity
        var event_entity = ecs.Create();
        ecs.AddComponent(event_entity, evt);

        // Request is processed
        ActionHelpers.Complete<WantsToAttack>(ecs, e, action);
      }
    }
  }
}