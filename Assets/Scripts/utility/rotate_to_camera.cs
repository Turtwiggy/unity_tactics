using UnityEngine;

namespace Wiggy
{
  public class util_rotate_to_camera : MonoBehaviour
  {
    public static void Rotate(GameObject go)
    {
      var fwd = Camera.main.transform.forward;
      go.transform.rotation = Quaternion.LookRotation
      (
          fwd
      );
    }
  }
}
