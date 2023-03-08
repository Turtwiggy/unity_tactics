using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public static class unit_attack
  {
    public static bool IsCharacter(map_manager map, int index)
    {
      var go = map.gos[index];
      if (go == null)
        return false;
      return go.GetComponent<character_stats>() != null;
    }

    public static void Attack(map_manager map, int from, int to, int x_max)
    {
      var atk = map.gos[from].GetComponent<character_stats>();
      var def = map.gos[to].GetComponent<character_stats>();
      var atk_cell = map.cells[from];
      var def_cell = map.cells[to];

      // ... check range
      var dst = def_cell.pos - atk_cell.pos;
      var in_range = Mathf.Abs(dst.x) <= atk.attack_range || Mathf.Abs(dst.y) <= atk.attack_range;
      if (!in_range)
      {
        Debug.Log("attack is out of range.");
        return;
      }

      // TODO: work out which quadrant player is in???
      // var pull = def_cell.pos - atk_cell.pos;
      // bool attacker_flanking_N = Mathf.Abs(pull.y) > Mathf.Abs(pull.x) ? pull.y > 0;
      // bool attacker_flanking_S = Mathf.Abs(pull.y) > Mathf.Abs(pull.x) ? pull.y > 0;
      // bool attacker_flanking_E = Mathf.Abs(pull.y) > Mathf.Abs(pull.x) ? pull.y > 0;
      // bool attacker_flanking_W = Mathf.Abs(pull.y) > Mathf.Abs(pull.x) ? pull.y > 0;

      // A defender can be in high cover, 
      // but the attacker can flank the defender.
      bool defender_flanked_N = atk_cell.pos.y > def_cell.pos.y;
      bool defender_flanked_S = atk_cell.pos.y < def_cell.pos.y;
      bool defender_flanked_E = atk_cell.pos.x > def_cell.pos.x;
      bool defender_flanked_W = atk_cell.pos.x < def_cell.pos.x;

      foreach (var cover_dir in map.high_cover_spots[to].covered_by)
      {
        // check if covered from flank...
        if (cover_dir == square_direction.N)
          defender_flanked_N = false;
        if (cover_dir == square_direction.S)
          defender_flanked_S = false;
        if (cover_dir == square_direction.E)
          defender_flanked_E = false;
        if (cover_dir == square_direction.W)
          defender_flanked_W = false;
      }

      // TODO: carry this on
      Debug.Log(string.Format("def:{0} atk:{1}", def_cell.pos.ToString(), atk_cell.pos.ToString()));
      Debug.Log(string.Format("defender flanked N: {0}", defender_flanked_N));
      Debug.Log(string.Format("defender flanked S: {0}", defender_flanked_S));
      Debug.Log(string.Format("defender flanked E: {0}", defender_flanked_E));
      Debug.Log(string.Format("defender flanked W: {0}", defender_flanked_W));



      // if (is_covered_from_attacker)
      //   e.amount /= 2;

      // DamageEvent e = new DamageEvent();
      // e.amount = atk.damage;
      // e.attackers_type = atk.weakness;

      // // ... generate crit
      // e.amount *= atk.RndCritAmount();

      // // ... check weakness
      // int dmg_amount = d.amount;
      // if (weakness.IsWeakTo(d.attackers_type))
      //   dmg_amount = d.amount * 2; // double damage

      // def.TakeDamage(e, () =>
      // {
      //   // check if the defender was DESTROYED
      //   Object.Destroy(def.gameObject);
      //   map.gos[to] = null;
      //   map.cells[to].path_cost = 0;

      //   // give the attacker XP
      //   atk.GiveXP();
      // });
    }
  }
} // namespace Wiggy