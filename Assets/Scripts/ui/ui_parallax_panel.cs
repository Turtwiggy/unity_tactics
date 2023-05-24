using UnityEngine;

namespace Wiggy
{

  [RequireComponent(typeof(RectTransform))]
  public class ui_parallax_panel : MonoBehaviour
  {
    //Set to 0 if you want don't want it to rotate along this axis
    public float y_maxRot;

    //Set to 0 if you want don't want it to rotate along this axis
    public float x_maxRot;

    //Speed for the rotation
    public float speed;

    RectTransform rect;
    //The rect we want to rotate
    public RectTransform rectToRotate;

    private void Awake()
    {
      rect = GetComponent<RectTransform>();
    }

    //Our target eulerangles rotation
    Vector2 targetEulerAngles = Vector3.zero;

    /// <summary>
    /// Check each frame where the mouse is and rotates the panel
    /// P.S. if you have multiple panels, use a list of "Panels classes" and use only one update, it's better for performance
    /// You could also look for UI methods from the EventTrigger component
    /// </summary>
    private void DoUpdate()
    {
      //Difference between the mouse pos and the panel's position
      Vector2 diff = (Vector2)transform.position - (Vector2)Input.mousePosition;

      //If the mouse is inside the rect/panel [...]
      if (Mathf.Abs(diff.x) <= (rect.sizeDelta.x / 2f) &&
          Mathf.Abs(diff.y) <= (rect.sizeDelta.y / 2f))
      {
        targetEulerAngles = new Vector3(
            //Rotates along the X axis, based on the Y distance from the centre
            x_maxRot * -Mathf.Clamp(diff.y / (rect.sizeDelta.y / 2f), -1, 1),
            //Same thing, but along the Y axis (so, depends on the X distance)
            y_maxRot * Mathf.Clamp(diff.x / (rect.sizeDelta.x / 2f), -1, 1),
            //No rotation along the Z axis
            0);
      }
      else  //Mouse is outside the rect, target euler is zero
      {
        targetEulerAngles = Vector3.zero;
      }

      // Lerps the rotation
      rectToRotate.eulerAngles = AngleLerp(rectToRotate.eulerAngles, targetEulerAngles, speed * Time.deltaTime);
    }

    // Lerps two angles (using Mathf.LerpAngle for each axis)
    public static Vector3 AngleLerp(Vector3 StartAngle, Vector3 FinishAngle, float t)
    {
      return new Vector3(
          Mathf.LerpAngle(StartAngle.x, FinishAngle.x, t),
          Mathf.LerpAngle(StartAngle.y, FinishAngle.y, t),
          Mathf.LerpAngle(StartAngle.z, FinishAngle.z, t)
          );
    }
  }

}