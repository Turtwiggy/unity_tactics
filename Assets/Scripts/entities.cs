using UnityEngine;

namespace Wiggy
{
  public static class Entities
  {
    public static Entity create_player(Wiggy.registry ecs, Vector2Int spot, string name, Optional<GameObject> prefab)
    {
      var e = ecs.Create();

      GridPositionComponent gpc = new();
      gpc.position = spot;
      ecs.AddComponent(e, gpc);

      if (prefab.IsSet)
      {
        ToBeInstantiatedComponent tbic = new();
        tbic.prefab = prefab.Data;
        tbic.name = name;
        ecs.AddComponent(e, tbic);
      }

      ActionsComponent actions = new();
      actions.allowed_actions_per_turn = 2;
      actions.done = new();
      ecs.AddComponent(e, actions);

      HealthComponent health = new();
      health.max = 100;
      health.cur = health.max;
      ecs.AddComponent(e, health);

      TargetsComponent targets = new();
      targets.targets = new();
      ecs.AddComponent(e, targets);

      AmmoComponent ammo = new();
      ammo.max = 100;
      ammo.cur = ammo.max;
      ecs.AddComponent(e, ammo);

      WeaponComponent weapon = new();
      weapon.min_range = 0;
      weapon.max_range = 3;
      weapon.damage = 100;
      ecs.AddComponent(e, weapon);

      TeamComponent team = new();
      team.team = Team.PLAYER;
      ecs.AddComponent(e, team);

      PlayerComponent player = new();
      ecs.AddComponent(e, player);

      return e;
    }

    public static Entity create_enemy(Wiggy.registry ecs, Vector2Int spot, string name, Optional<GameObject> prefab)
    {
      var e = ecs.Create();

      GridPositionComponent gpc = new();
      gpc.position = spot;
      ecs.AddComponent(e, gpc);

      if (prefab.IsSet)
      {
        ToBeInstantiatedComponent tbic = new();
        tbic.prefab = prefab.Data;
        tbic.name = name;
        ecs.AddComponent(e, tbic);
      }

      ActionsComponent actions = new();
      actions.allowed_actions_per_turn = 2;
      actions.done = new();
      ecs.AddComponent(e, actions);

      HealthComponent health = new();
      health.max = 10;
      health.cur = health.max;
      ecs.AddComponent(e, health);

      TargetsComponent targets = new();
      targets.targets = new();
      ecs.AddComponent(e, targets);

      AmmoComponent ammo = new();
      ammo.max = 100;
      ammo.cur = ammo.max;
      ecs.AddComponent(e, ammo);

      WeaponComponent weapon = new();
      weapon.min_range = 0;
      weapon.max_range = 3;
      weapon.damage = 10;
      ecs.AddComponent(e, weapon);

      TeamComponent team = new();
      team.team = Team.ENEMY;
      ecs.AddComponent(e, team);

      AIMoveConsiderationComponent move = new();
      move.positions = new();
      ecs.AddComponent(e, move);

      DefaultBrainComponent brain = AiBuilder.BuildDefaultAI();
      ecs.AddComponent(e, brain);

      return e;
    }

    public static Entity create_grenade(Wiggy.registry ecs, Vector2Int spot, string name, Optional<GameObject> prefab)
    {
      var e = ecs.Create();

      GridPositionComponent gpc = new();
      gpc.position = spot;
      ecs.AddComponent(e, gpc);

      if (prefab.IsSet)
      {
        ToBeInstantiatedComponent tbic = new();
        tbic.prefab = prefab.Data;
        tbic.name = name;
        ecs.AddComponent(e, tbic);
      }

      WeaponComponent weapon = new();
      weapon.damage = 5;
      weapon.min_range = 0;
      weapon.max_range = 1;
      ecs.AddComponent(e, weapon);

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

    public static Entity create_effect(Wiggy.registry ecs, Vector2Int spot, GameObject prefab, string name)
    {
      var e = ecs.Create();

      ToBeInstantiatedComponent effect = new()
      {
        name = name,
        prefab = prefab
      };
      ecs.AddComponent(e, effect);

      GridPositionComponent effect_position = new()
      {
        position = spot
      };
      ecs.AddComponent(e, effect_position);

      return e;
    }


  }
}