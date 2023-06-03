using System.Diagnostics;
using System.Linq;

namespace Wiggy
{
  public class UseItemSystem : ECSSystem
  {
    private SelectSystem select_system;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<WantsToUse>());
      ecs.SetSystemSignature<UseItemSystem>(s);
    }

    public void Start(Wiggy.registry ecs, SelectSystem select_system)
    {
      this.select_system = select_system;
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var request = ecs.GetComponent<WantsToUse>(e);
        var item = e;
        var targets = request.targets;

        // DOOR <==> KEYCARD INTERACTION
        {
          var optional_door = map_manager.GetFirst<DoorComponent>(ecs, targets);
          var keycard_default = default(KeycardComponent);
          var keycard = ecs.TryGetComponent(item, ref keycard_default, out var is_keycard);

          if (is_keycard && optional_door.IsSet)
          {
            UnityEngine.Debug.Log("You tried to use a keycard on a door!");

            var door_ent_idx = optional_door.Data;
            var door = targets[door_ent_idx];
            var door_pos = ecs.GetComponent<GridPositionComponent>(door).position;

            // Open the door! By Killing it!
            ecs.AddComponent(door, new IsDeadComponent());

            // deselect the item, as its about to die
            select_system.ClearSelect();
          }
        }

        ecs.RemoveComponent<WantsToUse>(e); // process request

        // Was this item consumed?
        // var consumable_default = default(ConsumableComponent);
        // ecs.TryGetComponent(e, ref consumable_default, out var is_consumable);
        // if (is_consumable)
        //   ecs.Destroy(e);
      }
    }
  }
}