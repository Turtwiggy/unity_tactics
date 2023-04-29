using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  public class SelectSystem : ECSSystem
  {
    private camera_handler camera;
    private map_manager map;
    private UnitSpawnSystem unit_spawn_system;
    private Entity cursor;
    public UnityEvent<Entity> new_entity_selected;

    // data
    private Optional<Entity> selected = new();

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<PlayerComponent>());
      s.Set(ecs.GetComponentType<InstantiatedComponent>());
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      ecs.SetSystemSignature<SelectSystem>(s);
    }

    public void Start(Wiggy.registry ecs, UnitSpawnSystem uss, GameObject selected_cursor_prefab)
    {
      camera = Object.FindObjectOfType<camera_handler>();
      map = Object.FindObjectOfType<map_manager>();
      unit_spawn_system = uss;
      cursor = Entities.create_cursor(ecs, selected_cursor_prefab);
      new_entity_selected = new();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities)
      {
        if (HasSpecificSelected(e))
        {
          // note: instantiated is cursor, not unit
          ref var p = ref ecs.GetComponent<GridPositionComponent>(e);
          ref var i = ref ecs.GetComponent<InstantiatedComponent>(cursor);

          var world_space = Grid.GridSpaceToWorldSpace(p.position, map.size);
          i.instance.transform.position = world_space;
          i.instance.SetActive(true);
        }
      }
    }

    public void Select()
    {
      var camera_pos = camera.grid_index;
      var index = Grid.GetIndex(camera_pos, map.width);
      Debug.Log("trying to select index: " + index);

      if (HasAnySelected())
        return;

      var camera_entity = unit_spawn_system.units[index].Data;

      //
      // Limits entities to only players via Components
      //

      foreach (var e in entities)
      {
        if (e.Equals(camera_entity))
        {
          selected.Set(e);
          new_entity_selected.Invoke(e);
        }
      }
    }

    public void ClearSelect()
    {
      selected.Reset();
    }

    public bool HasAnySelected()
    {
      return selected.IsSet;
    }

    public bool HasSpecificSelected(Entity e)
    {
      return selected.IsSet && e.id == selected.Data.id;
    }

    public Entity GetSelected()
    {
      return selected.Data;
    }
  }
} // namespace Wiggy