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
      Assert.AreEqual(entity0, 0);
      Assert.AreEqual(entity1, 1);
      Assert.AreEqual(entity2, 2);
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
      Assert.AreEqual(entity0, 0);
      Assert.AreEqual(entity2, 2);
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

  }
}
