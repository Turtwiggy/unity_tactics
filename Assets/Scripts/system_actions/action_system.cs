
namespace Wiggy
{
  using Entity = System.Int32;

  public class ActionsPerTurn
  {
    public int allowed_actions = 2;

    // movement
    public bool move;

    // combat
    public bool heal;
    public bool shoot;
    public bool melee;
    public bool overwatch;
    public bool reload;
  }

  [System.Serializable]
  public class ActionSystem : ECSSystem
  {
    private Entity cursor;

    public void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      ecs.SetSystemSignature<ActionSystem>(s);
    }

    public void Start(Wiggy.registry ecs)
    {
      // Entity[] view = ecs.View<CursorComponent>();

      // // Get the first cursor
      // Entity cursor = default;
      // foreach (var e in view)
      // {
      //   cursor = e;
      //   break;
      // }

      // if (cursor != default)
      // {
      // }
    }

    public void Update()
    {

    }
  }
}

// using UnityEngine.Events;

// namespace Wiggy
// {
//   using Entity = System.Int32;

//   public class unit_act : MonoBehaviour
//   {
//     public void Act(Optional<Entity>[] units, unit_select select, camera_handler camera, map_manager map)
//     {
//       var hover_index = Grid.GetIndex(camera.grid_index, map.width);

//       //
//       // Must select a unit
//       //
//       bool unit_selected = select.from_index != -1;
//       if (!unit_selected)
//       {
//         select.Select(hover_index);
//         return;
//       }

//       //
//       // Now there's a unit selected.
//       // What did the user select next?
//       //
//       var from = select.from_index;
//       var to = hover_index;

//       // the same tile?
//       if (from == to)
//         return;

//       // a unit?
//       if (units[from].IsSet && units[to].IsSet)
//       {
//         AttackEvent e = new()
//         {
//           from = units[from].Data,
//           target = units[to].Data
//         };
//         attack_event.Invoke(e);
//       }

//       // a different tile?
//       else if (map.obstacle_map[to].entities.Contains(EntityType.tile_type_floor))
//       {
//         MoveEvent e = new()
//         {
//           from_index = from,
//           to_index = to
//         };
//         move_event.Invoke(e);
//       }

//       //
//       // Assume action was successful, and clear the selected tile
//       //
//       select.ClearSelection();
//     }
//   }
// } // namespace Wiggy