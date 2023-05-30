using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  public class map_visual_manager : MonoBehaviour
  {
    private map_manager map;

    [SerializeField]
    private MaterialPropertyBlock[] property;

    public void DoStart()
    {
      // RefreshVisuals(); // editor driven
    }

    public void RefreshVisuals()
    {
      map = FindObjectOfType<map_manager>();

      property = new MaterialPropertyBlock[map.obstacle_map.Length];
      for (int i = 0; i < property.Length; i++)
        property[i] = new();

      // Update all visualz innit
      for (int i = 0; i < map.obstacle_map.Length; i++)
      {
        var pos = Grid.IndexToPos(i, map.width, map.height);
        var neighbour_idxs = a_star.square_neighbour_indicies_with_diagonals(pos.x, pos.y, map.width, map.height);

        // broken
        List<GameObject> obstacles = new(); // map.obstacle_map[i].instantiated;
        if (obstacles.Count == 0)
          continue;
        var obstacle = obstacles[0];

        int wall_mask = 0;
        int floor_mask = 0;
        for (int idx = 0; idx < neighbour_idxs.Length; idx++)
        {
          var (dir, index) = neighbour_idxs[idx];

          var entities = map.obstacle_map[index].entities;
          bool is_wall = entities.Contains(EntityType.tile_type_wall);
          bool is_floor = entities.Contains(EntityType.tile_type_floor);

          if (!dir.IsDiagonal()) // ignore diags for walls
            wall_mask += is_wall ? ((int)dir) : 0;

          if (dir.IsDiagonal()) // only diags for floors
            floor_mask += is_floor ? ((int)dir) : 0;
        }

        var (sprite_x, sprite_y) = GetWallSprite(wall_mask, floor_mask);
        var r = obstacle.GetComponent<Renderer>();
        property[i].SetInt("_SpriteX", sprite_x);
        property[i].SetInt("_SpriteY", sprite_y);
        r.SetPropertyBlock(property[i]);
      }

      Debug.Log("done refreshing visuals");
    }

    private (int, int) GetWallSprite(int wall_mask, int floor_mask)
    {
      int sprite_x = 4;
      int sprite_y = 0;

      switch (wall_mask)
      {
        // 0s: pillars
        case 0:
          break;

        // 1s: deadends
        case 1: // N
          sprite_x = 0;
          sprite_y = 24;
          break;
        case 2: // E
          sprite_x = 4;
          sprite_y = 24;
          break;
        case 4: // S
          sprite_x = 6;
          sprite_y = 24;
          break;
        case 8: // W
          sprite_x = 2;
          sprite_y = 24;
          break;

        // 2s: opposites
        case 5: // NS
          sprite_x = 10;
          sprite_y = 24;
          break;
        case 10: // WE
          sprite_x = 8;
          sprite_y = 24;
          break;

        // 2s: corners
        case 3: // NE
          sprite_x = 18;
          sprite_y = 2;
          break;
        case 12: // SW
          sprite_x = 20;
          sprite_y = 0;
          break;
        case 6: // ES
          sprite_x = 18;
          sprite_y = 0;
          break;
        case 9: // NW
          sprite_x = 20;
          sprite_y = 2;
          break;

        // 3s
        case 7: // NES
          sprite_x = 18;
          sprite_y = 1;
          break;
        case 11: // NEW
          sprite_x = 19;
          sprite_y = 2;
          break;
        case 13: // NSW
          sprite_x = 20;
          sprite_y = 1;
          break;
        case 14: // WSE
          sprite_x = 19;
          sprite_y = 0;
          break;

        // all four walls connected
        case 15:
          sprite_x = 0;
          sprite_y = 0;

          // Handle floor corner cases
          switch (floor_mask)
          {
            case 16: // ne
              sprite_x = 18;
              sprite_y = 4;
              break;
            case 32: // se
              sprite_x = 18;
              sprite_y = 3;
              break;
            case 64: // sw
              sprite_x = 19;
              sprite_y = 3;
              break;
            case 128: // nw
              sprite_x = 19;
              sprite_y = 4;
              break;
          }
          break;

        default:
          break;
      }

      return (sprite_x, sprite_y);
    }

    public void DoUpdate()
    {

    }
  }

}