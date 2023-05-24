using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class HealSystem : ECSSystem
  {
    private GameObject vfx_heal;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<HealthComponent>());
      s.Set(ecs.GetComponentType<WantsToHeal>());
      ecs.SetSystemSignature<HealSystem>(s);
    }

    public void Start(Wiggy.registry ecs, GameObject vfx_heal)
    {
      this.vfx_heal = vfx_heal;
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var action = new Heal();

        if (!ActionHelpers.Valid<WantsToHeal>(ecs, e, action))
        {
          Debug.Log("WantsToHeal invalid action");
          ecs.RemoveComponent<WantsToHeal>(e);
          continue;
        }

        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        ref var hp = ref ecs.GetComponent<HealthComponent>(e);
        var pos = ecs.GetComponent<GridPositionComponent>(e);

        Debug.Log("Implement heal items?");
        hp.cur += Mathf.Abs(10);
        hp.cur = Mathf.Min(hp.cur, hp.max);
        Debug.Log($"entity healed: {10}");

        // Heal Effects
        Entities.create_effect(ecs, pos.position, vfx_heal, "Grenade Effect");

        // Request is processed
        ActionHelpers.Complete<WantsToHeal>(ecs, e, action);
      }
    }
  }

}