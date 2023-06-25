using UnityEngine;

namespace Wiggy
{
  public class persistent_economy : MonoBehaviour
  {
    private static persistent_economy singleton;

    // should probably be called by scene's Main()
    void Start()
    {
      if (singleton == null)
      {
        singleton = this;
        DontDestroyOnLoad(this);
      }
      else
        Destroy(this.gameObject);
      Debug.Log("persistent_economy start() called");

    }
  }
}