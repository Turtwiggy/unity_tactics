using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Wiggy
{
  //
  // Considerations for Actions
  //

  public abstract class Consideration
  {
    public abstract float Evaluate(Wiggy.registry ecs, Entity e);
  }

  public class AmmoConsideration : Consideration
  {
    public override float Evaluate(Wiggy.registry ecs, Entity e)
    {
      var ammo = ecs.GetComponent<AmmoComponent>(e);
      float percent01 = ammo.cur / (float)ammo.max;

      // response curve model
      //
      // for ammo, use a negative exponential
      // because the less ammo we have, 
      // the more important it becomes to reload
      var utility = WiggyMath.ExponentialDecay(percent01);
      // Debug.Log("(ammo) " + utility);

      return Mathf.Clamp01(utility);
    }
  }

  public class HealthConsideration : Consideration
  {
    public override float Evaluate(Wiggy.registry ecs, Entity e)
    {
      var health = ecs.GetComponent<HealthComponent>(e);
      float percent01 = health.cur / (float)health.max;

      // response curve model
      //
      // for health, use a logistic function
      // because it's ok to have some damage taken,
      // but the more damage we take, the more urgent it becomes to heal
      var utility = WiggyMath.Logistic(percent01);
      // Debug.Log("(health) " + utility);

      return Mathf.Clamp01(utility);
    }

  }

  public class WeaponDistanceConsideration : Consideration
  {
    public override float Evaluate(Wiggy.registry ecs, Entity e)
    {
      var from = ecs.GetComponent<GridPositionComponent>(e);
      var targets = ecs.GetComponent<TargetsComponent>(e);
      var weapon = ecs.GetComponent<WeaponComponent>(e);

      float optimal_distance = (weapon.min_range + weapon.max_range) / 2.0f;

      if (targets.targets.Count == 0)
      {
        Debug.Log("no targets; not attacking");
        return 0;
      }
      if (targets.targets.Count > 1)
        Debug.LogWarning("InsideWeaponDistanceConsideration >1 target.. choosing [0]");

      var dst = targets.targets[0].distance;
      var to = targets.targets[0].entity;
      var to_pos = ecs.GetComponent<GridPositionComponent>(to);
      Debug.Log($"(WeaponDistanceConsideration) dst: {dst}");

      // no utility in attacking; out of range!
      if (dst < weapon.min_range || dst > weapon.max_range)
      {
        Debug.Log("ai weapon out of range; not attacking!");
        return 0;
      }

      // model this as a negative quadratic parabola
      // where the optimal distance away from the target is the middle of the weapon range.

      // note: we are already in range, so this has to count for something.
      float utility = 0.55f;

      float r1 = weapon.min_range;
      float r2 = weapon.max_range;
      float h = optimal_distance; // x midpoint is optimal_distance
      float k = 1; // clamps y max at 1
      utility += WiggyMath.Parabola(dst, r1, r2, h, k);
      Debug.Log($"WeaponDistanceConsideration: {utility}");

      return Mathf.Clamp01(utility);
    }
  }

  // Movement considerations
  // Should I move in range (of weapon?)?
  // Should I move offensively?
  // Should I move defensively?
  public class MoveConsiderations : Consideration
  {
    public override float Evaluate(Wiggy.registry ecs, Entity e)
    {
      var move = ecs.GetComponent<AIMoveConsiderationComponent>(e);
      var pos = ecs.GetComponent<GridPositionComponent>(e).position;

      // Stupid simple...

      if (move.positions.Count < 2)
        return 0.0f;

      var best_spot = move.positions[^1].Item1;

      if (pos.x == best_spot.x && pos.y == best_spot.y)
        return 0.0f; // same spot

      // if there's a better spot.. there's some utility moving
      return 0.5f;
    }
  }

  //
  // Actions
  //

  public class Action
  {
    public List<Consideration> considerations = new();

    public float Evaluate(Wiggy.registry ecs, Entity e)
    {
      float score = 0.0f;
      float EPSILON = 0.01f;

      if (considerations.Count != 0)
        score = 1.0f;

      foreach (var c in considerations)
        score *= c.Evaluate(ecs, e);

      if (score - EPSILON > 0)
        return score;
      return 0;
    }
  };

  public class Move : Action
  {
    public Move()
    {
      considerations.Add(new MoveConsiderations());
    }
  }

  public class Attack : Action
  {
    public Attack()
    {
      considerations.Add(new WeaponDistanceConsideration());
    }
  };

  public class Overwatch : Action
  {
    public Overwatch()
    {
      // considerations.Add(new TargetsAreVisibleConsideration<Entity>(targets));
    }
  };

  public class Grenade : Action
  {
    public Grenade()
    {
      // considerations.Add(new ClusteredTargetsConsideration(targets));
    }
  };

  public class Reload : Action
  {
    public Reload()
    {
      considerations.Add(new AmmoConsideration());
    }
  };

  public class Heal : Action
  {
    public Heal()
    {
      considerations.Add(new HealthConsideration());
    }
  };

  //
  // AI
  //

  // A simple 3 state FSM for AI.
  // IDLE: accept new tasks
  // MOVE: moving physically in world
  // ANIMATE: an animation is playing, which triggers events e.g. sound
  public enum BRAIN_STATE
  {
    IDLE,
    MOVE,
    ANIMATE,
  };

  public class DefaultBrainComponent
  {
    public List<Action> actions;
    public BRAIN_STATE brain_fsm;
  }

  public static class AiBuilder
  {
    public static DefaultBrainComponent BuildDefaultAI()
    {
      DefaultBrainComponent brain = new()
      {
        brain_fsm = BRAIN_STATE.IDLE,
        actions = new()
        {
          // Actions an AI could take
          // Different ai could have different actions
          new Move(),
          new Attack(),
          // new Heal(),
          // new Reload(),
          // new Overwatch(),
          // new Grenade(),
        }
      };
      return brain;
    }
  }

  static class Reasoner
  {
    public static Optional<Action> Evaluate(DefaultBrainComponent b, Wiggy.registry ecs, Entity e)
    {
      SortedList<float, Action> sorted_actions = new();

      // Evaluate all actions
      foreach (var a in b.actions)
      {
        float score = a.Evaluate(ecs, e);
        Debug.Log($"action: {a.GetType()} score: {score}");

        if (score > 0)
          sorted_actions.Add(score, a);
      }

      // Choose best action
      var result = new Optional<Action>();
      if (sorted_actions.Count > 0)
        result.Set(sorted_actions.Values[^1]);

      return result;
    }
  }

}