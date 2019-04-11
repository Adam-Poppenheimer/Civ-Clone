Shader "Civ Clone/Road" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 200
		ZTEST Always

		Stencil{
			Ref 0
			Comp Equal
			Pass Keep
		}
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;

		UNITY_INSTANCING_CBUFFER_START(Props)

		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 textureSample = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = textureSample.rgb;
			o.Alpha = textureSample.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
