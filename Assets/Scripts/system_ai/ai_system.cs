using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  public class AiSystem : ECSSystem
  {
    private map_manager map;
    private UnitSpawnSystem spawner;
    private ActionSystem action_system;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<DefaultBrainComponent>());
      s.Set(ecs.GetComponentType<ActionsComponent>());
      ecs.SetSystemSignature<AiSystem>(s);
    }

    public void Start(Wiggy.registry ecs, UnitSpawnSystem spawner, ActionSystem action_system)
    {
      this.map = Object.FindObjectOfType<map_manager>();
      this.spawner = spawner;
      this.action_system = action_system;
    }

    public void Update(Wiggy.registry ecs)
    {
      Debug.Log("decising best action for entity...");

      foreach (var e in entities)
      {
        // Ai Entity
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        ref var targets = ref ecs.GetComponent<TargetsComponent>(e);
        ref var move = ref ecs.GetComponent<AIMoveConsiderationComponent>(e);
        var brain = ecs.GetComponent<DefaultBrainComponent>(e);
        var position = ecs.GetComponent<GridPositionComponent>(e);
        var weapon = ecs.GetComponent<WeaponComponent>(e);

        if (brain.brain_fsm == BRAIN_STATE.IDLE)
        {
          Debug.Log("brain in idle state... choosing action");
        }

        //
        // Before evaluating, 
        //

        // Update valid targets
        // Used by the WeaponDistanceConsideration

        targets.targets.Clear();
        {
          var players = ecs.View<PlayerComponent>();
          for (int i = 0; i < players.Length; i++)
          {
            var entity = players[i];
            var entity_pos = ecs.GetComponent<GridPositionComponent>(entity);
            var dst = Mathf.Abs(Vector2Int.Distance(position.position, entity_pos.position));
            if (dst >= weapon.min_range && dst <= weapon.max_range)
              targets.targets.Add(entity);
          }
        }

        if (targets.targets.Count > 0)
          Debug.Log(string.Format("Entity has {0} targets in range", targets.targets.Count));

        // Possible move spots.

        List<Vector2Int> spots = new();
        var astar = map_manager.GameToAStar(map.obstacle_map, map.width, map.height);
        {
          var movement_range = 3; // TODO: parameterize per unit
          var index = Grid.GetIndex(position.position, map.width);
          var s = a_star.generate_accessible_areas(astar, index, movement_range, map.width, map.height);
          for (int i = 0; i < s.Length; i++)
            spots.Add(s[i].pos);
        }

        // Evaluate quality of possible move spots

        // Create a path from current spot to player spot
        var start_spot = position.position;
        // var player = targets.targets[0]; // ai scanning range??
        var player = ecs.View<PlayerComponent>()[0]; // assume first player
        var player_pos = ecs.GetComponent<GridPositionComponent>(player).position;
        var from = Grid.GetIndex(start_spot, map.width);
        var to = Grid.GetIndex(player_pos, map.width);
        var path = a_star.generate_direct(astar, from, to, map.width);

        move.positions.Clear();
        for (int i = 0; i < spots.Count; i++)
        {
          var spot = spots[i];
          int quality = CombatHelpers.SpotQuality(ecs, map, e, spot, player, path);
          move.positions.Add((spot, quality));
        }
        move.positions.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        //
        // Evaluate
        //

        var action = Reasoner.Evaluate(brain, ecs, e);

        if (action.IsSet)
        {
          var a = action.Data;
          Debug.Log(string.Format("EID: {0} decided: {1}", e.id, a.GetType().ToString()));

          // Implement data?
          if (a.GetType() == typeof(Move))
          {
            var to_idx = Grid.GetIndex(move.positions[^1].Item1, map.width);
            action_system.RequestMoveAction(ecs, e, to_idx);
          }
          else if (a.GetType() == typeof(Attack))
            action_system.RequestAttackAction(ecs, e);
          else if (a.GetType() == typeof(Grenade))
            // currently unknown how ai handles grenade spot choosing
            action_system.RequestGrenadeAction(ecs, e, -1);
          else
            action_system.RequestActionIfImmediate(ecs, a, e);
        }
        else
          Debug.Log("Ai brain cannot take any actions");
      }
    }
  }
}