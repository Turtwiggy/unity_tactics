using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wiggy
{
  public class scene_manager : MonoBehaviour
  {
    public void LoadMenu()
    {
      SceneManager.LoadScene("main_menu");
    }
    public void LoadGame()
    {
      SceneManager.LoadScene("main");
    }
  }
}
