using System.Collections.Generic;
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

  public enum TileState
  {
    HIDDEN,
    VISIBLE,
    PREVIOUSLY_SEEN,
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

    public static void mark_visible(TileState[] tiles, int index)
    {
      tiles[index] = TileState.VISIBLE;
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

    public static void compute_fov(TileState[] tiles, map_manager mm, Vector2Int origin, int x_max)
    {
      { // Set origin as visible
        var index = Grid.GetIndex(origin, x_max);
        if (index < 0 || index >= tiles.Length)
          return; // invalid origin
        mark_visible(tiles, index);
      }

      //
      // Symmetric Shadowcasting Algorithm
      //

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
              mark_visible(tiles, curr_tile_index);

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

  public class FovSystem : ECSSystem
  {
    public struct FovSystemInit
    {
      public Vector2Int fov_pos;
      public GameObject fov_cursor_prefab;
      public GameObject fov_debug_cursor;
      public GameObject fov_holder;
      public GameObject fov_grid_prefab;
      public int max_dst;
    }

    // Data
    public TileState[] fov_map_live { get; private set; }
    public TileState[] fov_map_mask { get; private set; }
    public SpriteRenderer[] fov_cursor_map { get; private set; }
    private map_manager map_manager;

    // Unity/UI
    private GameObject fov_holder;
    private GameObject fov_debug_cursor;
    private Color enable_color = Color.white;
    private Color disable_color = Color.grey;
    private Color previously_seen_colour = Color.yellow;
    private int max_dst;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      s.Set(ecs.GetComponentType<InstantiatedComponent>());
      ecs.SetSystemSignature<FovSystem>(s);
    }

    public void Start(Wiggy.registry ecs, FovSystemInit init)
    {
      map_manager = Object.FindObjectOfType<map_manager>();

      fov_map_live = new TileState[map_manager.obstacle_map.Length];
      for (int i = 0; i < fov_map_live.Length; i++)
        fov_map_live[i] = TileState.HIDDEN;

      fov_map_mask = new TileState[map_manager.obstacle_map.Length];
      for (int i = 0; i < fov_map_mask.Length; i++)
        fov_map_mask[i] = TileState.HIDDEN;

      // Unity/UI
      fov_holder = init.fov_holder;
      fov_debug_cursor = Object.Instantiate(init.fov_cursor_prefab);
      max_dst = init.max_dst;

      //
      // Instantiate squares all over, to visulize the fov
      //
      for (int i = 0; i < fov_map_live.Length; i++)
      {
        var gpos = Grid.IndexToPos(i, map_manager.width, map_manager.height);
        var wpos = Grid.GridSpaceToWorldSpace(gpos, map_manager.size);
        Object.Instantiate(init.fov_grid_prefab, wpos, Quaternion.identity, fov_holder.transform);
      }
      fov_cursor_map = fov_holder.GetComponentsInChildren<SpriteRenderer>();

      //
      // Do the FOV once on start
      //
      Update(ecs, init.fov_pos); // update once...
      var fov_wpos = Grid.GridSpaceToWorldSpace(init.fov_pos, map_manager.size);
      fov_debug_cursor.transform.position = fov_wpos;
    }

    public void Update(Wiggy.registry ecs, Vector2Int pos)
    {
      Debug.Log("computing fov");
      int x_max = map_manager.width;

      // The live view from the position
      for (int i = 0; i < fov_map_live.Length; i++)
        fov_map_live[i] = TileState.HIDDEN;
      symmetric_shadowcasting.compute_fov(fov_map_live, map_manager, pos, x_max);

      // Combine the live view and the mask
      for (int i = 0; i < fov_map_mask.Length; i++)
      {
        var live_view_state = fov_map_live[i];
        var mask_view_state = fov_map_mask[i];

        var dst = pos - Grid.IndexToPos(i, x_max, map_manager.height);
        bool in_range = Mathf.Abs(dst.x) <= max_dst && Mathf.Abs(dst.y) <= max_dst;

        //
        // Update the mask based on the live state
        //

        // visible conditions
        bool v0 = live_view_state == TileState.VISIBLE;
        bool v1 = in_range;
        if (v0 && v1)
          fov_map_mask[i] = TileState.VISIBLE;

        // hidden conditions
        bool h0 = live_view_state == TileState.HIDDEN;
        bool h1 = !in_range;
        bool h2 = mask_view_state != TileState.PREVIOUSLY_SEEN;
        if (h0 && h1 && h2)
          fov_map_mask[i] = TileState.HIDDEN;

        // previously seen conditions
        bool p0 = live_view_state == TileState.HIDDEN;
        bool p1 = mask_view_state == TileState.VISIBLE;
        if (p0 && p1)
          fov_map_mask[i] = TileState.PREVIOUSLY_SEEN;

        //
        // Update Grid View
        //
        fov_cursor_map[i].color = disable_color;
        fov_cursor_map[i].gameObject.SetActive(false);

        if (fov_map_mask[i] == TileState.VISIBLE)
        {
          fov_cursor_map[i].gameObject.SetActive(true);
          fov_cursor_map[i].color = enable_color;
        }
        if (fov_map_mask[i] == TileState.HIDDEN)
        {
          fov_cursor_map[i].gameObject.SetActive(false);
          fov_cursor_map[i].color = disable_color;
        }
        if (fov_map_mask[i] == TileState.PREVIOUSLY_SEEN)
        {
          fov_cursor_map[i].gameObject.SetActive(true);
          fov_cursor_map[i].color = previously_seen_colour;
        }

        //
        // Update obstacle view
        // It's a for loop because there could be multiple instantiated items?
        //

        for (int iidx = 0; iidx < map_manager.obstacle_map[i].instantiated.Count; iidx++)
        {
          var instance = map_manager.obstacle_map[i].instantiated[iidx];

          instance.SetActive(false);
          // settings.object_when_was_seen.SetActive(false);
          // settings.object_when_hidden.SetActive(false);

          if (fov_map_mask[i] == TileState.VISIBLE)
            instance.SetActive(true);

          // if (fov_map_mask[i] == TileState.HIDDEN)
          //   settings.object_when_hidden.SetActive(false);

          // if (fov_map_mask[i] == TileState.PREVIOUSLY_SEEN)
          //   settings.object_when_was_seen.SetActive(true);
        }
      }

      // Update Cursor
      fov_debug_cursor.transform.position = Grid.GridSpaceToWorldSpace(pos, map_manager.size);

      //
      // Set actors on visible tiles as visible
      //
      foreach (var e in entities)
      {
        var p = ecs.GetComponent<GridPositionComponent>(e);
        var i = ecs.GetComponent<InstantiatedComponent>(e);
        var idx = Grid.GetIndex(p.position, x_max);
        i.instance.SetActive(fov_map_mask[idx] == TileState.VISIBLE);
      }

      //
    }
  }
}