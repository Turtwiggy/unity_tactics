using UnityEngine;
using TMPro;

namespace Wiggy
{
  public class main_ui_worldspace_hover : MonoBehaviour
  {
    private main main;

    private float ui_distance_above_floor = 3.0f;
    public GameObject hover_ui;
    public TextMeshProUGUI hover_ui_name;
    public TextMeshProUGUI hover_ui_hp;
    public TextMeshProUGUI hover_ui_weapon;
    public GameObject hover_ui_name_icon;
    public GameObject hover_ui_hp_icon;
    public GameObject hover_ui_weapon_icon;
    public TextMeshProUGUI hover_ui_multiple_entity_info;

    public void DoStart(main main)
    {
      this.main = main;
    }

    public void DoUpdate()
    {
      var hovered_index = Grid.GetIndex(main.camerah.grid_index, main.map.width);
      var hovered_entities = main.map.entity_map[hovered_index].entities;
      if (hovered_entities.Count == 0)
      {
        hover_ui.SetActive(false);
        return;
      }

      var hovered_floor_idx = main.select_system.GetSelectedFloorIndex();
      var hovered_entity = hovered_entities[hovered_floor_idx];

      // top bar
      // set the ui for which entity we're hovered on
      hover_ui_multiple_entity_info.SetText($"{hovered_floor_idx + 1}/{hovered_entities.Count}");
      hover_ui_multiple_entity_info.transform.parent.gameObject.SetActive(true);

      var hovered_entity_is_selected = main.select_system.HasSpecificSelected(hovered_entity);
      // Set the hover_ui as active only if there is no selected entity
      hover_ui.SetActive(!hovered_entity_is_selected);

      SetPosition(hovered_entity);
      DisplayTagComponent(hovered_entity);
      DisplayHealthComponent(hovered_entity);
      DisplayWeaponComponent(hovered_entity);
    }

    private void DisplayTagComponent(Entity e)
    {
      var tag = main.ecs.GetComponent<TagComponent>(e);
      hover_ui_name.SetText($"{tag.name.ToLower()}");
    }

    private void DisplayHealthComponent(Entity e)
    {
      var incoming_damage = CalculateDamageForUI(main.ecs);

      HealthComponent health_default = default;
      ref var hp = ref main.ecs.TryGetComponent(e, ref health_default, out var has_hp);
      if (has_hp)
      {
        if (incoming_damage.IsSet)
          hover_ui_hp.SetText($"{hp.cur} <color=red>(-{incoming_damage.Data})</color>");
        else
          hover_ui_hp.SetText($"{hp.cur}");
      }
      hover_ui_hp.gameObject.SetActive(has_hp);
      hover_ui_hp_icon.SetActive(has_hp);
    }

    private void DisplayWeaponComponent(Entity e)
    {
      WeaponComponent weapon_default = default;
      ref var weapon = ref main.ecs.TryGetComponent(e, ref weapon_default, out var has_weapon);
      if (has_weapon)
        hover_ui_weapon.SetText($"{weapon.display_name.ToLower()}");
      hover_ui_weapon.gameObject.SetActive(has_weapon);
      hover_ui_weapon_icon.SetActive(has_weapon);
    }

    private void SetPosition(Entity e)
    {
      var position = main.ecs.GetComponent<GridPositionComponent>(e);
      var worldspace = Grid.GridSpaceToWorldSpace(position.position, main.map.size);
      worldspace.y = ui_distance_above_floor;
      hover_ui.transform.position = worldspace;
    }

    private Optional<int> CalculateDamageForUI(Wiggy.registry ecs)
    {
      Optional<int> damage = new();
      if (
          main.action_system.action_selected_from_ui &&
          main.action_system.action_selected.GetType() == typeof(Attack) &&
          main.select_system.HasAnySelected()
        )
      {
        var attacker = main.select_system.GetSelected();
        var hovered = map_manager.GetHoveredEntities(main.map, main.camerah);
        if (hovered.Count > 0)
        {
          var floor_idx = main.select_system.GetSelectedFloorIndex();
          var defender = hovered[floor_idx];
          if (attacker.id != defender.id)
            damage.Set(CombatHelpers.CalculateDamage(ecs, main.map, main.astar, attacker, defender));
        }
      }
      return damage;
    }
  }
}