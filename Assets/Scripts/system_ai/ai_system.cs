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

    private void UpdateTargetsInWeaponRange(Wiggy.registry ecs, Entity e)
    {
      ref var targets = ref ecs.GetComponent<TargetsComponent>(e);
      targets.targets.Clear();

      var atk_weapon = ecs.GetComponent<WeaponComponent>(e);
      var atk_pos = ecs.GetComponent<GridPositionComponent>(e).position;

      foreach (var player in ecs.View<PlayerComponent>())
      {
        var def_pos = ecs.GetComponent<GridPositionComponent>(player).position;

        var (in_range, distance) = CombatHelpers.InWeaponRange(atk_pos, def_pos, atk_weapon);
        if (in_range)
        {
          TargetInfo info = new()
          {
            entity = player,
            distance = distance
          };
          targets.targets.Add(info);
        }
      }

      if (targets.targets.Count > 0)
      {
        Debug.Log($"EID: {e.id} has {targets.targets.Count} targets in range");

        // Sort targets by distance
        targets.targets.Sort(delegate (TargetInfo a, TargetInfo b)
        {
          return a.distance.CompareTo(b.distance);
        });
      }
    }

    private void EvaluateSurroundingPositionQuality(Wiggy.registry ecs, Entity e, ref AIMoveConsiderationComponent move_component, astar_cell[] astar, List<Vector2Int> spots)
    {
      ref var move_positions = ref move_component.positions;
      var position = ecs.GetComponent<GridPositionComponent>(e).position;

      for (int i = 0; i < spots.Count; i++)
      {
        var cur_pos = position;
        var new_pos = spots[i];

        // do not move to players position
        if (cur_pos.x == new_pos.x && cur_pos.y == new_pos.y)
          continue;

        int quality = CombatHelpers.SpotQuality(ecs, e, map, astar, cur_pos, new_pos);
        move_positions.Add((new_pos, quality));
      }

      move_positions.Sort((a, b) => a.Item2.CompareTo(b.Item2));
    }

    public void Update(Wiggy.registry ecs, astar_cell[] astar)
    {
      // Debug.Log("decising best action for entity...");

      foreach (var e in entities)
      {
        // Ai Entity

        // if (brain.brain_fsm == BRAIN_STATE.IDLE)
        // Debug.Log("brain in idle state... choosing action");

        //
        // Before evaluating, 
        //

        // Update valid targets (Used by the WeaponDistanceConsideration)
        UpdateTargetsInWeaponRange(ecs, e);

        // All possible move spots.
        var position = ecs.GetComponent<GridPositionComponent>(e);
        var movement_range = ecs.GetComponent<DexterityComponent>(e).amount;
        var move_index = Grid.GetIndex(position.position, map.width);
        var move_s = a_star.generate_accessible_areas(astar, move_index, movement_range, map.width, map.height);
        var spots = a_star.convert_to_points(move_s);

        // Evaluate quality of possible move spots
        ref var move = ref ecs.GetComponent<AIMoveConsiderationComponent>(e);
        move.positions.Clear();

        // If we have targets in range, no need to evaluate new spots? just shoot em
        var targets_in_range_of_current_spot = ecs.GetComponent<TargetsComponent>(e);
        if (targets_in_range_of_current_spot.targets.Count == 0)
          EvaluateSurroundingPositionQuality(ecs, e, ref move, astar, spots);

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
          var targets = ecs.GetComponent<TargetsComponent>(e).targets;
          if (targets.Count == 0)
          {
            Debug.Log("AI requested attack action but no targets");
            return;
          }
          var target = targets[0].entity;
          var target_pos = ecs.GetComponent<GridPositionComponent>(target).position;
          var player_idx = Grid.GetIndex(target_pos, map.width);
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