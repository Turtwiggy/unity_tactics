Shader "Custom/floor"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _LineColour ("Line Colour", Color) = (1,1,1,1)
    _NoiseTex ("Noise", 2D) = "white" {}
    _Glossiness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
    _XSpeed("X Speed", Float) = 10
    _YSpeed("Y Speed", Float) = 20
    _GridSections("Grid Sections", Range(1, 100)) = 30
    _LineSize("Line Size", Range(0, 1)) = 0.001
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 200

    CGPROGRAM
    #pragma surface surf Standard fullforwardshadows nolightmap
    #pragma target 3.0

    sampler2D _NoiseTex;

    struct Input
    {
      float2 uv_MainTex;
      float2 uv_NoiseTex;
    };

    half _Glossiness;
    half _Metallic;
    fixed4 _Color;
    fixed4 _LineColour;
    float _XSpeed;
    float _YSpeed;
    int _GridSections;
    float _LineSize;

    float posterize(float v, float k) {
      return ceil(v*k)/k;
    }

    void surf (Input IN, inout SurfaceOutputStandard o)
    {
      const float2 uv = IN.uv_MainTex;
      
      fixed3 c = _Color;

      // Grid Approach A
      // https://madebyevan.com/shaders/grid/
      // float2 grid_uv = uv * _GridSections;
      // float2 grid = abs( frac(grid_uv - 0.5f) - 0.5f) / fwidth(grid_uv);
      // float l = min(grid.x, grid.y);
      // float color = min(l, 1.0f);

      // Grid Approach B
      // https://smoothslerp.com/diffused-posterization/
      const int k = _GridSections;
      const float inv_k = 1.0/k;
      fixed x_post = posterize(uv.x, k);
      fixed y_post = posterize(uv.y, k);
      float2 noise_uv = IN.uv_NoiseTex + float2(_Time.y/_XSpeed, _Time.y/_YSpeed);
      fixed noise = tex2D(_NoiseTex, noise_uv);

      fixed x_bar = step(uv.x, x_post) - step(uv.x, x_post - noise/k);
      fixed y_bar = step(uv.y, y_post) - step(uv.y, y_post - noise/k);
      fixed3 x_col = (1 - x_bar) + x_bar * _LineColour;
      fixed3 y_col = (1 - y_bar) + y_bar * _LineColour;

      c.rgb *= x_col * y_col;

      o.Albedo = c.rgb;
      o.Metallic = _Metallic;
      o.Smoothness = _Glossiness;
      o.Alpha = _Color.a;
    }
    ENDCG
  }
  FallBack "Diffuse"
}
