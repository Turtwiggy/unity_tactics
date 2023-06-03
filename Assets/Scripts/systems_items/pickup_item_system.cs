using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class PickUpItemSystem : ECSSystem
  {
    private map_manager map;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<WantsToPickup>());
      ecs.SetSystemSignature<PickUpItemSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      map = Object.FindObjectOfType<map_manager>();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var player_entity in entities.ToArray()) // readonly because this is modified
      {
        var request = ecs.GetComponent<WantsToPickup>(player_entity);

        foreach (var item_entity in request.items)
        {
          Debug.Log("processing item...");
          {
            AbleToBePickedUp entity_default = default;
            ecs.TryGetComponent(item_entity, ref entity_default, out var able_to_be_picked_up);
            if (!able_to_be_picked_up)
              continue;
            ecs.RemoveComponent<AbleToBePickedUp>(item_entity);
          }

          // Otherwise, we've good to pick it up
          // This sets the item to have an 
          // "InBackpackComponent" with the parent as the 
          // entity that picked it up

          InBackpackComponent backpack = new();
          backpack.parent = player_entity;
          ecs.AddComponent(item_entity, backpack);

          // Remove the world representation
          // A cooler thing might be to do would be to put 
          // it in a "holster" on the 3d models body
          Entities.RemoveWorldEntity(ecs, item_entity, map);
        }

        UnityEngine.Debug.Log("item picked up");
        ecs.RemoveComponent<WantsToPickup>(player_entity);
      }
    }
  }
}