using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class UseItemSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<WantsToUse>());
      ecs.SetSystemSignature<UseItemSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly because this is modified
      {
        var request = ecs.GetComponent<WantsToUse>(e);
      }
    }
  }
}