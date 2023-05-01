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

        if (!ActionHelpers.Valid<WantsToAttack>(ecs, e, action))
          continue;

        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        var targets = ecs.GetComponent<TargetsComponent>(e);
        var attacker = ecs.GetComponent<GridPositionComponent>(e);

        if (targets.targets.Count == 0)
        {
          Debug.Log("No targets to attack!");
          ActionHelpers.Complete<WantsToAttack>(ecs, e, action);
          continue;
        }

        // Weapon may or may not be equipped
        WeaponComponent weapon = default;
        ecs.TryGetComponent(e, ref weapon);
        int damage = 0;

        var defender = targets.targets[0];
        var defender_pos = ecs.GetComponent<GridPositionComponent>(defender).position;
        ref var defender_health = ref ecs.GetComponent<HealthComponent>(defender);

        if (!weapon.Equals(default(WeaponComponent)))
        {
          damage = weapon.damage;

          // Check range
          var dst = Mathf.Abs(Vector2Int.Distance(attacker.position, defender_pos));
          var in_weapon_range = dst <= weapon.max_range && dst >= weapon.min_range;
          if (!in_weapon_range)
            damage = 0;

          // Check flanked
          var flanked = CombatHelpers.SpotIsFlanked(map, attacker.position, defender_pos);
          if (flanked)
            damage *= 2;

          // Check weaknesses
          // TODO

          // Random crit amount?
          // TODO
        }

        // Take Damage
        defender_health.cur -= damage;
        defender_health.cur = Mathf.Max(defender_health.cur, 0);

        // Request is processed
        ActionHelpers.Complete<WantsToAttack>(ecs, e, action);
      }
    }
  }
}