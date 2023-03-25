using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class InstantiateSystem : ECSSystem
  {
    private map_manager map;

    public void Start(Wiggy.registry ecs, map_manager map)
    {
      this.map = map;

      Signature s = new();
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      s.Set(ecs.GetComponentType<ToBeInstantiatedComponent>());
      ecs.SetSystemSignature<InstantiateSystem>(s);
    }

    public void Update(Wiggy.registry ecs)
    {
      // If removing a component from an entity iterate backwards
      foreach (var e in entities.ToArray())
      {
        var p = ecs.GetComponent<GridPositionComponent>(e);
        var r = ecs.GetComponent<ToBeInstantiatedComponent>(e);

        var wpos = Grid.GridSpaceToWorldSpace(p.position, map.size);
        var obj = Object.Instantiate(r.prefab, wpos, Quaternion.identity);
        obj.name = r.name;

        // Done with request
        Debug.Log("Processed instantiate request...");
        ecs.RemoveComponent<ToBeInstantiatedComponent>(e);

        var instance = new InstantiatedComponent();
        instance.instance = obj;
        ecs.AddComponent(e, instance);
      }
    }
  }
}