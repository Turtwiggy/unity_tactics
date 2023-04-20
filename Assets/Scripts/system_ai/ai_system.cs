using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        var brain = ecs.GetComponent<DefaultBrainComponent>(e);
        var position = ecs.GetComponent<GridPositionComponent>(e);
        var weapon = ecs.GetComponent<WeaponComponent>(e);
        var spots = ecs.GetComponent<AvailableSpotsComponent>(e);
        var targets = ecs.GetComponent<TargetsComponent>(e);

        //
        // Before evaluating, 
        //

        // Update possible move spots.
        var movement_range = 3;
        var index = Grid.GetIndex(position.position, map.width);
        var astar = map_manager.GameToAStar(map.obstacle_map, map.width, map.height);
        var s = a_star.generate_accessible_areas(astar, index, movement_range, map.width, map.height);
        spots.spots.Clear();
        for (int i = 0; i < s.Length; i++)
          spots.spots.Add(Grid.GetIndex(s[i].pos, map.width));

        // Update valid targets
        targets.targets.Clear();

        // Get units in range
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