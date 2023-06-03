using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class CombatSystem : ECSSystem
  {
    private map_manager map;

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
      map = Object.FindObjectOfType<map_manager>();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var action = new Attack();
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        var request = ecs.GetComponent<WantsToAttack>(e);

        if (!ActionHelpers.Valid<WantsToAttack>(ecs, e, action))
        {
          Debug.Log("WantsToAttack invalid action");
          ecs.RemoveComponent<WantsToAttack>(e);
          continue;
        }

        // Create event
        AttackEvent evt = new();
        evt.amount = new Optional<int>();
        evt.from = new Optional<Entity>(e);
        evt.to = map.entity_map[request.map_idx].entities;
        Debug.Log("creating attack event...");

        // Create event entity
        var event_entity = ecs.Create();
        ecs.AddComponent(event_entity, evt);

        // Request is processed
        ActionHelpers.Complete<WantsToAttack>(ecs, e, action);
      }
    }
  }
}