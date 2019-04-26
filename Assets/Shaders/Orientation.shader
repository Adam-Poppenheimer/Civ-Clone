Shader "Civ Clone/Orientation" {
	Properties {
		
	}

	CGINCLUDE

	struct appdata {
		float4 Vertex : POSITION;
		float4 Color : COLOR;
	};

	struct v2f {
		float4 Pos   : SV_POSITION;
		float4 Color : COLOR;
	};

	v2f vert(appdata v) {
		v2f o;

		o.Pos = UnityObjectToClipPos(v.Vertex);
		o.Color = v.Color;

		return o;
	}

	half4 frag(v2f i) : SV_Target{
		return i.Color;
	}

	ENDCG

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 200

		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#pragma target 3.0

			ENDCG

		}
	}
	FallBack "Diffuse"
}
