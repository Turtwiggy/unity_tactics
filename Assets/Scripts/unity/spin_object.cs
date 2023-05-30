using UnityEngine;

namespace Wiggy
{
  public class spin_object : MonoBehaviour
  {
    public Vector3 rotation;

    void Update()
    {
      transform.Rotate(rotation, Space.World);
    }
  }
}