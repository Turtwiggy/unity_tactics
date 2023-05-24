// using UnityEngine;

// namespace Wiggy
// {
//   [RequireComponent(typeof(Animator))]
//   public class animation_handler : MonoBehaviour
//   {
//     public Animator anim;
//     int vertical;
//     int horizontal;

//     public void Start()
//     {
//       anim = GetComponent<Animator>();
//       vertical = Animator.StringToHash("Vertical");
//       horizontal = Animator.StringToHash("Horizontal");
//     }

//     public void UpdateAnimatorValues(float v, float h)
//     {
//       v = Mathf.Clamp01(v);
//       h = Mathf.Clamp01(h);
//       anim.SetFloat(vertical, v, 0.1f, Time.deltaTime);
//       anim.SetFloat(horizontal, h, 0.1f, Time.deltaTime);
//     }

//     public void PlayTargetAnimation(string anim_name)
//     {
//       // anim.applyRootMotion = is_interacting;
//       // anim.SetBool("IsInteracting", is_interacting);
//       anim.CrossFade(anim_name, 0.2f);
//     }

//     private void OnAnimatorMove()
//     {
//       // float delta = Time.deltaTime;
//       // player_locomotion.rigidbody.drag = 0;
//       // Vector3 delta_position = anim.deltaPosition;
//       // delta_position.y = 0;
//       // Vector3 velocity = delta_position / delta;
//       // player_locomotion.rigidbody.velocity = velocity;
//     }
//   }
// }