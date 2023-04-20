
namespace Wiggy
{
  // struct EntityTargetEvent
  // {
  //   Entity attacker;
  //   List<Entity> targets;
  // }

  struct WeaponComponent
  {
    // float attack_rate = 0.15f;
    // int use_cooldown = 1;
    // int projectiles = 1;
    // int ammo = 1;
    // float fire_rate = 1.0f;
    // bool infinite_ammo = false;
    // float time_between_shots = 1.0f;
    // float time_since_last_shot = 0.0f;
    // float bullet_speed = 500.0f;
  }

  struct DefenceComponent
  {
    int defence;
  }

  public struct HealthComponent
  {
    public int max;
    public int cur;
  }
  public struct AmmoComponent
  {
    public int max;
    public int cur;
  }
}