Shader "Custom/Culture" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }
		Offset 0, -2000
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = _Color;
			o.Albedo = c.rgb * IN.color;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			float uvAlpha = IN.uv_MainTex.y;
			if (uvAlpha <= 0.5) {
				o.Alpha = IN.uv_MainTex.y * 2;
			}
			else {
				o.Alpha = 0;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}
