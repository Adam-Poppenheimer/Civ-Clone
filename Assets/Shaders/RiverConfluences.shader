Shader "Custom/River Confluence" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular("Specular", Color) = (0.2, 0.2, 0.2)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf StandardSpecular alpha vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "WaterEffects.cginc"
		#include "HexCellData.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float2 centerRightRiverUV;
			float2 leftRightRiverUV;
			float4 barycentricCoords : COLOR;

			float2 visibility;
		};

		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			data.centerRightRiverUV = v.texcoord1.xy;
			data.leftRightRiverUV   = v.texcoord2.xy;

			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

			data.visibility.x = cell0.x * v.color.x + cell1.x * v.color.y + cell2.x * v.color.z;
			data.visibility.x = lerp(0.25, 1, data.visibility.x);

			data.visibility.y = cell0.y * v.color.x + cell1.y * v.color.y + cell2.y * v.color.z;
		}
		
		UNITY_INSTANCING_CBUFFER_START(Props)
			
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			float centerLeftRiver  = River(IN.uv_MainTex,         _MainTex);
			float centerRightRiver = River(IN.centerRightRiverUV, _MainTex);
			float leftRightRiver   = River(IN.leftRightRiverUV,   _MainTex);

			float centerLeftWeight  = saturate(1 - IN.barycentricCoords.b * 3);
			float centerRightWeight = saturate(1 - IN.barycentricCoords.g * 3);
			float leftRightWeight   = saturate(1 - IN.barycentricCoords.r * 3);

			centerLeftWeight = saturate(
				centerLeftWeight -
				centerRightWeight * centerRightWeight -
				leftRightWeight   * leftRightWeight
			);

			centerRightWeight = saturate(
				centerRightWeight -
				centerLeftWeight * centerLeftWeight -
				leftRightWeight  * leftRightWeight
			);

			leftRightWeight = saturate(
				leftRightWeight -
				centerLeftWeight  * centerLeftWeight -
				centerRightWeight * centerRightWeight
			);

			float weightedRiver = centerLeftRiver  * centerLeftWeight +
								  centerRightRiver * centerRightWeight +
							      leftRightRiver   * leftRightWeight;

			fixed4 c = saturate(_Color + weightedRiver);

			float explored = IN.visibility.y;

			o.Albedo = c.rgb * IN.visibility.x;
			o.Specular = _Specular * explored;
			o.Smoothness = _Glossiness;
			o.Occlusion = explored;
			o.Alpha = c.a * explored;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
