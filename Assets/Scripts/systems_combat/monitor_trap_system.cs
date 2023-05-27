using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class MonitorTrapSystem : ECSSystem
  {
    private List<MoveInformation> move_events;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<TrapComponent>());
      ecs.SetSystemSignature<MonitorTrapSystem>(s);
    }

    public void Start(Wiggy.registry ecs, MoveSystem move_system)
    {
      move_events = new();
      move_system.something_moved.AddListener((e) => { move_events.Add(e); });
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly as modified
      {

        // TODO: Did an entity move over a trap?

      }

      move_events.Clear(); // processed
    }
  }

}