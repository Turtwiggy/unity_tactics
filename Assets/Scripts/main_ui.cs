using TMPro;
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

    private scene_manager scene;

    public void Start()
    {
      scene = FindObjectOfType<scene_manager>();

      // ui events
      extraction_button.onClick.AddListener(() => scene.Load());

      // Instantiate Action UI
      foreach (Transform t in action_holder.transform)
        Destroy(t.gameObject);
      for (int i = 0; i < 5; i++)
      {
        var go = Instantiate(action_prefab, Vector3.zero, Quaternion.identity, action_holder.transform);
        go.transform.name = "Action: " + i;
        var text = go.GetComponentInChildren<TextMeshProUGUI>();
        text.SetText("Action: " + i);

        var local = i;
        go.GetComponent<Button>().onClick.AddListener(() =>
        {
          // action_system.DoActionFromUI(ecs, )
          Debug.Log(local);
        });
      }
    }

    public void DoUpdate(main_ui_data data)
    {
      // set ui active/inactive
      extraction_holder.SetActive(data.ready_for_extraction);
    }
  }
}