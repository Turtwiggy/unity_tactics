using System.Collections.Generic;

namespace Wiggy
{
  // setup a battlefield. (x inches by y inches).
  // agree game is going to last 3 battlerounds.
  // four objective markers.
  // points if held at beginning of turn
  // deploy squads so they're 24" away from eachother

  public struct HealthComponent
  {
    public int max;
    public int cur;
  }

  public struct AmmoComponent
  {
    public int max;
    public int cur;
    // bool infinite_ammo = false;
  }

  // Who is this entity targeting?

  public struct TargetInfo
  {
    public Entity entity;
    public float distance;
  }

  public struct TargetsComponent
  {
    public List<TargetInfo> targets;
  }

  public struct WeaponComponent
  {
    public string display_name;
    public int min_range; // e.g. melee: 0 gun: 5
    public int max_range; // e.g. melee: 3 gun: 10
    public int damage;

    // float attack_rate = 0.15f;
    // int use_cooldown = 1;
    // int projectiles = 1;
    // float fire_rate = 1.0f;
    // float time_between_shots = 1.0f;
    // float time_since_last_shot = 0.0f;
    // float bullet_speed = 500.0f;
  }
}