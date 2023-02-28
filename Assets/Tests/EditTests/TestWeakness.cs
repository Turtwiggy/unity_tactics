using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wiggy;

public class TestWeakness
{
  [Test]
  public void TestWeakness__GeneralCases()
  {
    // arrange
    WEAKNESS rock = WEAKNESS.ROCK;
    WEAKNESS paper = WEAKNESS.PAPER;
    WEAKNESS scissors = WEAKNESS.SCISSORS;

    // act & assert

    // yes weak (next in line)
    var a = rock.IsWeakTo(paper);
    var b = paper.IsWeakTo(scissors);
    var c = scissors.IsWeakTo(rock); // last case
    Assert.True(a, "a");
    Assert.True(b, "b");
    Assert.True(c, "c");

    // no weak (same)
    var d = rock.IsWeakTo(rock);
    var e = paper.IsWeakTo(paper);
    var f = scissors.IsWeakTo(scissors);
    Assert.False(d, "d");
    Assert.False(e, "e");
    Assert.False(f, "f");

    // no weak (too far away in line)
    // in this rock-paper-scissors this isnt bidirectional
    var g = rock.IsWeakTo(scissors);
    var h = paper.IsWeakTo(rock);
    var i = scissors.IsWeakTo(paper);
    Assert.False(g, "g");
    Assert.False(h, "h");
    Assert.False(i, "i");
  }
}
