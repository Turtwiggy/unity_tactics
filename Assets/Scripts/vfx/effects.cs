using UnityEngine;

namespace Wiggy
{
  public class effects : MonoBehaviour
  {
    public float effect_time = 2;
    public float pause = 1;
    private ParticleSystem _ps;
    private bool Initialized = false;

    public bool IsDone
    {
      get
      {
        if (!Initialized)
          Start();
        return !_ps.IsAlive();
      }
    }

    public void Start()
    {
      if (Initialized)
        return;
      Initialized = true;

      // shaderProperty = Shader.PropertyToID("_cutoff");
      _ps = GetComponentInChildren<ParticleSystem>();
      _ps.Stop();

      var main = _ps.main;
      main.duration = effect_time;
      _ps.Play();
    }
  }
}