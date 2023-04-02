using System.Collections;
using UnityEngine;

namespace Wiggy
{
  using Entity = System.Int32;

  public class UnitSpawnSystem : ECSSystem
  {
    public struct UnitSpawnSystemInit
    {
      public GameObject player_prefab;
      public GameObject enemy_prefab;
    }

    // x * y representation
    public Optional<Entity>[] units { get; private set; }

    private map_manager map;

    private void CreateUnit(Wiggy.registry ecs, GameObject prefab, Vector2Int gpos, string name)
    {
      var idx = Grid.GetIndex(gpos, map.width);
      var pos = Grid.IndexToPos(idx, map.width, map.height);
      var enemy = Entities.create_unit(ecs, prefab, pos, name);
      units[idx] = new Optional<Entity>(enemy);
    }

    public void Start(Wiggy.registry ecs, UnitSpawnSystemInit data)
    {


      map = Object.FindObjectOfType<map_manager>();

      units = new Optional<Entity>[map.width * map.height];
      for (int i = 0; i < map.width * map.height; i++)
        units[i] = new Optional<Entity>();

      // set players at start spots
      CreateUnit(ecs, data.player_prefab, map.srt_spots[0], "Wiggy");
      CreateUnit(ecs, data.player_prefab, map.srt_spots[1], "Wallace");
      CreateUnit(ecs, data.player_prefab, map.srt_spots[2], "Sherbert");
      CreateUnit(ecs, data.player_prefab, map.srt_spots[3], "Grunbo");

      // TODO: map_gen_items_and_enemies
      var voronoi_map = map.voronoi_map;
      var voronoi_zones = map.voronoi_zones;
      foreach (IndexList zone_idxs in voronoi_zones)
      {
        foreach (int idx in zone_idxs.idxs)
        {
          // TODO: checks
          // In obstacle?
          // Player unit in zone?

          CreateUnit(ecs, data.enemy_prefab, Grid.IndexToPos(idx, map.width, map.height), "Random Enemy");
          break;
        }
      }
    }

    public void Update()
    {

    }
  }
}