using UnityEngine;
using UnityEngine.InputSystem;

namespace Wiggy
{
  public class DamageEvent
  {
    // Other Data
    public int amount;
    public WEAKNESS attackers_type;
  }

  [System.Serializable]
  public class character_handler : MonoBehaviour
  {
    // damage
    public float chance_to_crit { get; private set; }
    public int crit_multiplier { get; private set; }
    public int damage { get; private set; }

    // health
    public int max_hp { get; private set; }
    public int current_hp { get; private set; }
    public WEAKNESS weakness { get; private set; }

    // rpg/xp
    public int xp { get; private set; }
    public int xp_to_gain_from_kill { get; private set; }
    public int xp_between_levels { get; private set; }
    public int level { get; private set; }
    public int level_max { get; private set; }

    private void Start()
    {
      chance_to_crit = 0.05f;
      crit_multiplier = 2;
      damage = 3;

      max_hp = 100;
      current_hp = 100;
      weakness = WeaknessMethods.GetRandomWeakness();

      xp = 0;
      xp_to_gain_from_kill = 10;
      xp_between_levels = 20;
      level = 1;
      level_max = 20;
    }

    #region Combat

    private void HandleDamageUnit()
    {
      // // The grid currently hovering on
      // var index = Grid.GetIndex(camera_handler.grid_index, camera_handler.grid_width);
      // var cell = map_manager.cells[index];
      // var go = map_manager.gos[index];
      // var hcs = map_manager.high_cover_spots[index];

      // // only interested if a unit is there
      // if (go == null)
      //   return;

      // // only interested if the unit is in cover
      // if (hcs.covered_by.Count == 0)
      //   return;

      // if (go.TryGetComponent(out character_handler ch))
      // {
      //   Debug.Log("You clicked a unit in cover!");
      //   DamageEvent dmg_evt = new()
      //   {
      //     amount = 10,
      //     attackers_type = WeaknessMethods.GetRandomWeakness()
      //   };
      //   ch.TakeDamage(dmg_evt, () =>
      //   {
      //     // it died!
      //     Destroy(go);
      //     map_manager.gos[index] = null;
      //     map_manager.cells[index].path_cost = 0;
      //   });
      // }
    }

    public void TakeDamage(DamageEvent d, System.Action died)
    {
      int dmg_amount = d.amount;
      if (weakness.IsWeakTo(d.attackers_type))
        dmg_amount = d.amount * 2; // double damage

      current_hp -= dmg_amount;
      // bool was_double_damage = weakness.IsWeakTo(d.attackers_type);
      // Debug.Log("AttackID: " + d.AttackID + "amount: " + dmg_amount + " you are: " + weakness.ToString() + " attack: " + d.attackers_type.ToString() + " was_dd: " + was_double_damage);

      current_hp = Mathf.Clamp(current_hp, 0, max_hp);
      if (current_hp <= 0)
        died();
    }

    public int RndCritAmount()
    {
      if (Random.value < chance_to_crit)
        return crit_multiplier;
      return 1; // 1x multiplier is not a crit
    }

    public int RndDamage()
    {
      return RndCritAmount() * damage;
    }

    #endregion

    #region XP

    public void GiveXP()
    {
      xp += xp_to_gain_from_kill;
      if (xp > xp_between_levels)
      {
        Debug.Log("Level Up");
        xp -= xp_between_levels;
        level += 1;
        level = Mathf.Clamp(level, 1, level_max);
      }
    }

    #endregion

  }

}
