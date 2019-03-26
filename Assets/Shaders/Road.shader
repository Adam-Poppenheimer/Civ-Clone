Shader "Civ Clone/Road" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0.2, 0.2, 0.2)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Transparent+1" }
		LOD 200
		Offset -1, -1
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows decal:blend

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "HexCellData.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;

		UNITY_INSTANCING_CBUFFER_START(Props)

		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			float4 noise = tex2D(_MainTex, IN.worldPos.xz * 0.025);

			int cellIndex = GetCellIndexFromWorld(IN.worldPos);

			float4 cellData = GetCellData(cellIndex);

			fixed4 c = _Color * (noise.y * 0.75 + 0.25) * lerp(0.25, 1, cellData.x);

			float blend = IN.uv_MainTex.x;
			blend *= noise.x + 0.5;
			blend = smoothstep(0.4, 0.7, blend);

			o.Albedo = c.rgb;
			o.Specular = _Specular;
			o.Smoothness = _Glossiness;
			o.Alpha = blend;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
