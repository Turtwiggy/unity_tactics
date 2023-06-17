using UnityEngine;
using TMPro;

namespace Wiggy
{
  public class main_menu : MonoBehaviour
  {
    private scene_manager sm;

    public TextMeshProUGUI wins;
    public TextMeshProUGUI fails;

    void Start()
    {
      sm = new GameObject("scene_manager").AddComponent<scene_manager>();

      wins.SetText($"Successful runs: {PlayerPrefs.GetInt("wins", 0)}");
      fails.SetText($"Failed runs: {PlayerPrefs.GetInt("fails", 0)}");
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
