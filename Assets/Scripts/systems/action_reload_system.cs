using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class ReloadSystem : ECSSystem
  {
    private GameObject vfx_reload;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<WantsToReload>());
      s.Set(ecs.GetComponentType<AmmoComponent>());
      ecs.SetSystemSignature<ReloadSystem>(s);
    }

    public void Start(Wiggy.registry ecs, GameObject vfx_reload)
    {
      this.vfx_reload = vfx_reload;
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var action = new Reload();

        if (!ActionHelpers.Valid<WantsToReload>(ecs, e, action))
          continue;

        ref var ammo = ref ecs.GetComponent<AmmoComponent>(e);
        var pos = ecs.GetComponent<GridPositionComponent>(e);

        ammo.cur = ammo.max;
        Debug.Log("entity reloaded");

        // Reload Effect
        Entities.create_effect(ecs, pos.position, vfx_reload, "Reload Effect");

        ActionHelpers.Complete<WantsToReload>(ecs, e, action);
      }
    }
  }
}