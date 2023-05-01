Shader "Custom/spritesheet"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
    _Glossiness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
    _SpriteX("SpriteX", Int) = 0
    _SpriteY("SpriteY", Int) = 0
  }

  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 200

    CGPROGRAM
    // Physically based Standard lighting model, and enable shadows on all light types
    #pragma surface surf Standard fullforwardshadows

    // Use shader model 3.0 target, to get nicer looking lighting
    #pragma target 3.0

    sampler2D _MainTex;

    struct Input
    {
      float2 uv_MainTex;
      float3 worldNormal;
    };

    half _Glossiness;
    half _Metallic;
    fixed4 _Color;
    int _SpriteX;
    int _SpriteY;

    // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
    // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
    // #pragma instancing_options assumeuniformscaling
    UNITY_INSTANCING_BUFFER_START(Props)
    // put more per-instance properties here
    UNITY_INSTANCING_BUFFER_END(Props)

    void surf (Input IN, inout SurfaceOutputStandard o)
    {
      float2 uv = IN.uv_MainTex;

      const float sprite_x = 48; // kennynl
      const float sprite_y = 22; // kennynl
      const float scale_x = 1.0f / sprite_x;
      const float scale_y = 1.0f / sprite_y;

      // chosen sprite
      int sprite_pos_x =_SpriteX;
      int sprite_pos_y =sprite_y - _SpriteY - 1;

      if(IN.worldNormal.x > 0) // RIGHT/XAXIS
      {
        sprite_pos_y = 1;
        sprite_pos_x = 11;
      }
      else if(IN.worldNormal.y > 0) // UP?
      {
        sprite_pos_x = _SpriteX;
        sprite_pos_y = _SpriteY;
      }
      else if(IN.worldNormal.z > 0) // FORWARD/ZAXIS
      {
        sprite_pos_x = 15;
        sprite_pos_y = 3;
      }
      else if(IN.worldNormal.x <  0)  // LEFT/-XAXIS
      {
        sprite_pos_x = 10;
        sprite_pos_y = 4;
      }
      else if(IN.worldNormal.y < 0) // DOWN??
      {
        sprite_pos_x = 11;
        sprite_pos_y = 5;
      }
      else if(IN.worldNormal.z < 0) // BACKWARDS/-ZAXIS
      {        
        sprite_pos_x = 15;
        sprite_pos_y = 6;
      }
      
      float2 sprite_uv = float2(
      uv.x / sprite_x + sprite_pos_x * scale_x,
      uv.y / sprite_y + sprite_pos_y * scale_y
      );

      fixed4 c = tex2D (_MainTex, sprite_uv) * _Color;

      o.Albedo = c.rgb;
      o.Metallic = _Metallic;
      o.Smoothness = _Glossiness;

      o.Alpha = c.a;
    }
    ENDCG
  }
  FallBack "Diffuse"
}
