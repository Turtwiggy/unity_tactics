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

    public void Update(Wiggy.registry ecs, astar_cell[] astar)
    {
      var array = entities.ToArray();
      for (int i = 0; i < array.Length; i++) // readonly because this is modified
      {
        var e = array[i];
        var evt = ecs.GetComponent<AttackEvent>(e);

        var attacker = evt.from.Data;
        var defenders = evt.to;
        Debug.Log($"{defenders.Count} defenders taking damage");

        foreach (var defender in defenders)
        {
          int damage = evt.amount.IsSet ? evt.amount.Data : 0;

          // Get the attacker to play an animation
          // Note: this shouldn't be here, probably
          if (evt.from.IsSet && !evt.amount.IsSet)
          {

            // shouldnt be here
            var humanoid_default = default(HumanoidComponent);
            ecs.TryGetComponent(attacker, ref humanoid_default, out var is_humanoid);
            if (is_humanoid)
            {
              ref var instance = ref ecs.GetComponent<InstantiatedComponent>(attacker);
              var gameObject = instance.instance;
              var animator = gameObject.GetComponentInChildren<Animator>();
              animation_handler.PlayAnimation(animator, "breakdance_ending_1", false);
            }

            // Calculate damage from the attacker
            damage = CombatHelpers.CalculateDamage(ecs, map, astar, attacker, defender);
          }

          // deal damage
          var health_default = default(HealthComponent);
          ref var defender_health = ref ecs.TryGetComponent(defender, ref health_default, out var has_hp);
          if (!has_hp)
            continue; // IT MUST BLEED
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
        }

        ecs.Destroy(e); // destroys event (not unit)
      }
    }
  }
}