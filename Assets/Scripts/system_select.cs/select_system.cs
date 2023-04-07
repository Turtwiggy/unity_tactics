using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class SelectSystem : ECSSystem
  {
    private map_manager map;
    private camera_handler camera;

    private int from_index = -1;

    public void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<CursorComponent>());
      s.Set(ecs.GetComponentType<GridPositionComponent>());
      s.Set(ecs.GetComponentType<InstantiatedComponent>());
      ecs.SetSystemSignature<SelectSystem>(s);
    }

    public void Start(Wiggy.registry ecs, GameObject selected_cursor_prefab)
    {
      var cursor = Entities.create_cursor(ecs, selected_cursor_prefab);
      map = Object.FindObjectOfType<map_manager>();
      camera = Object.FindObjectOfType<camera_handler>();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // ReadOnly Copy
      {
        // ref var c = ref ecs.GetComponent<CursorComponent>(e);
        // ref var p = ref ecs.GetComponent<GridPositionComponent>(e);
        ref var i = ref ecs.GetComponent<InstantiatedComponent>(e);

        if (HasSelected())
        {
          var pos = Grid.IndexToPos(from_index, map.width, map.height);
          var world_space = Grid.GridSpaceToWorldSpace(pos, map.size);
          i.instance.transform.position = world_space;
          i.instance.SetActive(from_index != -1);
        }
      }
    }

    public void Select()
    {
      var camera_pos = camera.grid_index;
      var index = Grid.GetIndex(camera_pos, map.width);

      if (HasSelected())
        return;

      from_index = index;
    }

    public void ClearSelect()
    {
      from_index = -1;
    }

    public bool HasSelected()
    {
      return from_index != -1;
    }

    public int GetSelected()
    {
      return from_index;
    }
  }
} // namespace Wiggy