using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{

  // inspired by:
  // https://github.com/scikit-image/scikit-image/blob/main/skimage/draw/_draw.pyx

  public static class line_algorithm
  {
    public static List<(int, int)> create(int r0, int c0, int r1, int c1)
    {
      List<(int, int)> results = new();

      int r = r0;
      int c = c0;
      int dr = Mathf.Abs(r1 - r0);
      int dc = Mathf.Abs(c1 - c0);
      int sc = c1 - c > 0 ? 1 : -1;
      int sr = r1 - r > 0 ? 1 : -1;

      int steep = 0;
      if (dr > dc)
      {
        steep = 1;
        (r, c) = (c, r);
        (dr, dc) = (dc, dr);
        (sr, sc) = (sc, sr);
      }
      int d = (2 * dr) - dc;

      (int, int) result;
      for (int i = 0; i < dc; i++)
      {
        if (steep == 1)
        {
          result.Item1 = c;
          result.Item2 = r;
          results.Add(result);
        }
        else
        {
          result.Item1 = r;
          result.Item2 = c;
          results.Add(result);
        }

        while (d >= 0)
        {
          r += sr;
          d -= 2 * dc;
        }
        c += sc;
        d += 2 * dr;
      }

      // result.Item1 = r1;
      // result.Item2 = c1;
      // results.Add(result);

      return results;
    }
  }
}