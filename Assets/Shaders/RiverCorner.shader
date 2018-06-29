Shader "Custom/River Corner" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard alpha vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "WaterEffects.cginc"
		#include "HexCellData.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 barycentricCoords : COLOR;
			float visibility;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);

			data.visibility = cell0.x * v.color.x + cell1.x * v.color.y;
			data.visibility = lerp(0.25, 1, data.visibility);
		}
		
		UNITY_INSTANCING_CBUFFER_START(Props)
			
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float aWeight = IN.barycentricCoords.x;
			float bWeight = IN.barycentricCoords.y;
			float cWeight = IN.barycentricCoords.z;

			float oppositeA = 1 - aWeight;
			float oppositeB = 1 - bWeight;
			float oppositeC = 1 - cWeight;

			float flowToZ   = lerp(-0.1, 0.1, cWeight);
			float flowFromY = lerp(-0.1, 0.1, oppositeB);

			float flowWeight = 0;

			if (bWeight >= cWeight) {
				flowWeight = 1 - (cWeight / bWeight) * 0.5;
			}
			else {
				flowWeight = (bWeight / cWeight) * 0.5;
			}

			float flowProgress = flowToZ * flowWeight + flowFromY * (1 - flowWeight);

			float u = IN.uv_MainTex.x < 0.5 ? aWeight : oppositeA;
			float v = flowProgress;

			float2 curvyUV = float2(u, v);

			float river = River(curvyUV, _MainTex);

			fixed4 c = saturate(_Color + river);
			o.Albedo = c.rgb * IN.visibility;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
