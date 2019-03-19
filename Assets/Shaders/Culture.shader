Shader "Custom/Culture" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0.2, 0.2, 0.2)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }
		Offset -5, -2000
		LOD 200
		
		CGPROGRAM
		#pragma surface surf StandardSpecular alpha:fade vertex:vert

		#pragma target 3.0

		#include "HexCellData.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float2 visibility;
		};

		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

			data.visibility.x = cell0.x;
			data.visibility.x = max(data.visibility.x, cell1.x);
			data.visibility.x = max(data.visibility.x, cell2.x);

			data.visibility.x = lerp(0.25, 1, data.visibility.x);

			data.visibility.y = cell0.y;
			data.visibility.y = max(data.visibility.y, cell1.y);
			data.visibility.y = max(data.visibility.y, cell2.y);
		}

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			fixed4 c = _Color;

			float explored = IN.visibility.y;

			o.Albedo = c.rgb * IN.color * IN.visibility.x;
			o.Specular = _Specular * explored;
			o.Occlusion = explored;
			o.Smoothness = _Glossiness;

			o.Alpha = IN.uv_MainTex.y;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
