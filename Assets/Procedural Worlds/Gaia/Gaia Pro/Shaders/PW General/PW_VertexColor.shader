Shader "PWS/PW_VertexColor"
{
	Properties
	{
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back

		CGPROGRAM

		#pragma target 2.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float4 vertexColor : COLOR;
		};

		uniform float _Metallic;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = pow( i.vertexColor , 2.2 ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
