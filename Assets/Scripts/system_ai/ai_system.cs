using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  public class AiSystem : ECSSystem
  {
    private map_manager map;
    private UnitSpawnSystem spawner;

    public void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<DefaultBrainComponent>());
      s.Set(ecs.GetComponentType<ActionsComponent>());
      ecs.SetSystemSignature<AiSystem>(s);
    }

    public void Start(Wiggy.registry ecs, UnitSpawnSystem spawner)
    {
      this.map = Object.FindObjectOfType<map_manager>();
      this.spawner = spawner;
    }

    public void Update(Wiggy.registry ecs)
    {
      Debug.Log("decising best action for entity...");

      foreach (var e in entities)
      {
        // Ai Entity
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        ref var targets = ref ecs.GetComponent<TargetsComponent>(e);
        var brain = ecs.GetComponent<DefaultBrainComponent>(e);
        var position = ecs.GetComponent<GridPositionComponent>(e);
        var weapon = ecs.GetComponent<WeaponComponent>(e);

        //
        // Before evaluating, 
        //

        // Possible move spots.
        List<Vector2Int> spots = new();
        {
          var movement_range = 3; // TODO: parameterize per unit
          var index = Grid.GetIndex(position.position, map.width);
          var astar = map_manager.GameToAStar(map.obstacle_map, map.width, map.height);
          var s = a_star.generate_accessible_areas(astar, index, movement_range, map.width, map.height);
          for (int i = 0; i < s.Length; i++)
          {
            var pos = s[i].pos;
            if (pos == position.position)
              continue; // skip start spot
            spots.Add(pos);
          }
        }

        // Evaluate quality of possible move spots

        var best_spot = position.position;  // Assume best spot is current spot
        var best_spot_quality = CombatHelpers.SpotQuality(ecs, map, spawner, e, best_spot);

        for (int i = 0; i < spots.Count; i++)
        {
          var spot = spots[i];
          int quality = CombatHelpers.SpotQuality(ecs, map, spawner, e, spot);
          if (quality > best_spot_quality)
          {
            best_spot = spot;
            best_spot_quality = quality;
          }
        }

        Debug.Log(string.Format("ai current spot:{0} best spot: {1}", position.position, best_spot));

        // Update valid targets
        targets.targets.Clear();
        foreach (var other in spawner.entities)
        {
          if (e.id == other.id)
            continue; // dont compare self
          var other_pos = ecs.GetComponent<GridPositionComponent>(other);
          var dst = Mathf.Abs(Vector2Int.Distance(position.position, other_pos.position));
          if (dst >= weapon.min_range && dst <= weapon.max_range)
            targets.targets.Add(other);
        }
        if (targets.targets.Count > 0)
          Debug.Log(string.Format("Entity has {0} targets in range", targets.targets.Count));

        //
        // Evaluate
        //
        var action = Reasoner.Evaluate(brain);

        if (action.IsSet)
        {
          var chosen = action.Data;
          Debug.Log(string.Format("EID: {0} decided: {1}", e.id, chosen.GetType().ToString()));

          // request action for entity
          actions.requested.Add(chosen);
        }
        else
          Debug.Log("Ai brain cannot take any actions");
      }
    }
  }
}