using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class StandingOnItemSystem : ECSSystem
  {
    private map_manager map;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<PlayerComponent>());
      ecs.SetSystemSignature<StandingOnItemSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      map = GameObject.FindObjectOfType<map_manager>();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities)
      {
        var pos = ecs.GetComponent<GridPositionComponent>(e);
        var idx = Grid.GetIndex(pos.position, map.width);
        var ents = map.entity_map[idx].entities;

        // More than one entity
        if (ents.Count < 2)
          continue;

        // Is the player standing on an item?
        bool standing_on_entity = false;
        Entity pick_up_entity = default;
        {
          foreach (var ent in ents)
          {
            AbleToBePickedUp entity_default = default;
            ecs.TryGetComponent(ent, ref entity_default, out var on_entity);
            if (!on_entity)
              continue;
            standing_on_entity = true;
            pick_up_entity = ent;
            break;
          }
        }
        if (!standing_on_entity)
          continue;

        // Wait for user input?
        // Note: if this entity tries to pick up multiple items 
        // dis will break, because WantsToPickup will already be attached

        Debug.Log("Player wants to pickup item");
        WantsToPickup pickup = new() { items = new() { pick_up_entity } };
        ecs.AddComponent(e, pickup);
      }
    }
  }
}