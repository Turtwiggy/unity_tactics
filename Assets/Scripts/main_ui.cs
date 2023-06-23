using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wiggy
{
  [System.Serializable]
  [RequireComponent(typeof(main_ui_worldspace_hover))]
  public class main_ui : MonoBehaviour
  {
    [HideInInspector]
    public main main;

    [Header("Extraction")]
    public GameObject extraction_holder;
    public Button extraction_button;

    [Header("Selected Unit Info")]
    public TextMeshProUGUI selected_text;
    public TextMeshProUGUI action_text;

    [Header("Selected Unit Actions")]
    public GameObject action_holder;
    public GameObject action_prefab;

    [Header("Move Actions UI")]
    public TextMeshProUGUI move_actions_left_text;

    private List<(Button, Action)> action_buttons = new();

    [Header("Next Turn")]
    public Button next_turn_button;

    [Header("Inventory")]
    public GameObject inventory_holder;
    public GameObject inventory_row_prefab;

    [Header("Use Items")]
    public TextMeshProUGUI door_info_text;

    [Header("Action Button Sprites")]
    public Sprite move_sprite;
    public Sprite attack_sprite;
    public Sprite heal_sprite;
    public Sprite overwatch_sprite;

    private main_ui_worldspace_hover hover_ui;

    [SerializeField]
    private GameObject selected_ui;

    public void DoStart(main main)
    {
      // all the game/data
      this.main = main;
      hover_ui = GetComponent<main_ui_worldspace_hover>();
      hover_ui.DoStart(this.main);

      // UI Systems
      main.display_inventory_system.Start(main.ecs, main.select_system, inventory_holder, inventory_row_prefab);

      // ui events
      extraction_button.onClick.AddListener(() =>
      {
        // Record a win
        PlayerPrefs.SetInt("wins", PlayerPrefs.GetInt("wins", 0) + 1);

        // What to do when extract?
        main.scene_manager.LoadMenu();
      });

      // Actions UI
      {
        foreach (Transform t in action_holder.transform)
          Destroy(t.gameObject);
        void CreateActionButton<T>(string name, Sprite sprite) where T : Action, new()
        {
          var go = Instantiate(action_prefab, Vector3.zero, Quaternion.identity, action_holder.transform);
          go.transform.name = name;

          var img = go.transform.GetChild(0).GetComponent<Image>();
          img.sprite = sprite;

          var button = go.GetComponent<Button>();
          button.onClick.AddListener(() =>
          {
            main.action_system.RequestActionFromUI<T>(main.ecs);
          });

          action_buttons.Add((button, new T()));
        }
        CreateActionButton<Move>("Move", move_sprite);
        CreateActionButton<Attack>("Attack", attack_sprite);
        CreateActionButton<Heal>("Heal", heal_sprite);
        CreateActionButton<Overwatch>("Overwatch", overwatch_sprite);
        // CreateActionButton<Reload>("Reload");
        // CreateActionButton<Grenade>("Grenade");
      }

      // Next turn UI
      next_turn_button.onClick.AddListener(() =>
      {
        // Update End Turn System
        main.end_turn_system.EndPlayerTurn(main.ecs);

        // Update AI system
        main.ai_system.Update(main.ecs, main.astar);

        // End AI turn
        main.end_turn_system.EndAiTurn(main.ecs);
      });
    }

    public void DoUpdate(Wiggy.registry ecs)
    {
      //
      // Extraction UI
      //
      extraction_holder.SetActive(main.extraction_system.ready_for_extraction);

      //
      // Hover UI
      // Must come before Selected UI
      //
      hover_ui.DoUpdate();

      //
      // Selected UI
      //
      if (!main.select_system.HasAnySelected())
      {
        selected_text.SetText("Nothing selected");
        move_actions_left_text.gameObject.SetActive(false);

        if (selected_ui != null)
          Destroy(selected_ui);
      }
      else
      {
        RefreshActionUI(main.select_system.GetSelected());

        if (selected_ui == null)
        {
          selected_ui = Instantiate(hover_ui.hover_ui);
          selected_ui.SetActive(true);
          selected_ui.transform.GetChild(0).GetChild(0).GetChild(2).gameObject.SetActive(false);
          selected_ui.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
          selected_ui.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().SetText("SELECTED");
        }
      }

      // Debug which action is selected from UI
      if (main.action_system.action_selected_from_ui)
        action_text.SetText(main.action_system.action_selected.GetType().ToString());
      else
        action_text.SetText("No Action");
      //
      // Inventory
      //
      main.display_inventory_system.Update(ecs);
      door_info_text.SetText("Doors: " + main.standing_next_to_door_system.eligable_doors.Count.ToString());
    }

    void RefreshActionUI(Entity selected)
    {
      // Refresh Selected UI
      var unity = main.ecs.GetComponent<InstantiatedComponent>(selected);
      selected_text.SetText(unity.instance.name);

      // Refresh Actions UI (only humanoid has actions)
      var humanoid_default = default(HumanoidComponent);
      main.ecs.TryGetComponent(selected, ref humanoid_default, out var is_humanoid);
      if (!is_humanoid)
        return;
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

      // Refresh Actions To Take UI
      move_actions_left_text.gameObject.SetActive(true);
      var actions_left = actions.allowed_actions_per_turn - actions.done.Count;
      move_actions_left_text.SetText($"Actions Left: {actions_left}");
    }
  }
}