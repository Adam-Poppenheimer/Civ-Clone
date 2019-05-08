// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Civ Clone/Simple Water" {
	Properties{
		_MainTex ("Albedo (RGB)",  2D) = "white" {}
		_SplatMap("Splat Map", 2D) = "white" {}
	}
	SubShader{
		Tags{ "RenderType"="Transparent" "Queue"="Transparent+3" }
		LOD 200
		ZTest Always

		Stencil {
			Ref 0
			Comp Equal
			Pass Keep
		}

		Blend OneMinusDstAlpha DstAlpha
		BlendOp Add, Max

		CGPROGRAM
		#pragma surface surf Standard keepalpha

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _SplatMap;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		UNITY_INSTANCING_BUFFER_START(Props)

		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 waterColor = tex2D(_MainTex, IN.worldPos.xz * 0.065);

			float waterAlpha = tex2D(_SplatMap, IN.worldPos.xz * 0.075).r;

			o.Albedo = waterColor.rgb;
			o.Alpha = IN.uv_MainTex.y * waterAlpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
