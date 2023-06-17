using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace Wiggy
{

  // ITS GAMEOVER MAN

  public class GameoverSystem : ECSSystem
  {
    public bool IsGameOver { get; private set; }

    private scene_manager scene_manager;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<PlayerComponent>());
      ecs.SetSystemSignature<GameoverSystem>(s);
    }

    public void Start(Wiggy.registry ecs, scene_manager scene_manager)
    {
      this.scene_manager = scene_manager;
    }

    public void Update(Wiggy.registry ecs)
    {
      // Are players alive?
      IsGameOver |= entities.Count == 0;

      if (IsGameOver)
      {
        Debug.Log("game is over! (loss)");

        PlayerPrefs.SetInt("fails", PlayerPrefs.GetInt("fails", 0) + 1);

        // What to do when loss?
        scene_manager.LoadMenu();
      }
    }
  }
}