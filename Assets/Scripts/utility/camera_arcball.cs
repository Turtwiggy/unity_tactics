using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arcball : MonoBehaviour
{
  public float damping = 0.9f;
  public float speed = 0.1f;

  private Vector3 vDown;
  private Vector3 vDrag;
  private bool dragging;
  private float angularVelocity;
  private Vector3 rotationAxis;

  void Start()
  {
    dragging = false;
    angularVelocity = 0;
    rotationAxis = Vector3.zero;
  }

  // Update is called once per frame
  void Update()
  {
    // on mouse down
    if (Input.GetMouseButton(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;

      // if the object was clicked
      if (Physics.Raycast(ray, out hit))
      {
        if (!dragging)
        {
          // extract vDown from the RaycastHit
          vDown = hit.point - transform.position;

          // start dragging
          dragging = true;
        }
        else
        {
          // extract vDrag from the RaycastHit
          vDrag = hit.point - transform.position;

          // compute the rotation axis and angular velocity from vDown and vDrag
          rotationAxis = Vector3.Cross(vDown, vDrag);
          angularVelocity = Vector3.Angle(vDown, vDrag) * speed;
        }
      }
      else
        dragging = false;
    }

    // on mouse up stop dragging
    if (Input.GetMouseButtonUp(0))
      dragging = false;

    // apply the angular velocity
    if (angularVelocity > 0)
    {
      transform.RotateAround(rotationAxis, angularVelocity * Time.deltaTime);
      angularVelocity = (angularVelocity > 0.01f) ? angularVelocity * damping : 0;
    }
  }
}
