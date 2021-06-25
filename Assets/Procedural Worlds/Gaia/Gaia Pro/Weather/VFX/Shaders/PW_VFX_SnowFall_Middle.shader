// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PWS/VFX/PW_VFX_SnowFall_Middle"
{
	Properties
	{
		//_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		//_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_MainTex("MainTex", 2D) = "white" {}
		_Snow_Speed("Snow_Speed", Range( 0 , 5)) = 0.1
		_Snow_Speed_2("Snow_Speed_2", Range( 0 , 5)) = 0
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
				uniform float _Snow_Speed_2;
				uniform float _Snow_Speed;

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

					float4 color43 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
					float2 uv05 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float2 appendResult53 = (float2(0.0 , _Snow_Speed_2));
					float smoothstepResult57 = smoothstep( 0.0 , 1.0 , ( tex2D( _MainTex, ( uv05 + ( appendResult53 * frac( _Time.x ) ) ) ).r + tex2D( _MainTex, ( uv05 + frac( ( _Time.x * _Snow_Speed ) ) ) ).g ));
					float2 uv031 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float clampResult36 = clamp( pow( ( uv031.x * uv031.y * ( 1.0 - uv031.x ) * ( 1.0 - uv031.y ) * 16.0 ) , 2.0 ) , 0.0 , 1.0 );
					float smoothstepResult44 = smoothstep( 0.0 , 1.0 , clampResult36);
					float4 appendResult37 = (float4(color43.rgb , ( ( smoothstepResult57 * smoothstepResult44 ) * i.color.a )));
					

					fixed4 col = appendResult37;
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
1;1;1918;1017;1507.605;972.9554;1.784792;True;False
Node;AmplifyShaderEditor.TimeNode;20;-2686.073,380.7194;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;52;-3096.258,136.4057;Float;False;Property;_Snow_Speed_2;Snow_Speed_2;2;0;Create;True;0;0;False;0;0;0.1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2885.417,679.9478;Float;False;Property;_Snow_Speed;Snow_Speed;1;0;Create;True;0;0;False;0;0.1;0.1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;31;-17.61673,-492.6149;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-2377.25,486.1568;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;53;-2639.258,164.4057;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FractNode;54;-2335.098,269.8856;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-2301.202,111.011;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FractNode;27;-2059.933,452.6716;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-2695.213,-257.3115;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;537.6455,-315.4961;Float;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;False;0;16;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;33;293.1562,-715.3718;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;34;307.2823,-327.4489;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;594.9331,-744.0461;Float;False;Constant;_Float4;Float 4;1;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;50;-1623.794,85.35143;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1760.67,-209.2254;Float;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;e1e370b3a428bf248b277f3bca379b81;None;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-1677.85,386.2098;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;564.811,-623.0093;Float;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-769,126;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-767,-129;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;45;795.9576,-683.1954;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-234.6097,-5.634915;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;36;950.5608,-525.2135;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;44;1282.763,-506.0766;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;57;150.67,180.8698;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;47;683.8544,373.3418;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;650.7183,71.7713;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;1025.601,140.7068;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;43;1009.344,-169.1476;Float;False;Constant;_Color0;Color 0;1;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;37;1258.935,-128.9355;Float;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1802.479,-114.8795;Float;False;True;2;Float;ASEMaterialInspector;0;7;PWS/VFX/PW_VFX_SnowFall_Middle;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;29;0;20;1
WireConnection;29;1;30;0
WireConnection;53;1;52;0
WireConnection;54;0;20;1
WireConnection;51;0;53;0
WireConnection;51;1;54;0
WireConnection;27;0;29;0
WireConnection;33;0;31;1
WireConnection;34;0;31;2
WireConnection;50;0;5;0
WireConnection;50;1;51;0
WireConnection;28;0;5;0
WireConnection;28;1;27;0
WireConnection;32;0;31;1
WireConnection;32;1;31;2
WireConnection;32;2;33;0
WireConnection;32;3;34;0
WireConnection;32;4;35;0
WireConnection;3;0;1;0
WireConnection;3;1;28;0
WireConnection;2;0;1;0
WireConnection;2;1;50;0
WireConnection;45;0;32;0
WireConnection;45;1;46;0
WireConnection;11;0;2;1
WireConnection;11;1;3;2
WireConnection;36;0;45;0
WireConnection;44;0;36;0
WireConnection;57;0;11;0
WireConnection;38;0;57;0
WireConnection;38;1;44;0
WireConnection;48;0;38;0
WireConnection;48;1;47;4
WireConnection;37;0;43;0
WireConnection;37;3;48;0
WireConnection;0;0;37;0
ASEEND*/
//CHKSM=C1A44C9C1FD15508CE09262FF178B99120D22752