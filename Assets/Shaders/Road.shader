﻿Shader "Civ Clone/Road" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0.2, 0.2, 0.2)
		_BackgroundColor ("Background Color", Color) = (0, 0, 0)
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
		half3 _BackgroundColor;

		UNITY_INSTANCING_CBUFFER_START(Props)

		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			float4 noise = tex2D(_MainTex, IN.worldPos.xz * 0.025);

			float4 cellData = GetCellDataFromWorld(IN.worldPos);

			float visibility = cellData.x;
			float explored = cellData.y;

			fixed4 c = _Color * (noise.y * 0.75 + 0.25) * lerp(0.25, 1, visibility) * explored;

			float blend = IN.uv_MainTex.x;
			blend *= noise.x + 0.5;
			blend = smoothstep(0.4, 0.7, blend);

			o.Albedo = c.rgb;
			o.Specular = _Specular * explored;
			o.Smoothness = _Glossiness;
			o.Alpha = blend;
			o.Emission = _BackgroundColor * (1 - explored);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
