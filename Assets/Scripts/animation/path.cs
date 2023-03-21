using System.Threading.Tasks;
using UnityEngine;

namespace Wiggy
{
  public static class Animate
  {
    public static async Task AlongPath(GameObject go, Vector2Int[] path, int width, int size)
    {
      if (path.Length <= 1)
        return;

      int nodes = path.Length;
      float percent = 0.0f;
      float percentage_amount = 1 / ((float)nodes - 1);

      const float seconds_per_move = 0.5f;
      const float EPSILON = 0.001f;
      float move_speed_seconds = seconds_per_move * nodes;

      while (1f - percent > EPSILON)
      {
        percent += Time.deltaTime / move_speed_seconds;
        percent = Mathf.Clamp01(percent);

        // Take the percent (0-1) and convert it to an index (0 to nodes-1)
        int prev_index = (int)Mathf.Lerp(0, nodes - 1, percent);
        int next_index = Mathf.Clamp(prev_index + 1, 0, nodes - 1);

        // Convert index to percentage amount (1/nodes-1)
        // e.g. 1/3 = 0.333
        // <0.333 = 0-1 convert 0.00-0.32 to (0-1)
        // <0.666 = 1-2 convert 0.33-0.65 to (0-1)
        // <0.999 = 2-3 convert 0.66-0.99 to (0-1)
        float relative_percentage_amount = (percent / percentage_amount) - prev_index;
        float scaled_percentage_amount = Mathf.Clamp(relative_percentage_amount, 0.0f, 1.0f);

        var this_cell = path[prev_index];
        var next_cell = path[next_index];

        var a = Grid.GridSpaceToWorldSpace(this_cell, size);
        var b = Grid.GridSpaceToWorldSpace(next_cell, size);
        go.transform.localPosition = Vector3.Lerp(a, b, scaled_percentage_amount);

        // Rotation?
        //  var dir = (next_cell.transform.position - this_cell.transform.position).normalized;
        //  if (dir.sqrMagnitude > EPSILON)
        //    unit.transform.localRotation = Quaternion.LookRotation(dir);

        await Task.Yield();
      }

      // Done Move
    }
  }

}