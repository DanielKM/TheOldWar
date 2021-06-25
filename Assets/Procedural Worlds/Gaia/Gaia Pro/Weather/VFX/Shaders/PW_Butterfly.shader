// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PWS/VFX/Buterfly"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		//_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Speed("Speed", Range( -3 , 3)) = 1
		_MainTex("MainTex", 2D) = "white" {}
		_GlobalPower("GlobalPower", Range( 0 , 1)) = 0.4678517
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
				uniform float _GlobalPower;
				uniform float _Speed;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float2 uv02 = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float4 appendResult11 = (float4(0.0 , ( ( 1.0 - ( ( 1.0 - uv02.x ) * uv02.x * 4.0 ) ) * 0.5 * ( ( ( ( sqrt( pow( ( ( sin( ( _Time.w * _Speed * ( v.color.a * 8.0 ) ) ) + 1.0 ) * 0.5 ) , 1.5 ) ) * 2.0 ) + -1.0 ) * 1.0 ) * 2.0 ) ) , 0.0 , 0.0));
					

					v.vertex.xyz += ( _GlobalPower * appendResult11 ).xyz;
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
					float4 clampResult86 = clamp( ( ( i.color + i.color ) * tex2D( _MainTex, uv_MainTex ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
					

					fixed4 col = clampResult86;
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
154;663;1906;786;-4797.611;1077.98;1;True;True
Node;AmplifyShaderEditor.VertexColorNode;66;-267.6138,-2042.193;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;7;-318.7896,-1457.81;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;53;-296.4553,-1769.048;Float;False;Property;_Speed;Speed;0;0;Create;True;0;0;False;0;1;3;-3;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;80.14273,-1966.769;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;636.2762,-1280.186;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;8;934.445,-1176.088;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;1131.098,-1173.233;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;1341.044,-1172.375;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;21;1555.573,-1166.486;Float;False;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;1799.763,-1340.792;Float;False;Constant;_Float4;Float 4;3;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;20;1801.665,-1150.728;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;1986.006,-1157.914;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-335.2494,-502.338;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;3;34.123,-368.2633;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;664.666,94.55766;Float;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;2206.863,-1161.302;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;2350.272,-1594.23;Float;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;1149.834,-552.816;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;2894.13,-1551.015;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;3381.846,-416.9917;Float;False;Constant;_Float3;Float 3;2;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;6;2438.589,-318.4832;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;3308.074,-1553.081;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;74;4548.937,-1084.994;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;3266.381,-969.5363;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;85;4799.79,-1221.935;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;-1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;4690.388,-593.8016;Float;True;Property;_MainTex;MainTex;1;0;Create;False;0;0;False;0;472758e9340981942a05fc1b2e946cac;472758e9340981942a05fc1b2e946cac;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;69;3967.248,-1031.581;Float;False;Property;_GlobalPower;GlobalPower;2;0;Create;True;0;0;False;0;0.4678517;0.123;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;4109.172,-893.9886;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;5143.202,-939.3683;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;2,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;86;5673.609,-932.9803;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;4547.293,-853.4461;Float;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;77;6061.549,-883.7;Float;False;True;2;Float;ASEMaterialInspector;0;7;PWS/VFX/Buterfly;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;67;0;66;4
WireConnection;9;0;7;4
WireConnection;9;1;53;0
WireConnection;9;2;67;0
WireConnection;8;0;9;0
WireConnection;15;0;8;0
WireConnection;16;0;15;0
WireConnection;21;0;16;0
WireConnection;20;0;21;0
WireConnection;17;0;20;0
WireConnection;17;1;64;0
WireConnection;3;0;2;1
WireConnection;18;0;17;0
WireConnection;4;0;3;0
WireConnection;4;1;2;1
WireConnection;4;2;5;0
WireConnection;63;0;18;0
WireConnection;63;1;13;0
WireConnection;6;0;4;0
WireConnection;50;0;63;0
WireConnection;30;0;6;0
WireConnection;30;1;31;0
WireConnection;30;2;50;0
WireConnection;85;0;74;0
WireConnection;85;1;74;0
WireConnection;11;1;30;0
WireConnection;84;0;85;0
WireConnection;84;1;1;0
WireConnection;86;0;84;0
WireConnection;68;0;69;0
WireConnection;68;1;11;0
WireConnection;77;0;86;0
WireConnection;77;1;68;0
ASEEND*/
//CHKSM=1406302A3C2E65A1CDFEA5B557EE93E09FAEEC5E