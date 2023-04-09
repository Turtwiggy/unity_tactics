using System.Collections.Generic;

namespace Wiggy
{
  public class Action
  {
    //
  };
  public class Move : Action
  {
    public int from;
    public int to;
  };
  public class Attack : Action
  {
    public int from;
    public int to;
  };
  public class Overwatch : Action
  {
    //
  };
  public class Reload : Action
  {
    //
  };
  public class Heal : Action
  {
    //
  };

  public struct ActionsComponent
  {
    public int allowed_actions_per_turn;
    public List<Action> done;
    public List<Action> requested;
  }
}