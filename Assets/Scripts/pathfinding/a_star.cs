using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public enum square_direction
  {
    N = 1,
    E = 2,
    S = 4,
    W = 8,
    NE = 16,
    SE = 32,
    SW = 64,
    NW = 128,
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

      if (sd == square_direction.NE)
        return square_direction.SW;
      if (sd == square_direction.SE)
        return square_direction.NW;
      if (sd == square_direction.SW)
        return square_direction.NE;
      if (sd == square_direction.NW)
        return square_direction.SE;

      return sd;
    }

    public static bool InQuadrant(this square_direction a, square_direction desired)
    {
      if (desired == square_direction.N)
        return a == square_direction.N || a == square_direction.NE || a == square_direction.NW;
      if (desired == square_direction.S)
        return a == square_direction.S || a == square_direction.SE || a == square_direction.SW;
      if (desired == square_direction.E)
        return a == square_direction.E || a == square_direction.NE || a == square_direction.SE;
      if (desired == square_direction.W)
        return a == square_direction.W || a == square_direction.NW || a == square_direction.SW;
      return false;
    }

    public static bool IsDiagonal(this square_direction d)
    {
      bool is_diagonal = false;
      is_diagonal |= d == square_direction.NE;
      is_diagonal |= d == square_direction.SE;
      is_diagonal |= d == square_direction.SW;
      is_diagonal |= d == square_direction.NW;
      return is_diagonal;
    }
  }

  public class astar_cell
  {
    public Vector2Int pos;
    public int path_cost = 1;
    public int distance = int.MaxValue;
  }

  public static class a_star
  {
    // Generate path from a to b
    public static astar_cell[] generate_direct(astar_cell[] map, int from_idx, int to_idx, int x_max)
    {
      var from = map[from_idx];
      var to = map[to_idx];
      var came_from = new Dictionary<astar_cell, astar_cell>();
      var cost_so_far = new Dictionary<astar_cell, int>();

      var frontier = new PriorityQueue<astar_cell>();
      frontier.Enqueue(from, 0);

      came_from[from] = from;
      cost_so_far[from] = 0;

      while (frontier.Count > 0)
      {
        var current = frontier.Dequeue();

        if (current.Equals(to))
          return reconstruct_path(came_from, from, to);

        var neighbours = square_neighbour_indicies(current.pos.x, current.pos.y, x_max, x_max);
        for (int i = 0; i < neighbours.Length; i++)
        {
          var neighbour_idx = neighbours[i].Item2;
          var neighbour = map[neighbour_idx];
          var neighbour_cost = neighbour.path_cost;

          if (neighbour_cost == -1)
            continue; // impassable

          int new_cost = cost_so_far[current] + neighbour_cost;
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

    // Generate path from a to b 
    public static astar_cell[] generate_direct_with_diagonals(astar_cell[] map, int from_idx, int to_idx, int x_max, bool obstacles = true)
    {
      var from = map[from_idx];
      var to = map[to_idx];

      var frontier = new PriorityQueue<astar_cell>();
      frontier.Enqueue(from, 0);
      var came_from = new Dictionary<astar_cell, astar_cell>();
      var cost_so_far = new Dictionary<astar_cell, int>();
      came_from[from] = from;
      cost_so_far[from] = 0;

      while (frontier.Count > 0)
      {
        var current = frontier.Dequeue();

        if (current.Equals(to))
          return reconstruct_path(came_from, from, to);

        var neighbours = square_neighbour_indicies_with_diagonals(current.pos.x, current.pos.y, x_max, x_max);
        for (int i = 0; i < neighbours.Length; i++)
        {
          var neighbour_idx = neighbours[i].Item2;
          var neighbour = map[neighbour_idx];

          int map_cost = neighbour.path_cost;

          if (!obstacles) // ignore obstacles
            map_cost = 1;

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
    public static astar_cell[] generate_accessible_areas(astar_cell[] map, int from_idx, int range, int x_max, int y_max)
    {
      var from = map[from_idx];
      from.distance = 0;

      var visible_cell = new HashSet<astar_cell>();
      var frontier = new Queue<astar_cell>();
      frontier.Enqueue(from);

      while (frontier.Count > 0)
      {
        var current = frontier.Dequeue();
        visible_cell.Add(current);

        // Check neighbours
        var neighbours = square_neighbour_indicies(current.pos.x, current.pos.y, x_max, y_max);
        for (int i = 0; i < neighbours.Length; i++)
        {
          var neighbour_idx = neighbours[i].Item2;
          var neighbour = map[neighbour_idx];

          if (neighbour.path_cost == -1)
            continue; // skip

          int distance = current.distance + 1;

          if (distance > range)
            continue; // skip

          if (neighbour.distance == int.MaxValue)
          {
            neighbour.distance = distance;
            frontier.Enqueue(neighbour);
          }
          else if (distance < neighbour.distance)
            neighbour.distance = distance; // update distance, but don't process it twice
        }
      }

      return visible_cell.ToArray();
    }

    public static List<Vector2Int> convert_to_points(astar_cell[] results)
    {
      List<Vector2Int> spots = new();
      for (int i = 0; i < results.Length; i++)
        spots.Add(results[i].pos);
      return spots;
    }

    // default heuristic
    private static int heuristic(Vector2Int point, Vector2Int end)
    {
      return distance(point, end);
    }

    private static int distance(Vector2Int a, Vector2Int b)
    {
      return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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

    public static (square_direction, int)[] square_neighbour_indicies_with_diagonals(int x, int y, int x_max, int y_max)
    {
      int max_idx = x_max * y_max;
      int idx_north = x_max * (y + 1) + x;
      int idx_east = x_max * y + (x + 1);
      int idx_south = x_max * (y - 1) + x;
      int idx_west = x_max * y + (x - 1);
      int idx_north_east = idx_north + 1;
      int idx_north_west = idx_north - 1;
      int idx_south_east = idx_south + 1;
      int idx_south_west = idx_south - 1;

      bool ignore_north = y >= y_max - 1;
      bool ignore_east = x >= x_max - 1;
      bool ignore_south = y <= 0;
      bool ignore_west = x <= 0;
      bool ignore_north_east = ignore_north | ignore_east;
      bool ignore_south_east = ignore_south | ignore_east;
      bool ignore_south_west = ignore_south | ignore_west;
      bool ignore_north_west = ignore_north | ignore_west;

      List<(square_direction, int)> results = new();

      bool in_bounds(int x)
      {
        return x >= 0 && x < max_idx;
      };

      if (!ignore_north && in_bounds(idx_north))
        results.Add((square_direction.N, idx_north));

      if (!ignore_east && in_bounds(idx_east))
        results.Add((square_direction.E, idx_east));

      if (!ignore_south && in_bounds(idx_south))
        results.Add((square_direction.S, idx_south));

      if (!ignore_west && in_bounds(idx_west))
        results.Add((square_direction.W, idx_west));

      if (!ignore_north_east && in_bounds(idx_north_east))
        results.Add((square_direction.NE, idx_north_east));

      if (!ignore_south_east && in_bounds(idx_south_east))
        results.Add((square_direction.SE, idx_south_east));

      if (!ignore_south_west && in_bounds(idx_south_west))
        results.Add((square_direction.SW, idx_south_west));

      if (!ignore_north_west && in_bounds(idx_north_west))
        results.Add((square_direction.NW, idx_north_west));

      return results.ToArray();
    }
  }
}