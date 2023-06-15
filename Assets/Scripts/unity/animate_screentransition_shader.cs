using UnityEngine;
using UnityEngine.UI;

namespace Wiggy
{
  [RequireComponent(typeof(Image))]
  public class animate_shader : MonoBehaviour
  {
    [SerializeField]
    private float timer = 0;
    [SerializeField]
    private float progress = 0;
    private float animation_time = 2f;

    Image img;
    Material mat;

    private void Start()
    {
      img = GetComponent<Image>();
      mat = img.material;
      SetAlpha(1);
    }

    void Update()
    {
      timer += Time.deltaTime / animation_time;
      timer = Mathf.Clamp01(timer);

      progress = timer;
      mat.SetFloat("_Progress", progress);
      if (progress >= 1.0)
        gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
      var col = img.color;
      col.a = alpha;
      img.color = col;
    }

  }
}
