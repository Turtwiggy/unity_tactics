
using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  public class ExtractionSystem : ECSSystem
  {
    public bool ready_for_extraction { get; private set; }

    public void Start(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<PlayerComponent>());
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      ecs.SetSystemSignature<ExtractionSystem>(s);
    }

    public void Update(Wiggy.registry ecs, List<Vector2Int> spots)
    {
      // Gotta be a better way than taking a copy every frame
      var spots_copy = new List<Vector2Int>(spots);

      foreach (var ent in entities)
      {
        var _ = ecs.GetComponent<PlayerComponent>(ent);
        var pos = ecs.GetComponent<GridPositionComponent>(ent);
        if (spots_copy.Contains(pos.position))
          spots_copy.Remove(pos.position);
      }

      var spots_filled = spots_copy.Count == 0;
      ready_for_extraction = spots_filled;
    }
  }
}