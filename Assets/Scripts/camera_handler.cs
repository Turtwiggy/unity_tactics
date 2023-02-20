using UnityEngine;
using UnityEngine.InputSystem;

namespace Wiggy
{
  public class camera_handler : MonoBehaviour
  {
    Plane ground_plane;
    Camera view_camera;
    Vector3 point;
    public GameObject misc;

    void Start()
    {
      ground_plane = new Plane(Vector3.up, Vector3.zero);
      view_camera = Camera.main;
    }

    void Update()
    {
      Vector2 mouse_pos = Mouse.current.position.ReadValue();
      point = Vector3.zero; // reset variable

      var ray = view_camera.ScreenPointToRay(mouse_pos);
      if (ground_plane.Raycast(ray, out var ray_distance))
        point = ray.GetPoint(ray_distance);

      // alternative
      // var world_pos = Camera.main.ScreenToWorldPoint(mouse_pos);
      // world_pos.z = 0.0f; // ignore z;

      if (point != Vector3.zero)
        misc.transform.position = point;

      // int x = Mathf.Clamp(point.x, 0, max_grid_x - 1);
      // int y = Mathf.Clamp(point.y, 0, max_grid_y - 1);
    }
  }
}
