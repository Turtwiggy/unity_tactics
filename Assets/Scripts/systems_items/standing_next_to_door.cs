using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  public class StandingNextToDoorSystem : ECSSystem
  {
    private map_manager map;
    public List<Entity> eligable_doors { get; private set; }

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<PlayerComponent>());
      ecs.SetSystemSignature<StandingNextToDoorSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      map = Object.FindObjectOfType<map_manager>();
      eligable_doors = new();
    }

    public void Update(Wiggy.registry ecs)
    {
      eligable_doors.Clear();

      foreach (var e in entities)
      {
        var pos = ecs.GetComponent<GridPositionComponent>(e).position;
        var neighbours = a_star.square_neighbour_indicies(pos.x, pos.y, map.width, map.height);

        foreach (var (_, idx) in neighbours)
        {
          var neighbour_ents = map.entity_map[idx].entities;
          foreach (var ent in neighbour_ents)
          {
            var door_default = default(DoorComponent);
            var door = ecs.TryGetComponent(ent, ref door_default, out var is_door);
            if (!is_door)
              continue; // We're next to a door!
            eligable_doors.Add(ent);
          }
        }
      }
    }
  }
}