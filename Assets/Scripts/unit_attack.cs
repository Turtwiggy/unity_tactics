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

      // ... work out which quadrant player is in
      var dir = def_cell.pos - atk_cell.pos;
      bool on_diagonal = Mathf.Abs(dir.x) == Mathf.Abs(dir.y);
      bool north = dir.y < 0;
      bool south = dir.y > 0;
      bool east = dir.x < 0;
      bool west = dir.x > 0;
      bool y_pull = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);
      bool x_pull = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);
      bool in_quad_N = y_pull && north;
      bool in_quad_S = y_pull && south;
      bool in_quad_E = x_pull && east;
      bool in_quad_W = x_pull && west;
      bool in_quad_NE = on_diagonal && north && east;
      bool in_quad_NW = on_diagonal && north && west;
      bool in_quad_SE = on_diagonal && south && east;
      bool in_quad_SW = on_diagonal && south && west;
      Debug.Log(string.Format("Quadrant -- N:{0} S:{1} E:{2} W:{3} ", in_quad_N, in_quad_S, in_quad_E, in_quad_W));
      Debug.Log(string.Format("Diag Quadrant:{4} -- NE:{0} SE:{1} SW:{2} NW:{3} ", in_quad_NE, in_quad_SE, in_quad_SW, in_quad_NW, on_diagonal));

      // A defender can be in high cover, 
      // but the attacker can flank the defender.
      bool flanked_N = atk_cell.pos.y > def_cell.pos.y && (in_quad_N || in_quad_NE || in_quad_NW);
      bool flanked_S = atk_cell.pos.y < def_cell.pos.y && (in_quad_S || in_quad_SE || in_quad_SW);
      bool flanked_E = atk_cell.pos.x > def_cell.pos.x && (in_quad_E || in_quad_NE || in_quad_SE);
      bool flanked_W = atk_cell.pos.x < def_cell.pos.x && (in_quad_W || in_quad_NW || in_quad_SW);

      foreach (var cover_dir in map.high_cover_spots[to].covered_by)
      {
        // check if covered from flank...
        if (cover_dir == square_direction.N)
          flanked_N = false;
        if (cover_dir == square_direction.S)
          flanked_S = false;
        if (cover_dir == square_direction.E)
          flanked_E = false;
        if (cover_dir == square_direction.W)
          flanked_W = false;
      }
      Debug.Log(string.Format("def:{0} atk:{1}", def_cell.pos.ToString(), atk_cell.pos.ToString()));
      Debug.Log(string.Format("flanked N:{0} S:{1} E:{2} W:{3}", flanked_N, flanked_S, flanked_E, flanked_W));

      // ... check line of sight
      // Note: this only matters for "high" cover 
      // or objects that shouldnt be shot through

      bool line_of_sight_blocked = false;

      //
      // APPROACH 1
      //

      // var direct_path = line_algorithm.create(atk_cell.pos.x, atk_cell.pos.y, def_cell.pos.x, def_cell.pos.y);
      // for (int i = 0; i < direct_path.Count; i++)
      // {
      //   (int, int) p = direct_path[i];
      //   int index = Grid.GetIndex(new Vector2Int(p.Item1, p.Item2), x_max);
      //   Debug.Log(string.Format("x:{0}, y:{1}", p.Item1, p.Item2));
      //   if (map.gos[index] != null)
      //   {
      //     line_of_sight_blocked = true;
      //     Debug.Log(string.Format("Blocked line of sight!", p.Item1, p.Item2));
      //     break;
      //   }
      // }

      //
      // APPROACH 2:
      //

      var atk_go = map.gos[from];
      var def_go = map.gos[to];
      if (Physics.Linecast(atk_go.transform.position, def_go.transform.position, layer_mask))
      {
        line_of_sight_blocked = true;
      }
      Debug.Log("Blocked line of sight: " + line_of_sight_blocked.ToString());


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