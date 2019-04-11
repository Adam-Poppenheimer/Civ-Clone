Shader "Civ Clone/Bake Occlusion" {

	CGINCLUDE

	#include "UnityCG.cginc"

	struct v2f {
		float4 pos : SV_POSITION;
	};

	v2f vert(appdata_full v) {
		v2f o;

		o.pos = UnityObjectToClipPos(v.vertex);

		return o;
	}

	half4 frag(v2f i) : SV_Target{
		return half4(0, 0, 0, 0);
	}

	ENDCG

	Subshader {
		Tags { "RenderType"="Opaque" }
			
		//
		Offset -1, -1
		ColorMask 0

		Pass {

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			ENDCG
		}
	}

	Subshader {
		Tags{ "RenderType"="Transparent" }

		ColorMask 0

		Pass{
			Stencil{
				Ref 0
				Comp Equal
				Pass IncrSat
			}

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			ENDCG
		}
	}
}