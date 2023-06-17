using UnityEngine;
using UnityEngine.InputSystem;

namespace Wiggy
{
  public class camera_handler : MonoBehaviour
  {
    private map_manager map;

    private Plane ground_plane;
    private Camera view_camera;

    private Vector2 y_clamp = new(5, 15);
    private Vector2 x_clamp = new(0, 30);
    private Vector2 z_clamp = new(-10, 30);

    private float camera_move_speed = 20.0f;
    private Vector3 fixed_lookat_point;

    public Vector2Int grid_index { get; private set; }
    public GameObject camera_follow;
    // public GameObject camera_lookat;
    public GameObject cursor_prefab;
    private GameObject cursor_instance;

    bool camera_z_lock;
    public float camera_follow_z_lock = 8f;

    public void Start()
    {
      map = FindObjectOfType<map_manager>();
      ground_plane = new Plane(Vector3.up, Vector3.zero);
      view_camera = Camera.main;
      cursor_instance = Instantiate(cursor_prefab);
      cursor_instance.name = "Cursor Instance";
    }

    public void HandleCursor()
    {
      Vector2 mouse_pos = Mouse.current.position.ReadValue();
      var ray = view_camera.ScreenPointToRay(mouse_pos);
      if (ground_plane.Raycast(ray, out var ray_distance))
      {
        var point = ray.GetPoint(ray_distance);

        // TODO: should probably use smoothdamp or something
        cursor_instance.transform.position = new Vector3(point.x, cursor_instance.transform.position.y, point.z);
      }
    }

    public void HandleCursorOnGrid()
    {
      Vector2 mouse_pos = Mouse.current.position.ReadValue();
      var ray = view_camera.ScreenPointToRay(mouse_pos);
      if (ground_plane.Raycast(ray, out var ray_distance))
      {
        var point = ray.GetPoint(ray_distance);
        var grid_index = Grid.WorldSpaceToGridSpace(point, map.size, map.width);
        var world_space = Grid.GridSpaceToWorldSpace(grid_index, map.size);

        // TODO: should probably use smoothdamp or something
        cursor_instance.transform.position = world_space;

        this.grid_index = grid_index;
      }
    }

    public void HandleCameraMovement(float delta, Vector2 amount)
    {
      Vector3 move_dir = camera_follow.transform.forward * amount.y;
      move_dir += camera_follow.transform.right * amount.x;
      move_dir.Normalize();
      move_dir.y = 0;

      camera_follow.transform.position += camera_move_speed * delta * move_dir;

      // Clamp camera in boundaries 
      var pos = camera_follow.transform.position;
      pos.x = Mathf.Clamp(pos.x, x_clamp.x, x_clamp.y);
      pos.y = Mathf.Clamp(pos.y, y_clamp.x, y_clamp.y);
      pos.z = Mathf.Clamp(pos.z, z_clamp.x, z_clamp.y);
      camera_follow.transform.position = pos;
    }

    public void HandleCameraLookAt()
    {
      // warning: a bug if fixed lookat point is actually 0,0,0
      // if (fixed_lookat_point != Vector3.zero)
      //   camera_lookat.transform.position = fixed_lookat_point;
      // else
      // {
      //   // fixes the lookat a point but moves along the x-axis
      //   if (camera_z_lock)
      //   {
      //     camera_lookat.transform.position = new Vector3()
      //     {
      //       x = camera_follow.transform.position.x,
      //       y = 0f,
      //       z = camera_follow_z_lock
      //     };
      //   }
      //   else
      //   {
      //     // This a lookat point z+5 in front of the camera
      //     camera_lookat.transform.position = new Vector3()
      //     {
      //       x = camera_follow.transform.position.x,
      //       y = 0f,
      //       z = camera_follow.transform.position.z + 5f
      //     };
      //   }
      // }
    }

    public void HandleCameraZoom()
    {
      // Scroll camera up/down
      float mouse_y = Mouse.current.scroll.ReadValue().y;

      if (mouse_y > 0.05f)
        camera_follow.transform.position += Vector3.up;

      if (mouse_y < -0.05f)
        camera_follow.transform.position -= Vector3.up;
    }
  }
} // namespace Wiggy
