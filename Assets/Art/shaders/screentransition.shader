Shader "Unlit/screen_transition"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    _Progress("Progress", Range(0, 1)) = 0 
    _HMM("HMM", Color) = (1, 1, 1,1)
    _HMMAGAIN("HMM AGAIN", Color) = (1, 1, 1,1)
  }
  SubShader
  {
    Tags {
      "RenderPipeline" = "UniversalPipeline" 
      "Queue"="Transparent" 
      "IgnoreProjector"="True" 
      "RenderType"="Transparent"
    }
    LOD 100

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha 

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_fog

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
        float4 worldSpacePos: TEXCOORD1;
        float2 screenPos: TEXCOORD2;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float _Progress;
      float4 _HMM;
      float4 _HMMAGAIN;

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.worldSpacePos = mul(unity_ObjectToWorld, v.vertex);
        o.screenPos = ComputeScreenPos(v.vertex);

        UNITY_TRANSFER_FOG(o,o.vertex);
        return o;
      }

      float when_lt(float x, float y) {
        return max(sign(y - x), 0.0);
      }

      fixed4 frag (v2f i) : SV_Target
      {
        fixed4 col = _HMM;
        float2 UV = i.uv;

        float diamondPixelSize = 50.0;
        float xFract = frac(i.screenPos.x / diamondPixelSize );
        float yFract = frac(i.screenPos.y / diamondPixelSize );

        float xDistance = abs(xFract - 0.5);
        float yDistance = abs(yFract - 0.5);

        if(xDistance + yDistance + UV.x + UV.y > _Progress * 4.0) {
          col = _HMMAGAIN;
        }

        col.a = clamp(1 - _Progress, 0, 1);
        return col;
      }
      ENDCG
    }
  }
}
