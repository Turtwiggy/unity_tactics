using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class GrenadeSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<WantsToGrenade>());
      ecs.SetSystemSignature<GrenadeSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      //
    }

    public void Update(Wiggy.registry ecs)
    {
      Entity[] array = entities.ToArray(); // readonly because this is modified
      for (int i = 0; i < array.Length; i++)
      {
        var action = new Grenade();

        Entity e = array[i];
        if (!ActionHelpers.Valid<WantsToGrenade>(ecs, e, action))
          continue;

        // TODO: Implement grenade request
        Debug.Log("implement grenade request");

        // Request is processed
        ActionHelpers.Complete<WantsToGrenade>(ecs, e, action);
      }
    }
  }

}