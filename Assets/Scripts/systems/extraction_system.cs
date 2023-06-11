
using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  // Similar to standing_on_pickup_item_system
  public class ExtractionSystem : ECSSystem
  {
    public bool ready_for_extraction { get; private set; }

    private map_manager map;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<PlayerComponent>());
      ecs.SetSystemSignature<ExtractionSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      map = Object.FindObjectOfType<map_manager>();
    }

    public void Update(Wiggy.registry ecs)
    {
      int num_players = entities.Count;

      // the game should be over
      if (num_players == 0)
      {
        ready_for_extraction = false;
        return;
      }

      int num_players_left_to_extract = num_players;

      foreach (var e in entities)
      {
        var pos = ecs.GetComponent<GridPositionComponent>(e);
        var idx = Grid.GetIndex(pos.position, map.width);
        var ents = map.entity_map[idx].entities;

        // More than one entity
        if (ents.Count < 2)
          continue;

        foreach (var ent in ents)
        {
          ExitComponent exit_default = default;
          ecs.TryGetComponent(ent, ref exit_default, out var on_exit);
          if (!on_exit)
            continue;
          num_players_left_to_extract--;
          break;
        }
      }

      var spots_filled = num_players_left_to_extract == 0;
      ready_for_extraction = spots_filled;
    }
  }
}