using NUnit.Framework;
using UnityEngine;

namespace Wiggy
{
  public class TextEcs
  {
    [Test]
    public void TestEcs__CreateEntity()
    {
      // Setup
      Wiggy.registry ecs = new();

      // Act
      var entity0 = ecs.Create();
      var entity1 = ecs.Create();
      var entity2 = ecs.Create();

      // Assert
      Assert.AreEqual(entity0.id, 0);
      Assert.AreEqual(entity1.id, 1);
      Assert.AreEqual(entity2.id, 2);
      Assert.AreEqual(ecs.entity_manager.alive, 3);
    }

    [Test]
    public void TestEcs__DestroyEntity()
    {
      // Setup
      Wiggy.registry ecs = new();

      // Act
      var entity0 = ecs.Create();
      {
        var entity1 = ecs.Create();
        ecs.Destroy(entity1);
      }
      var entity2 = ecs.Create();

      // Assert
      Assert.AreEqual(entity0.id, 0);
      Assert.AreEqual(entity2.id, 2);
      Assert.AreEqual(ecs.entity_manager.alive, 2);
    }

    [Test]
    public void TestEcs__GetComponentIsMutable()
    {
      // Setup
      Wiggy.registry ecs = new();
      ecs.RegisterComponent<GridPositionComponent>();

      var start = new Vector2Int(10, 12);
      var change = new Vector2Int(14, 16);

      // Create entity
      var e = ecs.Create();
      GridPositionComponent gpc = new()
      {
        position = start
      };
      ecs.AddComponent(e, gpc);

      // Act
      {
        ref var pos = ref ecs.GetComponent<GridPositionComponent>(e);
        pos.position = change;
      }

      // Assert
      {
        var pos = ecs.GetComponent<GridPositionComponent>(e);
        Assert.AreEqual(change, pos.position);
      }
    }

    [Test]
    public void TestEcs__CreateMultipleEntities()
    {
      // Arrange
      Wiggy.registry ecs = new();
      ecs.RegisterComponent<GridPositionComponent>();

      // an entity without the component
      var wildcard_entity = ecs.Create();

      Entity create_entity(int i)
      {
        var e = ecs.Create();
        var a = new GridPositionComponent();
        a.position = new();
        a.position.x = i;
        a.position.y = i;
        ecs.AddComponent(e, a);
        return e;
      }

      // Act
      var e0 = create_entity(0);
      var e1 = create_entity(1);
      var e2 = create_entity(2);
      var e3 = create_entity(3);

      // Assert

      var a0 = ecs.GetComponent<GridPositionComponent>(e0);
      var a1 = ecs.GetComponent<GridPositionComponent>(e1);
      var a2 = ecs.GetComponent<GridPositionComponent>(e2);
      var a3 = ecs.GetComponent<GridPositionComponent>(e3);
      Assert.AreEqual(0, a0.position.x);
      Assert.AreEqual(1, a1.position.x);
      Assert.AreEqual(2, a2.position.x);
      Assert.AreEqual(3, a3.position.x);
    }

    [Test]
    public void TestEcs__TryGetSuccess()
    {
      // Arrange
      Wiggy.registry ecs = new();
      ecs.RegisterComponent<GridPositionComponent>();
      ecs.RegisterComponent<TeamComponent>();

      var e = ecs.Create();
      {
        ecs.AddComponent<GridPositionComponent>(e, new());

        TeamComponent tc = new();
        tc.team = Team.NEUTRAL;
        ecs.AddComponent(e, tc);
      }

      // Act / Assert

      // Get the component and modify it

      {
        TeamComponent backup = default;
        ref var component = ref ecs.TryGetComponent(e, ref backup);
        Assert.AreEqual(Team.NEUTRAL, component.team);

        component.team = Team.PLAYER; // modify it
      }

      {
        TeamComponent backup = default;
        ref var component = ref ecs.TryGetComponent(e, ref backup);
        Assert.AreEqual(Team.PLAYER, component.team);
      }
    }
  }
}
