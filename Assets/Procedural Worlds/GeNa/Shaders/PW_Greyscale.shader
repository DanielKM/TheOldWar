// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Procedural Worlds/GeNa/PW_GreyScale"
{

    Properties
    {
        _MainTex ("", 2D) = "white" {}
        _UseAlpha("_UseAlpha", Float) = 0
    }

    SubShader
    {

        ZTest Always Cull Off ZWrite Off Fog
        {
            Mode Off
        } //Rendering settings

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            //we include "UnityCG.cginc" to use the appdata_img struct

            struct v2f
            {
                float4 pos : POSITION;
                half2 uv : TEXCOORD0;
            };

            //Our Vertex Shader 
            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
                return o;
            }

            sampler2D _MainTex; //Reference in Pass is necessary to let us use this variable in shaders
            float _InvertAlpha;

            //Our Fragment Shader
            float4 frag(v2f i) : COLOR
            {
                float4 orgCol = tex2D(_MainTex, i.uv); //Get the orginal rendered color
                if (_InvertAlpha < 1.0f)
                    orgCol.a = 1.0f - orgCol.a; 
                //Make changes on the color
                float maxValue = max(orgCol.r, orgCol.g);
                maxValue = max(maxValue, orgCol.b);
                maxValue = max(maxValue, orgCol.a);
                // float a = ;
                // a = clamp(1.0f - orgCol.a, 0.f, 1.f);
                // maxValue = max(maxValue, a);
                return saturate(maxValue).xxxx;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}