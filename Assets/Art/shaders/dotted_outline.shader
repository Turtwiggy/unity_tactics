Shader "Unlit/dotted_outline"
{
  Properties
  {
    _AColour("A Colour", Color) = (1, 1, 1,1)

  }
  SubShader
  {
    Tags {
      "RenderPipeline" = "UniversalPipeline" 
      "Queue"="Transparent" 
      "IgnoreProjector"="True" 
      "RenderType"="Transparent"
    }
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha 

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
        float4 worldSpacePos: TEXCOORD1;
        float4 screenCoord : TEXCOORD2;
      };

      float4 _AColour;

      v2f vert (appdata v)
      {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.uv = v.uv;
        o.worldSpacePos = mul(unity_ObjectToWorld, v.vertex);
        o.screenCoord = ComputeScreenPos(v.vertex);

        return o;
      }

      // fixed3 draw_rectangle(fixed2 tl, float width, fixed2 st)
      // {
        //   fixed2 coord = st + tl;

        //   fixed2 wh = float2(width, width);
        //   float2 bl = step(wh, coord);       // bottom-left
        //   float2 tr = step(wh, 1.0-coord);   // top-right
        //   float c = bl.x * bl.y * tr.x * tr.y;
        //   return fixed3(c, c, c);
      // }

      // fixed3 draw_rectangle_smoothstep(float width, fixed2 st)
      // {
        //   fixed2 wh = float2(width, width);
        //   float2 bl = smoothstep(wh.x - 0.05, wh.y + 0.05, st);       // bottom-left
        //   float2 tr = smoothstep(wh.x - 0.05, wh.y + 0.05, 1.0-st);   // top-right
        //   float c = bl.x * bl.y * tr.x * tr.y;
        //   return fixed3(c, c, c);
      // }

      float posterize(float v, float k) {
        return ceil(v*k)/k;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        float2 st = i.uv;
        // float2 textureCoordinate = i.screenPos.xy / i.screenPos.w; // perspective divide
        // float aspect = _ScreenParams.x / _ScreenParams.y;

        // convert st from 0->1 to -1->1
        // float2 c = ((st * 2.0) - 1);

        const float epsilon = 0.001;
        float sections = 101;
        fixed post_x = posterize(i.uv.x, sections);
        fixed post_y = posterize(i.uv.y, sections);

        float section_x = post_x * (sections);
        float section_y = post_y * (sections);

        fixed4 out_col = fixed4(post_x, 0, post_y, 1);

        if((section_x) % 2 == 0)
        {
          clip(-1);
        }
        if((section_y) % 2 == 0)
        {
          clip(-1);
        }

        float ok = 0;
        ok += section_x == 1 ? 1 : 0;
        ok += section_y == 1 ? 1 : 0;
        ok += section_x == sections ? 1 : 0;
        ok += section_y == sections ? 1: 0;
        if(ok == 0)
        {
          clip(-1);
        }

        return out_col;
      }
      ENDCG
    }
  }
}
