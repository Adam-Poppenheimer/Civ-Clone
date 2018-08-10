Shader "Custom/Flood Plains" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SplatMap("Splat Map", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader{
		Tags{ "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 200
		Offset -0.1, -0.1

		CGPROGRAM
		#pragma surface surf Standard alpha:fade vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.5

		#include "HexCellData.cginc"

		sampler2D _MainTex;
		sampler2D _SplatMap;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 visibility;
		};

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

			data.visibility =
				cell0.x * v.color.x + cell1.x * v.color.y + cell2.x * v.color.z;
			data.visibility = lerp(0.25, 1, data.visibility);
		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;


		UNITY_INSTANCING_CBUFFER_START(Props)

		UNITY_INSTANCING_CBUFFER_END

		float GetAlpha(float2 worldXZ, sampler2D splatMap, float colorAlpha, float rolloffAlpha) {
			float textureAlpha = tex2D(splatMap, worldXZ * 0.3).r;

			return min(min(textureAlpha, colorAlpha), rolloffAlpha * rolloffAlpha);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 color = tex2D(_MainTex, IN.worldPos.xz * 0.02);
			float alpha = GetAlpha(IN.worldPos.xz, _SplatMap, color.a, IN.uv_MainTex.y);

			o.Albedo = color.rgb * _Color * IN.visibility;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = alpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
