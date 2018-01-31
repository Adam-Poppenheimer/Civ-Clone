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
		#pragma surface surf Standard alpha:fade vertex:vert

		#pragma target 3.0

		#include "HexCellData.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float3 visibility;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

			float allVisibility = cell0.x;
			allVisibility = max(allVisibility, cell1.x);
			allVisibility = max(allVisibility, cell2.x);

			data.visibility.x = allVisibility;
			data.visibility.y = allVisibility;
			data.visibility.z = allVisibility;
			data.visibility = lerp(0.25, 1, data.visibility);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = _Color;
			o.Albedo = c.rgb * IN.color * IN.visibility;
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
