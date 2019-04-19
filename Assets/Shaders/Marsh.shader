﻿Shader "Civ Clone/Marsh" {
	Properties{
		_MainTex ("Land (RGB)",  2D) = "white" {}
		_WaterTex("Water (RGB)", 2D) = "white" {}
		_SplatMap("Splat Map", 2D) = "white" {}
	}
	SubShader{
		Tags{ "RenderType"="Transparent" "Queue"="Transparent+2" }
		LOD 200
		ZTest Always

		Stencil {
			Ref 0
			Comp Equal
			Pass Keep
		}

		Blend OneMinusDstAlpha DstAlpha, One One
		BlendOp Add, Max

		CGPROGRAM
		#pragma surface surf Standard keepalpha

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _WaterTex;
		sampler2D _SplatMap;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		UNITY_INSTANCING_CBUFFER_START(Props)

		UNITY_INSTANCING_CBUFFER_END

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 waterColor = tex2D(_WaterTex, IN.worldPos.xz * 0.065);
			fixed4 landColor  = tex2D(_MainTex,  IN.worldPos.xz * 0.065);

			float waterAlpha = tex2D(_SplatMap, IN.worldPos.xz * 0.075).r;

			fixed4 c = saturate(waterColor * waterAlpha + landColor * (1 - waterAlpha));

			o.Albedo = c.rgb;
			o.Alpha = IN.uv_MainTex.y * waterAlpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
