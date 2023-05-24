using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class MonitorParticleEffectSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<InstantiatedComponent>());
      s.Set(ecs.GetComponentType<ParticleEffectComponent>());
      ecs.SetSystemSignature<MonitorParticleEffectSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly as modified
      {
        var instance = ecs.GetComponent<InstantiatedComponent>(e);

        // Unity
        var eff = instance.instance.GetComponent<effects>();

        if (eff.IsDone)
        {
          Debug.Log("destroying particles");

          // cleanup unity gameobject
          Object.Destroy(instance.instance);

          // cleanup entity
          ecs.Destroy(e);
        }
      }
    }
  }
}