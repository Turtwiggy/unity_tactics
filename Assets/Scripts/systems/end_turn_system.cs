
namespace Wiggy
{
  public class EndTurnSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      ecs.SetSystemSignature<EndTurnSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {

    }

    public void Update(Wiggy.registry ecs)
    {
      UnityEngine.Debug.Log("ending turn");

      foreach (var e in entities)
      {
        var actions = ecs.GetComponent<ActionsComponent>(e);
        actions.done.Clear();
      }
    }
  }
}