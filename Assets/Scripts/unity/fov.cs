using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class fov : MonoBehaviour
{
  public float radius;
  [Range(0, 360)]
  public float angle;

  public LayerMask target_mask;
  public LayerMask obstacle_mask;
  public List<Transform> visible_targets;
  private WaitForSeconds seconds_to_wait;

  private void Start()
  {
    seconds_to_wait = new WaitForSeconds(0.2f);
    StartCoroutine("FindTargetsWithDelay");
  }

  IEnumerator FindTargetsWithDelay()
  {
    while (true)
    {
      yield return seconds_to_wait;
      FindVisibleTargets();
    }
  }

  void FindVisibleTargets()
  {
    visible_targets.Clear();
    Collider[] targets_in_view_radius = Physics.OverlapSphere(transform.position, radius, target_mask);
    for (int i = 0; i < targets_in_view_radius.Length; i++)
    {
      Transform target = targets_in_view_radius[i].transform;
      Vector3 dir_to_target = (target.position - transform.position).normalized;
      if (Vector3.Angle(transform.forward, dir_to_target) < angle / 2.0f)
      {
        // within our view angle!

        // is there anything in the way?
        float distance_to_target = Vector3.Distance(transform.position, target.position);
        if (!Physics.Raycast(transform.position, dir_to_target, distance_to_target, obstacle_mask))
          visible_targets.Add(target);
      }
    }
  }

  public Vector3 DirFromAngle(float degrees, bool angle_is_global)
  {
    if (!angle_is_global)
    {
      degrees += transform.eulerAngles.y;
    }
    return new Vector3(Mathf.Sin(degrees * Mathf.Deg2Rad), 0.0f, Mathf.Cos(degrees * Mathf.Deg2Rad));
  }
}
