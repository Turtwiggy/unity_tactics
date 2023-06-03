using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  // Helpers

  public struct TagComponent
  {
    public string name;
  };

  public struct ToBeInstantiatedComponent
  {
    public GameObject prefab;
    public GameObject parent;
  };

  public struct InstantiatedComponent
  {
    public GameObject instance;
  };

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

  public struct DoorComponent
  {
    //
  };

  public struct KeycardComponent
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

  // Is a humanoid like character
  public struct HumanoidComponent
  {
  };

  // Other

  public struct GridPositionComponent
  {
    public Vector2Int position;
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
    public int map_idx;
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

  public struct ExplodesOnDeath
  {
  };

  public struct TrapAbleToSpring
  {
  };

  // Events

  // attack event does the attack
  public struct AttackEvent
  {
    public Optional<Entity> from;
    public Optional<int> amount;
    public List<Entity> to;
  };

  // Stats

  public struct DexterityComponent
  {
    public int amount;
  };

  // Items / Inventory

  struct AbleToBePickedUp
  {
  };

  struct InBackpackComponent
  {
    public Entity parent;
  };

  struct WantsToPickup
  {
    public List<Entity> items;
  };

  public struct WantsToUse
  {
    public List<Entity> targets;
  };

  // struct ConsumableComponent
  // {
  // };

};