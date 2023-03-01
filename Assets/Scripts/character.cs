using UnityEngine;
using UnityEngine.InputSystem;

namespace Wiggy
{
  public class DamageEvent
  {
    // Give attack an ID
    // private static int _attack_id_counter = 0;
    // private readonly int _attack_id = 0;
    // public int AttackID
    // {
    //   get
    //   {
    //     return _attack_id;
    //   }
    // }

    // Other Data
    public int amount;
    public WEAKNESS attackers_type;

    // public DamageEvent()
    // {
    //   _attack_id_counter += 1;
    //   _attack_id = _attack_id_counter;
    // }
  }

  [System.Serializable]
  public class character : MonoBehaviour
  {
    // health
    //
    [SerializeField, Range(0, 1)] private float chance_to_crit = 0.05f;
    [SerializeField] private int crit_multiplier = 2;
    [SerializeField] private int damage = 3;
    [SerializeField] private int max_hp = 100;
    [SerializeField] private WEAKNESS weakness;
    [SerializeField] private int current_hp = 100;

    // rpg/xp
    //
    [SerializeField] private int xp = 0;
    [SerializeField] private int xp_to_gain_from_kill = 10;
    [SerializeField] private int xp_between_levels = 20;
    [SerializeField] private int level = 1;
    [SerializeField] private int level_max = 20;

    private void Start()
    {
      weakness = WeaknessMethods.GetRandomWeakness();
    }

#if DEBUG
    private void Update()
    {
      if (Mouse.current.leftButton.wasPressedThisFrame)
      {
        DamageEvent dmg_evt = new()
        {
          amount = RndDamage(),
          attackers_type = WeaknessMethods.GetRandomWeakness()
        };

        TakeDamage(dmg_evt, null);

        current_hp = max_hp; // reset health
      }
    }
#endif

    #region Combat

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

    private int RndCritAmount()
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
