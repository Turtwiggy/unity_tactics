Shader "Custom/floor"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _LineColour ("Line Colour", Color) = (1,1,1,1)
    _NoiseTex ("Noise", 2D) = "white" {}
    _XSpeed("X Speed", Float) = 10
    _YSpeed("Y Speed", Float) = 20
    _GridSections("Grid Sections", Range(1, 100)) = 30
    _Antialiasing("Band Smoothing", Float) = 5.0
    _Glossiness("Glossiness/Shininess", Float) = 400
    _Fresnel("Fresnel/Rim Amount", Range(0, 1)) = 0.5
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }

    CGPROGRAM
    #pragma surface surf Cel fullforwardshadows nolightmap
    #pragma target 3.0
    
    sampler2D _NoiseTex;
    
    float _Antialiasing;
    float _Glossiness;
    float _Fresnel;
    fixed4 _Color;
    fixed4 _LineColour;
    float _XSpeed;
    float _YSpeed;
    int _GridSections;
    
    // https://danielilett.com/2019-06-23-tut2-5-cel-shading-end/
    float4 LightingCel(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
    {
      float3 normal = normalize(s.Normal);
      float diffuse = dot(normal, lightDir);
      float delta = fwidth(diffuse) * _Antialiasing;
      float diffuseSmooth = smoothstep(0, delta, diffuse);

      float3 halfVec = normalize(lightDir + viewDir);
      float specular = dot(normal, halfVec);
      specular = pow(specular * diffuseSmooth, _Glossiness);
      float specularSmooth = smoothstep(0, 0.01 * _Antialiasing, specular);

      float rim = 1 - dot(normal, viewDir);
      rim = rim * pow(diffuse, 0.3);
      float fresnelSize = 1 - _Fresnel;

      float rimSmooth = smoothstep(fresnelSize, fresnelSize * 1.1, rim);

      float3 col = s.Albedo * ((diffuseSmooth + specularSmooth + rimSmooth) * _LightColor0 + unity_AmbientSky);
      return float4(col, s.Alpha);
    }

    struct Input
    {
      float2 uv_MainTex;
      float2 uv_NoiseTex;
    };

    float posterize(float v, float k) {
      return ceil(v*k)/k;
    }

    void surf (Input IN, inout SurfaceOutput o)
    {
      const float2 uv = IN.uv_MainTex;
      fixed4 c = _Color;

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

      fixed x_bar = step(uv.x, x_post) - step(uv.x, x_post - 0.002f - noise/k);
      fixed y_bar = step(uv.y, y_post) - step(uv.y, y_post - 0.002f - noise/k);
      fixed3 x_col = (1 - x_bar) + x_bar * _LineColour;
      fixed3 y_col = (1 - y_bar) + y_bar * _LineColour;
      c.rgb *= x_col * y_col;

      // o.Albedo = c;
      o.Albedo = c.rgb;
      o.Alpha = _Color.a;
    }
    ENDCG
  }
  FallBack "Diffuse"
}
