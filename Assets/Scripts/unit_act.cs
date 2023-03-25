using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  public struct AttackEvent
  {
    public GameObject from;
    public GameObject target;
  };
  public struct MoveEvent
  {
    public int from;
    public int to;
  };

  public class unit_act : MonoBehaviour
  {
    public UnityEvent<AttackEvent> attack_event { get; private set; }
    public UnityEvent<MoveEvent> move_event { get; private set; }

    public void DoStart()
    {
      attack_event = new();
      move_event = new();
    }

    public void Act(GameObject[] units, unit_select select, camera_handler camera, map_manager map)
    {
      var hover_index = Grid.GetIndex(camera.grid_index, map.width);

      //
      // Must select a unit
      //
      bool unit_selected = select.from_index != -1;
      if (!unit_selected)
      {
        select.Select(hover_index);
        return;
      }

      //
      // Now there's a unit selected.
      // What did the user select next?
      //
      var from = select.from_index;
      var to = hover_index;

      // the same tile?
      if (from == to)
        return;

      // a unit?
      if (units[to] != null)
      {
        AttackEvent e = new()
        {
          from = units[from],
          target = units[to]
        };
        attack_event.Invoke(e);
      }

      // a different tile?
      else if (map.obstacle_map[to].entities.Contains(EntityType.tile_type_floor))
      {
        MoveEvent e = new()
        {
          from = from,
          to = to
        };
        move_event.Invoke(e);
      }

      //
      // Assume action was successful, and clear the selected tile
      //
      select.ClearSelection();
    }
  }
} // namespace Wiggy