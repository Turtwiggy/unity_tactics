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
    public abstract float Evaluate();
  }

  public class AmmoConsideration : Consideration
  {
    AmmoComponent ammo;

    public AmmoConsideration(AmmoComponent a)
    {
      this.ammo = a;
    }

    public override float Evaluate()
    {
      float percent01 = ammo.cur / (float)ammo.max;

      // response curve model
      //
      // for ammo, use a negative exponential
      // because the less ammo we have, 
      // the more important it becomes to reload
      var utility = WiggyMath.ExponentialDecay(percent01);
      Debug.Log("(ammo) " + utility);

      return Mathf.Clamp01(utility);
    }
  }

  public class HealthConsideration : Consideration
  {
    HealthComponent health;

    public HealthConsideration(HealthComponent h)
    {
      this.health = h;
    }

    public override float Evaluate()
    {
      float percent01 = health.cur / (float)health.max;

      // response curve model
      //
      // for health, use a logistic function
      // because it's ok to have some damage taken,
      // but the more damage we take, the more urgent it becomes to heal
      var utility = WiggyMath.Logistic(percent01);
      Debug.Log("(health) " + utility);

      return Mathf.Clamp01(utility);
    }

  }

  public class WeaponDistanceConsideration : Consideration
  {
    Wiggy.registry ecs;
    GridPositionComponent from;
    TargetsComponent targets;
    WeaponComponent weapon;

    public WeaponDistanceConsideration(Wiggy.registry ecs, GridPositionComponent from, TargetsComponent targets, WeaponComponent weapon)
    {
      this.ecs = ecs;
      this.from = from;
      this.targets = targets;
      this.weapon = weapon;
    }

    public override float Evaluate()
    {
      float optimal_distance = (weapon.min_range + weapon.max_range) / 2.0f;

      if (targets.targets.Count == 0)
        return 0;
      if (targets.targets.Count > 1)
        Debug.LogError("WeaponDistanceConsideration is not built for >1 target");

      var to = targets.targets[0];
      var to_pos = ecs.GetComponent<GridPositionComponent>(to);
      var x = Mathf.Abs(Vector2.Distance(from.position, to_pos.position));

      // model this as a negative quadratic parabola
      // where the optimal distance away from the target is the middle of the weapon range.

      float r1 = weapon.min_range;
      float r2 = weapon.max_range;
      float h = optimal_distance; // x midpoint is optimal_distance
      float k = 1; // clamps y to 0, 1
      float utility = WiggyMath.Parabola(x, r1, r2, h, k);
      Debug.Log("(weapon_distance) " + utility);

      return Mathf.Clamp01(utility);
    }
  }

  //
  // Actions
  //

  public class Action
  {
    public List<Consideration> considerations = new();

    public float Evaluate()
    {
      float score = 0.0f;
      float EPSILON = 0.01f;

      if (considerations.Count != 0)
        score = 1.0f;

      foreach (var c in considerations)
        score *= c.Evaluate();

      if (score - EPSILON > 0)
        return score;
      return 0;
    }
  };

  public class Move : Action
  {
    public int from;
    public int to;

    public Move() { }
    public Move(Wiggy.registry ecs, Entity from)
    {
      var targets = ecs.GetComponent<TargetsComponent>(from);
      var spots = ecs.GetComponent<AvailableSpotsComponent>(from);
      var weapon = ecs.GetComponent<WeaponComponent>(from);
      // considerations.Add(new WeaponDistanceConsideration(targets, weapon));
      // considerations.Add(new CurrentPositionQualityConsideration(from)); // e.g in cover, or flanked?
      // considerations.Add(new NewPositionQualityConsideration(spots)); // e.g. nearby cover
    }
  }

  public class Attack : Action
  {
    public Attack() { }
    public Attack(Wiggy.registry ecs, Entity from)
    {
      var pos = ecs.GetComponent<GridPositionComponent>(from);
      var targets = ecs.GetComponent<TargetsComponent>(from);
      var weapon = ecs.GetComponent<WeaponComponent>(from);
      considerations.Add(new WeaponDistanceConsideration(ecs, pos, targets, weapon));
    }
  };

  public class Overwatch : Action
  {
    public Overwatch() { }
    public Overwatch(Wiggy.registry ecs, Entity from)
    {
      var targets = ecs.GetComponent<TargetsComponent>(from);
      // considerations.Add(new TargetsAreVisibleConsideration<Entity>(targets));
    }
  };

  public class Grenade : Action
  {
    public Grenade() { }
    public Grenade(Wiggy.registry ecs, Entity from)
    {
      // considerations.Add(new ClusteredTargetsConsideration(targets));
    }
  };

  public class Reload : Action
  {
    public Reload() { }
    public Reload(Wiggy.registry ecs, Entity from)
    {
      var ammo = ecs.GetComponent<AmmoComponent>(from);
      considerations.Add(new AmmoConsideration(ammo));
    }
  };

  public class Heal : Action
  {
    public Heal() { }
    public Heal(Wiggy.registry ecs, Entity from)
    {
      var health = ecs.GetComponent<HealthComponent>(from);
      considerations.Add(new HealthConsideration(health));
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

  public struct DefaultBrainComponent
  {
    public List<Action> actions;
    public BRAIN_STATE brain_fsm;
  }

  public static class AiBuilder
  {
    public static DefaultBrainComponent BuildDefaultAI(Wiggy.registry ecs, Entity e)
    {
      ref var health = ref ecs.GetComponent<HealthComponent>(e);
      ref var ammo = ref ecs.GetComponent<AmmoComponent>(e);

      DefaultBrainComponent brain = new()
      {
        brain_fsm = BRAIN_STATE.IDLE,
        actions = new()
        {
          new Heal(ecs, e),
          new Reload(ecs, e),
          // new Move(ecs, e),
          new Attack(ecs, e),
          // new Overwatch(),
          // new Grenade(),
        }
      };
      return brain;
    }
  }

  static class Reasoner
  {
    public static Optional<Action> Evaluate(DefaultBrainComponent b)
    {
      SortedList<float, Action> sorted_actions = new();

      // Evaluate all actions
      foreach (var a in b.actions)
      {
        float score = a.Evaluate();
        Debug.Log(string.Format("action: {0} score: {1}", a.GetType(), score));

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