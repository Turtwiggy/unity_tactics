
namespace Wiggy
{
  public class EndTurnSystem : ECSSystem
  {
    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<TeamComponent>());
      ecs.SetSystemSignature<EndTurnSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {

    }

    public void Update(Wiggy.registry ecs)
    {
      UnityEngine.Debug.Log("ending turn... (clearing allll actions)");

      foreach (var e in entities)
      {
        var actions = ecs.GetComponent<ActionsComponent>(e);
        var team = ecs.GetComponent<TeamComponent>(e);

        if (team.team == Team.PLAYER)
          actions.done.Clear();
      }
    }

    public void EndAiTurn(Wiggy.registry ecs)
    {
      foreach (var e in entities)
      {
        var actions = ecs.GetComponent<ActionsComponent>(e);
        var team = ecs.GetComponent<TeamComponent>(e);

        if (team.team == Team.ENEMY)
          actions.done.Clear();
      }
    }

  }
}