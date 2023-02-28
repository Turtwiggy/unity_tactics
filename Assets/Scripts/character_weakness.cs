using UnityEngine;

namespace Wiggy
{
  [System.Serializable]
  public enum WEAKNESS
  {
    ROCK,
    PAPER,
    SCISSORS,
  };

  public static class WeaknessMethods
  {
    public static WEAKNESS GetRandomWeakness()
    {
      var available = System.Enum.GetValues(typeof(WEAKNESS));
      var rnd_idx = Random.Range(0, available.Length);
      return (WEAKNESS)available.GetValue(rnd_idx);
    }

    public static bool IsWeakTo(this WEAKNESS a, WEAKNESS b)
    {
      int distance = (int)a - (int)b;

      // this handles: rock < paper < scissors
      if (distance == -1)
        return true;

      // this handles scissors < rock
      var available = System.Enum.GetValues(typeof(WEAKNESS));
      int max_distance = (int)available.GetValue(available.Length - 1);
      if (distance == max_distance)
        return true;

      return false;
    }
  }
}