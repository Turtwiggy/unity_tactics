using UnityEngine;

namespace Wiggy
{
  public class main_menu : MonoBehaviour
  {
    private scene_manager sm;

    void Start()
    {
      sm = new GameObject("scene_manager").AddComponent<scene_manager>();
    }

    public void StartGame()
    {
      sm.LoadGame();
    }

    public void QuitGame()
    {
      Application.Quit();
    }
  }
}
