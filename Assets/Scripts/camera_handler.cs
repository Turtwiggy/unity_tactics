using UnityEngine;
using UnityEngine.InputSystem;

namespace Wiggy
{
  public class camera_handler : MonoBehaviour
  {
    private Plane ground_plane;
    private Camera view_camera;
    private input_handler input_handler;

    private float camera_move_speed = 20.0f;
    private Vector3 fixed_lookat_point;

    public GameObject camera_follow;
    public GameObject camera_lookat;
    public GameObject cursor;

    void Start()
    {
      ground_plane = new Plane(Vector3.up, Vector3.zero);
      view_camera = Camera.main;
      input_handler = FindObjectOfType<input_handler>();
    }

    void Update()
    {
      // int x = Mathf.Clamp(point.x, 0, max_grid_x - 1);
      // int y = Mathf.Clamp(point.y, 0, max_grid_y - 1);
      HandleCursor();

      // try use lmb to fix lookat point
      if (Mouse.current.leftButton.wasPressedThisFrame)
        fixed_lookat_point = cursor.transform.position;

      // try use to remove lookat point
      if (Mouse.current.rightButton.wasPressedThisFrame)
        fixed_lookat_point = Vector3.zero;
    }

    private void LateUpdate()
    {
      float delta = Time.deltaTime;
      HandleCameraMovement(delta);

      // warning: a bug if fixed lookat point is actually 0,0,0
      if (fixed_lookat_point != Vector3.zero)
        camera_lookat.transform.position = fixed_lookat_point;
      else
      {
        camera_lookat.transform.position = new Vector3()
        {
          x = camera_follow.transform.position.x,
          y = 0f,
          z = camera_follow.transform.position.z + 5f
        };
      }
    }

    private void HandleCursor()
    {
      Vector2 mouse_pos = Mouse.current.position.ReadValue();
      var ray = view_camera.ScreenPointToRay(mouse_pos);
      if (ground_plane.Raycast(ray, out var ray_distance))
      {
        var point = ray.GetPoint(ray_distance);

        // TODO: should probably use smoothdamp or something
        cursor.transform.position = new Vector3(point.x, cursor.transform.position.y, point.z);
      }
    }

    private void HandleCameraMovement(float delta)
    {
      var h = input_handler.l_analogue.x;
      var v = input_handler.l_analogue.y;

      Vector3 move_dir = camera_follow.transform.forward * v;
      move_dir += camera_follow.transform.right * h;
      move_dir.Normalize();
      move_dir.y = 0;

      //  TODO: should probably use smoothdamp or something
      camera_follow.transform.position += camera_move_speed * delta * move_dir;
    }
  }
} // namespace Wiggy
