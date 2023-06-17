using UnityEngine;

namespace Wiggy
{
  public class effects : MonoBehaviour
  {
    public float effect_time = 2;
    private bool Initialized = false;

    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private bool is_done;

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

    private void Update()
    {
      is_done = IsDone;
    }

  }
}