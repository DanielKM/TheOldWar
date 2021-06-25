// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PWS/VFX/PW_Clouds"
{
	Properties
	{
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_MainTex1("MainTex", 2D) = "white" {}
		_Opacity1("Opacity", Range( 0 , 1)) = 1
		_MaskExp1("MaskExp", Range( 0.1 , 8)) = 1
		_LightMultiplier("Light Multiplier", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
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
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR
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
					float3 ase_normal : NORMAL;
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
				UNITY_DECLARE_TEX2D_NOSAMPLER(_MainTex1);
				uniform float4 _MainTex1_ST;
				SamplerState sampler_MainTex1;
				uniform float3 PW_SunDirection;
				uniform float4 PW_SunColor;
				uniform float4 PW_AmbientColor;
				uniform float _LightMultiplier;
				uniform float PW_SkyDome_Brightness;
				uniform float PW_Clouds_Fade;
				uniform float _MaskExp1;
				uniform float _Opacity1;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
					o.ase_texcoord3.xyz = ase_worldNormal;
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord4.xyz = ase_worldPos;
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
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

					float2 uv_MainTex1 = i.texcoord.xy * _MainTex1_ST.xy + _MainTex1_ST.zw;
					float4 tex2DNode45 = SAMPLE_TEXTURE2D( _MainTex1, sampler_MainTex1, uv_MainTex1 );
					float3 appendResult4 = (float3(0.0 , PW_SunDirection.y , PW_SunDirection.z));
					float3 normalizeResult6 = normalize( appendResult4 );
					float3 ase_worldNormal = i.ase_texcoord3.xyz;
					float fresnelNdotV9 = dot( ase_worldNormal, normalizeResult6 );
					float fresnelNode9 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV9, 3.0 ) );
					float clampResult21 = clamp( fresnelNode9 , 0.0 , 1.0 );
					float3 normalizeResult12 = normalize( PW_SunDirection );
					float fresnelNdotV15 = dot( ase_worldNormal, normalizeResult12 );
					float fresnelNode15 = ( 0.0 + 0.25 * pow( 1.0 - fresnelNdotV15, 6.0 ) );
					float4 blendOpSrc27 = ( clampResult21 * PW_SunColor );
					float4 blendOpDest27 = ( fresnelNode15 * PW_SunColor );
					float clampResult14 = clamp( ( 1.0 - fresnelNode9 ) , 0.0 , 1.0 );
					float4 lerpBlendMode27 = lerp(blendOpDest27,2.0f*blendOpDest27*blendOpSrc27 + blendOpDest27*blendOpDest27*(1.0f - 2.0f*blendOpSrc27),( PW_AmbientColor * clampResult14 * 1.0 ).r);
					float3 ase_worldPos = i.ase_texcoord4.xyz;
					float clampResult29 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / PW_Clouds_Fade ) , 1.8 ) , 0.0 , 1.0 );
					float4 appendResult42 = (float4(( ( tex2DNode45 * ( ( saturate( lerpBlendMode27 )) * _LightMultiplier ) * i.color ) * ( PW_SkyDome_Brightness * clampResult29 ) ).rgb , ( ( pow( tex2DNode45.a , _MaskExp1 ) * i.color.a * _Opacity1 ) * clampResult29 )));
					

					fixed4 col = appendResult42;
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
2026;77;1153;925;460.5103;817.1752;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;2;-2360.014,-711.7124;Inherit;False;2059.479;943.4741;Comment;17;27;46;22;23;26;17;19;14;15;16;21;12;13;9;6;4;3;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;3;-2310.014,-48.9314;Float;False;Global;PW_SunDirection;PW_SunDirection;1;0;Create;False;0;0;False;0;False;0,0,0;0,0,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;4;-2074.702,63.71045;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;6;-1889.466,-95.77954;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;5;-1327.622,276.0525;Inherit;False;961.6251;498.87;Comment;7;29;25;20;11;10;8;7;DistanceFade;1,1,1,1;0;0
Node;AmplifyShaderEditor.FresnelNode;9;-1716.881,-218.4045;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;13;-1441.111,-184.7224;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;12;-2090.654,-26.44556;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;7;-1277.622,354.8186;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;8;-1286.341,515.3726;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;19;-1724.66,-455.3403;Float;False;Global;PW_SunColor;PW_SunColor;2;0;Create;False;0;0;False;0;False;0,0,0,0;0.9998221,0.589154,0.4751627,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;16;-1172.012,-5.464355;Float;False;Constant;_Float4;Float 3;1;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1276.843,675.6906;Float;False;Global;PW_Clouds_Fade;PW_Clouds_Fade;3;0;Create;False;0;0;False;0;False;128;272;0;512;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;10;-1041.233,399.5675;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;14;-1170.304,-126.3044;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;-1724.18,-661.7124;Float;False;Global;PW_AmbientColor;PW_AmbientColor;1;0;Create;False;0;0;False;0;False;0.2917853,0.7172324,0.745283,0;0.2871658,0.2815544,0.3312261,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;21;-1061.48,-500.8015;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;15;-1698.75,29.10547;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0.25;False;3;FLOAT;6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-884.5342,-494.4866;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;20;-847.5459,403.5636;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;128;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;18;-288.104,-684.9265;Inherit;False;1523.398;723.1475;Comment;12;45;42;38;37;35;34;33;32;31;30;28;24;Main Texture and Mixing;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-893.2539,-360.9604;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-891.4902,-240.5076;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;45;-238.104,-614.2944;Inherit;True;Property;_MainTex1;MainTex;0;0;Create;False;0;0;False;0;False;-1;472758e9340981942a05fc1b2e946cac;dc68ac1cb7a6531479268b63e792c1c0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;-209.4351,-399.9104;Float;False;Property;_MaskExp1;MaskExp;2;0;Create;True;0;0;False;0;False;1;2.3;0.1;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;25;-699.2402,403.6975;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-677.4868,-97.55171;Inherit;False;Property;_LightMultiplier;Light Multiplier;4;0;Create;True;0;0;False;0;False;2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;27;-732.7623,-397.6185;Inherit;True;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;32;154.0327,-510.5115;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-192.1372,-271.6785;Float;False;Property;_Opacity1;Opacity;1;0;Create;True;0;0;False;0;False;1;0.25;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-416.8191,-317.6711;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;28;159.1899,-634.9265;Float;False;Global;PW_SkyDome_Brightness;PW_SkyDome_Brightness;3;0;Create;False;0;0;False;0;False;1;0.675;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;30;-76.70703,-168.7795;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;29;-518.4912,390.3616;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;576.895,-473.4524;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;393.0229,-329.8904;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;160.874,-397.0894;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;36;-1331.079,843.7345;Inherit;False;977.6294;412.2828;Comment;5;44;43;41;40;39;Deprecated;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;818.8418,-405.0564;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;843.6011,-274.7834;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;41;-936.479,1047.974;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;40;-1098.079,1013.018;Float;False;Constant;_Float1;Float 0;3;0;Create;True;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-845.2959,893.7345;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;43;-684.4492,946.1276;Inherit;True;Property;_PW_Clouds_HDRI1;PW_Clouds_HDRI;3;0;Create;True;0;0;False;0;False;-1;60995c357a320a4488263256076c8893;60995c357a320a4488263256076c8893;True;0;False;white;LockedToCube;False;Object;-1;MipLevel;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;44;-1281.079,1140.018;Float;False;Global;PW_Clouds_HDRI_Blur_Level1;PW_Clouds_HDRI_Blur_Level;3;0;Create;True;0;0;False;0;False;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;42;1064.294,-355.7434;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;1289.939,-348.4624;Float;False;True;-1;2;ASEMaterialInspector;0;9;PWS/VFX/PW_Clouds;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;True;0
WireConnection;4;1;3;2
WireConnection;4;2;3;3
WireConnection;6;0;4;0
WireConnection;9;4;6;0
WireConnection;13;0;9;0
WireConnection;12;0;3;0
WireConnection;10;0;7;0
WireConnection;10;1;8;0
WireConnection;14;0;13;0
WireConnection;21;0;9;0
WireConnection;15;4;12;0
WireConnection;26;0;21;0
WireConnection;26;1;19;0
WireConnection;20;0;10;0
WireConnection;20;1;11;0
WireConnection;22;0;15;0
WireConnection;22;1;19;0
WireConnection;23;0;17;0
WireConnection;23;1;14;0
WireConnection;23;2;16;0
WireConnection;25;0;20;0
WireConnection;27;0;26;0
WireConnection;27;1;22;0
WireConnection;27;2;23;0
WireConnection;32;0;45;4
WireConnection;32;1;24;0
WireConnection;47;0;27;0
WireConnection;47;1;46;0
WireConnection;29;0;25;0
WireConnection;34;0;28;0
WireConnection;34;1;29;0
WireConnection;33;0;32;0
WireConnection;33;1;30;4
WireConnection;33;2;31;0
WireConnection;35;0;45;0
WireConnection;35;1;47;0
WireConnection;35;2;30;0
WireConnection;37;0;35;0
WireConnection;37;1;34;0
WireConnection;38;0;33;0
WireConnection;38;1;29;0
WireConnection;39;0;40;0
WireConnection;39;1;44;0
WireConnection;43;1;41;0
WireConnection;43;2;39;0
WireConnection;42;0;37;0
WireConnection;42;3;38;0
WireConnection;1;0;42;0
ASEEND*/
//CHKSM=77C0AAC48DAAD71AE369A1AFF1CC3A0D12F899BD