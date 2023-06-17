using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  public class SelectSystem : ECSSystem
  {
    private camera_handler camera;
    private input_handler input;
    private map_manager map;
    private Entity cursor;
    public UnityEvent<Entity> new_entity_selected;

    // Handles multiple entities on one square
    private int selected_entity_on_floor = 0;
    private int last_hovered_index = 0;

    // data
    private Optional<Entity> selected = new();

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<InstantiatedComponent>());
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      ecs.SetSystemSignature<SelectSystem>(s);
    }

    public void Start(Wiggy.registry ecs, GameObject selected_cursor_prefab)
    {
      camera = Object.FindObjectOfType<camera_handler>();
      map = Object.FindObjectOfType<map_manager>();
      input = Object.FindObjectOfType<input_handler>();
      cursor = Entities.create_cursor(ecs, selected_cursor_prefab, new Optional<GameObject>());
      new_entity_selected = new();
    }

    public void Update(Wiggy.registry ecs)
    {
      //
      // Handle user input on hovered square
      //

      var index = Grid.GetIndex(camera.grid_index, map.width);
      var map_entities = map.entity_map[index].entities;

      if (last_hovered_index != index)
        selected_entity_on_floor = 0;
      last_hovered_index = index;

      if (input.window_r)
      {
        selected_entity_on_floor += 1;
        if (selected_entity_on_floor >= map_entities.Count)
          selected_entity_on_floor = 0;
      }
      if (input.window_l)
      {
        selected_entity_on_floor -= 1;
        if (selected_entity_on_floor < 0)
          selected_entity_on_floor = map_entities.Count - 1;
      }

      //
      // Sets the "cursor" on the selected unit 
      //

      if (HasAnySelected())
      {
        var e = selected.Data;

        // note: instantiated is cursor, not unit
        ref var p = ref ecs.GetComponent<GridPositionComponent>(e);
        ref var i = ref ecs.GetComponent<InstantiatedComponent>(cursor);

        var world_space = Grid.GridSpaceToWorldSpace(p.position, map.size);
        i.instance.transform.position = world_space;
        i.instance.SetActive(true);
      }
    }

    public void Select()
    {
      var camera_pos = camera.grid_index;
      var index = Grid.GetIndex(camera_pos, map.width);

      if (HasAnySelected())
        return;

      var map_entities = map.entity_map[index].entities;
      if (map_entities.Count == 0)
        return;

      var entity = map_entities[selected_entity_on_floor];
      selected.Set(entity);
      new_entity_selected.Invoke(entity);
    }

    public void ClearSelect()
    {
      selected.Reset();
      selected_entity_on_floor = 0;
      Debug.Log("clear selected to 0");
    }

    public bool HasAnySelected()
    {
      return selected.IsSet;
    }

    public bool HasSpecificSelected(Entity e)
    {
      return selected.IsSet && e.id == selected.Data.id;
    }

    public bool HasSelectedOnTile(Entity e)
    {
      var entities = map.entity_map[selected_entity_on_floor].entities;
      return entities.Contains(e);
    }

    public Entity GetSelected()
    {
      return selected.Data;
    }

    public int GetSelectedFloorIndex()
    {
      return selected_entity_on_floor;
    }

  }
} // namespace Wiggy