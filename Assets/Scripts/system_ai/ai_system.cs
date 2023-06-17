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

    public void Update(Wiggy.registry ecs, astar_cell[] astar)
    {
      // Debug.Log("decising best action for entity...");

      foreach (var e in entities)
      {
        // Ai Entity
        var position = ecs.GetComponent<GridPositionComponent>(e);

        // if (brain.brain_fsm == BRAIN_STATE.IDLE)
        // Debug.Log("brain in idle state... choosing action");

        //
        // Before evaluating, 
        //

        // Update valid targets
        // Used by the WeaponDistanceConsideration

        var weapon = ecs.GetComponent<WeaponComponent>(e);
        ref var targets = ref ecs.GetComponent<TargetsComponent>(e);
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
          Debug.Log($"EID: {e.id} has {targets.targets.Count} targets in range");

        // Possible move spots.
        var movement_range = ecs.GetComponent<DexterityComponent>(e).amount;
        var move_index = Grid.GetIndex(position.position, map.width);
        var move_s = a_star.generate_accessible_areas(astar, move_index, movement_range, map.width, map.height);
        var spots = a_star.convert_to_points(move_s);

        // Evaluate quality of possible move spots

        // Create a path from current spot to player spot
        var player = ecs.View<PlayerComponent>()[0]; // assume first player
        var player_pos = ecs.GetComponent<GridPositionComponent>(player).position;

        ref var move = ref ecs.GetComponent<AIMoveConsiderationComponent>(e);
        move.positions.Clear();
        for (int i = 0; i < spots.Count; i++)
        {
          var cur_pos = position.position;
          var new_pos = spots[i];
          int quality = CombatHelpers.SpotQuality(ecs, map, astar, cur_pos, new_pos, player_pos);

          if (new_pos.x == player_pos.x && new_pos.y == player_pos.y)
            continue; // do not move to players position
                      // NOTE: SHOULD CONSIDER ALL ENTITIES

          move.positions.Add((new_pos, quality));
        }
        move.positions.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        //
        // Evaluate
        //
        var brain = ecs.GetComponent<DefaultBrainComponent>(e);
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);

        var action = Reasoner.Evaluate(brain, ecs, e);
        if (!action.IsSet)
        {
          Debug.Log("Ai brain cannot take any actions");
          continue;
        }

        var a = action.Data;
        Debug.Log($"EID: {e.id} decided: {a.GetType()}");

        if (a.GetType() == typeof(Move))
        {
          var to_idx = Grid.GetIndex(move.positions[^1].Item1, map.width);
          action_system.AIRequestMoveAction(ecs, astar, e, to_idx);
        }
        else if (a.GetType() == typeof(Attack))
        {
          var player_idx = Grid.GetIndex(player_pos, map.width);
          // warning: this could introduce a bug where the ai now attacks 
          // everything at a specific index, instead of just the intended entity
          // as/if multiple units on the same tile, reevaluate this
          action_system.AIRequestAttackAction(ecs, e, player_idx);
        }
        // else if (a.GetType() == typeof(Grenade))
        // action_system.RequestGrenadeAction(ecs, e, -1); // -1 because currently unknown how ai handles grenade spot choosing
        else
          action_system.RequestActionIfImmediate(ecs, a, e);
      }
    }
  }
}