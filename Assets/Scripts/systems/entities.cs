using UnityEngine;

namespace Wiggy
{
  public static class Entities
  {
    public static Entity create_player(Wiggy.registry ecs, GameObject prefab, Vector2Int spot, string name)
    {
      var e = ecs.Create();

      GridPositionComponent gpc = new();
      gpc.position = spot;
      ecs.AddComponent(e, gpc);

      ToBeInstantiatedComponent tbic = new();
      tbic.prefab = prefab;
      tbic.name = name;
      ecs.AddComponent(e, tbic);

      ActionsComponent actions = new();
      actions.allowed_actions_per_turn = 2;
      actions.done = new();
      actions.requested = new();
      ecs.AddComponent(e, actions);

      PlayerComponent player = new();
      ecs.AddComponent(e, player);

      return e;
    }

    public static Entity create_enemy(Wiggy.registry ecs, GameObject prefab, Vector2Int spot, string name)
    {
      var e = ecs.Create();

      GridPositionComponent gpc = new();
      gpc.position = spot;
      ecs.AddComponent(e, gpc);

      ToBeInstantiatedComponent tbic = new();
      tbic.prefab = prefab;
      tbic.name = name;
      ecs.AddComponent(e, tbic);

      ActionsComponent actions = new();
      actions.allowed_actions_per_turn = 2;
      actions.done = new();
      actions.requested = new();
      ecs.AddComponent(e, actions);

      HealthComponent health = new();
      health.max = 100;
      health.cur = 100;
      ecs.AddComponent(e, health);

      AmmoComponent ammo = new();
      ammo.max = 100;
      ammo.cur = 100;
      ecs.AddComponent(e, ammo);

      DefaultBrainComponent brain = AiBuilder.BuildDefaultAI(ecs, e);
      ecs.AddComponent(e, brain);

      return e;
    }

    public static Entity create_cursor(Wiggy.registry ecs, GameObject prefab)
    {
      var e = ecs.Create();

      GridPositionComponent gpc = new();
      gpc.position = new Vector2Int(0, 0);
      ecs.AddComponent(e, gpc);

      CursorComponent cc = new();
      ecs.AddComponent(e, cc);

      ToBeInstantiatedComponent tbi = new();
      tbi.prefab = prefab;
      tbi.name = "cursor";
      ecs.AddComponent(e, tbi);

      return e;
    }

  }
}