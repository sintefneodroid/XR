﻿#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN)
	#define CLIP_FAR 1
	#define CLIP_NEAR -1
#else
	#define CLIP_FAR 0
	#define CLIP_NEAR 1
#endif

samplerCUBE _Skybox;
float4x4 _EyeProjection[2];
float4x4 _EyeToWorld[2];
float4 _EyeOffset;

inline float4 screenCoords(float2 uv){
	float2 c = (uv - 0.5) * 2;
	float4 vPos = mul(_EyeProjection[unity_StereoEyeIndex], float4(c, CLIP_FAR, 1));
	vPos.xyz /= vPos.w;
	return vPos;
}
inline fixed3 sampleSkybox(float4 vPos){
	float3 dir = normalize(mul(_EyeToWorld[unity_StereoEyeIndex], vPos).xyz);
	return texCUBE(_Skybox, dir).rgb;
}