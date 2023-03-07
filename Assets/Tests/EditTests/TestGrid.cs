using NUnit.Framework;
using UnityEngine;

namespace Wiggy
{
  public class TestGrid
  {
    [Test]
    public void TestGrid__GeneralCases()
    {
      // Arrange
      int x_max = 5;
      int y_max = 5;

      // Act
      int x = 2;
      int y = 3;
      var result_index = Grid.GetIndex(x, y, x_max);
      var result_pos = Grid.IndexToPos(result_index, x_max, y_max);

      // Assert
      Assert.AreEqual(result_index, 17);
      Assert.AreEqual(result_pos, new Vector2Int(x, y));
    }
  }

}