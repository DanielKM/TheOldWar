// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PW_VFX_Rain"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_PW_VFX_Rain_SnowMask("PW_VFX_Rain_SnowMask", 2D) = "white" {}
		_TilingAndSpeed("TilingAndSpeed", Vector) = (12,2,0,0)
		_PW_VFX_Weather_Intensity("PW_VFX_Weather_Intensity", Range( 0 , 1)) = 1
		_DetailNoiseScale("DetailNoiseScale", Range( 0 , 6)) = 1.35
		_MainColor("MainColor", Color) = (1,1,1,1)

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha One
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest Always
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "Lighting.cginc"
				#include "UnityShaderVariables.cginc"
				#include "AutoLight.cginc"
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
				
				
				//#if UNITY_VERSION >= 560
				//UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				//#else
				//uniform sampler2D_float _CameraDepthTexture;
				//#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				//This is a late directive
				
				uniform float4 _MainColor;
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				uniform float4 _CameraDepthTexture_TexelSize;
				UNITY_DECLARE_TEX2D_NOSAMPLER(_PW_VFX_Rain_SnowMask);
				uniform float _DetailNoiseScale;
				uniform float4 _TilingAndSpeed;
				SamplerState sampler_PW_VFX_Rain_SnowMask;
				uniform float _PW_VFX_Weather_Intensity;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
					float4 screenPos = ComputeScreenPos(ase_clipPos);
					o.ase_texcoord4 = screenPos;
					
					
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
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
					float4 ase_lightColor = 0;
					#else //aselc
					float4 ase_lightColor = _LightColor0;
					#endif //aselc
					float3 appendResult138 = (float3(ase_lightColor.rgb));
					float3 appendResult140 = (float3(_MainColor.r , _MainColor.g , _MainColor.b));
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
					ase_worldViewDir = normalize(ase_worldViewDir);
					float3 normalizeResult147 = normalize( ase_worldViewDir );
					float3 worldSpaceLightDir = UnityWorldSpaceLightDir(ase_worldPos);
					float dotResult144 = dot( -normalizeResult147 , worldSpaceLightDir );
					float4 lerpResult157 = lerp( ( float4( ( appendResult138 * appendResult140 * ( pow( saturate( dotResult144 ) , 8.0 ) * 3.0 ) ) , 0.0 ) + unity_AmbientSky * 2) , unity_FogColor , unity_FogParams.y);
					float4 screenPos = i.ase_texcoord4;
					float4 ase_screenPosNorm = screenPos / screenPos.w;
					ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
					float eyeDepth90 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
					float2 appendResult43 = (float2(_TilingAndSpeed.x , _TilingAndSpeed.y));
					float2 texCoord5 = i.texcoord.xy * appendResult43 + float2( 0,0 );
					float2 appendResult8 = (float2(( _TilingAndSpeed.z * _Time.x ) , ( _TilingAndSpeed.w * _Time.x )));
					float4 break96 = ( SAMPLE_TEXTURE2D( _PW_VFX_Rain_SnowMask, sampler_PW_VFX_Rain_SnowMask, ( ( _DetailNoiseScale * texCoord5 ) + appendResult8 ) ) * SAMPLE_TEXTURE2D( _PW_VFX_Rain_SnowMask, sampler_PW_VFX_Rain_SnowMask, ( texCoord5 + appendResult8 ) ) * SAMPLE_TEXTURE2D( _PW_VFX_Rain_SnowMask, sampler_PW_VFX_Rain_SnowMask, ( texCoord5 + ( appendResult8 * 1.36 ) ) ) );
					float temp_output_107_0 = saturate( ( break96.r * break96.g ) );
					float2 texCoord117 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float4 appendResult76 = (float4(lerpResult157.rgb , saturate( ( _MainColor.a * ( saturate( ( max( ( eyeDepth90 - ( screenPos.w + ( ( ( temp_output_107_0 * 2.0 ) - 1.0 ) * 1 ) ) ) , 0.0 ) * 0.5 ) ) * temp_output_107_0 * pow( ( 1.0 - abs( ( ( texCoord117.y * 2.0 ) - 1.0 ) ) ) , 1.5 ) ) * _PW_VFX_Weather_Intensity ) )));
					

					fixed4 col = appendResult76;
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
10;6;1355;1383;997.3047;4365.741;4.877806;True;False
Node;AmplifyShaderEditor.CommentaryNode;131;-2109.905,-1091.61;Inherit;False;2886.114;1331.56;Uv Animation and Sampling;22;2;107;101;96;103;4;3;18;66;6;15;19;11;8;20;12;5;43;9;44;42;7;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;42;-2038.471,-481.5449;Float;False;Property;_TilingAndSpeed;TilingAndSpeed;1;0;Create;True;0;0;False;0;False;12,2,0,0;18.29,2,0,20;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;7;-2033.086,-294.0786;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-1529.605,-36.79254;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-1533.142,-152.2987;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;43;-1487.73,-773.1542;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-1241.397,11.05293;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-1330.636,-796.7303;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;12,2;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-1025.321,126.9895;Float;False;Constant;_Float1;Float 1;1;0;Create;True;0;0;False;0;False;1.36;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1120.65,-928.646;Float;False;Property;_DetailNoiseScale;DetailNoiseScale;3;0;Create;True;0;0;False;0;False;1.35;2;0;6;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-785.9631,13.43624;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-786.0518,-822.208;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;6;-1036.521,-415.7932;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;66;-538.118,-820.9416;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-524.3586,-7.989709;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-613.1974,-447.2869;Float;True;Property;_PW_VFX_Rain_SnowMask;PW_VFX_Rain_SnowMask;0;0;Create;True;0;0;False;0;False;cb3cd1f6be145ff408bf2887e1f60aae;cb3cd1f6be145ff408bf2887e1f60aae;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;18;-270.0418,-645.3334;Inherit;True;Property;_TextureSample2;Texture Sample 2;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-265.3526,-240.9159;Inherit;True;Property;_TextureSample1;Texture Sample 1;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-270.4646,-447.5845;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;121.341,-225.5579;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;96;259.0341,-225.2965;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;491.4924,-225.2996;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;132;841.202,-553.3414;Inherit;False;600.9607;232.5358;Depth Bias;6;112;110;113;111;109;77;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;107;635.1962,-225.4816;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;77;863.5222,-408.2732;Inherit;False;Constant;_Float2;Float 2;3;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;1023.523,-520.2733;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;1007.523,-408.2732;Inherit;False;Constant;_Float4;Float 4;3;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;159;2838.259,-1120.594;Inherit;False;1688.313;780.7059;Lighting;21;148;157;156;153;155;135;158;140;154;138;152;143;137;149;150;144;145;146;147;136;163;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;130;1266.258,-108.8569;Inherit;False;1343.094;323.2939;Top Bottom Gradient;9;123;125;121;119;122;118;120;117;162;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;133;1518.264,-762.6522;Inherit;False;1070.308;328.5938;Depth Blend;9;81;106;80;115;116;86;90;89;134;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;136;2860.427,-681.168;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;117;1346.119,-48.60686;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;110;1151.523,-520.2733;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;113;1151.523,-408.2732;Inherit;False;Constant;_Scale;Scale;3;0;Create;True;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;147;3043.427,-676.168;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;89;1545.695,-647.4409;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;1295.523,-520.2733;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;1678.117,109.3931;Inherit;False;Constant;_Float7;Float 7;3;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;118;1566.118,-48.60686;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;1828.117,5.393099;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;145;3096.427,-510.1682;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NegateNode;146;3205.427,-677.168;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;134;1745.831,-547.3257;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;90;1706.385,-722.968;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;1827.117,108.3931;Inherit;False;Constant;_Float8;Float 8;3;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;144;3351.427,-677.168;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;121;1980.115,5.393099;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;116;1918.457,-530.9678;Inherit;False;Constant;_Float6;Float 6;3;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;86;1921.439,-658.968;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;3478.016,-520.3257;Inherit;False;Constant;_Float13;Float 13;3;0;Create;True;0;0;False;0;False;8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;125;2140.116,5.393099;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;81;2095.948,-526.9858;Inherit;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;149;3483.864,-677.4526;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;115;2094.746,-658.968;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;162;2283.476,123.7885;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;137;3676.427,-579.1682;Inherit;False;Constant;_Float5;Float 5;3;0;Create;True;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;163;3435.724,-891.1638;Float;False;Property;_MainColor;MainColor;4;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,0.8188919;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;152;3649.016,-703.3256;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;2286.747,-546.9679;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;143;3506.427,-1035.168;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.OneMinusNode;123;2278.891,5.393099;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;106;2436.309,-546.9679;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;138;3714.427,-1035.168;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;161;2505.955,48.05359;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;140;3716.427,-862.1682;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;154;3830.155,-690.8046;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;3998.427,-882.1682;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;2693.552,-248.5451;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;166;3615.379,-148.0577;Float;False;Property;_PW_VFX_Weather_Intensity;PW_VFX_Weather_Intensity;2;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;158;3987.438,-733.3904;Inherit;False;unity_AmbientSky;0;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;153;4235.022,-829.3256;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;167;3934.202,-270.2276;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FogParamsNode;156;4054.995,-528.0334;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;155;4047.994,-611.0333;Inherit;False;unity_FogColor;0;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;157;4345.998,-637.0333;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;168;4111.238,-269.8202;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;165;1894.667,-1123.703;Float;False;Property;_PW_SunDirection_Clouds_HA;PW_SunDirection_Clouds_HA;5;0;Create;True;0;0;False;0;False;0,0,0;0.3,0.3,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;148;3875.865,-497.4529;Inherit;False;Constant;_Float12;Float 12;3;0;Create;True;0;0;False;0;False;0.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;76;4590.653,-293.3297;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;164;1657.915,-1125.185;Float;False;Global;PW_SkyDome_Fog_Color1;PW_SkyDome_Fog_Color;1;0;Create;True;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;169;4788.427,-293.612;Float;False;True;-1;2;ASEMaterialInspector;0;7;PW_VFX_Rain;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;False;False;False;True;2;False;-1;True;7;False;-1;False;True;4;Queue=Transparent=Queue=1;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;True;0
WireConnection;9;0;42;4
WireConnection;9;1;7;1
WireConnection;44;0;42;3
WireConnection;44;1;7;1
WireConnection;43;0;42;1
WireConnection;43;1;42;2
WireConnection;8;0;44;0
WireConnection;8;1;9;0
WireConnection;5;0;43;0
WireConnection;11;0;8;0
WireConnection;11;1;12;0
WireConnection;19;0;20;0
WireConnection;19;1;5;0
WireConnection;6;0;5;0
WireConnection;6;1;8;0
WireConnection;66;0;19;0
WireConnection;66;1;8;0
WireConnection;15;0;5;0
WireConnection;15;1;11;0
WireConnection;18;0;2;0
WireConnection;18;1;66;0
WireConnection;4;0;2;0
WireConnection;4;1;15;0
WireConnection;3;0;2;0
WireConnection;3;1;6;0
WireConnection;103;0;18;0
WireConnection;103;1;3;0
WireConnection;103;2;4;0
WireConnection;96;0;103;0
WireConnection;101;0;96;0
WireConnection;101;1;96;1
WireConnection;107;0;101;0
WireConnection;109;0;107;0
WireConnection;109;1;77;0
WireConnection;110;0;109;0
WireConnection;110;1;111;0
WireConnection;147;0;136;0
WireConnection;112;0;110;0
WireConnection;112;1;113;0
WireConnection;118;0;117;0
WireConnection;119;0;118;1
WireConnection;119;1;120;0
WireConnection;146;0;147;0
WireConnection;134;0;89;4
WireConnection;134;1;112;0
WireConnection;144;0;146;0
WireConnection;144;1;145;0
WireConnection;121;0;119;0
WireConnection;121;1;122;0
WireConnection;86;0;90;0
WireConnection;86;1;134;0
WireConnection;125;0;121;0
WireConnection;149;0;144;0
WireConnection;115;0;86;0
WireConnection;115;1;116;0
WireConnection;152;0;149;0
WireConnection;152;1;150;0
WireConnection;80;0;115;0
WireConnection;80;1;81;0
WireConnection;123;0;125;0
WireConnection;106;0;80;0
WireConnection;138;0;143;0
WireConnection;161;0;123;0
WireConnection;161;1;162;0
WireConnection;140;0;163;1
WireConnection;140;1;163;2
WireConnection;140;2;163;3
WireConnection;154;0;152;0
WireConnection;154;1;137;0
WireConnection;135;0;138;0
WireConnection;135;1;140;0
WireConnection;135;2;154;0
WireConnection;104;0;106;0
WireConnection;104;1;107;0
WireConnection;104;2;161;0
WireConnection;153;0;135;0
WireConnection;153;1;158;0
WireConnection;167;0;163;4
WireConnection;167;1;104;0
WireConnection;167;2;166;0
WireConnection;157;0;153;0
WireConnection;157;1;155;0
WireConnection;157;2;156;2
WireConnection;168;0;167;0
WireConnection;76;0;157;0
WireConnection;76;3;168;0
WireConnection;169;0;76;0
ASEEND*/
//CHKSM=18A134089B35CB75BF7657B1B4C13B3419FFDD49