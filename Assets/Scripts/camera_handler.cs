using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace Wiggy
{
  [System.Serializable]
  public class camera_handler
  {
    private readonly float camera_move_speed = 20.0f;
    private readonly float camera_rotate_speed = 5.0f;
    private readonly Vector2 y_clamp = new(5, 30);
    private readonly Vector2 x_clamp = new(0, 30);
    private readonly Vector2 z_clamp = new(-10, 30);

    private map_manager map;
    private input_handler input;

    private Transform camera_parent;
    private Camera camera_main;
    private CinemachineTransposer camera_virtual;

    private Plane ground_plane;
    private bool mouse_ground_intersection;
    private Vector3 mouse_ground_intersection_point;
    private Vector3 start_drag;

    public Vector2Int grid_index { get; private set; }
    private GameObject cursor_instance;

    public void DoStart(map_manager map, input_handler input, GameObject cursor_prefab)
    {
      this.map = map;
      this.input = input;

      ground_plane = new Plane(Vector3.up, Vector3.zero);
      camera_parent = GameObject.Find("camera-follow").transform;
      camera_main = Camera.main;
      camera_virtual = GameObject.Find("camera-virtual")
        .GetComponent<CinemachineVirtualCamera>()
        .GetCinemachineComponent<CinemachineTransposer>();
      cursor_instance = Object.Instantiate(cursor_prefab);
      cursor_instance.name = "Cursor Instance";
    }

    public void DoUpdate(float delta)
    {
      // Inputs
      var mouse_pos = Mouse.current.position.ReadValue();

      // mouse-to-groundplane intersection
      var ray = camera_main.ScreenPointToRay(mouse_pos);
      mouse_ground_intersection = ground_plane.Raycast(ray, out var ray_distance);

      if (mouse_ground_intersection)
      {
        mouse_ground_intersection_point = ray.GetPoint(ray_distance);
        grid_index = Grid.WorldSpaceToGridSpace(mouse_ground_intersection_point, map.size, map.width);
        SetCursorToGrid();
      }
    }

    public void DoLateUpdate(float delta)
    {
      var delta_x = input.r_analogue.x;
      var rmb = Mouse.current.rightButton.isPressed;
      var mmb = Mouse.current.middleButton.isPressed;

      HandleCameraMovement(delta, input.l_analogue);

      if (rmb)
        HandleCameraDrag();
      if (mmb)
        HandleCameraRotation(delta_x);

      // Clamp camera in boundaries 
      var pos = camera_parent.transform.position;
      pos.x = Mathf.Clamp(pos.x, x_clamp.x, x_clamp.y);
      pos.y = Mathf.Clamp(pos.y, y_clamp.x, y_clamp.y);
      pos.z = Mathf.Clamp(pos.z, z_clamp.x, z_clamp.y);
      camera_parent.transform.position = pos;

      HandleCameraZoom();
    }

    private void HandleCameraRotation(float delta_x)
    {
      camera_parent.transform.rotation = Quaternion.Euler(
        camera_parent.transform.rotation.eulerAngles.x,
        delta_x * camera_rotate_speed + camera_parent.transform.rotation.eulerAngles.y,
        0.0f
      );
    }

    private void HandleCameraDrag()
    {
      if (mouse_ground_intersection)
      {
        if (Mouse.current.rightButton.wasPressedThisFrame)
          start_drag = mouse_ground_intersection_point;
        else
          camera_parent.transform.position += start_drag - mouse_ground_intersection_point;
      }
    }

    private void HandleCameraMovement(float delta, Vector2 amount)
    {
      var fwd = camera_parent.transform.forward;
      fwd.y = 0;
      var rgt = camera_parent.transform.right;
      rgt.y = 0;

      Vector3 move_dir;
      move_dir = fwd * amount.y + rgt * amount.x;
      move_dir.Normalize();
      move_dir.y = 0;

      camera_parent.transform.position += camera_move_speed * delta * move_dir;
    }

    private void HandleCameraZoom()
    {
      // Scroll camera up/down
      float mouse_y = Mouse.current.scroll.ReadValue().y;

      if (mouse_y > 0.05f)
        camera_parent.transform.position += Vector3.up;

      if (mouse_y < -0.05f)
        camera_parent.transform.position -= Vector3.up;

      camera_virtual.m_FollowOffset = new Vector3(0, 1, -1) * camera_parent.transform.position.y;
    }

    private void SetCursorToGrid()
    {
      var world_space = Grid.GridSpaceToWorldSpace(grid_index, map.size);
      cursor_instance.transform.position = world_space;
    }

  }
} // namespace Wiggy
