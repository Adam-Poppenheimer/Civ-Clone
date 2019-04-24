Shader "Civ Clone/Orientation" {
	Properties {
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 200

		CGPROGRAM
		#pragma surface surf NoLighting keepalpha

		#pragma target 3.0

		struct Input {
			float4 Color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo   = IN.Color.rgb;
			o.Emission = IN.Color.rgb;
			o.Alpha    = IN.Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
