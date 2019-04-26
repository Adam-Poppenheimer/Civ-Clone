Shader "Civ Clone/River Weights" {
	Properties{

	}

	CGINCLUDE

	struct appdata {
		float4 Vertex     : POSITION;
		float2 uv_MainTex : TEXCOORD0;
	};

	struct v2f {
		float4 Pos        : SV_POSITION;
		float2 uv_MainTex : TEXCOORD0;
	};

	v2f vert(appdata v) {
		v2f o;

		o.Pos        = UnityObjectToClipPos(v.Vertex);
		o.uv_MainTex = v.uv_MainTex;

		return o;
	}

	half4 frag(v2f i) : SV_Target {
		return half4(0, 0, 0, i.uv_MainTex.x);
	}

	ENDCG

	SubShader {
		Tags{ "RenderType"="Opaque" "Queue"="Geometry+1" }
		LOD 200

		Pass{
			Blend Zero OneMinusSrcAlpha
			ZTest Always

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#pragma target 3.0

			ENDCG

		}
	}
	FallBack "Diffuse"
}
