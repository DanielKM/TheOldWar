Shader "Hidden/FogOfWarURP"
{
    HLSLINCLUDE
    #pragma target 4.5
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	
	#pragma multi_compile CAMERA_ORTHOGRAPHIC CAMERA_PERSPECTIVE
	#pragma multi_compile PLANE_XY PLANE_YZ PLANE_XZ
	#pragma multi_compile FOG_COLORED FOG_TEXTURED_WORLD FOG_TEXTURED_SCREEN
	#pragma multi_compile _ FOGFARPLANE
	#pragma multi_compile _ FOGOUTSIDEAREA

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        //UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
	    float3 interpolatedRay : TEXCOORD1;
        //UNITY_VERTEX_OUTPUT_STEREO
    };

    ///// FogOfWar Start /////
    
    TEXTURE2D_X(_MainTex);
    TEXTURE2D(_FogTex);
	SAMPLER(sampler_FogTex);
	TEXTURE2D_FLOAT(_CameraDepthTexture);//CHANGED: this is replaced by SampleCameraDepth()
	SAMPLER(sampler_CameraDepthTexture);
    TEXTURE2D(_FogColorTex);
	SAMPLER(sampler_FogColorTex);
    uniform float2 _FogColorTexScale;

    // for fast world space reconstruction
    uniform float4x4 _FoWInverseView; //CHANGED: HDRP's equivalent appears to be wrong
    uniform float4x4 _FoWInverseProj; //CHANGED: HDRP's equivalent appears to be wrong

    uniform float3 _CameraWorldPosition;
    uniform float _FogTextureSize;
    uniform float _MapSize;
    uniform float4 _MapOffset;
    uniform float4 _MainFogColor;
    uniform float _OutsideFogStrength;
    uniform float _StereoSeparation;

    Varyings Vert(Attributes input)
    {
        Varyings output;
        //UNITY_SETUP_INSTANCE_ID(input);
        //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        //
        
	    float far = _ProjectionParams.z;
	    float2 orthoSize = unity_OrthoParams.xy;
	    float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

	    // Vertex pos -> clip space vertex position
		float3 viewpos = float3(output.texcoord.xy * 2 - 1, 1);

	    // Perspective: view space vertex position of the far plane
	    float3 rayPers = mul(_FoWInverseProj, viewpos.xyzz * far).xyz;//CHANGED: _FoWInverseProj was unity_CameraInvProjection

	    // Orthographic: view space vertex position
	    float3 rayOrtho = float3(orthoSize * viewpos.xy, 0);
		
        output.positionCS = float4(viewpos.x, -viewpos.y, 1, 1);
        output.texcoord = (viewpos.xy + 1) * 0.5;
	    output.interpolatedRay = lerp(rayPers, rayOrtho, isOrtho);

        //

        return output;
    }

    ///// FogOfWar End /////

    struct FogData
    {
	    float3 cameraPosition;
	    float3 worldPosition;
	    float2 mapPosition;
	    float2 screenPosition;
	    float2 planePosition;
	    float fogAmount;
	    float4 backgroundColor;
    };

	inline float Linear01Depth( float z )
	{
		//return LinearEyeDepth(z, _ZBufferParams);// 
		return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
	}

	float3 ComputeViewSpacePosition(float2 texcoord, float3 ray, out float rawdepth)
	{
		// Render settings
		float near = _ProjectionParams.y;
		float far = _ProjectionParams.z;
		float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

		// Z buffer sample
		float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, texcoord);

		// Far plane exclusion
		#if !defined(EXCLUDE_FAR_PLANE)
		float mask = 1;
		#elif defined(UNITY_REVERSED_Z)
		float mask = z > 0;
		#else
		float mask = z < 1;
		#endif

		// Perspective: view space position = ray * depth
		rawdepth = Linear01Depth(z);//CHANGED: This is no longer a function and has been manually created
		float3 vposPers = ray * rawdepth;

		// Orthographic: linear depth (with reverse-Z support)
		#if defined(UNITY_REVERSED_Z)
		float depthOrtho = -lerp(far, near, z);
		#else
		float depthOrtho = -lerp(near, far, z);
		#endif

		// Orthographic: view space position
		float3 vposOrtho = float3(ray.xy, depthOrtho);

		// Result: view space position
		return lerp(vposPers, vposOrtho, isOrtho) * mask;
	}

	float3 GetWorldPosition(float2 uv, float3 interpolatedRay, out float rawdepth)
	{
		// for VR
		//uv = UnityStereoTransformScreenSpaceTex(uv);//CHANGED: This is set at the start of the fragment shader now

		float3 viewspacepos = ComputeViewSpacePosition(uv, interpolatedRay, rawdepth);
		float3 wsPos = mul(_FoWInverseView, float4(viewspacepos, 1)).xyz;

		// single pass VR requires the world space separation between eyes to be manually set
		#if UNITY_SINGLE_PASS_STEREO
		wsPos.x += unity_StereoEyeIndex * _StereoSeparation;
		#endif

		return wsPos;
	}

	float2 WorldPositionToFogPosition(float3 worldpos)
	{
		#ifdef PLANE_XY
			float2 modepos = worldpos.xy;
		#elif PLANE_YZ
			float2 modepos = worldpos.yz;
		#else//#elif PLANE_XZ
			float2 modepos = worldpos.xz;
		#endif

		return (modepos - _MapOffset.xy) / _MapSize + float2(0.5f, 0.5f);
	}

	float GetFogAmount(float2 fogpos, float rawdepth)
	{
		// if it is beyond the map
		float isoutsidemap = min(1, step(fogpos.x, 0) + step(1, fogpos.x) + step(fogpos.y, 0) + step(1, fogpos.y));

		// if outside map, use the outside fog color
		float fog = lerp(SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, fogpos).a, _OutsideFogStrength, isoutsidemap);//CHANGED: tex2D changed to LOAD_TEXTURE2D_X
				
		#ifndef FOGFARPLANE
			if (rawdepth == 0) // there's some weird behaviour with the far plane in some rare cases that this should fix...
				fog = 0;
			else
				fog *= step(rawdepth, 0.999); // don't show fog on the far plane
		#endif

		return fog;
	}

	FogData GetFogData(float2 uv, float3 interpolatedray)
	{
		FogData fogdata;

		float rawdepth;
		fogdata.worldPosition = GetWorldPosition(uv, interpolatedray, rawdepth);
		fogdata.mapPosition = WorldPositionToFogPosition(fogdata.worldPosition);
		fogdata.fogAmount = GetFogAmount(fogdata.mapPosition, rawdepth);
		fogdata.screenPosition = uv;
		fogdata.cameraPosition = _CameraWorldPosition;
	    fogdata.backgroundColor = LOAD_TEXTURE2D_X(_MainTex, uv * _ScreenParams.xy);//CHANGED: tex2D changed to LOAD_TEXTURE2D_X, added _ScreenParams

		float3 rayorigin = _CameraWorldPosition;
		float3 raydir = normalize(fogdata.worldPosition - rayorigin);
		float3 planeorigin = float3(0, _FogColorTexScale.y, 0);
		float3 planenormal = float3(0, 1, 0);
		float t = dot(planeorigin - rayorigin, planenormal) / dot(planenormal, raydir);
		fogdata.planePosition = (rayorigin + raydir * t).xz * _FogColorTexScale.x;

		return fogdata;
	}

	float4 GetDefaultFogColor(FogData fogdata)
	{
		#ifdef FOG_COLORED
			return _MainFogColor;
		#elif FOG_TEXTURED_WORLD
			return SAMPLE_TEXTURE2D(_FogColorTex, sampler_FogColorTex, fogdata.planePosition);//CHANGED: tex2D changed to LOAD_TEXTURE2D_X
		#elif FOG_TEXTURED_SCREEN
			return SAMPLE_TEXTURE2D(_FogColorTex, sampler_FogColorTex, fogdata.screenPosition);//CHANGED: tex2D changed to LOAD_TEXTURE2D_X
		#endif
	}

	float4 FogShader(FogData fogdata)
	{
		half4 fogcolor = GetDefaultFogColor(fogdata);
		return lerp(fogdata.backgroundColor, fogcolor, fogdata.fogAmount * fogcolor.a);
	}
	
    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		
	    FogData fogdata = GetFogData(input.texcoord, input.interpolatedRay);
		return FogShader(fogdata);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "FogOfWarHDRP"
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }

    Fallback Off
}
