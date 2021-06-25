// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PWS/VFX/Embers"
{
	Properties
	{
		//_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		//_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_MainTex("MainTex", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}


	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Back
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
				
				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

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
					float4 break94 = ( ( tex2DNode1 * 16.0 ) * ( i.color * 8.0 ) );
					float4 appendResult93 = (float4(break94.r , break94.g , break94.b , ( i.color.a * tex2DNode1.a )));
					

					fixed4 col = appendResult93;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=16700
88;448;1906;774;-4021.551;880.9536;1.30713;True;True
Node;AmplifyShaderEditor.VertexColorNode;74;4620.512,-1066.388;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;4563.485,-671.2613;Float;True;Property;_MainTex;MainTex;0;0;Create;False;0;0;False;0;472758e9340981942a05fc1b2e946cac;dca99474f0cb9e045ac6a41cb560e150;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;89;4933.38,-534.0864;Float;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;False;0;16;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;4809.356,-900.7655;Float;False;Constant;_Float1;Float 1;1;0;Create;True;0;0;False;0;8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;5006.178,-764.6089;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;5016.961,-961.4293;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;5250.232,-758.3492;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;2,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;5378.248,-1005.916;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;94;5406.557,-610.9272;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;93;5724.706,-604.1871;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;77;6109.85,-586.4934;Float;False;True;2;Float;ASEMaterialInspector;0;7;PWS/VFX/Embers;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;0;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;88;0;1;0
WireConnection;88;1;89;0
WireConnection;90;0;74;0
WireConnection;90;1;91;0
WireConnection;84;0;88;0
WireConnection;84;1;90;0
WireConnection;92;0;74;4
WireConnection;92;1;1;4
WireConnection;94;0;84;0
WireConnection;93;0;94;0
WireConnection;93;1;94;1
WireConnection;93;2;94;2
WireConnection;93;3;92;0
WireConnection;77;0;93;0
ASEEND*/
//CHKSM=BF1B30F6C4486929E426B76B93492938C840663B