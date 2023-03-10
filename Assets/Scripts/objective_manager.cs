using UnityEngine;
using TMPro;

namespace Wiggy
{
  public class objective_manager : MonoBehaviour
  {
    private map_manager map;
    private camera_handler cam;

    public Color player_held;
    public Color enemy_held;
    public Color default_held;

    public GameObject objective_holder_manager;

    public TMP_Text player_held_text;
    public TMP_Text enemy_held_text;

    public void DoStart()
    {
      map = FindObjectOfType<map_manager>();
      cam = FindObjectOfType<camera_handler>();

      foreach (Transform tr in objective_holder_manager.transform)
      {
        var zone = tr.gameObject.GetComponent<objective_hold_zone>();
        var zone_pos = zone.transform.position;

        float t = zone_pos.z + .25f;
        float b = zone_pos.z - .25f;
        float l = zone_pos.x - .25f;
        float r = zone_pos.x + .25f;

        var zone_tl = new Vector3(l, zone_pos.y, t);
        var zone_tr = new Vector3(r, zone_pos.y, t);
        var zone_bl = new Vector3(l, zone_pos.y, b);
        var zone_br = new Vector3(r, zone_pos.y, b);

        var zone_tl_idx = Grid.WorldSpaceToIndex(zone_tl, cam.grid_size, cam.grid_width);
        var zone_tr_idx = Grid.WorldSpaceToIndex(zone_tr, cam.grid_size, cam.grid_width);
        var zone_bl_idx = Grid.WorldSpaceToIndex(zone_bl, cam.grid_size, cam.grid_width);
        var zone_br_idx = Grid.WorldSpaceToIndex(zone_br, cam.grid_size, cam.grid_width);

        // one index points to a shared zone
        map.objective_spots[zone_tl_idx] = zone;
        map.objective_spots[zone_tr_idx] = zone;
        map.objective_spots[zone_bl_idx] = zone;
        map.objective_spots[zone_br_idx] = zone;

        // a shared zone knows which indicies it is part of
        zone.AddIndex(zone_tl_idx);
        zone.AddIndex(zone_tr_idx);
        zone.AddIndex(zone_bl_idx);
        zone.AddIndex(zone_br_idx);
      }
    }

    public void UnitMovedEvent(Vector2Int new_pos)
    {
      // var index = Grid.GetIndex(new_pos, cam.grid_width);
      // var objective = map.objective_spots[index];
      // var team = map.gos[index].GetComponent<comp_team>();

      // Refresh ALL objective state
      int player_objectives_held = 0;
      int enemy_objectives_held = 0;

      foreach (Transform t in objective_holder_manager.transform)
      {
        var obj = t.gameObject.GetComponent<objective_hold_zone>();
        var team = obj.WhichTeamControlsThisPoint();

        if (team == Team.PLAYER)
        {
          player_objectives_held++;
          obj.SetColour(player_held);
        }

        else if (team == Team.ENEMY)
        {
          enemy_objectives_held++;
          obj.SetColour(enemy_held);
        }

        else
          obj.SetColour(default_held);
      }

      player_held_text.SetText(player_objectives_held.ToString());
      enemy_held_text.SetText(enemy_objectives_held.ToString());
    }
  }
}
