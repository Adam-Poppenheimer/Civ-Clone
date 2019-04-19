Shader "Civ Clone/Culture" {
	Properties {
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BackgroundColor("Background Color", Color) = (0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200
		ZTest Always

		Stencil {
			Ref 0
			Comp Equal
			Pass Keep
		}

		Blend One Zero

		CGPROGRAM
		#pragma surface surf Standard keepalpha

		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		half _Glossiness;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = IN.color;

			o.Alpha = IN.uv_MainTex.y;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
