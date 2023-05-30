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
      // s.Set(ecs.GetComponentType<WantsToPickup>());
      s.Set(ecs.GetComponentType<PlayerComponent>());
      ecs.SetSystemSignature<PickUpItemSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      map = GameObject.FindObjectOfType<map_manager>();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        // var request = ecs.GetComponent<WantsToPickup>(e);
        var pos = ecs.GetComponent<GridPositionComponent>(e);
        var idx = Grid.GetIndex(pos.position, map.width);
        var entities = map.entity_map[idx].entities;

        // Is the player standing on a keycard?
        bool on_keycard = false;
        Entity keycard_entity = default;
        KeycardComponent keycard_default = default;
        foreach (var ent in entities)
        {
          var keycard = ecs.TryGetComponent(ent, ref keycard_default, out var is_keycard);
          if (is_keycard)
          {
            on_keycard = true;
            keycard_entity = ent;
            break;
          }
        }

        if (on_keycard)
        {
          Debug.Log("standing on a keycard!");
          // WantsToPickup pickup = new();
          // pickup.items = new();
          // pickup.items.Add(keycard_entity);
          // ecs.AddComponent<WantsToPickup>(e);
        }
      }
    }
  }
}