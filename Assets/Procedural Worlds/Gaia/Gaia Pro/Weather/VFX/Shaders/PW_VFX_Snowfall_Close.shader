// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PWS/VFX/PW_VFX_Snowfall_Close"
{
	Properties
	{
		//_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		//_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_MainTex("MainTex", 2D) = "white" {}
		_MaxDistance("MaxDistance", Range( 1 , 32)) = 18.1
		_Exponent("Exponent", Range( 1 , 8)) = 2
		_Hard("Hard", Range( 0 , 5)) = 1.8
		_Close_Opacity("Close_Opacity", Range( 0 , 1)) = 0.4974815
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}


	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float _Hard;
				uniform float _Close_Opacity;
				uniform float _MaxDistance;
				uniform float _Exponent;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 uv_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex );
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float clampResult12 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _MaxDistance ) , _Exponent ) , 0.0 , 1.0 );
					float smoothstepResult25 = smoothstep( 0.0 , 1.0 , clampResult12);
					float lerpResult14 = lerp( round( ( _Hard * tex2DNode1.r ) ) , ( tex2DNode1.r * _Close_Opacity ) , ( 1.0 - smoothstepResult25 ));
					float clampResult33 = clamp( lerpResult14 , 0.0 , 1.0 );
					float4 appendResult11 = (float4(1.0 , 1.0 , 1.0 , clampResult33));
					

					fixed4 col = ( i.color * appendResult11 );
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	//CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=16700
1;1;1918;1017;1859.174;1475.827;2.813046;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;2;-1510.474,133.8232;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;4;-1526.355,379.694;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DistanceOpNode;3;-1143.712,176.9028;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1147.699,577.044;Float;False;Property;_MaxDistance;MaxDistance;1;0;Create;True;0;0;False;0;18.1;18.1;1;32;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;9;-838.6873,190.2979;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-764.8734,596.9078;Float;False;Property;_Exponent;Exponent;2;0;Create;True;0;0;False;0;2;2;1;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;7;-596.0183,208.0013;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1541,-450.4;Float;False;Property;_Hard;Hard;3;0;Create;True;0;0;False;0;1.8;0.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;12;-346.2074,208.0827;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1519.46,-762.6422;Float;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;0000000000000000f000000000000000;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;-1530.025,-132.4626;Float;False;Property;_Close_Opacity;Close_Opacity;4;0;Create;True;0;0;False;0;0.4974815;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;25;-106.7115,207.9051;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1025.735,-451.7841;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;20;-386.394,-435.2159;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;35;130.8964,208.0306;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-381.934,-137.0304;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;14;383.6651,-192.6556;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;388.155,-512.6792;Float;False;Constant;_Float3;Float 3;5;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;33;767.6204,-192.9728;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;1020.983,-510.516;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexColorNode;31;383.9003,-769.1465;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;1276.36,-767.5841;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1926.51,-765.6387;Float;False;True;2;Float;ASEMaterialInspector;0;7;PW_VFX_Snowfall_Close;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;3;0;2;0
WireConnection;3;1;4;0
WireConnection;9;0;3;0
WireConnection;9;1;6;0
WireConnection;7;0;9;0
WireConnection;7;1;8;0
WireConnection;12;0;7;0
WireConnection;25;0;12;0
WireConnection;21;0;16;0
WireConnection;21;1;1;1
WireConnection;20;0;21;0
WireConnection;35;0;25;0
WireConnection;27;0;1;1
WireConnection;27;1;24;0
WireConnection;14;0;20;0
WireConnection;14;1;27;0
WireConnection;14;2;35;0
WireConnection;33;0;14;0
WireConnection;11;0;19;0
WireConnection;11;1;19;0
WireConnection;11;2;19;0
WireConnection;11;3;33;0
WireConnection;32;0;31;0
WireConnection;32;1;11;0
WireConnection;0;0;32;0
ASEEND*/
//CHKSM=2EDAB8E4A043001C450E7CC53C3737C3EC15B72F