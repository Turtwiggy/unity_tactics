using System.Collections.Generic;
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
    public TextMeshProUGUI hovered_text;
    public TextMeshProUGUI hovered_enemy_text;
    public TextMeshProUGUI action_text;

    [Header("Selected Unit Actions")]
    public GameObject action_holder;
    public GameObject action_prefab;

    private List<(Button, Action)> action_buttons = new();

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
        void CreateActionButton<T>(string name) where T : Action, new()
        {
          var go = Instantiate(action_prefab, Vector3.zero, Quaternion.identity, action_holder.transform);
          go.transform.name = name;
          go.GetComponentInChildren<TextMeshProUGUI>().SetText(name);

          var button = go.GetComponent<Button>();
          button.onClick.AddListener(() =>
          {
            main.action_system.RequestActionFromUI<T>(main.ecs);
          });

          action_buttons.Add((button, new T()));
        }
        CreateActionButton<Move>("Move");
        CreateActionButton<Heal>("Heal");
        CreateActionButton<Attack>("Attack");
        CreateActionButton<Reload>("Reload");
        CreateActionButton<Overwatch>("Overwatch");
        CreateActionButton<Grenade>("Grenade");

        //
        // Note: these refresh events below seem wrong and/or to manual
        //

        main.select_system.new_entity_selected.AddListener((selected) =>
        {
          RefreshActionUI(selected);
        });
      }

      // Next turn UI
      next_turn_button.onClick.AddListener(() =>
      {
        // Update End Turn System
        main.end_turn_system.Update(main.ecs);

        // Update AI system
        main.ai_system.Update(main.ecs);

        Debug.Log("AI Turn Done?");

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
      else
        RefreshActionUI(main.select_system.GetSelected());

      //
      // Hover UI
      //
      var index = Grid.GetIndex(main.camera.grid_index, main.map.width);
      var entity = main.unit_spawn_system.units[index];
      if (entity.IsSet)
      {
        var unity = main.ecs.GetComponent<InstantiatedComponent>(entity.Data);
        var team = main.ecs.GetComponent<TeamComponent>(entity.Data);
        if (team.team == Team.ENEMY)
          hovered_enemy_text.SetText(unity.instance.name);
        else
          hovered_text.SetText(unity.instance.name);
      }
      else
      {
        hovered_text.SetText("Nothing hovered");
        hovered_enemy_text.SetText("");
      }

      // Debug which action is selected from UI
      if (main.action_system.action_selected_from_ui)
        action_text.SetText(main.action_system.action_selected.GetType().ToString());
      else
        action_text.SetText("No Action");
    }

    void RefreshActionUI(Entity selected)
    {
      // Refresh Selected UI
      var unity = main.ecs.GetComponent<InstantiatedComponent>(selected);
      selected_text.SetText(unity.instance.name);

      // Refresh Actions UI
      var actions = main.ecs.GetComponent<ActionsComponent>(selected);

      static void DisableButtonIfActionIsDone(Button b, ActionsComponent actions, Action a)
      {
        bool action_is_done = false;
        for (int i = 0; i < actions.done.Count; i++)
          if (actions.done[i].GetType() == a.GetType())
            action_is_done = true;
        // Can click button if action is still available
        b.interactable = !action_is_done;
      }

      for (int i = 0; i < action_buttons.Count; i++)
      {
        var (button, action) = action_buttons[i];
        DisableButtonIfActionIsDone(button, actions, action);
      }
    }
  }
}