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
        var neighbour_idxs = a_star.square_neighbour_indicies(pos.x, pos.y, map.width, map.height);
        var obstacles = map.obstacle_map[i].instantiated;

        if (i == 98)
          Debug.Log("wait!");

        if (obstacles.Count == 0)
          continue;
        var obstacle = obstacles[0];

        int mask = 0;
        for (int idx = 0; idx < neighbour_idxs.Length; idx++)
        {
          var (dir, index) = neighbour_idxs[idx];

          var entities = map.obstacle_map[index].entities;
          bool is_wall = entities.Contains(EntityType.tile_type_wall);
          bool is_floor = entities.Contains(EntityType.tile_type_floor);

          mask += is_wall ? ((int)dir) : 0;
        }

        var (sprite_x, sprite_y) = GetWallSprite(mask);
        var script = obstacle.GetComponent<obstacle>();
        var active = script.object_when_active;

        var r = active.GetComponent<Renderer>();
        property[i].SetInt("_SpriteX", sprite_x);
        property[i].SetInt("_SpriteY", sprite_y);
        r.SetPropertyBlock(property[i]);
      }

      Debug.Log("done refreshing visuals");
    }

    private (int, int) GetWallSprite(int mask)
    {
      int sprite_x = 0;
      int sprite_y = 15;

      switch (mask)
      {
        // 0s: pillars
        case 0:
          break;

        // 1s: ugly: no sprite matching this case perfectly
        case 1: // N
        case 2: // E
        case 4: // S
        case 8: // W
          sprite_x = 0;
          sprite_y = 15;
          break;

        // 2s: ugly: to no sprites matching these either
        case 3: // NE
        case 12: // SW
          sprite_x = 0;
          sprite_y = 15;
          break;

        // 2s: pretty cases
        case 5:
          sprite_x = 18;
          sprite_y = 2;
          break;
        case 6:
          sprite_x = 18;
          sprite_y = 0;
          break;
        case 9:
          sprite_x = 20;
          sprite_y = 2;
          break;
        case 10:
          sprite_x = 20;
          sprite_y = 0;
          break;

        // 3s
        case 7: // nse
          sprite_x = 18;
          sprite_y = 1;
          break;
        case 11: // nsw
          sprite_x = 20;
          sprite_y = 1;
          break;
        case 13: // new
          sprite_x = 19;
          sprite_y = 2;
          break;
        case 14: // sew
          sprite_x = 19;
          sprite_y = 0;
          break;

        // all four walls connected
        case 15:
          sprite_x = 0;
          sprite_y = 0;
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