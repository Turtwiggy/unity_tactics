Shader "Unlit/grid_unlit"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    [HDR] _GridColour("GridColour", Color) = (0,0,0,0)
    _GridSize("GridSize", Range(0.01, 1.0)) = 0.1
    _GridLineThickness ("Grid Line Thickness", Range(0.00001, 0.010)) = 0.003
  }
  SubShader
  {
    Tags{ "RenderType"="Transparent" "Queue"="Transparent"}
    Blend SrcAlpha OneMinusSrcAlpha
    LOD 100

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      // make fog work
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
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float4 _GridColour;
      float _GridSize;
      float _GridLineThickness;
      fixed4 _Color;

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        UNITY_TRANSFER_FOG(o,o.vertex);
        return o;
      }

      float GridTest(float2 r) {
        float result;
        for (float i = 0.0; i <= 1; i += _GridSize) {
          for (int j = 0; j < 2; j++) {
            result += 1.0 - smoothstep(0.0, _GridLineThickness,abs(r[j] - i));
          }
        }
        return result;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        fixed4 grid_colour = (_GridColour * GridTest(i.uv));
        fixed4 col = grid_colour;

        // apply fog
        UNITY_APPLY_FOG(i.fogCoord, col);
        return col;
      }
      ENDCG
    }
  }
}
