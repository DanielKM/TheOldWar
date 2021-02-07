Shader "FoW/BasicLighting"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
        LOD 100
		Cull Back
		ZWrite On
		ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed3 lightdir = normalize(fixed3(0.5, -1, 0.5));
				fixed lightstr = saturate(-dot(lightdir, normalize(i.normal)));
				lightstr = sqrt(lightstr);

				fixed4 fullcolor = _Color;
				fixed4 halfcolor = fixed4(_Color.rgb * 0.5f, _Color.a);
                return lerp(halfcolor, fullcolor, lightstr);
            }
            ENDCG
        }
    }
	Fallback "Diffuse"
}
