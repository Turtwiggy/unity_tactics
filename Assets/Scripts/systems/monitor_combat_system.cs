using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class MonitorCombatEventsSystem : ECSSystem
  {
    private map_manager map;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<AttackEvent>());
      ecs.SetSystemSignature<MonitorCombatEventsSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      map = Object.FindObjectOfType<map_manager>();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var evt = ecs.GetComponent<AttackEvent>(e);

        var attacker = evt.from;
        var defender = evt.to;

        int damage = CombatHelpers.CalculateDamage(ecs, map, attacker, defender);

        // deal damage
        ref var defender_health = ref ecs.GetComponent<HealthComponent>(defender);
        defender_health.cur -= damage;
        defender_health.cur = Mathf.Max(defender_health.cur, 0);
        Debug.Log($"defender took damage: {damage}");

        ecs.Destroy(e); // processed event
      }
    }
  }
}