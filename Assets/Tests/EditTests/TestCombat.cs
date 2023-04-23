using NUnit.Framework;
using UnityEngine;

namespace Wiggy
{
  public class TestCombat
  {
    [Test]
    public void TestCombat__Flanks__NotFlankedBecauseInCover()
    {
      // Arrange
      const int MAP_SIZE = 9;
      const int MAP_WIDTH = 3;
      const int MAP_HEIGHT = 3;

      // Setup ecs...
      var go = new GameObject("main");
      main main = go.AddComponent<main>();
      map_manager map = go.AddComponent<map_manager>();
      main.map = map;
      main.map.width = MAP_WIDTH;
      main.map.height = MAP_HEIGHT;
      main.ecs = new();
      main.RegisterComponents(main.ecs);
      main.RegisterSystems(main.ecs);
      main.RegisterSystemSignatures(main.ecs);

      // Setup obstacle map...
      map.obstacle_map = new MapEntry[MAP_SIZE]; // a 3x3 map
      for (int i = 0; i < MAP_SIZE; i++)
      {
        map.obstacle_map[i] = new() { entities = new() };
        if (i == 4)
          map.obstacle_map[i].entities.Add(EntityType.tile_type_wall);
        else
          map.obstacle_map[i].entities.Add(EntityType.tile_type_floor);
      }

      // Setup unit map...
      var units = main.unit_spawn_system;
      units.map = map;
      units.units = new Optional<Entity>[MAP_SIZE];
      for (int i = 0; i < units.units.Length; i++)
        units.units[i] = new();
      var atk = units.CreatePlayer(main.ecs, new Vector2Int(1, 0), "Player", new Optional<GameObject>());
      var def = units.CreateEnemy(main.ecs, new Vector2Int(1, 2), "Enemy", new Optional<GameObject>());

      // Act
      var atk_pos = main.ecs.GetComponent<GridPositionComponent>(atk).position;
      var def_pos = main.ecs.GetComponent<GridPositionComponent>(def).position;
      var flanked = CombatHelpers.SpotIsFlanked(map, atk_pos, def_pos);

      // Assert
      Assert.AreEqual(true, map.obstacle_map[4].entities.Contains(EntityType.tile_type_wall));
      Assert.AreEqual(false, flanked);
    }

    [Test]
    public void TestCombat__Flanks__FlankedWhenNotInCover()
    {
      // Arrange
      const int MAP_SIZE = 9;
      const int MAP_WIDTH = 3;
      const int MAP_HEIGHT = 3;

      // Setup ecs...
      var go = new GameObject("main");
      main main = go.AddComponent<main>();
      map_manager map = go.AddComponent<map_manager>();
      main.map = map;
      main.map.width = MAP_WIDTH;
      main.map.height = MAP_HEIGHT;
      main.ecs = new();
      main.RegisterComponents(main.ecs);
      main.RegisterSystems(main.ecs);
      main.RegisterSystemSignatures(main.ecs);

      // Setup obstacle map...
      map.obstacle_map = new MapEntry[MAP_SIZE]; // a 3x3 map
      for (int i = 0; i < MAP_SIZE; i++)
      {
        map.obstacle_map[i] = new() { entities = new() };
        map.obstacle_map[i].entities.Add(EntityType.tile_type_floor);
      }

      // Setup unit map...
      var units = main.unit_spawn_system;
      units.map = map;
      units.units = new Optional<Entity>[MAP_SIZE];
      for (int i = 0; i < units.units.Length; i++)
        units.units[i] = new();
      var atk = units.CreatePlayer(main.ecs, new Vector2Int(1, 0), "Player", new Optional<GameObject>());
      var def = units.CreateEnemy(main.ecs, new Vector2Int(1, 2), "Enemy", new Optional<GameObject>());

      // Act
      var atk_pos = main.ecs.GetComponent<GridPositionComponent>(atk).position;
      var def_pos = main.ecs.GetComponent<GridPositionComponent>(def).position;
      var flanked = CombatHelpers.SpotIsFlanked(map, atk_pos, def_pos);

      // Assert
      Assert.AreEqual(true, flanked);
    }

    [Test]
    public void TestCombat__Flanks__FlankedEvenInCover()
    {
      // Arrange
      const int MAP_SIZE = 9;
      const int MAP_WIDTH = 3;
      const int MAP_HEIGHT = 3;

      // Setup ecs...
      var go = new GameObject("main");
      main main = go.AddComponent<main>();
      map_manager map = go.AddComponent<map_manager>();
      main.map = map;
      main.map.width = MAP_WIDTH;
      main.map.height = MAP_HEIGHT;
      main.ecs = new();
      main.RegisterComponents(main.ecs);
      main.RegisterSystems(main.ecs);
      main.RegisterSystemSignatures(main.ecs);

      // Setup obstacle map...
      map.obstacle_map = new MapEntry[MAP_SIZE]; // a 3x3 map
      for (int i = 0; i < MAP_SIZE; i++)
      {
        map.obstacle_map[i] = new() { entities = new() };
        map.obstacle_map[i].entities.Add(EntityType.tile_type_floor);
      }

      // Setup unit map...
      var units = main.unit_spawn_system;
      units.map = map;
      units.units = new Optional<Entity>[MAP_SIZE];
      for (int i = 0; i < units.units.Length; i++)
        units.units[i] = new();
      var atk = units.CreatePlayer(main.ecs, new Vector2Int(0, 2), "Player", new Optional<GameObject>());
      var def = units.CreateEnemy(main.ecs, new Vector2Int(1, 2), "Enemy", new Optional<GameObject>());

      // Act
      var atk_pos = main.ecs.GetComponent<GridPositionComponent>(atk).position;
      var def_pos = main.ecs.GetComponent<GridPositionComponent>(def).position;
      var flanked = CombatHelpers.SpotIsFlanked(map, atk_pos, def_pos);

      // Assert
      Assert.AreEqual(true, flanked);
    }

    [Test]
    public void TestCombat__Flanks__FlankedFromAllDirections()
    {
      // Arrange
      const int MAP_SIZE = 9;
      const int MAP_WIDTH = 3;
      const int MAP_HEIGHT = 3;

      // Setup ecs...
      var go = new GameObject("main");
      main main = go.AddComponent<main>();
      map_manager map = go.AddComponent<map_manager>();
      main.map = map;
      main.map.width = MAP_WIDTH;
      main.map.height = MAP_HEIGHT;
      main.ecs = new();
      main.RegisterComponents(main.ecs);
      main.RegisterSystems(main.ecs);
      main.RegisterSystemSignatures(main.ecs);

      // Setup unit map...
      var units = main.unit_spawn_system;
      units.map = map;
      units.units = new Optional<Entity>[MAP_SIZE];
      for (int i = 0; i < units.units.Length; i++)
        units.units[i] = new();

      // Setup obstacle map...
      map.obstacle_map = new MapEntry[MAP_SIZE]; // a 3x3 map
      for (int i = 0; i < MAP_SIZE; i++)
        map.obstacle_map[i] = new() { entities = new() };

      // Generate all neighbours around a unit
      var def = units.CreateEnemy(main.ecs, new Vector2Int(1, 1), "Enemy", new Optional<GameObject>());
      var def_pos = main.ecs.GetComponent<GridPositionComponent>(def).position;
      var idxs = a_star.square_neighbour_indicies_with_diagonals(1, 1, map.width, map.height);

      // Act / Assert
      {
        for (int i = 0; i < idxs.Length; i++)
        {
          var atk_pos = Grid.IndexToPos(idxs[i].Item2, map.width, map.height);
          var flanked = CombatHelpers.SpotIsFlanked(map, atk_pos, def_pos);
          Assert.AreEqual(true, flanked);
        }
      }
    }

  }

}