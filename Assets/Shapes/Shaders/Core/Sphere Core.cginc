﻿// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
UNITY_DEFINE_INSTANCED_PROP(half, _Radius)
UNITY_DEFINE_INSTANCED_PROP(int, _RadiusSpace)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	half pxCoverage : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
    half radiusTarget = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);
    int radiusSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _RadiusSpace);
	LineWidthData widthData = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, radiusTarget * 2, radiusSpace );
    o.pxCoverage = widthData.thicknessPixelsTarget;
    half radius = widthData.thicknessMeters * 0.5;
	half3 localPos = v.vertex.xyz * radius;
	o.pos = LocalToClipPos( localPos );
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	half4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
	return ShapesOutput( color, saturate(i.pxCoverage) ); 
}