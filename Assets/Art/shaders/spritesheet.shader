Shader "Custom/spritesheet"
{
  Properties
  {
    _Color("Color", Color) = (0, 0, 0, 1)
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
    _Glossiness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
    _WallSpriteX("WallSpriteX", int) = 0
    _WallSpriteY("WallSpriteY", int) = 0
    [PerRendererData] _SpriteX("SpriteX", int) = 0
    [PerRendererData] _SpriteY("SpriteY", int) = 0
  }

  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 200

    CGPROGRAM
    // Physically based Standard lighting model, and enable shadows on all light types
    #pragma surface surf Standard fullforwardshadows 
    // #pragma multi_compile_instancing

    // Use shader model 3.0 target, to get nicer looking lighting
    #pragma target 3.0

    sampler2D _MainTex;

    struct Input
    {
      float2 uv_MainTex;
      float3 worldNormal;
    };

    fixed4 _Color;
    half _Glossiness;
    half _Metallic;

    int _WallSpriteX;
    int _WallSpriteY;

    // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
    // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
    #pragma instancing_options assumeuniformscaling
    UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(int, _SpriteX)
    UNITY_DEFINE_INSTANCED_PROP(int, _SpriteY)
    UNITY_INSTANCING_BUFFER_END(Props)

    void surf (Input IN, inout SurfaceOutputStandard o)
    {
      float2 uv = IN.uv_MainTex;

      const float sprite_x = 64; // 1024x1024
      const float sprite_y = 64; // 1024x1024
      const float scale_x = 1.0f / sprite_x;
      const float scale_y = 1.0f / sprite_y;

      const int WALL_SPRITE_X = _WallSpriteX;
      const int WALL_SPRITE_Y = sprite_y - _WallSpriteY - 1;

      const int SPRITE_X = UNITY_ACCESS_INSTANCED_PROP(Props, _SpriteX);
      const int SPRITE_Y = UNITY_ACCESS_INSTANCED_PROP(Props, _SpriteY);

      // chosen sprite
      int sprite_pos_x = 0;
      int sprite_pos_y = 0;

      if(IN.worldNormal.x > 0) // RIGHT/XAXIS
      {
        sprite_pos_x = WALL_SPRITE_X;
        sprite_pos_y = WALL_SPRITE_Y;
      }
      else if(IN.worldNormal.y > 0) // UP
      {
        sprite_pos_x = SPRITE_X;
        sprite_pos_y = sprite_y - SPRITE_Y - 1;
      }
      else if(IN.worldNormal.z > 0) // FORWARD/ZAXIS
      {
        sprite_pos_x = WALL_SPRITE_X;
        sprite_pos_y = WALL_SPRITE_Y;
      }
      else if(IN.worldNormal.x <  0)  // LEFT/-XAXIS
      {
        sprite_pos_x = WALL_SPRITE_X;
        sprite_pos_y = WALL_SPRITE_Y;
      }
      else if(IN.worldNormal.y < 0) // DOWN
      {
        sprite_pos_x = WALL_SPRITE_X;
        sprite_pos_y = WALL_SPRITE_Y;
      }
      else if(IN.worldNormal.z < 0) // BACKWARDS/-ZAXIS
      {        
        sprite_pos_x = WALL_SPRITE_X;
        sprite_pos_y = WALL_SPRITE_Y;
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
