//We need to be very careful about how we perform blending, since
//our camera is clearing to RGBA(0, 0, 0, 0). We need to make sure
//we can handle completely blank pixels, keep the blackness of our
//underlying texture from seeping through, and properly mix our
//opaque meshes with culture. Rendering roads after culture and then
//blending based on the destination alpha suffices for this purpose.
//Adding additional shaders should, in theory, work, with the
//effective draw order the reverse of the queue order.
//So Transparent+1 would appear on top of Transparent+2.
//It's not clear how this would work with overlapping transparent types.

Shader "Civ Clone/Road" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Transparent+1" }
		LOD 200
		ZTEST Always

		Stencil{
			Ref 0
			Comp Equal
			Pass IncrSat
		}
		
		Blend OneMinusDstAlpha DstAlpha
		
		CGPROGRAM

		#pragma surface surf Standard

		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		UNITY_INSTANCING_CBUFFER_START(Props)

		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 textureSample = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = textureSample.rgb;
			o.Alpha = textureSample.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
