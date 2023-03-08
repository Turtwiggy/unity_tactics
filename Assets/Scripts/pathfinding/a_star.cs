using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public enum square_direction
  {
    N, E, S, W
  }

  public static class square_direction_extensions
  {
    public static square_direction Opposite(this square_direction sd)
    {
      if (sd == square_direction.N)
        return square_direction.S;
      if (sd == square_direction.S)
        return square_direction.N;
      if (sd == square_direction.E)
        return square_direction.W;
      if (sd == square_direction.W)
        return square_direction.E;
      return sd;
    }
  }

  [System.Serializable]
  public class cell
  {
    // public cell[] neighbours;
    public Vector2Int pos = new(-1, -1);
    public int path_cost = 0; // make all passable by default
    public int distance = -1;
  }

  public static class a_star
  {
    // Generate path from a to b
    public static cell[] generate_direct(cell[] map, int from_idx, int to_idx, int x_max)
    {
      var from = map[from_idx];
      var to = map[to_idx];

      var frontier = new PriorityQueue<cell>();
      frontier.Enqueue(from, 0);
      var came_from = new Dictionary<cell, cell>();
      var cost_so_far = new Dictionary<cell, int>();
      came_from[from] = from;
      cost_so_far[from] = 0;

      while (frontier.Count > 0)
      {
        cell current = frontier.Dequeue();

        if (current.Equals(to))
          return reconstruct_path(came_from, from, to);

        var neighbours = square_neighbour_indicies(current.pos.x, current.pos.y, x_max, x_max);
        for (int i = 0; i < neighbours.Length; i++)
        {
          var neighbour_idx = neighbours[i].Item2;
          var neighbour = map[neighbour_idx];

          int map_cost = neighbour.path_cost;
          if (map_cost == -1)
            continue; // impassable
          int new_cost = cost_so_far[current] + map_cost;
          if (!cost_so_far.ContainsKey(neighbour) || new_cost < cost_so_far[neighbour])
          {
            cost_so_far[neighbour] = new_cost;
            int priority = new_cost + heuristic(neighbour.pos, to.pos);
            frontier.Enqueue(neighbour, priority);
            came_from[neighbour] = current;
          }
        }
      }

      return null;
    }

    // Generate valid cells around a point
    public static cell[] generate_area(cell[] map, int from_idx, int range, int x_max)
    {
      var from = map[from_idx];

      for (int i = 0; i < map.Length; i++)
        map[i].distance = int.MaxValue;
      from.distance = 0;

      var valid_cells = new HashSet<cell>();
      var frontier = new Queue<cell>();
      frontier.Enqueue(from);

      while (frontier.Count > 0)
      {
        var current = frontier.Dequeue();

        // Skip invalid cells
        if (current.distance > range)
          continue;
        valid_cells.Add(current);

        // Check neighbours
        var neighbours = square_neighbour_indicies(current.pos.x, current.pos.y, x_max, x_max);
        for (int i = 0; i < neighbours.Length; i++)
        {
          var neighbour_idx = neighbours[i].Item2;
          var neighbour = map[neighbour_idx];
          int distance = current.distance;
          distance += 1; // default

          if (neighbour.distance == int.MaxValue)
          {
            neighbour.distance = distance;
            frontier.Enqueue(neighbour);
          }
          else if (distance < neighbour.distance)
            neighbour.distance = distance; // update distance, but don't process it twice
        }
      }

      return valid_cells.ToArray();
    }

    // default heuristic
    private static int heuristic(Vector2Int a, Vector2Int b)
    {
      return distance(a, b);
    }

    private static int distance(Vector2Int a, Vector2Int b)
    {
      var x = a.x < b.x ? b.x - a.x : a.x - b.x;
      var y = a.y < b.y ? b.y - a.y : a.y - b.y;
      return x + y;
    }

    private static T[] reconstruct_path<T>(Dictionary<T, T> came_from, T start, T goal)
    {
      T current = goal;

      List<T> path = new();

      while (!current.Equals(start))
      {
        path.Add(current);
        current = came_from[current];
      }

      path.Add(start); // optional
      path.Reverse(); // optional
      return path.ToArray();
    }

    public static (square_direction, int)[] square_neighbour_indicies(int x, int y, int x_max, int y_max)
    {
      int max_idx = x_max * y_max;
      int idx_north = x_max * (y + 1) + x;
      int idx_east = x_max * y + (x + 1);
      int idx_south = x_max * (y - 1) + x;
      int idx_west = x_max * y + (x - 1);
      bool ignore_north = y >= y_max - 1;
      bool ignore_east = x >= x_max - 1;
      bool ignore_south = y <= 0;
      bool ignore_west = x <= 0;

      List<(square_direction, int)> results = new();

      if (!ignore_north && idx_north >= 0 && idx_north < max_idx)
        results.Add((square_direction.N, idx_north));

      if (!ignore_east && idx_east >= 0 && idx_east < max_idx)
        results.Add((square_direction.E, idx_east));

      if (!ignore_south && idx_south >= 0 && idx_south < max_idx)
        results.Add((square_direction.S, idx_south));

      if (!ignore_west && idx_west >= 0 && idx_west < max_idx)
        results.Add((square_direction.W, idx_west));

      return results.ToArray();
    }
  }
}