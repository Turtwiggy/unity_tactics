using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class MonitorOverwatchSystem : ECSSystem
  {
    private List<MoveInformation> move_events;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<OverwatchStatus>());
      ecs.SetSystemSignature<MonitorOverwatchSystem>(s);
    }

    public void Start(Wiggy.registry ecs, MoveSystem move_system)
    {
      move_events = new();
      move_system.something_moved.AddListener((e) => { move_events.Add(e); });
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray()) // readonly as modified
      {
        // check if an entity moves in my weapon range!

        // may or may not have weapon equipped
        WeaponComponent backup = default;
        ref WeaponComponent weapon = ref ecs.TryGetComponent(e, ref backup);
        bool has_weapon = !weapon.Equals(backup);
        if (has_weapon)
        {
          Debug.Log("No weapon equipped for overwatch... processing overwatch action but it wont do anything");
          ecs.RemoveComponent<OverwatchStatus>(e); // used it
          continue;
        }

        var pos = ecs.GetComponent<GridPositionComponent>(e);
        var min = weapon.min_range;
        var max = weapon.max_range;
        var my_team = ecs.GetComponent<TeamComponent>(e);
        bool activated_overwatch = false;

        // Check if something moved through a path thats in my range...
        for (int i = 0; i < move_events.Count; i++)
        {
          var other = move_events[i].e;
          var other_team = ecs.GetComponent<TeamComponent>(other);
          if (my_team.team == other_team.team)
            continue; // friends are good

          var path = move_events[i].path;
          for (int p = 0; p < path.Length; p++)
          {
            var path_pos = path[p].pos;
            var dst = Mathf.Abs(Vector2Int.Distance(path_pos, pos.position));
            var in_weapon_range = dst >= min && dst <= max;
            if (in_weapon_range)
            {
              activated_overwatch = true;
              break;
            }
          }
        }

        if (activated_overwatch)
        {
          Debug.Log("TODO: You activated an overwatch attack -- implement damage & animations");
          ecs.RemoveComponent<OverwatchStatus>(e); // used it
        }
      }

      move_events.Clear(); // processed
    }
  }

}