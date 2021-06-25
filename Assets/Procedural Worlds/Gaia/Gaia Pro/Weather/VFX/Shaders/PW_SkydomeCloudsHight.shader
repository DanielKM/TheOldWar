// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PWS/VFX/PW_SkydomeCloudsHight"
{
	Properties
	{
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_PW_VFX_SkyCloud_Mask("PW_VFX_SkyCloud_Mask", 2D) = "white" {}
		[HDR]_Clouds_HA_Color_1("Clouds_HA_Color_1", Color) = (0.8490566,0.8490566,0.8490566,0)
		[HDR]_Clouds_HA_Color_2("Clouds_HA_Color_2", Color) = (0.8490566,0.8490566,0.8490566,0)
		[KeywordEnum(HightLevel,MiddleLevel,LowLevel)] _CloudsType("CloudsType", Float) = 0
		[Toggle(_MASKFROMSHELL_ON)] _MaskFromShell("MaskFromShell", Float) = 0
		_Tiling("Tiling", Vector) = (1,1,0,0)

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
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
				#pragma shader_feature_local _CLOUDSTYPE_HIGHTLEVEL _CLOUDSTYPE_MIDDLELEVEL _CLOUDSTYPE_LOWLEVEL
				#pragma shader_feature_local _MASKFROMSHELL_ON


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float3 ase_normal : NORMAL;
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

				uniform float _InvFade;
				uniform float4 PW_SkyDome_FinalSky_Color;
				uniform float4 PW_SkyDome_Sun_Color;
				uniform float3 PW_SunDirection_Clouds_HA;
				uniform sampler2D _PW_VFX_SkyCloud_Mask;
				uniform float PW_Clouds_Speed_HA;
				uniform float2 _Tiling;
				uniform float PW_Clouds_Hight_Thickness;
				uniform float PW_Clouds_Hight_Density;
				uniform float4 _Clouds_HA_Color_1;
				uniform float4 _Clouds_HA_Color_2;
				uniform float4 PW_SkyDome_FinalClouds_Color;
				uniform float PW_SkyDome_Brightness;
				uniform float PW_Clouds_Opacity;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
					o.ase_texcoord3.xyz = ase_worldNormal;
					
					o.ase_texcoord4.xy = v.ase_texcoord1.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
					o.ase_texcoord4.zw = 0;

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

					float3 desaturateInitialColor192 = PW_SkyDome_Sun_Color.rgb;
					float desaturateDot192 = dot( desaturateInitialColor192, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar192 = lerp( desaturateInitialColor192, desaturateDot192.xxx, -2.0 );
					float3 ase_worldNormal = i.ase_texcoord3.xyz;
					float fresnelNdotV135 = dot( normalize( ase_worldNormal ), PW_SunDirection_Clouds_HA );
					float fresnelNode135 = ( 0.0 + 0.05 * pow( 1.0 - fresnelNdotV135, 3.0 ) );
					float temp_output_201_0 = ( ( PW_Clouds_Speed_HA * 0.01 ) * ( 0.005 * _Time.x ) );
					float2 texCoord86 = i.texcoord.xy * _Tiling + float2( 0,0 );
					float4 tex2DNode34 = tex2D( _PW_VFX_SkyCloud_Mask, ( temp_output_201_0 + texCoord86 ) );
					float4 tex2DNode92 = tex2D( _PW_VFX_SkyCloud_Mask, ( ( temp_output_201_0 * -1.0 ) + ( texCoord86 * 2.364 ) ) );
					float4 tex2DNode107 = tex2D( _PW_VFX_SkyCloud_Mask, ( ( temp_output_201_0 * 1.12 ) + ( texCoord86 * 0.7 ) ) );
					float lerpResult149 = lerp( tex2DNode92.g , tex2DNode92.r , tex2DNode107.b);
					float blendOpSrc97 = tex2DNode34.b;
					float blendOpDest97 = lerpResult149;
					float blendOpSrc211 = tex2DNode92.r;
					float blendOpDest211 = tex2DNode34.r;
					float blendOpSrc213 = ( saturate( ( blendOpSrc211 * blendOpDest211 ) ));
					float blendOpDest213 = tex2DNode107.g;
					float blendOpSrc216 = tex2DNode34.g;
					float blendOpDest216 = tex2DNode107.g;
					#if defined(_CLOUDSTYPE_HIGHTLEVEL)
					float staticSwitch212 = ( saturate( 2.0f*blendOpDest97*blendOpSrc97 + blendOpDest97*blendOpDest97*(1.0f - 2.0f*blendOpSrc97) ));
					#elif defined(_CLOUDSTYPE_MIDDLELEVEL)
					float staticSwitch212 = ( saturate( 2.0f*blendOpDest213*blendOpSrc213 + blendOpDest213*blendOpDest213*(1.0f - 2.0f*blendOpSrc213) ));
					#elif defined(_CLOUDSTYPE_LOWLEVEL)
					float staticSwitch212 = ( saturate( 2.0f*blendOpDest216*blendOpSrc216 + blendOpDest216*blendOpDest216*(1.0f - 2.0f*blendOpSrc216) ));
					#else
					float staticSwitch212 = ( saturate( 2.0f*blendOpDest97*blendOpSrc97 + blendOpDest97*blendOpDest97*(1.0f - 2.0f*blendOpSrc97) ));
					#endif
					float smoothstepResult113 = smoothstep( 0.0 , PW_Clouds_Hight_Thickness , tex2DNode107.r);
					float clampResult242 = clamp( PW_Clouds_Hight_Density , 0.0 , 1.0 );
					float blendOpSrc112 = staticSwitch212;
					float blendOpDest112 = ( ( 1.0 - smoothstepResult113 ) * clampResult242 );
					float temp_output_5_0 = ( 1.0 * ( saturate( 2.0f*blendOpDest112*blendOpSrc112 + blendOpDest112*blendOpDest112*(1.0f - 2.0f*blendOpSrc112) )) );
					float clampResult8 = clamp( temp_output_5_0 , 0.0 , 1.0 );
					float fresnelNdotV153 = dot( normalize( ase_worldNormal ), PW_SunDirection_Clouds_HA );
					float fresnelNode153 = ( 0.0 + 0.1 * pow( 1.0 - fresnelNdotV153, 16.0 ) );
					float blendOpSrc157 = ( fresnelNode135 * ( 1.0 - clampResult8 ) );
					float blendOpDest157 = fresnelNode153;
					float smoothstepResult167 = smoothstep( 0.0 , 1.5 , pow( ( saturate(  (( blendOpSrc157 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpSrc157 - 0.5 ) ) * ( 1.0 - blendOpDest157 ) ) : ( 2.0 * blendOpSrc157 * blendOpDest157 ) ) )) , 1.0 ));
					float clampResult162 = clamp( smoothstepResult167 , 0.0 , 1.0 );
					float4 lerpResult89 = lerp( _Clouds_HA_Color_1 , _Clouds_HA_Color_2 , temp_output_5_0);
					float3 desaturateInitialColor172 = PW_SkyDome_FinalClouds_Color.rgb;
					float desaturateDot172 = dot( desaturateInitialColor172, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar172 = lerp( desaturateInitialColor172, desaturateDot172.xxx, 0.5 );
					float clampResult175 = clamp( fresnelNode135 , 0.0 , 1.0 );
					float4 lerpResult178 = lerp( PW_SkyDome_FinalSky_Color , ( float4( ( desaturateVar192 * clampResult162 ) , 0.0 ) + ( lerpResult89 * float4( desaturateVar172 , 0.0 ) ) ) , pow( ( 1.0 - clampResult175 ) , 16.0 ));
					float3 normalizeResult186 = normalize( PW_SunDirection_Clouds_HA );
					float dotResult183 = dot( normalizeResult186 , float3(0,1,0) );
					float clampResult184 = clamp( ( 1.0 - dotResult183 ) , 0.3 , 1.0 );
					float2 texCoord115 = i.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult118 = smoothstep( 0.2 , 0.5 , texCoord115.y);
					float2 texCoord226 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					#ifdef _MASKFROMSHELL_ON
					float staticSwitch217 = 1.0;
					#else
					float staticSwitch217 = 0.0;
					#endif
					float lerpResult222 = lerp( smoothstepResult118 , ( ( 1.0 - texCoord226.y ) * texCoord226.y * 4.0 ) , staticSwitch217);
					float clampResult221 = clamp( lerpResult222 , 0.0 , 1.0 );
					float smoothstepResult179 = smoothstep( 0.0 , 1.0 , ( clampResult221 * clampResult8 ));
					float clampResult245 = clamp( PW_Clouds_Opacity , 0.0 , 1.0 );
					float4 appendResult20 = (float4(( lerpResult178 * PW_SkyDome_Brightness * clampResult184 ).rgb , ( smoothstepResult179 * clampResult245 )));
					

					fixed4 col = appendResult20;
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
2072;172;1153;905;-2827.6;472.9846;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;236;-2644.493,-519.9899;Inherit;False;1138.787;509.5805;Comment;11;195;198;197;201;199;208;209;206;207;205;204;Cloud Speed;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;237;-2839.048,33.58638;Inherit;False;1648.46;432.3153;Comment;9;225;86;94;93;200;110;202;109;203;Tiling;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;209;-2562.012,-126.4094;Float;False;Constant;_Float9;Float 9;3;0;Create;True;0;0;False;0;False;0.005;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;199;-2594.493,-286.095;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;198;-2566.955,-384.1352;Float;False;Constant;_Float6;Float 6;3;0;Create;True;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;195;-2565.489,-469.9899;Float;False;Global;PW_Clouds_Speed_HA;PW_Clouds_Speed_HA;3;0;Create;True;0;0;False;0;False;1;1;-16;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;225;-2789.048,83.58638;Float;False;Property;_Tiling;Tiling;5;0;Create;True;0;0;False;0;False;1,1;2,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-2252.117,-296.3559;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;197;-2251.921,-417.6663;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;-2087.721,-409.9529;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-2358.608,349.9017;Float;False;Constant;_Float2;Float 2;4;0;Create;True;0;0;False;0;False;0.7;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;205;-2105.581,-199.1202;Float;False;Constant;_Float7;Float 7;3;0;Create;True;0;0;False;0;False;1.12;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;86;-2621.031,102.0796;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;94;-2556.83,252.1876;Float;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;False;2.364;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;207;-2090.019,-283.7939;Float;False;Constant;_Float8;Float 8;3;0;Create;True;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;238;-1158.132,-240.4485;Inherit;False;1596.515;722.4821;Comment;10;1;34;92;107;149;211;97;213;216;212;Texture Setup;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;204;-1667.706,-326.0867;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-1546.671,306.6951;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-2354.871,222.2762;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;206;-1892.259,-355.2232;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;202;-1545.76,199.4353;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;203;-1342.588,303.7091;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;200;-1555.782,84.67036;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1108.132,-117.5435;Float;True;Property;_PW_VFX_SkyCloud_Mask;PW_VFX_SkyCloud_Mask;0;0;Create;True;0;0;False;0;False;6534180a237278145875359c72d70589;6534180a237278145875359c72d70589;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;235;-530.712,553.8637;Inherit;False;1006.109;400.4943;Comment;6;234;113;244;242;7;214;Cloud Thickness/Density;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;34;-766.6683,-190.4484;Inherit;True;Property;_TextureSample2;Texture Sample 2;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;92;-760.9525,49.38112;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;107;-748.0622,252.0336;Inherit;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;214;-480.7121,686.3005;Float;False;Global;PW_Clouds_Hight_Thickness;PW_Clouds_Hight_Thickness;1;0;Create;True;0;0;False;0;False;1;0.7515635;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;211;-398.8763,-75.15759;Inherit;True;Multiply;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;149;-350.9746,225.098;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-485.6022,845.7202;Float;False;Global;PW_Clouds_Hight_Density;PW_Clouds_Hight_Density;1;0;Create;True;0;0;False;0;False;1;0.8748277;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;113;-167.2931,609.7485;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;242;-97.38129,828.5536;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;97;-104.3241,-160.9118;Inherit;False;SoftLight;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;213;-96.9478,-15.84282;Inherit;False;SoftLight;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;216;-96.40007,132.6311;Inherit;False;SoftLight;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;244;85.27191,641.2398;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;212;172.3834,-5.913619;Float;False;Property;_CloudsType;CloudsType;3;0;Create;True;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;HightLevel;MiddleLevel;LowLevel;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;234;247.2426,645.8638;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;112;536.4963,53.88621;Inherit;True;SoftLight;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;239;889.9561,-769.8973;Inherit;False;3034.853;1416.953;Comment;34;5;8;89;178;181;88;87;83;115;226;118;219;218;220;217;223;224;222;221;170;173;172;171;169;116;183;230;179;180;184;233;231;20;245;Cloud Coloring and Mixing;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;83;948.5154,-301.2495;Inherit;False;FLOAT;1;0;FLOAT;0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;1359.44,-410.6163;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;168;82.56129,-1713.591;Inherit;False;2163.866;779.9338;Comment;20;163;192;193;162;160;176;167;161;157;187;174;175;145;153;182;186;132;134;143;135;Clouds_Sun_SSS;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;8;1548.481,-421.8429;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;132;116.6132,-1388.759;Float;False;Global;PW_SunDirection_Clouds_HA;PW_SunDirection_Clouds_HA;5;0;Create;True;0;0;False;0;False;1,1,0;0,-0.8660254,-0.5000001;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;134;150.3425,-1569.246;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;143;756.3971,-1060.123;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;135;672.5592,-1374.12;Inherit;False;Standard;WorldNormal;LightDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0.05;False;3;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;928.9903,-1152.751;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;153;679.2745,-1596.422;Inherit;False;Standard;WorldNormal;LightDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0.1;False;3;FLOAT;16;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;157;1118.381,-1181.628;Inherit;True;HardLight;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;226;950.6318,-42.94806;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;220;1211.635,207.8548;Float;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;161;1379.483,-1158.664;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;115;966.4546,-175.0029;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;223;1184.598,303.4312;Float;False;Constant;_Float5;Float 5;5;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;224;1187.023,385.4888;Float;False;Constant;_Float10;Float 10;5;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;219;1217.889,10.95045;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;167;1577.832,-1166.2;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;160;1713.859,-1606.194;Float;False;Global;PW_SkyDome_Sun_Color;PW_SkyDome_Sun_Color;5;0;Create;True;0;0;False;0;False;1,0.7216981,0.7216981,0;1,0.9058824,0.8000001,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;170;1416.675,435.0557;Float;False;Global;PW_SkyDome_FinalClouds_Color;PW_SkyDome_FinalClouds_Color;5;0;Create;True;0;0;False;0;False;1,0.7216981,0.7216981,1;0.8033263,0.6229329,0.5535507,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;1403.784,173.6889;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;217;1413.708,315.8902;Float;False;Property;_MaskFromShell;MaskFromShell;4;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;173;1894.054,425.7968;Float;False;Constant;_Float4;Float 4;5;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;87;942.4274,-719.8973;Float;False;Property;_Clouds_HA_Color_1;Clouds_HA_Color_1;1;1;[HDR];Create;True;0;0;False;0;False;0.8490566,0.8490566,0.8490566,0;0.8584906,0.8254949,0.7977483,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;88;939.9561,-513.3448;Float;False;Property;_Clouds_HA_Color_2;Clouds_HA_Color_2;2;1;[HDR];Create;True;0;0;False;0;False;0.8490566,0.8490566,0.8490566,0;0.8867924,0.7926753,0.7738519,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;118;1229.54,-181.1187;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;193;1816.557,-1406.659;Float;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;192;2007.76,-1498.361;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode;172;2140.791,214.7885;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;182;230.0244,-1220.307;Float;False;Constant;_Vector0;Vector 0;5;0;Create;True;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ClampOpNode;175;985.5888,-1416.327;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;186;459.6922,-1195.117;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;89;1841.054,-392.7493;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;162;1852.1,-1166.845;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;222;1726.984,144.4112;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;221;1941.497,109.7173;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;163;2086.672,-1260.648;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;183;2517.883,-61.53906;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;171;2311.274,-386.7087;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;174;1199.996,-1426.216;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;187;1437.086,-1390.772;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;16;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;230;2735.5,-38.00713;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;169;2519.796,-380.1715;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;233;2920.365,8.73801;Float;False;Global;PW_Clouds_Opacity;PW_Clouds_Opacity;6;0;Create;True;0;0;False;0;False;1;1.681098;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;116;2511.573,-191.8098;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;176;1282.748,-1608.517;Float;False;Global;PW_SkyDome_FinalSky_Color;PW_SkyDome_FinalSky_Color;5;0;Create;True;0;0;False;0;False;1,0.7216981,0.7216981,0;0.6019223,0.7627782,1.018317,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;245;3236.6,-16.98462;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;178;3125.37,-461.409;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;179;2761.658,-229.1784;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;180;3097.415,-314.293;Float;False;Global;PW_SkyDome_Brightness;PW_SkyDome_Brightness;5;0;Create;True;0;0;False;0;False;0;0.7875;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;184;3200.401,-205.7176;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.3;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;181;3483.096,-464.3453;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;231;3435.654,-151.164;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;20;3670.809,-359.7355;Inherit;True;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;4026.211,-363.5012;Float;False;True;-1;2;ASEMaterialInspector;0;7;PWS/VFX/PW_SkydomeCloudsHight;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;1;False;-1;False;False;False;False;False;False;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Opaque=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;208;0;209;0
WireConnection;208;1;199;1
WireConnection;197;0;195;0
WireConnection;197;1;198;0
WireConnection;201;0;197;0
WireConnection;201;1;208;0
WireConnection;86;0;225;0
WireConnection;204;0;201;0
WireConnection;204;1;205;0
WireConnection;109;0;86;0
WireConnection;109;1;110;0
WireConnection;93;0;86;0
WireConnection;93;1;94;0
WireConnection;206;0;201;0
WireConnection;206;1;207;0
WireConnection;202;0;206;0
WireConnection;202;1;93;0
WireConnection;203;0;204;0
WireConnection;203;1;109;0
WireConnection;200;0;201;0
WireConnection;200;1;86;0
WireConnection;34;0;1;0
WireConnection;34;1;200;0
WireConnection;92;0;1;0
WireConnection;92;1;202;0
WireConnection;107;0;1;0
WireConnection;107;1;203;0
WireConnection;211;0;92;1
WireConnection;211;1;34;1
WireConnection;149;0;92;2
WireConnection;149;1;92;1
WireConnection;149;2;107;3
WireConnection;113;0;107;1
WireConnection;113;2;214;0
WireConnection;242;0;7;0
WireConnection;97;0;34;3
WireConnection;97;1;149;0
WireConnection;213;0;211;0
WireConnection;213;1;107;2
WireConnection;216;0;34;2
WireConnection;216;1;107;2
WireConnection;244;0;113;0
WireConnection;212;1;97;0
WireConnection;212;0;213;0
WireConnection;212;2;216;0
WireConnection;234;0;244;0
WireConnection;234;1;242;0
WireConnection;112;0;212;0
WireConnection;112;1;234;0
WireConnection;83;0;112;0
WireConnection;5;1;83;0
WireConnection;8;0;5;0
WireConnection;143;0;8;0
WireConnection;135;0;134;0
WireConnection;135;4;132;0
WireConnection;145;0;135;0
WireConnection;145;1;143;0
WireConnection;153;0;134;0
WireConnection;153;4;132;0
WireConnection;157;0;145;0
WireConnection;157;1;153;0
WireConnection;161;0;157;0
WireConnection;219;0;226;2
WireConnection;167;0;161;0
WireConnection;218;0;219;0
WireConnection;218;1;226;2
WireConnection;218;2;220;0
WireConnection;217;1;223;0
WireConnection;217;0;224;0
WireConnection;118;0;115;2
WireConnection;192;0;160;0
WireConnection;192;1;193;0
WireConnection;172;0;170;0
WireConnection;172;1;173;0
WireConnection;175;0;135;0
WireConnection;186;0;132;0
WireConnection;89;0;87;0
WireConnection;89;1;88;0
WireConnection;89;2;5;0
WireConnection;162;0;167;0
WireConnection;222;0;118;0
WireConnection;222;1;218;0
WireConnection;222;2;217;0
WireConnection;221;0;222;0
WireConnection;163;0;192;0
WireConnection;163;1;162;0
WireConnection;183;0;186;0
WireConnection;183;1;182;0
WireConnection;171;0;89;0
WireConnection;171;1;172;0
WireConnection;174;0;175;0
WireConnection;187;0;174;0
WireConnection;230;0;183;0
WireConnection;169;0;163;0
WireConnection;169;1;171;0
WireConnection;116;0;221;0
WireConnection;116;1;8;0
WireConnection;245;0;233;0
WireConnection;178;0;176;0
WireConnection;178;1;169;0
WireConnection;178;2;187;0
WireConnection;179;0;116;0
WireConnection;184;0;230;0
WireConnection;181;0;178;0
WireConnection;181;1;180;0
WireConnection;181;2;184;0
WireConnection;231;0;179;0
WireConnection;231;1;245;0
WireConnection;20;0;181;0
WireConnection;20;3;231;0
WireConnection;2;0;20;0
ASEEND*/
//CHKSM=0114AFBD0DE4ADC800A6D583D3A5A3B394647F16