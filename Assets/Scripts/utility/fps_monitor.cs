using UnityEngine;
using TMPro;

namespace Wiggy
{
  public class fps_monitor : MonoBehaviour
  {
    public TextMeshProUGUI text;

    void Update()
    {
      var fps = 1.0f / Time.smoothDeltaTime;
      text.SetText($"FPS:{Mathf.Ceil(fps)}");
    }
  }
}