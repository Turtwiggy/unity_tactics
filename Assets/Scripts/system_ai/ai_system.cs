using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  public class AiSystem : ECSSystem
  {
    public void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<DefaultBrainComponent>());
      ecs.SetSystemSignature<AiSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {

    }

    public void Update(Wiggy.registry ecs)
    {
      Debug.Log("decising best action for entity...");

      foreach (var e in entities)
      {
        var brain = ecs.GetComponent<DefaultBrainComponent>(e);

        Optional<Action> action = Reasoner.Evaluate(brain);

        if (action.IsSet)
        {
          Action a = action.Data;
          Debug.Log(string.Format("EID: {0} decided: {1}", e.id, a.GetType().ToString()));
        }
        else
        {
          Debug.Log("Ai brain cannot take any actions");
        }
      }
    }
  }
}