using UnityEngine;

namespace Wiggy
{
  public class effects : MonoBehaviour
  {
    public bool update_effect_time = false;
    public float effect_time = 2;
    public float pause = 1;
    private ParticleSystem ps;
    // public AnimationCurve fadeIn;

    public bool IsDone
    {
      get
      {
        return !ps.IsAlive();
      }
    }

    public void Start()
    {
      // shaderProperty = Shader.PropertyToID("_cutoff");
      ps = GetComponentInChildren<ParticleSystem>();

      var main = ps.main;
      main.duration = effect_time;
      ps.Play();
    }
  }
}