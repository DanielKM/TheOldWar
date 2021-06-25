// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PWS/VFX/PW_VFX_SkyDome_Space"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_PW_VFX_Nebulas("PW_VFX_Nebulas", 2D) = "white" {}
		_SpaceSize("SpaceSize", Range( 0.1 , 3)) = 0.5
		[HDR]_NebulaColor("NebulaColor", Color) = (0,0,0,0)
		[HDR]_NebulaColor_2("NebulaColor_2", Color) = (0,0,0,0)
		_NebulaFade("NebulaFade", Range( 1 , 16)) = 0
		_PW_VFX_Stars("PW_VFX_Stars", 2D) = "white" {}
		[HDR]_StarsColor1("StarsColor1", Color) = (0,0.860724,1,0)
		[HDR]_StarsColor2("StarsColor2", Color) = (1,0.3363244,0.1273585,0)
		[HDR]_StarsColor3("StarsColor3", Color) = (0,0.860724,1,0)
		[HDR]_StarsColor4("StarsColor4", Color) = (0,0.860724,1,0)
		_StarsDensity("StarsDensity", Range( 0 , 1)) = 0
		_Stars_Small_Density("Stars_Small_Density", Range( 0 , 1)) = 0
		_StarsHue("StarsHue", Range( -2 , 2)) = 0
		[HDR]_StarsSmallTint("StarsSmallTint", Color) = (0,0,0,0)
		[HDR]_StarsMainTint("StarsMainTint", Color) = (0,0,0,0)
		_Speed("Speed", Range( -1 , 1)) = 0
		_HorizonFadePower("Horizon Fade Power", Range( 0.001 , 16)) = 3
		_HorizonFadeScale("Horizon Fade Scale", Range( 0.001 , 16)) = 1.5
		_SeaLevel("Sea Level", Range( -100 , 100)) = 0

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent-2" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha One, SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Front
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
				#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
				#else//ASE Sampling Macros
				#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
				#endif//ASE Sampling Macros
				


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float4 ase_texcoord1 : TEXCOORD1;
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
					float4 ase_texcoord4 : TEXCOORD4;
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
				uniform float4 _StarsColor3;
				uniform float4 _StarsColor1;
				uniform float4 _StarsColor2;
				UNITY_DECLARE_TEX2D_NOSAMPLER(_PW_VFX_Stars);
				uniform float _SpaceSize;
				uniform float _Speed;
				SamplerState sampler_PW_VFX_Stars;
				UNITY_DECLARE_TEX2D_NOSAMPLER(_PW_VFX_Nebulas);
				SamplerState sampler_PW_VFX_Nebulas;
				uniform float _Stars_Small_Density;
				uniform float4 _StarsSmallTint;
				uniform float4 _StarsMainTint;
				uniform float _StarsDensity;
				uniform float4 _StarsColor4;
				uniform float _StarsHue;
				uniform float _NebulaFade;
				uniform float4 _NebulaColor;
				uniform float4 _NebulaColor_2;
				uniform float3 PW_SunDirection_Sky;
				uniform float _HorizonFadeScale;
				uniform float _SeaLevel;
				uniform float _HorizonFadePower;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord4.xyz = ase_worldPos;
					
					o.ase_texcoord3.xy = v.ase_texcoord1.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.zw = 0;
					o.ase_texcoord4.w = 0;

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
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 texCoord214 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float mulTime304 = _Time.y * ( _Speed * 0.005 );
					float cos302 = cos( mulTime304 );
					float sin302 = sin( mulTime304 );
					float2 rotator302 = mul( texCoord214 - float2( 0.7,-0.2 ) , float2x2( cos302 , -sin302 , sin302 , cos302 )) + float2( 0.7,-0.2 );
					float2 temp_output_223_0 = ( _SpaceSize * rotator302 );
					float2 temp_output_215_0 = ( 2.486 * temp_output_223_0 );
					float2 temp_output_316_0 = ( 1.65 * temp_output_215_0 );
					float4 tex2DNode251 = SAMPLE_TEXTURE2D( _PW_VFX_Stars, sampler_PW_VFX_Stars, temp_output_316_0 );
					float4 tex2DNode213 = SAMPLE_TEXTURE2D( _PW_VFX_Nebulas, sampler_PW_VFX_Nebulas, temp_output_215_0 );
					float4 tex2DNode211 = SAMPLE_TEXTURE2D( _PW_VFX_Nebulas, sampler_PW_VFX_Nebulas, temp_output_223_0 );
					float dotResult255 = dot( tex2DNode251 , ( 1.0 - ( tex2DNode213 * tex2DNode211 ) ) );
					float4 lerpResult254 = lerp( _StarsColor1 , _StarsColor2 , dotResult255);
					float4 lerpResult261 = lerp( _StarsColor3 , lerpResult254 , tex2DNode251.b);
					float smoothstepResult279 = smoothstep( ( 1.0 - _Stars_Small_Density ) , 1.0 , SAMPLE_TEXTURE2D( _PW_VFX_Stars, sampler_PW_VFX_Stars, ( 1.65 * temp_output_316_0 ) ).r);
					float smoothstepResult290 = smoothstep( 0.0 , 1.0 , _StarsDensity);
					float smoothstepResult270 = smoothstep( ( 1.0 - smoothstepResult290 ) , 1.0 , tex2DNode251.r);
					float3 desaturateInitialColor277 = ( ( lerpResult261 * ( ( smoothstepResult279 * _StarsSmallTint ) + ( _StarsMainTint * smoothstepResult270 ) ) ) * ( ( ( 1.0 - dotResult255 ) * 0.1 ) + _StarsColor4 ) ).rgb;
					float desaturateDot277 = dot( desaturateInitialColor277, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar277 = lerp( desaturateInitialColor277, desaturateDot277.xxx, _StarsHue );
					float3 temp_cast_1 = (0.0).xxx;
					float3 temp_cast_2 = (64.0).xxx;
					float3 clampResult294 = clamp( desaturateVar277 , temp_cast_1 , temp_cast_2 );
					float2 texCoord207 = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult244 = smoothstep( 0.0 , 1.0 , ( texCoord207.y * 1.0 ));
					float SpaceMask297 = smoothstepResult244;
					float blendOpSrc240 = tex2DNode213.r;
					float blendOpDest240 = tex2DNode211.g;
					float temp_output_241_0 = ( ( saturate( (( blendOpDest240 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest240 ) * ( 1.0 - blendOpSrc240 ) ) : ( 2.0 * blendOpDest240 * blendOpSrc240 ) ) )) * 2.0 );
					float smoothstepResult248 = smoothstep( -0.5 , _NebulaFade , temp_output_241_0);
					float clampResult426 = clamp( smoothstepResult248 , 0.0 , 1.0 );
					half3 FinalStars325 = ( clampResult294 * SpaceMask297 * ( 1.0 - ( temp_output_241_0 * clampResult426 ) ) );
					float4 lerpResult247 = lerp( _NebulaColor , _NebulaColor_2 , temp_output_241_0);
					half4 FinalNebula326 = ( ( clampResult426 * SpaceMask297 * lerpResult247 ) * 1.0 );
					float3 normalizeResult422 = normalize( PW_SunDirection_Sky );
					float dotResult420 = dot( normalizeResult422 , float3(0,4,0) );
					float clampResult425 = clamp( dotResult420 , 0.0 , 1.0 );
					float3 ase_worldPos = i.ase_texcoord4.xyz;
					float3 temp_output_431_0 = ( ase_worldPos - _WorldSpaceCameraPos );
					float3 break432 = temp_output_431_0;
					float3 appendResult433 = (float3(break432.x , 0.0 , break432.z));
					float3 normalizeResult435 = normalize( appendResult433 );
					float3 break437 = ( normalizeResult435 * _ProjectionParams.z );
					float3 appendResult438 = (float3(break437.x , ( _WorldSpaceCameraPos.y + _SeaLevel ) , break437.z));
					float3 normalizeResult439 = normalize( appendResult438 );
					float3 normalizeResult440 = normalize( temp_output_431_0 );
					float dotResult444 = dot( cross( float3(0,1,0) , cross( normalizeResult439 , normalizeResult440 ) ) , normalizeResult440 );
					float4 appendResult220 = (float4(( ( float4( FinalStars325 , 0.0 ) + FinalNebula326 ) * clampResult425 ).rgb , ( 1.0 - pow( ( 1.0 - saturate( ( _HorizonFadeScale * dotResult444 ) ) ) , _HorizonFadePower ) )));
					

					fixed4 col = appendResult220;
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
Version=18400
0;0;2560;1389;-968.1689;2746.008;3.674269;True;False
Node;AmplifyShaderEditor.CommentaryNode;319;1256.201,2099.158;Inherit;False;915.5765;593.2011;Comment;7;302;304;308;307;305;214;303;SpaceUV;0.2889073,0.09304912,0.4811321,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;308;1435.53,2560.071;Float;False;Constant;_Float12;Float 12;18;0;Create;True;0;0;False;0;False;0.005;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;305;1326.724,2431.712;Float;False;Property;_Speed;Speed;15;0;Create;True;0;0;False;0;False;0;-0.22;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;307;1616.876,2511.427;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;304;1785.271,2555.941;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;214;1337.465,2285.577;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;321;1807.493,892.8536;Inherit;False;1320.615;1159.738;Comment;13;250;211;213;210;283;251;285;284;316;216;224;223;215;Space textures;0.6698113,0.2524194,0.1548149,1;0;0
Node;AmplifyShaderEditor.Vector2Node;303;1329.196,2145.917;Float;False;Constant;_Vector1;Vector 1;17;0;Create;True;0;0;False;0;False;0.7,-0.2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RotatorNode;302;1948.695,2393.947;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0.1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;224;1881.578,1627.126;Float;False;Property;_SpaceSize;SpaceSize;1;0;Create;True;0;0;False;0;False;0.5;0.35;0.1;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;223;2167.798,1676.039;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;216;1987.638,1536.348;Float;False;Constant;_Float4;Float 4;3;0;Create;True;0;0;False;0;False;2.486;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;430;4123.933,-662.4857;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;429;4079.321,-496.1452;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;285;2334.552,1363.727;Float;False;Constant;_Float9;Float 9;15;0;Create;True;0;0;False;0;False;1.65;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;210;2365.443,1726.087;Float;True;Property;_PW_VFX_Nebulas;PW_VFX_Nebulas;0;0;Create;True;0;0;False;0;False;7dc0b3f4e1536c14e967a8bde29a9626;7dc0b3f4e1536c14e967a8bde29a9626;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;320;2411.571,517.0289;Inherit;False;1209.778;331.2847;Comment;7;276;270;279;290;282;275;281;Star Density;0.2069242,0.4314783,0.4716981,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;215;2342.203,1600.149;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;431;4358.01,-607.6781;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;432;4518.268,-907.1768;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;316;2496.408,1466.128;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;275;2467.522,696.5959;Float;False;Property;_StarsDensity;StarsDensity;10;0;Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;323;3704.584,231.8061;Inherit;False;1865.194;921.6894;Comment;19;286;288;280;287;289;263;261;273;274;268;269;254;260;252;253;272;255;257;256;Stars generator;0.3962264,0.07794069,0,1;0;0
Node;AmplifyShaderEditor.SamplerNode;213;2714.176,1589.463;Inherit;True;Property;_TextureSample3;Texture Sample 3;3;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;211;2725.901,1804.527;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;250;2497.866,1058.451;Float;True;Property;_PW_VFX_Stars;PW_VFX_Stars;5;0;Create;True;0;0;False;0;False;cd709d6c8c3e44549bdd784629dffdd1;cd709d6c8c3e44549bdd784629dffdd1;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;256;3754.584,988.0563;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;290;2827.058,690.5225;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;281;2461.571,567.0289;Float;False;Property;_Stars_Small_Density;Stars_Small_Density;11;0;Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;284;2623.363,1328.224;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;433;4832.016,-905.4724;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;435;5062.973,-906.8512;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;257;3912.831,990.194;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;282;2838.37,592.5515;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ProjectionParams;434;5023.767,-797.3458;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;283;2801.172,975.2112;Inherit;True;Property;_TextureSample5;Texture Sample 5;15;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;251;2810.721,1222.412;Inherit;True;Property;_TextureSample4;Texture Sample 4;8;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;276;3037.646,703.6221;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;461;4593.37,-437.0012;Inherit;True;Property;_SeaLevel;Sea Level;18;0;Create;True;0;0;False;0;False;0;100;-100;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;252;3873.037,284.7628;Float;False;Property;_StarsColor1;StarsColor1;6;1;[HDR];Create;True;0;0;False;0;False;0,0.860724,1,0;2.466714,2.995935,3.09434,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;318;2231.682,2165.79;Inherit;False;1172.509;225.3979;Comment;4;207;209;244;297;SpaceMask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;322;3517.25,1759.787;Inherit;False;1440.653;870.7143;Comment;16;326;232;228;208;298;247;245;426;246;248;241;249;242;240;315;312;Nebula calc;0.006140965,0.259456,0.4339623,1;0;0
Node;AmplifyShaderEditor.ColorNode;286;4346.329,285.1745;Float;False;Property;_StarsSmallTint;StarsSmallTint;13;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;33.89676,23.42603,17.56952,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;436;5341.482,-905.1503;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;279;3421.348,576.4819;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.8;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;255;4101.528,982.6069;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;288;4347.983,519.718;Float;False;Property;_StarsMainTint;StarsMainTint;14;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;18.4261,20.84704,25.68894,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;253;3860.784,491.1173;Float;False;Property;_StarsColor2;StarsColor2;7;1;[HDR];Create;True;0;0;False;0;False;1,0.3363244,0.1273585,0;111.4305,108.9988,100.3926,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;270;3412.932,692.3137;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.8;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;437;5618.738,-702.0388;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;287;4699.928,655.6025;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;462;4942.463,-457.0757;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;269;4367.074,883.624;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;242;3581.312,2105.755;Float;False;Constant;_Float7;Float 7;6;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;260;4106.837,281.8061;Float;False;Property;_StarsColor3;StarsColor3;8;1;[HDR];Create;True;0;0;False;0;False;0,0.860724,1,0;0.7374075,5.216475,4.042086,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;207;2281.682,2215.79;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;289;4693.362,797.8201;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;254;4112.696,751.3275;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;240;3567.25,1867.416;Inherit;False;Overlay;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;249;3808.487,2097.347;Float;False;Property;_NebulaFade;NebulaFade;4;0;Create;True;0;0;False;0;False;0;4.5;1;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;438;5907.738,-704.0388;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;261;4934.183,791.15;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;241;3917.686,1912.855;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;272;4114.321,516.2199;Float;False;Property;_StarsColor4;StarsColor4;9;1;[HDR];Create;True;0;0;False;0;False;0,0.860724,1,0;0.09831628,0.1345381,0.1862835,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;268;4707.987,1020.496;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;280;4946.66,652.0083;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;209;2705.682,2231.79;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;263;5194.446,794.7078;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;440;6105.821,-539.0764;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;274;4963.079,1015.996;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;439;6123.738,-853.0388;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;324;5701.494,1041.095;Inherit;False;927.6289;489.1403;Comment;8;294;300;295;296;277;278;299;325;FinalStars;1,0,0,1;0;0
Node;AmplifyShaderEditor.SmoothstepOpNode;248;4108.631,1972.388;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-0.5;False;2;FLOAT;15;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;244;2885.365,2235.188;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;273;5400.778,906.6036;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CrossProductOpNode;441;6355.925,-770.4087;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;297;3161.191,2238.496;Float;False;SpaceMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;278;5751.494,1301.002;Float;False;Property;_StarsHue;StarsHue;12;0;Create;True;0;0;False;0;False;0;0.16;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;442;6353.906,-926.3291;Inherit;False;Constant;_Vector3;Vector 3;16;0;Create;True;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;245;3830.542,2208.004;Float;False;Property;_NebulaColor;NebulaColor;2;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;1.020452,0.6211448,1.059274,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;426;4302.434,1957.286;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;246;3825.733,2401.3;Float;False;Property;_NebulaColor_2;NebulaColor_2;3;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;0,1.095744,1.216786,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;298;3592.119,2227.41;Inherit;False;297;SpaceMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CrossProductOpNode;443;6612.3,-789.4987;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;315;4496.268,1885.54;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;296;6089.559,1207.495;Float;False;Constant;_Float11;Float 11;17;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;277;6076.494,1100.703;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;295;6088.159,1296.495;Float;False;Constant;_Float10;Float 10;17;0;Create;True;0;0;False;0;False;64;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;247;4191.716,2285.871;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;457;6797.846,-964.4018;Inherit;False;Property;_HorizonFadeScale;Horizon Fade Scale;17;0;Create;True;0;0;False;0;False;1.5;1.52;0.001;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;294;6324.758,1091.095;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;312;4753.74,1886.388;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;299;5808.063,1395.194;Inherit;False;297;SpaceMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;428;5637.477,259.8836;Inherit;False;1318.336;527.5165;Comment;11;397;421;422;311;420;332;331;425;423;220;451;Finalize;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;4385.626,2134.411;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;444;6825.418,-787.7104;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;228;4412.785,2377.918;Float;False;Constant;_Float6;Float 6;4;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;397;5668.496,490.9193;Float;False;Global;PW_SunDirection_Sky;PW_SunDirection_Sky;21;0;Create;True;0;0;False;0;False;0,0,0;0,-1.788139E-07,-1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;232;4487.49,2012.555;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;300;6247.123,1358.235;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;458;7070.112,-799.9009;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;456;7228.094,-794.5616;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;421;5880.933,608.8908;Float;False;Constant;_Vector2;Vector 2;22;0;Create;True;0;0;False;0;False;0,4,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;326;4717.136,2035.636;Half;False;FinalNebula;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;422;5895.945,500.2959;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;325;6391.241,1260.827;Half;False;FinalStars;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;420;6084.139,498.4673;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;331;6000.497,319.4458;Inherit;False;325;FinalStars;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;459;7383.444,-793.584;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;332;5990.684,402.8487;Inherit;False;326;FinalNebula;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;446;7215.064,-644.1591;Inherit;False;Property;_HorizonFadePower;Horizon Fade Power;16;0;Create;True;0;0;False;0;False;3;3;0.001;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;425;6366.945,415.3152;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;311;6200.363,322.0553;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;447;7537.683,-790.0319;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;423;6549.937,313.6365;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;460;7761.219,-788.5298;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;451;6660.582,528.1727;Inherit;False;Constant;_Float0;Float 0;17;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;220;6799.809,311.319;Inherit;True;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;455;7405.596,28.73935;Float;False;True;-1;2;ASEMaterialInspector;0;9;PWS/VFX/PW_VFX_SkyDome_Space;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;8;5;False;-1;1;False;-1;2;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;True;1;False;-1;True;True;True;True;False;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=-2;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;;True;0
WireConnection;307;0;305;0
WireConnection;307;1;308;0
WireConnection;304;0;307;0
WireConnection;302;0;214;0
WireConnection;302;1;303;0
WireConnection;302;2;304;0
WireConnection;223;0;224;0
WireConnection;223;1;302;0
WireConnection;215;0;216;0
WireConnection;215;1;223;0
WireConnection;431;0;430;0
WireConnection;431;1;429;0
WireConnection;432;0;431;0
WireConnection;316;0;285;0
WireConnection;316;1;215;0
WireConnection;213;0;210;0
WireConnection;213;1;215;0
WireConnection;211;0;210;0
WireConnection;211;1;223;0
WireConnection;256;0;213;0
WireConnection;256;1;211;0
WireConnection;290;0;275;0
WireConnection;284;0;285;0
WireConnection;284;1;316;0
WireConnection;433;0;432;0
WireConnection;433;2;432;2
WireConnection;435;0;433;0
WireConnection;257;0;256;0
WireConnection;282;0;281;0
WireConnection;283;0;250;0
WireConnection;283;1;284;0
WireConnection;251;0;250;0
WireConnection;251;1;316;0
WireConnection;276;0;290;0
WireConnection;436;0;435;0
WireConnection;436;1;434;3
WireConnection;279;0;283;1
WireConnection;279;1;282;0
WireConnection;255;0;251;0
WireConnection;255;1;257;0
WireConnection;270;0;251;1
WireConnection;270;1;276;0
WireConnection;437;0;436;0
WireConnection;287;0;279;0
WireConnection;287;1;286;0
WireConnection;462;0;429;2
WireConnection;462;1;461;0
WireConnection;269;0;255;0
WireConnection;289;0;288;0
WireConnection;289;1;270;0
WireConnection;254;0;252;0
WireConnection;254;1;253;0
WireConnection;254;2;255;0
WireConnection;240;0;213;1
WireConnection;240;1;211;2
WireConnection;438;0;437;0
WireConnection;438;1;462;0
WireConnection;438;2;437;2
WireConnection;261;0;260;0
WireConnection;261;1;254;0
WireConnection;261;2;251;3
WireConnection;241;0;240;0
WireConnection;241;1;242;0
WireConnection;268;0;269;0
WireConnection;280;0;287;0
WireConnection;280;1;289;0
WireConnection;209;0;207;2
WireConnection;263;0;261;0
WireConnection;263;1;280;0
WireConnection;440;0;431;0
WireConnection;274;0;268;0
WireConnection;274;1;272;0
WireConnection;439;0;438;0
WireConnection;248;0;241;0
WireConnection;248;2;249;0
WireConnection;244;0;209;0
WireConnection;273;0;263;0
WireConnection;273;1;274;0
WireConnection;441;0;439;0
WireConnection;441;1;440;0
WireConnection;297;0;244;0
WireConnection;426;0;248;0
WireConnection;443;0;442;0
WireConnection;443;1;441;0
WireConnection;315;0;241;0
WireConnection;315;1;426;0
WireConnection;277;0;273;0
WireConnection;277;1;278;0
WireConnection;247;0;245;0
WireConnection;247;1;246;0
WireConnection;247;2;241;0
WireConnection;294;0;277;0
WireConnection;294;1;296;0
WireConnection;294;2;295;0
WireConnection;312;0;315;0
WireConnection;208;0;426;0
WireConnection;208;1;298;0
WireConnection;208;2;247;0
WireConnection;444;0;443;0
WireConnection;444;1;440;0
WireConnection;232;0;208;0
WireConnection;232;1;228;0
WireConnection;300;0;294;0
WireConnection;300;1;299;0
WireConnection;300;2;312;0
WireConnection;458;0;457;0
WireConnection;458;1;444;0
WireConnection;456;0;458;0
WireConnection;326;0;232;0
WireConnection;422;0;397;0
WireConnection;325;0;300;0
WireConnection;420;0;422;0
WireConnection;420;1;421;0
WireConnection;459;0;456;0
WireConnection;425;0;420;0
WireConnection;311;0;331;0
WireConnection;311;1;332;0
WireConnection;447;0;459;0
WireConnection;447;1;446;0
WireConnection;423;0;311;0
WireConnection;423;1;425;0
WireConnection;460;0;447;0
WireConnection;220;0;423;0
WireConnection;220;3;460;0
WireConnection;455;0;220;0
ASEEND*/
//CHKSM=5A7412F0DB749CD341157F5C33F227C7D9B0AF78