using System.Collections.Generic;
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
      foreach (var e in entities)
      {
        var trap_position = ecs.GetComponent<GridPositionComponent>(e);

        // Did an entity move over a trap?
        Entity entity_to_take_damage = default;

        foreach (var move_evt in move_events)
        {
          var path = move_evt.path;
          foreach (var pos in path)
          {
            if (pos == trap_position.position)
            {
              Debug.Log("entity hit trap!");
              entity_to_take_damage = move_evt.e;
              break;
            }
          }
        }

        if (entity_to_take_damage.Equals(default(Entity)))
          continue;

        // Is this trap able to spring?
        TrapAbleToSpring sprung_default = default;
        ref var springable = ref ecs.TryGetComponent(e, ref sprung_default, out bool able_to_spring);
        if (!able_to_spring)
        {
          Debug.Log("Trap already sprung");
          continue;
        }
        Debug.Log("Trap gonna SPRING");
        ecs.RemoveComponent<TrapAbleToSpring>(e);

        // Send a damage event
        var evt = new AttackEvent();
        evt.amount = new Optional<int>(10); // TODO: should not be hard coded
        evt.from = new Optional<Entity>(e);
        evt.to = new List<Entity>() { entity_to_take_damage };
        var ent = ecs.Create();
        ecs.AddComponent(ent, evt);
      }
      move_events.Clear(); // processed

      foreach (var e in entities)
      {
        // Update the trap visuals
        var instance = ecs.GetComponent<InstantiatedComponent>(e);

        TrapAbleToSpring def = default;
        ecs.TryGetComponent(e, ref def, out bool able_to_spring);
        var mat = instance.instance.transform.GetComponent<MeshRenderer>().material;
        if (able_to_spring)
          mat.color = Color.red;
        else
          mat.color = Color.grey;
      }
    }
  }
}