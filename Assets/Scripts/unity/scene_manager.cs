using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wiggy
{
  public class scene_manager : MonoBehaviour
  {
    public string scene_to_load;

    public void Load()
    {
      SceneManager.LoadScene(scene_to_load);
    }
  }
}
