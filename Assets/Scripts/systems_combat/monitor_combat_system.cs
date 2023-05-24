using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class MonitorCombatEventsSystem : ECSSystem
  {
    private map_manager map;
    private GameObject vfx_take_damage;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<AttackEvent>());
      ecs.SetSystemSignature<MonitorCombatEventsSystem>(s);
    }

    public void Start(Wiggy.registry ecs, GameObject vfx_take_damage)
    {
      map = Object.FindObjectOfType<map_manager>();
      this.vfx_take_damage = vfx_take_damage;
    }

    public void Update(Wiggy.registry ecs)
    {
      var array = entities.ToArray();
      for (int i = 0; i < array.Length; i++) // readonly because this is modified
      {
        var e = array[i];
        var evt = ecs.GetComponent<AttackEvent>(e);

        var attacker = evt.from;
        var defender = evt.to;

        int damage = CombatHelpers.CalculateDamage(ecs, map, attacker, defender);

        // deal damage
        ref var defender_health = ref ecs.GetComponent<HealthComponent>(defender);
        defender_health.cur -= damage;
        defender_health.cur = Mathf.Max(defender_health.cur, 0);
        Debug.Log($"defender took damage: {damage}");

        // Damage Effects
        if (damage > 0 && defender_health.cur > 0)
        {
          var pos_clone = ecs.GetComponent<GridPositionComponent>(defender);
          Entities.create_effect(ecs, pos_clone.position, vfx_take_damage, "Damage Effect");
        }

        if (defender_health.cur <= 0)
          ecs.AddComponent(defender, new IsDeadComponent());

        ecs.Destroy(e); // destroys event (not unit)
      }
    }
  }
}