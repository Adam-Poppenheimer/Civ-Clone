Shader "Civ Clone/Riverbed" {
	Properties {
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Transparent+4" }
		LOD 200
		ZTEST Always

		Stencil {
			Ref 0
			Comp Always
			Pass IncrSat
		}

		Blend OneMinusDstAlpha DstAlpha

		CGPROGRAM
		#pragma surface surf Standard keepalpha

		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 mainColor = tex2D(_MainTex, IN.worldPos.xz * 0.025);

			o.Albedo = mainColor.rgb;
			o.Alpha = mainColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
