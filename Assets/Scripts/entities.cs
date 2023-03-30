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
  }
}