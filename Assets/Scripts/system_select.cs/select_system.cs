using System.Linq;
using UnityEngine;

namespace Wiggy
{
  using Entity = System.Int32;

  public class SelectSystem : ECSSystem
  {
    private map_manager map;
    private camera_handler camera;

    public int from_index { get; private set; }

    public void Start(Wiggy.registry ecs, GameObject selected_cursor_prefab)
    {
      Entities.create_cursor(ecs, selected_cursor_prefab);

      map = Object.FindObjectOfType<map_manager>();
      camera = Object.FindObjectOfType<camera_handler>();

      ClearSelect();
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // ReadOnly Copy
      {
        // ref var p = ref ecs.GetComponent<GridPositionComponent>(cursor);
        ref var i = ref ecs.GetComponent<InstantiatedComponent>(e);

        // if something is selected, move cursor position
        if (from_index != -1)
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

      if (from_index != -1)
        return;
      from_index = index;
    }

    public void ClearSelect()
    {
      from_index = -1;
    }
  }
} // namespace Wiggy