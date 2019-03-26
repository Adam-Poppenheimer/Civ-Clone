Shader "Civ Clone/Marsh" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SplatMap("Splat Map", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Specular("Specular", Color) = (0.2, 0.2, 0.2)
	}
	SubShader{
		Tags{ "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200

		CGPROGRAM
		#pragma surface surf StandardSpecular alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		//#include "WaterEffects.cginc"
		#include "HexCellData.cginc"

		sampler2D _MainTex;
		sampler2D _SplatMap;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float2 visibility;
		};

		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;


		UNITY_INSTANCING_CBUFFER_START(Props)

		UNITY_INSTANCING_CBUFFER_END

		float GetAlpha(float2 worldXZ, sampler2D splatMap, float colorAlpha, float rolloffAlpha) {
			float textureAlpha = tex2D(splatMap, worldXZ * 0.075).r;

			return min(min(textureAlpha, colorAlpha), rolloffAlpha * rolloffAlpha);
		}

		void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
			/*float waves = Waves(IN.worldPos.xz, _MainTex, 0.1) * 0.35;

			fixed4 c = saturate(_Color + waves);
			float alpha = GetAlpha(IN.worldPos.xz, _SplatMap, c.a, IN.uv_MainTex.y);

			float explored = IN.visibility.y;

			o.Albedo = c.rgb * IN.visibility.x;
			o.Specular = _Specular * explored;
			o.Smoothness = _Glossiness;
			o.Occlusion = explored;
			o.Alpha = alpha * explored;*/
		}
		ENDCG
	}
	FallBack "Diffuse"
}
