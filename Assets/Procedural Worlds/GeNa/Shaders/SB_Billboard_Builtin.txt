Shader "Procedural Worlds/SB_Billboard"
{
	//BEGIN STORM EDIT
	CGINCLUDE
	#pragma multi_compile_instancing
	#pragma instancing_options procedural:storm_setup
	#include "GeNaIndirectBase.cginc"
	ENDCG
	//END STORM EDIT

	Properties
	{
		_TintA("TintA", Color) = (0,1,0,1)
		_TintB("TintB", Color) = (0,1,1,0.5)
		_BaseMap("Input Texture", 2D) = "white" {}
		_Cutoff("Cutoff", Range( 0 , 1)) = 0.5
	}
	
    SubShader 
	{
    	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	    Blend SrcAlpha OneMinusSrcAlpha
	    ColorMask RGB
	    Cull back Lighting Off ZWrite Off
		ZTEST always
    	
    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        #pragma multi_compile_particles
        #pragma multi_compile_fog

        #include "UnityCG.cginc"

        sampler2D _BaseMap;
        fixed4 _TintColor;

        struct appdata_t {
            float4 vertex : POSITION;
            fixed4 color : COLOR;
            float2 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            fixed4 color : COLOR;
            float2 texcoord : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            #ifdef SOFTPARTICLES_ON
            float4 projPos : TEXCOORD2;
            #endif
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float4 _BaseMap_ST;

        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            
            o.vertex = UnityObjectToClipPos(v.vertex);
            
            #ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
            #endif
            
            o.color = v.color;
            o.texcoord = TRANSFORM_TEX(v.texcoord,_BaseMap);

            // billboard mesh towards camera
			float3 vpos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
			float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
			float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
			float4 outPos = mul(UNITY_MATRIX_P, viewPos);
			o.vertex = outPos;
			UNITY_TRANSFER_FOG(o,o.vertex);
            
            return o;
        }

        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
        float _InvFade;
        float4 _TintA;
        float4 _TintB;
        float _Cutoff;

        float4 frag (v2f i) : SV_Target
        {

            float4 texSample = tex2D( _BaseMap,  i.texcoord );
			float4  col = float4(1,1,1,1);
			col.a *= texSample.a;
			clip( col.a - _Cutoff);
            
            #ifdef SOFTPARTICLES_ON
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z - (texSample.b - 0.5f);
                float fade = saturate (10 * (sceneZ-partZ));
				i.color *= lerp(_TintB,_TintA,saturate(fade));
            #endif

            col = i.color * col;
            UNITY_APPLY_FOG(i.fogCoord, col);
            return col;
        }
        ENDCG
		}
	}	
}
