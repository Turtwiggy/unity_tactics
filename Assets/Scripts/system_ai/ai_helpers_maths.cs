using UnityEngine;

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
      var y = Mathf.Pow(e, -5 * x);
      return y;
    }

    // https://en.wikipedia.org/wiki/Generalised_logistic_function
    // The sigmoid expects:
    // input: 0 =< x <= 1
    // output: 0 =< y <= 1
    public static float Logistic(float x)
    {
      var e = Mathf.Exp(1);
      var y = 1 / (1 + Mathf.Pow(e, (12 * x) - 5));
      return y;
    }

    // public static float Linear(float x_max, float x)
    // {
    //   return (x_max - x) / x_max;
    // }

    // public static float Binary()
    // {
    // }

    // vertex at (h, k)
    // roots at r1, r2
    public static float Parabola(float x, float r1, float r2, float h, float k)
    {
      var y = -k / ((r1 - h) * (r2 - h) * Mathf.Pow(x - h, 2) + k);
      return y;
    }
  }

}