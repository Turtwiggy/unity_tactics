
using System.Collections.Generic;

namespace Wiggy
{
  public struct WeaponComponent
  {
    public int min_range; // e.g. melee: 0 gun: 5
    public int max_range; // e.g. melee: 3 gun: 10
    // float attack_rate = 0.15f;
    // int use_cooldown = 1;
    // int projectiles = 1;
    // float fire_rate = 1.0f;
    // float time_between_shots = 1.0f;
    // float time_since_last_shot = 0.0f;
    // float bullet_speed = 500.0f;
  }
  // struct DefenceComponent
  // {
  //   int defence;
  // }

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
  public struct TargetsComponent
  {
    public List<Entity> targets;
  }
}