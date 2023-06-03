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
    public TextMeshProUGUI action_text;
    public TextMeshProUGUI hovered_enemy_text;
    public TextMeshProUGUI hovered_enemy_hp_text;
    public TextMeshProUGUI hovered_enemy_weapon_text;
    public TextMeshProUGUI hovered_player;
    public TextMeshProUGUI hovered_player_hp_text;
    public TextMeshProUGUI hovered_player_weapon_text;

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

    public void DoStart(main main)
    {
      // all the game/data
      this.main = main;

      // UI Systems
      main.display_inventory_system.Start(main.ecs, main.select_system, inventory_holder, inventory_row_prefab);

      // ui events
      extraction_button.onClick.AddListener(() => main.scene_manager.LoadMenu());

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
        CreateActionButton<Attack>("Attack");
        // CreateActionButton<Heal>("Heal");
        // CreateActionButton<Overwatch>("Overwatch");
        // CreateActionButton<Reload>("Reload");
        // CreateActionButton<Grenade>("Grenade");
      }

      // Next turn UI
      next_turn_button.onClick.AddListener(() =>
      {
        // Update End Turn System
        main.end_turn_system.Update(main.ecs);

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
      // Selected UI
      //
      if (!main.select_system.HasAnySelected())
      {
        selected_text.SetText("Nothing selected");
        move_actions_left_text.gameObject.SetActive(false);
      }
      else
        RefreshActionUI(main.select_system.GetSelected());

      //
      // Hover UI
      //

      hovered_enemy_text.SetText("");
      hovered_enemy_hp_text.SetText("");
      hovered_enemy_weapon_text.SetText("");
      hovered_player.SetText("Nothing hovered");
      hovered_player_hp_text.SetText("No health");
      hovered_player_weapon_text.SetText("No weapon");

      var index = Grid.GetIndex(main.camera.grid_index, main.map.width);
      var entities = main.map.entity_map[index].entities;
      if (entities.Count > 0)
      {
        var entity = entities[0];
        TextMeshProUGUI hovered_name = hovered_player;
        TextMeshProUGUI hovered_hp = hovered_player_hp_text;
        TextMeshProUGUI hovered_weapon = hovered_player_weapon_text;

        TeamComponent default_team = default;
        var team = main.ecs.TryGetComponent(entity, ref default_team, out var has_team);
        if (has_team && team.team == Team.ENEMY)
        {
          hovered_name = hovered_enemy_text;
          hovered_hp = hovered_enemy_hp_text;
          hovered_weapon = hovered_enemy_weapon_text;
        }

        var tag = main.ecs.GetComponent<TagComponent>(entity);
        hovered_name.SetText(tag.name + $" ({entities.Count})");

        HealthComponent health_default = default;
        ref var hp = ref main.ecs.TryGetComponent(entity, ref health_default, out var has_hp);
        if (has_hp)
          hovered_hp.SetText("HP: " + hp.cur.ToString());

        WeaponComponent weapon_default = default;
        ref var weapon = ref main.ecs.TryGetComponent(entity, ref weapon_default, out var has_weapon);
        if (has_weapon)
          hovered_weapon.SetText("Weapon: " + weapon.display_name);
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