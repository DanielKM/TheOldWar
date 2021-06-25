Shader "PWS/PW_RiverShader"
{
    // 2020 Procedural Worlds Pty Limited
    Properties
    {
        [Header(Water Color)]
        _Color ("Color", Color) = (0.110,0.169,0.056,1)
        _ColorDepthStrength("Color Depth Strength", Range(0,1)) = 0.729
        _TintColor ("Tint Color", Color) = (0.7169812,0.521251,0.3754005,1)
        _TintStrength("Tint Strength", Range(0,1)) = 1
        
        [Header(Water PBR)]
        _Glossiness ("Smoothness", Range(0,1)) = 0.95
        _Specular ("Specular Color", Color) = (0.424,0.424,0.424,1)
        
        [Header(Flow)]
        _Speed ("Speed", Range(0.001,2)) = 0.158
        _NormalShift("Normal Shift", Range(0.01, 0.5)) = 0.05
        
        [Header(Blends)]
        _ShoreBlend ("Shore Blend", Range(0.01, 1)) = 0.13
        _NormalShoreBlend("Shore Normal Blend", Range(0.01, 1)) = 0.4
        
        [Header(Normal and Height)]
        _PackedNrmlHght ("Normal(RGB) Height(A)", 2D) = "" {}
        _NormalStrength ("Normal Strength", Range(0.05,1)) = 0.341
        _RippleHeight ("Shore Ripple Height", Range(0.01,1)) = 0.16
        _RefractionStrength("Refraction Strength", Range(0.01,1)) = 0.1
        
        [Header(Foam)]
        _FoamColor ("Foam Color", Color) = (1,1,1,0.2196078)
        _FoamAlbedo("Foam Albedo", 2D) = "white" {}
        [NoScaleOffset] _FoamNormal("Foam Normal", 2D) = "bump" {}
        _FoamNormalStrength ("Foam Normal Strength", Range(0.01, 2)) = 0.633
        [NoScaleOffset] _FoamMask("Foam Mask", 2D) = "white" {}
        _FoamBlend ("Foam Shore Blend", Range(0.01, 1)) = 0.125
        _FoamHeight ("Foam Height", Range(0.01, 1)) = 0.129
        _FoamRipple ("Foam Ripple", Range(0.01, 1)) = 0.09
        _FoamSpeed ("Foam Speed", Range(0.001,2)) = 0.137
        
        [Header(Sea Level)]
        _PWOceanHeight ("Sea Level Height", float) = 25
        _SeaLevelBlend ("Sea Level Blend", Range(0.001,10)) = 1.25
        _SeaLevelFoamColor ("Sea Level Foam Color", Color) = (1,1,1,0.78)
        _SeaLevelFoamNormalStrength ("Sea Level Foam Normal Strength", Range(0.01, 2)) = 1
        [HDR]_SeaLevelFoamMaskAttenuation("(R)Spec (G)Occlusion (B)Height (A) Gloss", Color) = (0.0274,1,1,0.95)
    }
    SubShader
    {
       Tags { "RenderType"="Transparent" "Queue"="Transparent-1 " "ForceNoShadowCasting"="True"}
       LOD 200
       Cull Back
       
       // first pass Zprime to check for camera going behind water
       // set stencil buffer to 2 when this happens
        
       Pass
       {
           
           Name "Zprime"
           
           ZTest Lequal
           Zwrite On
           Cull Back
           ColorMask 0
           
            Stencil {
                Ref 2
                Comp always
                Pass replace
            }
           
           CGPROGRAM
               #pragma vertex vert
               #pragma fragment frag
               #pragma fragmentoption ARB_precision_hint_fastest
               #include "UnityCG.cginc"

           struct v2f {
               float4 pos          : POSITION;
           };

           v2f vert (appdata_full v)
           {
               v2f o;
               o.pos = UnityObjectToClipPos(v.vertex);   
               return o;
           }


               half4 frag( v2f i ) : COLOR
               {
                   
                   return half4(0,0,0,0);
               }
           ENDCG          
       }
        
   
       // Main draw
        
 ZTest Lequal
 Zwrite off
        
  Stencil {
         Ref 2
         Comp equal
         Pass keep
     }
  
  CGPROGRAM
  // Physically based Standard lighting model
  #pragma surface surf StandardSpecular vertex:vert alpha:blend
  #pragma target 3.0

  // Water
  sampler2D _PackedNrmlHght;
  float _Glossiness;
  float4 _Color,_TintColor,_Specular;
  float _ColorDepthStrength,_TintStrength;
  float _Speed;
  float _NormalStrength,_RippleHeight,_RefractionStrength,_NormalShift;
  float _NormalShoreBlend,_ShoreBlend;
  
  //External
  sampler2D_float _CameraDepthTexture;
  sampler2D _CameraOpaqueTexture;
  float4 _CameraOpaqueTexture_TexelSize;

  //Foam
  sampler2D _FoamAlbedo;
  sampler2D _FoamMask;
  sampler2D _FoamNormal;
  float4 _FoamColor;
  float _FoamBlend,_FoamHeight,_FoamRipple,_FoamNormalStrength,_FoamSpeed;

  //Ocean Foam
  float _PWOceanHeight, _SeaLevelBlend,_SeaLevelFoamNormalStrength;
  float4 _SeaLevelFoamColor,_SeaLevelFoamMaskAttenuation;
  
  struct Input
  {
      float2 uv_PackedNrmlHght;
      float2 uv_FoamAlbedo;
      float4 screenPos;
      //float eyeDepth;
      float3 worldPos;
      float2 baseUV;
  };

  // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
  // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
  // #pragma instancing_options assumeuniformscaling
  UNITY_INSTANCING_BUFFER_START(Props)
      // put more per-instance properties here
  UNITY_INSTANCING_BUFFER_END(Props)

  void vert (inout appdata_full v, out Input o)
  {
      UNITY_INITIALIZE_OUTPUT(Input, o);
      o.baseUV = v.texcoord;
  }

  void surf (Input IN, inout SurfaceOutputStandardSpecular o)
  {
      float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, IN.screenPos).r;
      float sceneZ = LinearEyeDepth(rawZ).r;
      float surfaceZ = IN.screenPos.w;
      float depth = sceneZ - surfaceZ;

      // Compute Combined Normal
      float2 uv = IN.uv_PackedNrmlHght;
      uv.y += frac(_Time.y * -_Speed);

      float2 foamUv = IN.uv_FoamAlbedo;
      foamUv.y += frac(_Time.y * -_FoamSpeed);

      float4 normalA = tex2D(_PackedNrmlHght, uv);
      float4 normalB = tex2D(_PackedNrmlHght, uv + float2(_SinTime.w * _NormalShift, frac(_Time.y * 0.12f)));
      
      o.Normal = float3(normalA.rg * 2 - 1,normalA.a);
      o.Normal += float3(normalB.rg * 2 - 1,normalB.a * 0.7f);
      float normShoreBlend = 1.0f - saturate(exp(-depth / _NormalShoreBlend));
      normShoreBlend *= normShoreBlend;
      o.Normal.xy *= normShoreBlend * _NormalStrength;
      o.Normal.xy = saturate(o.Normal.xy);

      // Color depth Calculations
      float colorAlpha = 1 - saturate(exp(-depth * _ColorDepthStrength * 2.0f));
      _Color.a *= colorAlpha;
      _Color.rgb *= colorAlpha;

      //Sea Level Blend
      float seaLevelBlendIn = (IN.worldPos.y - _PWOceanHeight ) * _SeaLevelBlend;
      float sealevelBlendOut = 1.0f-saturate(-seaLevelBlendIn * 3.0f);
      float uvWidth = (saturate(IN.baseUV.xx) - 0.5f) * 2.0f;
      float edgeFalloff = 1.0f-dot(uvWidth,uvWidth);
      seaLevelBlendIn = 1.0f- saturate(lerp(edgeFalloff *  seaLevelBlendIn, saturate(1 + edgeFalloff) *  seaLevelBlendIn, seaLevelBlendIn * seaLevelBlendIn));
      seaLevelBlendIn *= _SeaLevelFoamColor.a;
          
      // Foam Calculations
      // Compute edge foam contribution
      float2 foamUV =  foamUv + float2(_SinTime.w * _NormalShift * 0.3f, frac(_Time.y * 0.07f)) + ((normalA.a + normalB.a - 1.0) * _FoamRipple);
      float4 foamAlbedo = tex2D(_FoamAlbedo, foamUV);
      float3 foamNormal = UnpackNormal(tex2D(_FoamNormal, foamUV));
      float4 foamMask = tex2D(_FoamMask, foamUV);
      float foamBlend = 1.0f-saturate((depth - (foamMask.b - 0.5f) * _FoamHeight) / _FoamBlend);

      //Add Ocean foam values into Foam
      foamBlend = saturate(foamBlend+seaLevelBlendIn);
      _FoamColor = lerp(_FoamColor,_SeaLevelFoamColor,seaLevelBlendIn);
      _FoamNormalStrength = lerp(_FoamNormalStrength,_SeaLevelFoamNormalStrength,seaLevelBlendIn);
      foamMask = lerp(foamMask,_SeaLevelFoamMaskAttenuation,seaLevelBlendIn);
 
      float4 foamColor = foamBlend * _FoamColor * foamAlbedo;
      o.Normal.xy = lerp( o.Normal.xy,foamNormal.xy * _FoamNormalStrength,foamBlend * foamColor.a);
      o.Normal.z = sqrt(1.0f - saturate(dot(o.Normal.xy, o.Normal.xy)));

      // GrabPass Calculations
      // Compute refraction color (from Grabpass).
      float2 offset = o.Normal.xy  * 100.0f * depth * _RefractionStrength;// * _GrabPassTexture_TexelSize.xy;
      IN.screenPos.xy += offset * IN.screenPos.z;
      float tintAlpha = 1 - saturate(exp(-depth * _TintStrength));
      float4 grabCol = tex2Dproj( _CameraOpaqueTexture, UNITY_PROJ_COORD(IN.screenPos));
      grabCol = lerp(grabCol, _TintColor * grabCol, tintAlpha) * 0.70f;

      // Combine with riverbed
      grabCol = grabCol * (1-foamColor.a);
      float depthOffset = (normalA.a + normalB.a - 1.0f + foamMask.b * foamColor.a * _FoamHeight) * _RippleHeight;
      
      o.Alpha = saturate((depth + depthOffset) / _ShoreBlend) * sealevelBlendOut;
      grabCol *= 1.0f - _Color.a;
      o.Emission = grabCol.rgb;

      // Metallic and smoothness come from slider variables.
      _Specular =lerp(_Specular,_SeaLevelFoamMaskAttenuation.r,seaLevelBlendIn);
      _Glossiness =lerp(_Glossiness,_SeaLevelFoamMaskAttenuation.a,seaLevelBlendIn);
      
      o.Specular = lerp(_Specular, foamMask.r, foamColor.a);
      o.Smoothness = lerp(_Glossiness, foamMask.a, foamColor.a);
      o.Albedo = foamColor * foamColor.a + _Color.rgb * _Color.a * (1-foamColor.a);
    
  }
  ENDCG
        
        ZTest Always
        Zwrite off
        
        Stencil {
               Ref 2
               Comp NotEqual
               Pass keep 
            }
        
        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf StandardSpecular vertex:vert alpha:blend
        #pragma target 3.0

        // Water
        sampler2D _PackedNrmlHght;
        float _Glossiness;
        float4 _Color,_TintColor,_Specular;
        float _ColorDepthStrength,_TintStrength;
        float _Speed;
        float _NormalStrength,_RippleHeight,_RefractionStrength,_NormalShift;
        float _NormalShoreBlend,_ShoreBlend;
        
        //External
        sampler2D_float _CameraDepthTexture;
        sampler2D _CameraOpaqueTexture;
        float4 _CameraOpaqueTexture_TexelSize;

        //Foam
        sampler2D _FoamAlbedo;
        sampler2D _FoamMask;
        sampler2D _FoamNormal;
        float4 _FoamColor;
        float _FoamBlend,_FoamHeight,_FoamRipple,_FoamNormalStrength,_FoamSpeed;

        //Ocean Foam
        float _PWOceanHeight, _SeaLevelBlend,_SeaLevelFoamNormalStrength;
        float4 _SeaLevelFoamColor,_SeaLevelFoamMaskAttenuation;
        
        struct Input
        {
            float2 uv_PackedNrmlHght;
            float2 uv_FoamAlbedo;
            float4 screenPos;
            //float eyeDepth;
            float3 worldPos;
            float2 baseUV;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.baseUV = v.texcoord;
        }

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, IN.screenPos).r;
            float sceneZ = LinearEyeDepth(rawZ).r;
            float surfaceZ = IN.screenPos.w;
            float depth = sceneZ - surfaceZ;

            // Compute Combined Normal
            float2 uv = IN.uv_PackedNrmlHght;
            uv.y += frac(_Time.y * -_Speed);

            float2 foamUv = IN.uv_FoamAlbedo;
            foamUv.y += frac(_Time.y * -_FoamSpeed);

            float4 normalA = tex2D(_PackedNrmlHght, uv);
            float4 normalB = tex2D(_PackedNrmlHght, uv + float2(_SinTime.w * _NormalShift, frac(_Time.y * 0.12f)));
            
            o.Normal = float3(normalA.rg * 2 - 1,normalA.a);
            o.Normal += float3(normalB.rg * 2 - 1,normalB.a * 0.7f);
            float normShoreBlend = 1.0f - saturate(exp(-depth / _NormalShoreBlend));
            normShoreBlend *= normShoreBlend;
            o.Normal.xy *= normShoreBlend * _NormalStrength;
            o.Normal.xy = saturate(o.Normal.xy);

            // Color depth Calculations
            float colorAlpha = 1 - saturate(exp(-depth * _ColorDepthStrength * 2.0f));
            _Color.a *= colorAlpha;
            _Color.rgb *= colorAlpha;

            //Sea Level Blend
            float seaLevelBlendIn = (IN.worldPos.y - _PWOceanHeight ) * _SeaLevelBlend;
            float sealevelBlendOut = 1.0f-saturate(-seaLevelBlendIn * 3.0f);
            float uvWidth = (saturate(IN.baseUV.xx) - 0.5f) * 2.0f;
            float edgeFalloff = 1.0f-dot(uvWidth,uvWidth);
            seaLevelBlendIn = 1.0f- saturate(lerp(edgeFalloff *  seaLevelBlendIn, saturate(1 + edgeFalloff) *  seaLevelBlendIn, seaLevelBlendIn * seaLevelBlendIn));
            seaLevelBlendIn *= _SeaLevelFoamColor.a;
                
            // Foam Calculations
            // Compute edge foam contribution
            float2 foamUV =  foamUv + float2(_SinTime.w * _NormalShift * 0.3f, frac(_Time.y * 0.07f)) + ((normalA.a + normalB.a - 1.0) * _FoamRipple);
            float4 foamAlbedo = tex2D(_FoamAlbedo, foamUV);
            float3 foamNormal = UnpackNormal(tex2D(_FoamNormal, foamUV));
            float4 foamMask = tex2D(_FoamMask, foamUV);
            float foamBlend = 1.0f-saturate((depth - (foamMask.b - 0.5f) * _FoamHeight) / _FoamBlend);

            //Add Ocean foam values into Foam
            foamBlend = saturate(foamBlend+seaLevelBlendIn);
            _FoamColor = lerp(_FoamColor,_SeaLevelFoamColor,seaLevelBlendIn);
            _FoamNormalStrength = lerp(_FoamNormalStrength,_SeaLevelFoamNormalStrength,seaLevelBlendIn);
            foamMask = lerp(foamMask,_SeaLevelFoamMaskAttenuation,seaLevelBlendIn);
      
            float4 foamColor = foamBlend * _FoamColor * foamAlbedo;
            o.Normal.xy = lerp( o.Normal.xy,foamNormal.xy * _FoamNormalStrength,foamBlend * foamColor.a);
            o.Normal.z = sqrt(1.0f - saturate(dot(o.Normal.xy, o.Normal.xy)));

            // GrabPass Calculations
            // Compute refraction color (from Grabpass).
            float2 offset = o.Normal.xy  * 100.0f * depth * _RefractionStrength;// * _GrabPassTexture_TexelSize.xy;
            IN.screenPos.xy += offset * IN.screenPos.z;
            float tintAlpha = 1 - saturate(exp(-depth * _TintStrength));
            float4 grabCol = tex2Dproj( _CameraOpaqueTexture, UNITY_PROJ_COORD(IN.screenPos));
            grabCol = lerp(grabCol, _TintColor * grabCol, tintAlpha) * 0.70f;

            // Combine with riverbed
            grabCol = grabCol * (1-foamColor.a);
            float depthOffset = (normalA.a + normalB.a - 1.0f + foamMask.b * foamColor.a * _FoamHeight) * _RippleHeight;
            
            o.Alpha = saturate((depth + depthOffset) / _ShoreBlend) * sealevelBlendOut;
            grabCol *= 1.0f - _Color.a;
            o.Emission = grabCol.rgb;

            // Metallic and smoothness come from slider variables.
            _Specular =lerp(_Specular,_SeaLevelFoamMaskAttenuation.r,seaLevelBlendIn);
            _Glossiness =lerp(_Glossiness,_SeaLevelFoamMaskAttenuation.a,seaLevelBlendIn);
            
            o.Specular = lerp(_Specular, foamMask.r, foamColor.a);
            o.Smoothness = lerp(_Glossiness, foamMask.a, foamColor.a);
            o.Albedo = foamColor * foamColor.a + _Color.rgb * _Color.a * (1-foamColor.a);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
