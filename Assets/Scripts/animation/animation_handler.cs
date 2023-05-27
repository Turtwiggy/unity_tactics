using UnityEngine;

namespace Wiggy
{
  [RequireComponent(typeof(Animator))]
  public class animation_handler : MonoBehaviour
  {
    public static void PlayAnimation(Animator anim, string target_anim, bool is_interacting)
    {
      anim.applyRootMotion = is_interacting;
      anim.SetBool("IsInteracting", is_interacting);
      anim.CrossFade(target_anim, 0.2f);
    }
  }
}