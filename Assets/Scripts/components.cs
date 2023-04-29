using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  // Tag Components

  public struct PlayerComponent
  {
  };

  public struct CursorComponent
  {
  };

  public enum Team
  {
    PLAYER,
    ENEMY,
    NEUTRAL,
  };

  public struct TeamComponent
  {
    public Team team;
  };

  // Other

  public struct GridPositionComponent
  {
    public Vector2Int position;
  };

  // Helpers

  public struct ToBeInstantiatedComponent
  {
    public GameObject prefab;
    public string name;
  };

  public struct InstantiatedComponent
  {
    public GameObject instance;
  };

  public struct AIMoveConsiderationComponent
  {
    public List<(Vector2Int, int)> positions;
  }

  // Combat

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

  public struct WeaponComponent
  {
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

  // Requests

  public struct ActionsComponent
  {
    public int allowed_actions_per_turn;
    public List<Action> done;
  }

  public interface Request { };

  public struct WantsToAttack : Request
  {
  }

  public struct WantsToHeal : Request
  {
  }

  public struct WantsToMove : Request
  {
    public int to;
  }

  public struct WantsToOverwatch : Request
  {
  }

  public struct WantsToReload : Request
  {
  }

};