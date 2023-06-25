using UnityEngine;
using TMPro;

namespace Wiggy
{
  public class main_menu : MonoBehaviour
  {
    private scene_manager sm;

    // ui
    public TextMeshProUGUI wins;
    public TextMeshProUGUI fails;

    public TextAsset names_f;
    public TextAsset names_m;
    public TextAsset names_g;
    public TextAsset names_town;

    void Start()
    {
      sm = new GameObject("scene_manager").AddComponent<scene_manager>();

      wins.SetText($"{PlayerPrefs.GetInt("wins", 0)}");
      fails.SetText($"{PlayerPrefs.GetInt("fails", 0)}");

      var fnames = names_f.text.Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
      var mnames = names_m.text.Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
      var gnames = names_g.text.Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
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
