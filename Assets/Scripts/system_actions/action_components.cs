using System.Collections.Generic;

namespace Wiggy
{
  public struct ActionsComponent
  {
    public int allowed_actions_per_turn;
    public List<Action> done;
    public List<Action> requested;
  }
}