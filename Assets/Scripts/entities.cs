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

      add_weapon_component(ecs, e, EntityType.pistol);

      TeamComponent team = new();
      team.team = Team.PLAYER;
      ecs.AddComponent(e, team);

      DexterityComponent dex = new();
      dex.amount = 4;
      ecs.AddComponent(e, dex);

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

      add_weapon_component(ecs, e, EntityType.shotgun);

      TeamComponent team = new();
      team.team = Team.ENEMY;
      ecs.AddComponent(e, team);

      DexterityComponent dex = new();
      dex.amount = 4;
      ecs.AddComponent(e, dex);

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

      add_weapon_component(ecs, e, EntityType.grenade);

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

    // vfx

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

      ParticleEffectComponent pfe = new();
      ecs.AddComponent(e, pfe);

      return e;
    }

    // weapons

    public static void add_weapon_component(Wiggy.registry ecs, Entity e, EntityType weapon)
    {
      WeaponComponent comp = new();
      comp.min_range = 0;
      comp.max_range = 0;
      comp.damage = 0;

      if (weapon == EntityType.pistol)
      {
        // w.attacks = 1;
        // w.range = 12;
        // w.strength = 4;
        // w.damage = 1;
        comp.display_name = "Pistol";
        comp.min_range = 0;
        comp.max_range = 5;
        comp.damage = 18;
      }
      else if (weapon == EntityType.sniper)
      {
        comp.display_name = "Sniper";
        comp.min_range = 5;
        comp.max_range = 20;
        comp.damage = 15;
      }
      else if (weapon == EntityType.shotgun)
      {
        comp.display_name = "Shotgun";
        comp.min_range = 0;
        comp.max_range = 10;
        comp.damage = 12;
      }
      else if (weapon == EntityType.grenade)
      {
        // str: 3
        // dmg: 1
        // attacks: 1d6
        // range: 6
        comp.display_name = "Grenade";
        comp.min_range = 0;
        comp.max_range = 6;
        comp.damage = 5;
      }
      else if (weapon == EntityType.rifle)
      {
        // attacks: 2
        // range: 18
        // str: 4
        // dmg: 1
        comp.display_name = "Rifle";
        comp.min_range = 2;
        comp.max_range = 15;
        comp.damage = 12;
      }
      else if (weapon == EntityType.sword)
      {
        comp.display_name = "Sword";
        // w.range = -1;
        // w.strength = -1; // user
        // w.damage = 1;
      }
      else
        Debug.LogError("WeaponType not implemented");

      ecs.AddComponent(e, comp);
    }

    // environment

    public static Entity create_barrel(Wiggy.registry ecs, Vector2Int spot, string name, Optional<GameObject> prefab)
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

      BarrelComponent barrel = new();
      ecs.AddComponent(e, barrel);

      TeamComponent team = new();
      team.team = Team.NEUTRAL;
      ecs.AddComponent(e, team);

      return e;
    }

    public static Entity create_trap(Wiggy.registry ecs, Vector2Int spot, string name, Optional<GameObject> prefab)
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

      TrapComponent trap = new();
      ecs.AddComponent(e, trap);

      TeamComponent team = new();
      team.team = Team.NEUTRAL;
      ecs.AddComponent(e, team);

      return e;
    }

  }
}