
namespace Wiggy
{
  public class EndTurnSystem : ECSSystem
  {
    public bool overwatch_needs_to_process_that_ai_ended_turn { get; private set; }

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<ActionsComponent>());
      s.Set(ecs.GetComponentType<TeamComponent>());
      ecs.SetSystemSignature<EndTurnSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      overwatch_needs_to_process_that_ai_ended_turn = false;
    }

    public void ClearActions(Wiggy.registry ecs, Team team)
    {
      foreach (var e in entities)
      {
        ref var actions = ref ecs.GetComponent<ActionsComponent>(e);
        var t = ecs.GetComponent<TeamComponent>(e);

        if (t.team == team)
          actions.done.Clear();
      }
    }

    public void EndPlayerTurn(Wiggy.registry ecs)
    {
      UnityEngine.Debug.Log("ending turn... (clearing allll actions)");
      ClearActions(ecs, Team.PLAYER);
    }

    public void EndAiTurn(Wiggy.registry ecs)
    {
      ClearActions(ecs, Team.ENEMY);
      overwatch_needs_to_process_that_ai_ended_turn = true;
    }
  }
}