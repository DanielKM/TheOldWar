Shader "FoW/CustomFogShader"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Cutoff ("Cutoff", Range(0, 1)) = 0.5

		_FogTex("FogTex", 2D) = "white" {}
		_FogTextureSize("FogTextureSize", Vector) = (1, 1, 1, 1)
		_MapSize("MapSize", float) = 1
		_MapOffset("MapOffset", Vector) = (1, 1, 1, 1)
		_OutsideFogStrength("OutsideFogStrength", float) = 1
	}
	SubShader
	{
		Tags { "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
		LOD 100

		Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			uniform float4 _Color;
			uniform float _Cutoff;

			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};

			//////////////////////////////////////////////////////////
			// FogOfWar-Specific
			//////////////////////////////////////////////////////////

			#pragma multi_compile PLANE_XY PLANE_YZ PLANE_XZ

			uniform sampler2D _FogTex;
			uniform float4 _FogTextureSize;
			uniform float _MapSize;
			uniform float4 _MapOffset;
			uniform float _OutsideFogStrength;

			float2 WorldPositionToFogPosition(float3 worldpos)
			{
				#ifdef PLANE_XY
					float2 modepos = worldpos.xy;
				#elif PLANE_YZ
					float2 modepos = worldpos.yz;
				#else// PLANE_XZ
					float2 modepos = worldpos.xz;
				#endif

				return (modepos - _MapOffset.xy) / _MapSize + float2(0.5f, 0.5f);
			}

			float GetFogAmountAtPosition(float2 fogpos)
			{
				// if it is beyond the map
				float isoutsidemap = min(1, step(fogpos.x, 0) + step(1, fogpos.x) + step(fogpos.y, 0) + step(1, fogpos.y));

				// if outside map, use the outside fog color
				float fog = lerp(tex2D(_FogTex, fogpos).a, _OutsideFogStrength, isoutsidemap);

				return fog;
			}

			float GetFogAmount(float3 worldposition)
			{
				float2 fogpos = WorldPositionToFogPosition(worldposition);
				return GetFogAmountAtPosition(fogpos);
			}

			//////////////////////////////////////////////////////////
			//////////////////////////////////////////////////////////

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float fogamount = GetFogAmount(i.worldPos);
				clip(1 - fogamount - _Cutoff);

				return _Color;
			}
			ENDCG
		}
	}

}