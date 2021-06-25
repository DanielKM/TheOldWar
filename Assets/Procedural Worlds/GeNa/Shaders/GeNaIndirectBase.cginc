#ifndef STORM_INDIRECT_BASE_INCLUDE
#define STORM_INDIRECT_BASE_INCLUDE
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
/*
float4x4 StormInverse(float4x4 input)
{
#define StormMinor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
	float4x4 AdjugateMatrix = float4x4(
		//find the cofactor of each section to create the Adjugate
		StormMinor(_22_23_24, _32_33_34, _42_43_44),
		-StormMinor(_21_23_24, _31_33_34, _41_43_44),
		StormMinor(_21_22_24, _31_32_34, _41_42_44),
		-StormMinor(_21_22_23, _31_32_33, _41_42_43),

		-StormMinor(_12_13_14, _32_33_34, _42_43_44),
		StormMinor(_11_13_14, _31_33_34, _41_43_44),
		-StormMinor(_11_12_14, _31_32_34, _41_42_44),
		StormMinor(_11_12_13, _31_32_33, _41_42_43),

		StormMinor(_12_13_14, _22_23_24, _42_43_44),
		-StormMinor(_11_13_14, _21_23_24, _41_43_44),
		StormMinor(_11_12_14, _21_22_24, _41_42_44),
		-StormMinor(_11_12_13, _21_22_23, _41_42_43),

		-StormMinor(_12_13_14, _22_23_24, _32_33_34),
		StormMinor(_11_13_14, _21_23_24, _31_33_34),
		-StormMinor(_11_12_14, _21_22_24, _31_32_34),
		StormMinor(_11_12_13, _21_22_23, _31_32_33)

		);
	return transpose(AdjugateMatrix) / determinant(input);
#undef StormMinor
}
//*/
StructuredBuffer<float4x4> StormMatrix;
StructuredBuffer<float4x4> StormInverse;
#ifdef STORM_FRAGMENT
float4x4 OriginalMatrix;
#endif	
#endif

void storm_setup()
{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
#ifdef STORM_FRAGMENT
	OriginalMatrix = unity_WorldToObject;
#endif	
	unity_ObjectToWorld = StormMatrix[unity_InstanceID];
	unity_WorldToObject = StormInverse[unity_InstanceID];
	//unity_WorldToObject = StormInverse(unity_ObjectToWorld);
#endif
}
#endif