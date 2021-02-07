Shader "Hidden/FogOfWarClouds"
{
    Properties
    {
		_Color1("Color 1", Color) = (1, 1, 1, 1)
		_Frequency1("Frequency 1", float) = 1
		_Speed1("Speed 1", float) = 1
		_Color2("Color 2", Color) = (1, 1, 1, 1)
		_Frequency2("Frequency 2", float) = 1
		_Speed2("Speed 2", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
            };

			float4 _Color1;
			float4 _Color2;
			float _Frequency1;
			float _Frequency2;
			float _Speed1;
			float _Speed2;

			float4 mod(float4 x, float4 y)
			{
				return x - y * floor(x / y);
			}

			float4 mod289(float4 x)
			{
				return x - floor(x / 289.0) * 289.0;
			}

			float4 permute(float4 x)
			{
				return mod289(((x*34.0) + 1.0)*x);
			}

			float4 taylorInvSqrt(float4 r)
			{
				return (float4)1.79284291400159 - r * 0.85373472095314;
			}

			float2 fade(float2 t) {
				return t * t*t*(t*(t*6.0 - 15.0) + 10.0);
			}

			// Classic Perlin noise
			float Noise(float2 P)
			{
				float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
				float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
				Pi = mod289(Pi); // To avoid truncation effects in permutation
				float4 ix = Pi.xzxz;
				float4 iy = Pi.yyww;
				float4 fx = Pf.xzxz;
				float4 fy = Pf.yyww;

				float4 i = permute(permute(ix) + iy);

				float4 gx = frac(i / 41.0) * 2.0 - 1.0;
				float4 gy = abs(gx) - 0.5;
				float4 tx = floor(gx + 0.5);
				gx = gx - tx;

				float2 g00 = float2(gx.x, gy.x);
				float2 g10 = float2(gx.y, gy.y);
				float2 g01 = float2(gx.z, gy.z);
				float2 g11 = float2(gx.w, gy.w);

				float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
				g00 *= norm.x;
				g01 *= norm.y;
				g10 *= norm.z;
				g11 *= norm.w;

				float n00 = dot(g00, float2(fx.x, fy.x));
				float n10 = dot(g10, float2(fx.y, fy.y));
				float n01 = dot(g01, float2(fx.z, fy.z));
				float n11 = dot(g11, float2(fx.w, fy.w));

				float2 fade_xy = fade(Pf.xy);
				float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
				float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
				return 2.3 * n_xy;
			}

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
				float t1 = Noise(i.worldPos.xz * _Frequency1 + _Time.y * _Speed1);
				float t2 = Noise(float2(i.worldPos.z, -i.worldPos.x) * _Frequency2 + _Time.y * _Speed2);
				float t = (t1 + t2) * 0.5 + 0.5;
                return lerp(_Color1, _Color2, t);
            }
            ENDCG
        }
    }
}
