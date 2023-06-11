using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace Wiggy
{

  // ITS GAMEOVER MAN

  public class GameoverSystem : ECSSystem
  {
    public bool IsGameOver { get; private set; }

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<PlayerComponent>());
      ecs.SetSystemSignature<GameoverSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
    }

    public void Update(Wiggy.registry ecs)
    {
      // Are players alive?
      IsGameOver |= entities.Count == 0;

      if (IsGameOver)
        Debug.Log("game is over! (loss)");
    }
  }
}