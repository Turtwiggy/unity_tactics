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
    //
  };

  public struct TrapComponent
  {
    //
  };

  public struct BarrelComponent
  {
    //
  };

  public struct ParticleEffectComponent
  {
    //
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

  public struct IsDeadComponent
  {
  };

  public struct AIMoveConsiderationComponent
  {
    public List<(Vector2Int, int)> positions;
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
    public Entity target;
  }

  public struct WantsToHeal : Request { }

  public struct WantsToMove : Request
  {
    public Vector2Int[] path;
  }

  public struct WantsToOverwatch : Request { }

  public struct WantsToReload : Request { }

  public struct WantsToGrenade : Request
  {
    public int index;
  }

  // Status effects e.g. poisoned, stunned, reduced armour

  public struct OverwatchStatus
  {
  };

  public struct IsDeadStatus
  {
  };

  // Events

  // attack event does the attack
  public struct AttackEvent
  {
    public Entity from;
    public Entity to;
  };

  // Stats

  public struct DexterityComponent
  {
    public int amount;
  };

};