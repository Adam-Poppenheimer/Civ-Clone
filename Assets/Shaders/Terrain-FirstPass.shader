// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Civ Clone/Terrain" {
	Properties {
		// set by terrain engine
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
		[HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
		[HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
		[HideInInspector] [Gamma] _Specular0 ("Specular 0", Color) = (0.2, 0.2, 0.2)	
		[HideInInspector] [Gamma] _Specular1 ("Specular 1", Color) = (0.2, 0.2, 0.2)
		[HideInInspector] [Gamma] _Specular2 ("Specular 2", Color) = (0.2, 0.2, 0.2)
		[HideInInspector] [Gamma] _Specular3 ("Specular 3", Color) = (0.2, 0.2, 0.2)
		[HideInInspector] _Smoothness0 ("Smoothness 0", Range(0.0, 1.0)) = 1.0	
		[HideInInspector] _Smoothness1 ("Smoothness 1", Range(0.0, 1.0)) = 1.0	
		[HideInInspector] _Smoothness2 ("Smoothness 2", Range(0.0, 1.0)) = 1.0	
		[HideInInspector] _Smoothness3 ("Smoothness 3", Range(0.0, 1.0)) = 1.0

		//Used for baking things like culture directly onto the terrain
		[HideInInspector] _BakeTexture("Bake Texture (RGBA)", 2D) = "clear" {}

		//Elements of the vector are (WorldX, WorldZ, ChunkWidth, ChunkHeight)
		[HideInInspector] _BakeTextureDimensions("Bake Texture Dimensions", Vector) = (0, 0, 1.0, 1.0)

		// used in fallback on old cards & base map
		[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)

		_BackgroundColor ("Background Color", Color) = (0, 0, 0)
	}

	SubShader {
		Tags {
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}

		CGPROGRAM
		#pragma surface surf StandardSpecular vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer fullforwardshadows noinstancing
		#pragma multi_compile_fog
		#pragma target 3.0
		// needs more than 8 texcoords
		#pragma exclude_renderers gles psp2
		#include "UnityPBSLighting.cginc"

		#pragma multi_compile __ _TERRAIN_NORMAL_MAP
		#if defined(SHADER_API_D3D9) && defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && defined(DYNAMICLIGHTMAP_ON) && defined(SHADOWS_SHADOWMASK) && defined(_TERRAIN_NORMAL_MAP) && defined(UNITY_SPECCUBE_BLENDING)
			// TODO : On d3d9 17 samplers would be used when : defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && defined(DYNAMICLIGHTMAP_ON) && defined(SHADOWS_SHADOWMASK) && defined(_TERRAIN_NORMAL_MAP) && defined(UNITY_SPECCUBE_BLENDING)
			// In that case it would be probably acceptable to undef UNITY_SPECCUBE_BLENDING however at the moment (10/2/2016) we can't undef UNITY_SPECCUBE_BLENDING or other platform defines. CGINCLUDE being added after "Lighting.cginc".
			// For now, remove _TERRAIN_NORMAL_MAP in this case.
			#undef _TERRAIN_NORMAL_MAP
			#define DONT_USE_TERRAIN_NORMAL_MAP // use it to initialize o.Normal to (0,0,1) because the surface shader analysis still see this shader writes to per-pixel normal.
		#endif

		#define TERRAIN_STANDARD_SHADER
		#define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandardSpecular
		#include "TerrainSplatmapCommon.cginc"

		half3 _Specular0;
		half3 _Specular1;
		half3 _Specular2;
		half3 _Specular3;
		
		half _Smoothness0;
		half _Smoothness1;
		half _Smoothness2;
		half _Smoothness3;

		half3 _BackgroundColor;

		sampler2D _BakeTexture;
		float4 _BakeTextureDimensions;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			half4 splat_control;
			half weight;
			fixed4 mixedDiffuse;
			half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);

			#ifdef DONT_USE_TERRAIN_NORMAL_MAP
				o.Normal = fixed3(0, 0, 1);
			#endif

			float3 normal = o.Normal;

			SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, normal);

			float2 posInCameraXZ = IN.worldPos.xz - _BakeTextureDimensions.xy;
			float2 bakeUV = float2(posInCameraXZ.x / _BakeTextureDimensions.z, posInCameraXZ.y / _BakeTextureDimensions.w);

			fixed4 bakedDiffuse = tex2D(_BakeTexture, bakeUV).rgba;

			float4 cellData = GetCellDataFromWorld(IN.worldPos);

			float visibility = cellData.x;
			float explored = cellData.y;

			o.Normal = normal;

			float cellDataFactor = lerp(0.25, 1, visibility) * explored;

			fixed3 splatAlbedo = mixedDiffuse.rgb * cellDataFactor;
			fixed3 bakedAlbedo = bakedDiffuse.rgb * cellDataFactor;

			o.Albedo = lerp(splatAlbedo, bakedAlbedo, bakedDiffuse.a);
			o.Alpha = weight;
			o.Smoothness = mixedDiffuse.a;
			o.Specular = splat_control.x * _Specular0 + splat_control.y * _Specular1 + splat_control.z * _Specular2 + splat_control.a * _Specular3;
			o.Specular *= explored;
			o.Occlusion = explored;
			o.Emission = _BackgroundColor * (1 - explored);
		}
		ENDCG
	}

	Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Terrain-AddPass"
	Dependency "BaseMapShader" = "Hidden/TerrainEngine/Splatmap/Terrain-Base"

	Fallback "Nature/Terrain/Diffuse"
}
