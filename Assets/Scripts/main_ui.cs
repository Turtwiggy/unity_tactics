using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wiggy
{
  [System.Serializable]
  public class main_ui : MonoBehaviour
  {
    [HideInInspector]
    public main main;

    [Header("Extraction")]
    public GameObject extraction_holder;
    public Button extraction_button;

    [Header("Selected Unit Info")]
    public TextMeshProUGUI selected_text;

    [Header("Selected Unit Actions")]
    public GameObject action_holder;
    public GameObject action_prefab;
    private Button button_move;
    private Button button_attack;
    private Button button_overwatch;
    private Button button_reload;
    private Button button_heal;

    [Header("Next Turn")]
    public Button next_turn_button;

    private scene_manager scene;

    public void DoStart(main main)
    {
      scene = new GameObject("scene_manager").AddComponent<scene_manager>();

      // all the game/data
      this.main = main;

      // ui events
      extraction_button.onClick.AddListener(() => scene.LoadMenu());

      // Actions UI
      {
        foreach (Transform t in action_holder.transform)
          Destroy(t.gameObject);
        Button CreateActionButton<T>(string name) where T : Action, new()
        {
          var go = Instantiate(action_prefab, Vector3.zero, Quaternion.identity, action_holder.transform);
          go.transform.name = name;
          go.GetComponentInChildren<TextMeshProUGUI>().SetText(name);

          var button = go.GetComponent<Button>();
          button.onClick.AddListener(() =>
          {
            main.action_system.RequestActionFromUI<T>(main.ecs);
          });
          return button;
        }
        button_move = CreateActionButton<Move>("Move");
        button_attack = CreateActionButton<Attack>("Attack");
        button_overwatch = CreateActionButton<Overwatch>("Overwatch");
        button_reload = CreateActionButton<Reload>("Reload");
        button_heal = CreateActionButton<Heal>("Heal");

        //
        // Note: these refresh events below seem wrong and/or to manual
        //

        main.select_system.new_entity_selected.AddListener((selected) =>
        {
          RefreshActionUI(selected);
        });

        main.action_system.action_complete.AddListener(() =>
        {
          if (main.select_system.HasAnySelected())
          {
            var selected = main.select_system.GetSelected();
            RefreshActionUI(selected);
          }
        });
      }

      // Next turn UI
      next_turn_button.onClick.AddListener(() =>
      {
        // Update System
        main.end_turn_system.Update(main.ecs);

        // Update UI
        if (main.select_system.HasAnySelected())
        {
          var selected = main.select_system.GetSelected();
          RefreshActionUI(selected);
        }
      });
    }

    public void DoUpdate()
    {
      //
      // Extraction UI
      //
      extraction_holder.SetActive(main.extraction_system.ready_for_extraction);

      //
      // Selected UI
      //
      if (!main.select_system.HasAnySelected())
        selected_text.SetText("Nothing selected");
    }

    void RefreshActionUI(Entity selected)
    {
      // Refresh Selected UI
      var unity = main.ecs.GetComponent<InstantiatedComponent>(selected);
      selected_text.SetText(unity.instance.name);

      // Refresh Actions UI
      var actions = main.ecs.GetComponent<ActionsComponent>(selected);

      void DisableButtonIfActionIsDone<T>(Button b, ActionsComponent actions)
      {
        bool action_is_done = false;

        for (int i = 0; i < actions.done.Count; i++)
          if (actions.done[i].GetType() == typeof(T))
            action_is_done = true;

        // Can click button if action is still available
        b.interactable = !action_is_done;
      }
      DisableButtonIfActionIsDone<Move>(button_move, actions);
      DisableButtonIfActionIsDone<Attack>(button_attack, actions);
      DisableButtonIfActionIsDone<Overwatch>(button_overwatch, actions);
      DisableButtonIfActionIsDone<Reload>(button_reload, actions);
      DisableButtonIfActionIsDone<Heal>(button_heal, actions);
    }
  }
}