using System.Collections;
using System.Collections.Generic;
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

    map_manager map;
    scene_manager scene;

    public void Start()
    {
      map = FindObjectOfType<map_manager>();
      scene = FindObjectOfType<scene_manager>();

      // ui events
      extraction_button.onClick.AddListener(() => scene.Load());
    }

    public void DoUpdate(main_ui_data data)
    {
      // update the cursor
      // unit_select.UpdateSelectedCursorUI(map.size);

      // set ui active/inactive
      extraction_holder.SetActive(data.ready_for_extraction);
    }
  }
}