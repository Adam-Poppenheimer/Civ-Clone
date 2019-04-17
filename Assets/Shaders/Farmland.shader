Shader "Civ Clone/Farmland" {
	Properties {
		_MainTex  ("Albedo (RGB)", 2D) = "white" {}
		_DetailTex("Detail (RGB)", 2d) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Transparent+2" }
		LOD 200
		ZTEST Always
		
		Stencil {
			Ref 0
			Comp Equal
			Pass IncrSat
		}

		Blend OneMinusDstAlpha DstAlpha

		CGPROGRAM
		#pragma surface surf Standard

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _DetailTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 mainColor   = tex2D(_MainTex,   IN.uv_MainTex);
			fixed4 detailColor = tex2D(_DetailTex, IN.worldPos.xz * 0.025);

			o.Albedo = mainColor.rgb * detailColor.rgb * IN.color;
			o.Alpha = mainColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
