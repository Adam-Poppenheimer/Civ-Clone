Shader "Custom/Terrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Terrain Texture Array", 2DArray) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.5

		#include "HexCellData.cginc"

		UNITY_DECLARE_TEX2DARRAY(_MainTex);

		struct Input {
			float4 color : COLOR;
			float3 worldPos;
			float3 terrain;
			float3 visibility;
		};

		

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

			data.terrain.x = cell0.w;
			data.terrain.y = cell1.w;
			data.terrain.z = cell2.w;

			data.visibility.x = cell0.x;
			data.visibility.y = cell1.x;
			data.visibility.z = cell2.x;
			data.visibility = lerp(0.25, 1, data.visibility);
		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float4 GetTerrainColor(Input IN, int index) {
			float3 uvw = float3(IN.worldPos.xz * 0.02, IN.terrain[index]);
			float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);
			return c * IN.color[index] * IN.visibility[index];
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c =
				GetTerrainColor(IN, 0) +
				GetTerrainColor(IN, 1) +
				GetTerrainColor(IN, 2);

			o.Albedo = c.rgb * _Color;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}