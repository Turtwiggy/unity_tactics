using UnityEngine;
using UnityEngine.UI;

namespace Wiggy
{
  public class main_ui_data
  {
    public bool ready_for_extraction;
  }

  [System.Serializable]
  public class main_ui : MonoBehaviour
  {
    [Header("Extraction")]
    public GameObject extraction_holder;
    public Button extraction_button;

    [Header("Actions")]
    public GameObject action_holder;
    public GameObject action_prefab;

    private map_manager map;
    private scene_manager scene;

    public void Start()
    {
      map = FindObjectOfType<map_manager>();
      scene = FindObjectOfType<scene_manager>();

      // ui events
      extraction_button.onClick.AddListener(() => scene.Load());
    }

    public void DoUpdate(main_ui_data data)
    {
      // set ui active/inactive
      extraction_holder.SetActive(data.ready_for_extraction);
    }
  }
}