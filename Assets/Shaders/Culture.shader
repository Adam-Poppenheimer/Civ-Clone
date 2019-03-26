Shader "Civ Clone/Culture" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0.2, 0.2, 0.2)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
		Offset -5, -5
		LOD 200
		
		Stencil {
			Ref 0
			Comp Always
			Pass Keep
		}

		CGPROGRAM
		#pragma surface surf StandardSpecular alpha:fade

		#pragma target 3.0

		#include "HexCellData.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float3 worldPos;
		};

		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			fixed4 c = _Color;

			int cellIndex = GetCellIndexFromWorld(IN.worldPos);

			float4 cellData = GetCellData(cellIndex);

			o.Albedo = c.rgb * IN.color * lerp(0.25, 1, cellData.x);
			o.Specular = _Specular;
			o.Smoothness = _Glossiness;

			o.Alpha = IN.uv_MainTex.y;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
