using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wiggy
{
  struct Row
  {
    public int depth;
    public float start_slope;
    public float end_slope;
    public int min_col;
    public int max_col;
  }

  struct Tile
  {
    public int depth;
    public int col;
  }

  // heavily based off:
  // https://www.albertford.com/shadowcasting/
  // License: CC0

  static class symmetric_shadowcasting
  {
    // Algorithm

    public static bool is_symmetric(Row row, Tile tile)
    {
      return (tile.col >= row.depth * row.start_slope) &&
             (tile.col <= row.depth * row.end_slope);
    }

    public static Row next(Row r)
    {
      return new Row()
      {
        depth = r.depth + 1,
        start_slope = r.start_slope,
        end_slope = r.end_slope
      };
    }

    public static float slope(Tile t)
    {
      return (2 * t.col - 1) / (2f * t.depth);
    }

    public static int round_up(float n)
    {
      var result = Mathf.FloorToInt(n + 0.5f);
      return result;
    }

    public static int round_down(float n)
    {
      var result = Mathf.CeilToInt(n - 0.5f);
      return result;
    }

    // Gameplay

    public static void mark_visible(ref int[] tiles, int index)
    {
      tiles[index] = 1;
    }

    private static (int, int) transform(Vector2Int origin, int dir, Tile t)
    {
      int row = t.depth;
      int col = t.col;
      if (dir == 0) // north
        return (origin.x + col, origin.y - row);
      if (dir == 2) // south
        return (origin.x + col, origin.y + row);
      if (dir == 1) // east
        return (origin.x + row, origin.y + col);
      if (dir == 3) // west
        return (origin.x - row, origin.y + col);
      throw new System.Exception("Error; should specify a dir");
    }

    private static bool is_wall(map_manager map, int index)
    {
      return map.obstacle_map[index].entities.Contains(EntityType.tile_type_wall);
    }

    private static bool is_floor(map_manager map, int index)
    {
      return map.obstacle_map[index].entities.Contains(EntityType.tile_type_floor);
    }

    public static void compute_fov(int[] tiles, map_manager mm, Vector2Int origin, int x_max)
    {
      for (int i = 0; i < tiles.Length; i++)
        tiles[i] = 0;

      { // Set origin as visible
        var index = Grid.GetIndex(origin, x_max);
        if (index < 0 || index >= tiles.Length)
          return; // invalid origin
        mark_visible(ref tiles, index);
      }

      for (int dir = 0; dir < 4; dir++)
      {
        Row first_row = new()
        {
          depth = 1,
          start_slope = -1.0f,
          end_slope = 1.0f
        };

        Queue<Row> rows = new();
        rows.Enqueue(first_row);

        while (!(rows.Count == 0)) // tiles()
        {
          Row row = rows.Dequeue();
          int min_col = round_up(row.depth * row.start_slope);
          int max_col = round_down(row.depth * row.end_slope);
          Optional<Tile> prev_tile = new();

          for (int i = min_col; i < (max_col + 1); i++)
          {
            Tile tile = new() { depth = row.depth, col = i };

            var (x, y) = transform(origin, dir, tile);
            var curr_tile_index = Grid.GetIndex(x, y, x_max);

            if (curr_tile_index < 0 || curr_tile_index >= tiles.Length)
              continue; // invalid?

            var (px, py) = prev_tile.IsSet ? transform(origin, dir, prev_tile.Data) : (0, 0);
            var prev_tile_index = Grid.GetIndex(px, py, x_max);

            if (is_wall(mm, curr_tile_index) || is_symmetric(row, tile))
              mark_visible(ref tiles, curr_tile_index);

            if (prev_tile.IsSet && is_wall(mm, prev_tile_index) && is_floor(mm, curr_tile_index))
              row.start_slope = slope(tile);

            if (prev_tile.IsSet && is_floor(mm, prev_tile_index) && is_wall(mm, curr_tile_index))
            {
              Row next_row = next(row);
              next_row.end_slope = slope(tile);
              rows.Enqueue(next_row);
            }

            prev_tile.Set(tile);
          }

          { // Another row?

            var (px, py) = prev_tile.IsSet ? transform(origin, dir, prev_tile.Data) : (0, 0);
            var prev_tile_index = Grid.GetIndex(px, py, x_max);

            if (prev_tile.IsSet && is_floor(mm, prev_tile_index))
              rows.Enqueue(next(row));
          }
        }
      }
    }
  }

  public struct fov_system_init
  {
    public Vector2Int fov_pos;
    public GameObject fov_cursor_prefab;
    public GameObject fov_debug_cursor;
    public GameObject fov_holder;
    public GameObject fov_enabled_prefab;
    public GameObject fov_disabled_prefab;
    public int max_dst;
  }


  [System.Serializable]
  class fov_system : ECSSystem
  {
    // Data
    public int[] fov_map { get; private set; }
    public SpriteRenderer[] fov_cursor_map { get; private set; }
    private map_manager map_manager;

    // Unity/UI
    private GameObject fov_holder;
    private GameObject fov_debug_cursor;
    private Color enable_color;
    private Color disable_color;
    private Color out_of_range_color = Color.red;
    private int max_dst;

    public void Start(Wiggy.registry ecs, map_manager mm, fov_system_init init)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      s.Set(ecs.GetComponentType<InstantiatedComponent>());
      ecs.SetSystemSignature<fov_system>(s);

      // Data
      map_manager = mm;
      fov_map = new int[mm.obstacle_map.Length];

      // Unity/UI
      fov_holder = init.fov_holder;
      enable_color = init.fov_enabled_prefab.GetComponentInChildren<SpriteRenderer>().color;
      disable_color = init.fov_disabled_prefab.GetComponentInChildren<SpriteRenderer>().color;
      fov_debug_cursor = Object.Instantiate(init.fov_cursor_prefab);
      max_dst = init.max_dst;

      //
      // Instantiate squares all over, to visiluse the fov
      //
      for (int i = 0; i < fov_map.Length; i++)
      {
        var enabled = fov_map[i] == 1;
        var gpos = Grid.IndexToPos(i, mm.width, mm.height);
        var wpos = Grid.GridSpaceToWorldSpace(gpos, mm.size);
        wpos.y = -0.01f;
        if (enabled)
          Object.Instantiate(init.fov_enabled_prefab, wpos, Quaternion.identity, fov_holder.transform);
        else
          Object.Instantiate(init.fov_disabled_prefab, wpos, Quaternion.identity, fov_holder.transform);
      }
      fov_cursor_map = fov_holder.GetComponentsInChildren<SpriteRenderer>();

      //
      // Do the FOV once on start
      //
      Update(ecs, init.fov_pos); // update once
      var fov_wpos = Grid.GridSpaceToWorldSpace(init.fov_pos, mm.size);
      fov_debug_cursor.transform.position = fov_wpos;
    }

    public void Update(Wiggy.registry ecs, Vector2Int pos)
    {
      Debug.Log("computing fov");
      int x_max = map_manager.width;
      symmetric_shadowcasting.compute_fov(fov_map, map_manager, pos, x_max);

      for (int i = 0; i < fov_map.Length; i++)
      {
        // Post processing (range cutoff)
        var gpos = Grid.IndexToPos(i, x_max, map_manager.height);
        var dst = pos - gpos;
        if (Mathf.Abs(dst.x) > max_dst || Mathf.Abs(dst.y) > max_dst)
          fov_map[i] = 2; // 2 = out of range

        //
        // Update Grid View
        //
        var enabled = fov_map[i] == 1;
        var out_of_range = fov_map[i] == 2;

        fov_cursor_map[i].color = disable_color;
        fov_cursor_map[i].gameObject.SetActive(true);

        if (enabled)
          fov_cursor_map[i].color = enable_color;

        if (out_of_range)
        {
          fov_cursor_map[i].color = out_of_range_color;
          fov_cursor_map[i].gameObject.SetActive(false);
        }

        //
        // Update obstacle view
        //
        for (int iidx = 0; iidx < map_manager.obstacle_map[i].instantiated.Count; iidx++)
        {
          var instance = map_manager.obstacle_map[i].instantiated[iidx];
          instance.SetActive(false);
          if (enabled)
            instance.SetActive(true);
        }
      }

      // Update Cursor
      fov_debug_cursor.transform.position = Grid.GridSpaceToWorldSpace(pos, map_manager.size);

      //
      // Set actors on visible tiles as visible
      //
      foreach (var e in entities.ToArray())
      {
        var p = ecs.GetComponent<GridPositionComponent>(e);
        var i = ecs.GetComponent<InstantiatedComponent>(e);
        var idx = Grid.GetIndex(p.position, x_max);
        var visible = fov_map[idx] == 1;

        i.instance.SetActive(visible);
      }
    }
  }
}