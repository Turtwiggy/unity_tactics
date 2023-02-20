Shader "Custom/vertex_colours"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Terrain Texture Array", 2DArray) = "white" {}
        _GridTex ("Grid Texture", 2D) = "white" {}
        _Size ("Size", Range(0,30)) = 1.0
        _Slider ("Slider", Range(0,30)) = 0.025
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        // Increase to target 3.5 to enable texture arrays on all platforms that support it
		#pragma target 3.5
		#pragma multi_compile _ GRID_ON

		UNITY_DECLARE_TEX2DARRAY(_MainTex);
        sampler2D _GridTex;

        struct Input
        {
            float4 colour: COLOR;
            float3 worldPos;
            float3 terrain;
        };

        void vert (inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			data.terrain = v.texcoord2.xyz;
		}

        half _Size;
        half _Slider;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float4 GetTerrainColor (Input IN, int index) {
			float3 uvw = float3(IN.worldPos.xz * _Slider, IN.terrain[index]);
			float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);
			return c * IN.colour[index];
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
		    fixed4 c =
				GetTerrainColor(IN, 0) +
				GetTerrainColor(IN, 1) +
				GetTerrainColor(IN, 2);
            fixed3 albedo = c.rgb * _Color;

            fixed4 grid = 1;
			#if defined(GRID_ON)
                float2 gridUV = IN.worldPos.xz;
                gridUV.x *= 1 / (4 * (_Size * 0.866025404f));
                gridUV.y *= 1 / (2 * (_Size * 3/2.0));
                grid = tex2D(_GridTex, gridUV);
                
                albedo *= grid;
			#endif

            o.Albedo = albedo;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
