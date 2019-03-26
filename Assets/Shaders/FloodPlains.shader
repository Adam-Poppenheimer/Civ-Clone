Shader "Civ Clone/Flood Plains" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SplatMap("Splat Map", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Specular("Specular", Color) = (0.2, 0.2, 0.2)
	}
	SubShader{
		Tags{ "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 200
		Offset -0.1, -0.1

		CGPROGRAM
		#pragma surface surf StandardSpecular alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.5

		#include "HexCellData.cginc"

		sampler2D _MainTex;
		sampler2D _SplatMap;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;


		UNITY_INSTANCING_CBUFFER_START(Props)

		UNITY_INSTANCING_CBUFFER_END

		float GetAlpha(float2 worldXZ, sampler2D splatMap, float colorAlpha, float rolloffAlpha) {
			float textureAlpha = tex2D(splatMap, worldXZ * 0.3).r;

			return min(min(textureAlpha, colorAlpha), rolloffAlpha * rolloffAlpha);
		}

		void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
			int cellIndex = GetCellIndexFromWorld(IN.worldPos);

			float4 cellData = GetCellData(cellIndex);

			fixed4 color = tex2D(_MainTex, IN.worldPos.xz * 0.02);
			float alpha = GetAlpha(IN.worldPos.xz, _SplatMap, color.a, IN.uv_MainTex.y);

			o.Albedo = color.rgb * _Color * lerp(0.25, 1, cellData.x);
			o.Specular = _Specular;
			o.Smoothness = _Glossiness;
			o.Alpha = alpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
