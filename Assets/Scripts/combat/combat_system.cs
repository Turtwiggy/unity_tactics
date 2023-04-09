// using UnityEngine;

// namespace Wiggy
// {
//   private void ProcessAttackQueue()
//   {
//     if (attack_event_queue.Count == 0)
//       return;
//     Debug.Log("pulling attack event from queue!");
//     var e = attack_event_queue[0];
//     attack_event_queue.RemoveAt(0);

//     var from = e.from;
//     var to = e.target;

//     Debug.Log("TODO: a attacking b...");
//   }

//   //   public struct DamageInfo
//   //   {
//   //     public bool defender_is_weak;
//   //     public bool defender_is_flanked;
//   //     public bool defender_is_out_of_sight;
//   //     public bool defender_receives_crit;
//   //   };

//   public static class unit_attack
//   {
//     private static square_direction WhichQuadrantIsAttacker(Vector2Int def_cell, Vector2Int atk_cell)
//     {
//       var dir = def_cell - atk_cell;
//       bool on_diagonal = Mathf.Abs(dir.x) == Mathf.Abs(dir.y);
//       bool north = dir.y < 0;
//       bool south = dir.y > 0;
//       bool east = dir.x < 0;
//       bool west = dir.x > 0;
//       bool y_pull = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);
//       bool x_pull = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

//       bool in_quad_N = y_pull && north;
//       bool in_quad_S = y_pull && south;
//       bool in_quad_E = x_pull && east;
//       bool in_quad_W = x_pull && west;
//       bool in_quad_NE = on_diagonal && north && east;
//       bool in_quad_NW = on_diagonal && north && west;
//       bool in_quad_SE = on_diagonal && south && east;
//       bool in_quad_SW = on_diagonal && south && west;

//       if (in_quad_N)
//         return square_direction.N;
//       if (in_quad_S)
//         return square_direction.S;
//       if (in_quad_E)
//         return square_direction.E;
//       if (in_quad_W)
//         return square_direction.W;

//       if (in_quad_NE)
//         return square_direction.NE;
//       if (in_quad_NW)
//         return square_direction.NW;
//       if (in_quad_SE)
//         return square_direction.SE;
//       if (in_quad_SW)
//         return square_direction.SW;

//       return default;
//     }

//     //     public static void Attack(map_manager map, int from, int to, int x_max)
//     //     {
//     //       var atk = map.gos[from].GetComponent<character_stats>();
//     //       var def = map.gos[to].GetComponent<character_stats>();
//     //       var atk_cell = map.cells[from];
//     //       var def_cell = map.cells[to];

//     //       // ... check range
//     //       var dst = def_cell.pos - atk_cell.pos;
//     //       var in_range = Mathf.Abs(dst.x) <= atk.attack_range || Mathf.Abs(dst.y) <= atk.attack_range;
//     //       if (!in_range)
//     //       {
//     //         Debug.Log("attack is out of range.");
//     //         return;
//     //       }

//     //       // ... work out which attacking player is in
//     //       var attacker_quadrant = WhichQuadrantIsAttacker(def_cell, atk_cell);

//     //       // A defender can be in high cover, 
//     //       // but the attacker can flank the defender.
//     //       bool flanked_N = atk_cell.pos.y > def_cell.pos.y && attacker_quadrant.InQuadrant(square_direction.N);
//     //       bool flanked_S = atk_cell.pos.y < def_cell.pos.y && attacker_quadrant.InQuadrant(square_direction.S);
//     //       bool flanked_E = atk_cell.pos.x > def_cell.pos.x && attacker_quadrant.InQuadrant(square_direction.E);
//     //       bool flanked_W = atk_cell.pos.x < def_cell.pos.x && attacker_quadrant.InQuadrant(square_direction.W);

//     //       // check if covered from flank...
//     //       foreach (var cover_dir in map.high_cover_spots[to].covered_by)
//     //       {
//     //         if (cover_dir == square_direction.N)
//     //           flanked_N = false;
//     //         if (cover_dir == square_direction.S)
//     //           flanked_S = false;
//     //         if (cover_dir == square_direction.E)
//     //           flanked_E = false;
//     //         if (cover_dir == square_direction.W)
//     //           flanked_W = false;
//     //       }

//     //       // ... check line of sight
//     //       // Note: this only matters for "high" cover 
//     //       // or objects that shouldnt be shot through

//     //       bool line_of_sight_blocked = false;

//     //       // APPROACH 1: more consistent???

//     //       // var path = a_star.generate_direct_with_diagonals(map_manager.cells, from, to, x_max, false);
//     //       // for (int i = 0; i < path.Length; i++)

//     //       // APPROACH 2: more visual??

//     //       var atk_go = map.gos[from];
//     //       var def_go = map.gos[to];
//     //       var atk_go_center = atk_go.transform.position;
//     //       atk_go_center.y += 0.1f;
//     //       var def_go_center = def_go.transform.position;
//     //       def_go_center.y += 0.1f;
//     //       if (Physics.Linecast(atk_go_center, def_go_center, out var hit))
//     //         line_of_sight_blocked = true;

//     //       //
//     //       // Take Damage
//     //       //

//     //       DamageInfo info = new();
//     //       info.defender_is_weak = def.weakness.IsWeakTo(atk.weakness);
//     //       info.defender_is_flanked = flanked_N || flanked_S || flanked_E || flanked_W;
//     //       info.defender_is_out_of_sight = line_of_sight_blocked;

//     //       DamageEvent e = new DamageEvent();
//     //       e.amount = atk.damage;
//     //       e.attackers_type = atk.weakness;

//     //       // adjust damage
//     //       if (info.defender_is_weak)
//     //         e.amount *= 2;
//     //       if (info.defender_is_flanked)
//     //         e.amount *= 2;
//     //       if (info.defender_is_out_of_sight)
//     //         e.amount = 0;

//     //       var crit_amount = atk.RndCritAmount();
//     //       e.amount *= crit_amount;

//     //       // Debug.Log(string.Format("def:{0} atk:{1}", def_cell.pos.ToString(), atk_cell.pos.ToString()));
//     //       Debug.Log(string.Format("Damage:{0}. Info, weak:{1}, flanked:{2}, outofsight:{3}, was_crit {4}x",
//     //         e.amount,
//     //         info.defender_is_weak,
//     //         info.defender_is_flanked,
//     //         info.defender_is_out_of_sight,
//     //         crit_amount)
//     //       );

//     //       def.TakeDamage(e, () =>
//     //       {
//     //         // check if the defender was DESTROYED
//     //         Object.Destroy(def.gameObject);
//     //         map.gos[to] = null;
//     //         map.cells[to].path_cost = 0;

//     //         // give the attacker XP
//     //         atk.GiveXP();
//     //       });
//     //     }
//   }
// } // namespace Wiggy