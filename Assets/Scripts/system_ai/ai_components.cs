using UnityEngine;
using System.Collections.Generic;

namespace Wiggy
{
  public static class WiggyMath
  {
    // https://en.wikipedia.org/wiki/Exponential_decay
    public static float ExponentialDecay(float x)
    {
      // hmm: (100 - distance ^ 3) / (100 ^ 3)
      var k = 1;
      var e = Mathf.Exp(k);
      return Mathf.Pow(e, -5 * x);
    }

    // https://en.wikipedia.org/wiki/Generalised_logistic_function
    // The sigmoid expects:
    // input: 0 =< x <= 1
    // output: 0 =< y <= 1
    public static float Logistic(float x)
    {
      var e = Mathf.Exp(1);
      return 1 / (1 + Mathf.Pow(e, (12 * x) - 5));
    }

    public static float Linear(float x_max, float x)
    {
      return (x_max - x) / x_max;
    }
  }

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
      float percent01 = ammo.cur / ammo.max;

      // response curve model
      //
      // for ammo, use a negative exponential
      // because the less ammo we have, 
      // the more important it becomes to reload
      var utility = WiggyMath.ExponentialDecay(percent01);

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
      float percent01 = health.cur / health.max;

      // response curve model
      //
      // for health, use a logistic function
      // because it's ok to have some damage taken,
      // but the more damage we take, the more urgent it becomes to heal
      var utility = WiggyMath.Logistic(percent01);

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
      float score = 1.0f;
      float EPSILON = 0.01f;

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
  }

  public class Attack : Action
  {
    public int from;
    public int to;
  };

  public class Overwatch : Action
  {
    //
  };

  public class Reload : Action
  {
    public Reload()
    {
    }
    public Reload(AmmoComponent ammo)
    {
      considerations.Add(new AmmoConsideration(ammo));
    }
  };

  public class Heal : Action
  {
    public Heal()
    {
    }
    public Heal(HealthComponent health)
    {
      considerations.Add(new HealthConsideration(health));
    }
  };

  //
  // AI
  //

  public struct DefaultBrainComponent
  {
    public List<Action> actions;
  }

  public static class AiBuilder
  {
    public static DefaultBrainComponent BuildDefaultAI(Wiggy.registry ecs, Entity e)
    {
      ref var health = ref ecs.GetComponent<HealthComponent>(e);
      ref var ammo = ref ecs.GetComponent<AmmoComponent>(e);

      DefaultBrainComponent brain = new()
      {
        actions = new()
        {
          new Heal(health),
          new Reload(ammo)
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
        result.Set(sorted_actions.Values[0]);

      return result;
    }
  }

}