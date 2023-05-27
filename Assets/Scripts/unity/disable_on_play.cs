
using UnityEngine;

namespace Wiggy
{
  public class disable_on_play : MonoBehaviour
  {
    void Start()
    {
      gameObject.SetActive(false);
    }
  }
}