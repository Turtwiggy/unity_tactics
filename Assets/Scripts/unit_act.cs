using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  using Entity = System.Int32;

  [System.Serializable]
  public struct AttackEvent
  {
    [SerializeField]
    public Entity from;
    [SerializeField]
    public Entity target;
  };

  [System.Serializable]
  public struct MoveEvent
  {
    [SerializeField]
    public int from_index;
    [SerializeField]
    public int to_index;
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

    public void Act(Optional<Entity>[] units, unit_select select, camera_handler camera, map_manager map)
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
      if (units[from].IsSet && units[to].IsSet)
      {
        AttackEvent e = new()
        {
          from = units[from].Data,
          target = units[to].Data
        };
        attack_event.Invoke(e);
      }

      // a different tile?
      else if (map.obstacle_map[to].entities.Contains(EntityType.tile_type_floor))
      {
        MoveEvent e = new()
        {
          from_index = from,
          to_index = to
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