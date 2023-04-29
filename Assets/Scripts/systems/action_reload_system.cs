using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class ReloadSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<WantsToReload>());
      s.Set(ecs.GetComponentType<AmmoComponent>());
      ecs.SetSystemSignature<ReloadSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {

    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var action = new Reload();

        if (!ActionHelpers.Valid<WantsToReload>(ecs, e, action))
          continue;

        ref var ammo = ref ecs.GetComponent<AmmoComponent>(e);

        ammo.cur = ammo.max;
        Debug.Log("entity reloaded");

        ActionHelpers.Complete<WantsToReload>(ecs, e, action);
      }
    }
  }
}