using UnityEngine;

namespace Wiggy
{
  public class effects : MonoBehaviour
  {
    public bool update_effect_time = false;
    public float effect_time = 2;
    public float pause = 1;

    [SerializeField]
    private ParticleSystem _ps;

    // public AnimationCurve fadeIn;

    public bool IsDone
    {
      get
      {
        if (_ps == null)
          _ps = GetComponentInChildren<ParticleSystem>();
        return !_ps.IsAlive();
      }
    }

    public void Start()
    {
      // shaderProperty = Shader.PropertyToID("_cutoff");
      _ps = GetComponentInChildren<ParticleSystem>();
      _ps.Stop();

      var main = _ps.main;
      main.duration = effect_time;
      _ps.Play();
    }
  }
}