using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class IsDeadSystem : ECSSystem
  {
    private UnitSpawnSystem units;
    private map_manager map;
    private GameObject vfx_death;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<IsDeadComponent>());
      ecs.SetSystemSignature<IsDeadSystem>(s);
    }

    public void Start(Wiggy.registry ecs, UnitSpawnSystem uss, GameObject vfx_death)
    {
      units = uss;
      map = GameObject.FindObjectOfType<map_manager>();
      this.vfx_death = vfx_death;
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray())
      {
        Debug.Log("something died!");

        var dead = ecs.GetComponent<IsDeadComponent>(e);

        // Remove unity record
        InstantiatedComponent backup = default;
        var instance = ecs.TryGetComponent(e, ref backup);
        bool has_instance = !instance.Equals(backup);
        if (has_instance)
          Object.Destroy(instance.instance);

        // Remove units record
        var pos = ecs.GetComponent<GridPositionComponent>(e);
        var idx = Grid.GetIndex(pos.position.x, pos.position.y, map.width);
        units.units[idx].Reset();

        // Death Effects
        Entities.create_effect(ecs, pos.position, vfx_death, "Death Effect");

        // Remove ecs record
        ecs.Destroy(e);
      }
    }
  }
}