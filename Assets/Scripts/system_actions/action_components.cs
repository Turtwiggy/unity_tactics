using System.Collections.Generic;

namespace Wiggy
{
  using Entity = System.Int32;

  public class Action { };
  public class Move : Action { };
  public class Attack : Action { };
  public class Overwatch : Action { };
  public class Reload : Action { };
  public class Heal : Action { };

  public struct ActionsComponent
  {
    public int allowed_actions_per_turn;
    public Action[] done;
    public Action[] requested;
  }

  // [System.Serializable]
  // public struct AttackEvent
  // {
  //   [SerializeField]
  //   public Entity from;
  //   [SerializeField]
  //   public Entity target;
  // };

  // [System.Serializable]
  // public struct MoveEvent
  // {
  //   [SerializeField]
  //   public int from_index;
  //   [SerializeField]
  //   public int to_index;
  // };

}