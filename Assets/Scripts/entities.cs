using UnityEngine;

namespace Wiggy
{
  using Entity = System.Int32;

  public static class Entities
  {
    public static Entity create_unit(Wiggy.registry ecs, GameObject prefab, Vector2Int spot, string name)
    {
      var e = ecs.Create();

      GridPositionComponent gpc = new();
      gpc.position = spot;
      ecs.AddComponent(e, gpc);

      ToBeInstantiatedComponent tbic = new();
      tbic.prefab = prefab;
      tbic.name = name;
      ecs.AddComponent(e, tbic);

      return e;
    }

    public static Entity create_cursor(Wiggy.registry ecs, GameObject prefab)
    {
      var e = ecs.Create();

      GridPositionComponent gpc = new();
      gpc.position = new Vector2Int(0, 0);
      ecs.AddComponent(e, gpc);

      CursorComponent cc = new();
      ecs.AddComponent(e, cc);

      ToBeInstantiatedComponent tbi = new();
      tbi.prefab = prefab;
      tbi.name = "cursor";
      ecs.AddComponent(e, tbi);

      return e;
    }

  }
}