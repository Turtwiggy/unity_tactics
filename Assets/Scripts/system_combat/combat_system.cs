using UnityEngine;

// namespace Wiggy
// {


//   //   public struct DamageInfo
//   //   {
//   //     public bool defender_is_weak;
//   //     public bool defender_is_flanked;
//   //     public bool defender_is_out_of_sight;
//   //     public bool defender_receives_crit;
//   //   };

//   public static class unit_attack
//   {
//     public static void Attack(map_manager map, int from, int to, int x_max)
//     {
//       var atk = map.gos[from].GetComponent<character_stats>();
//       var def = map.gos[to].GetComponent<character_stats>();
//       var atk_cell = map.cells[from];
//       var def_cell = map.cells[to];

//       // ... check range
//       var dst = def_cell.pos - atk_cell.pos;
//       var in_range = Mathf.Abs(dst.x) <= atk.attack_range || Mathf.Abs(dst.y) <= atk.attack_range;
//       if (!in_range)
//       {
//         Debug.Log("attack is out of range.");
//         return;
//       }


//       //
//       // Take Damage
//       //

//       DamageInfo info = new();
//       info.defender_is_weak = def.weakness.IsWeakTo(atk.weakness);
//       info.defender_is_flanked = flanked_N || flanked_S || flanked_E || flanked_W;
//       info.defender_is_out_of_sight = line_of_sight_blocked;

//       DamageEvent e = new DamageEvent();
//       e.amount = atk.damage;
//       e.attackers_type = atk.weakness;

//       // adjust damage
//       if (info.defender_is_weak)
//         e.amount *= 2;
//       if (info.defender_is_flanked)
//         e.amount *= 2;
//       if (info.defender_is_out_of_sight)
//         e.amount = 0;

//       var crit_amount = atk.RndCritAmount();
//       e.amount *= crit_amount;

//       // Debug.Log(string.Format("def:{0} atk:{1}", def_cell.pos.ToString(), atk_cell.pos.ToString()));
//       Debug.Log(string.Format("Damage:{0}. Info, weak:{1}, flanked:{2}, outofsight:{3}, was_crit {4}x",
//         e.amount,
//         info.defender_is_weak,
//         info.defender_is_flanked,
//         info.defender_is_out_of_sight,
//         crit_amount)
//       );

//       def.TakeDamage(e, () =>
//       {
//         // check if the defender was DESTROYED
//         Object.Destroy(def.gameObject);
//         map.gos[to] = null;
//         map.cells[to].path_cost = 0;

//         // give the attacker XP
//         atk.GiveXP();
//       });
//     }
//   }
// } // namespace Wiggy