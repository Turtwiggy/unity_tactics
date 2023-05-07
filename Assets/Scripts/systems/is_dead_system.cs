using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class IsDeadSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<IsDeadComponent>());
      ecs.SetSystemSignature<IsDeadSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities)
      {
        Debug.Log("something died!");

        var dead = ecs.GetComponent<IsDeadComponent>(e);

        // Remove unity record
        InstantiatedComponent backup = default;
        var instance = ecs.TryGetComponent(e, ref backup);
        bool has_instance = !instance.Equals(backup);
        if (has_instance)
          Object.Destroy(instance.instance);

        // Remove ecs record
        ecs.Destroy(e);
      }

    }
  }
}