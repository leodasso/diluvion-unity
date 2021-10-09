// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


// Surface version of the MeshDefault Grab shader.
// Could be used when a mesh with blend mode effect needs to receive shadows.

Shader "BlendModes/Extra/SurfMesh" 
{
	Properties
	{
		_Color("Tint Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags 
		{ 
			"Queue" = "AlphaTest" 
			"IgnoreProjector" = "True" 
		}
		
		LOD 200

		GrabPass { }

		CGPROGRAM

		#include "UnityCG.cginc"
		#include "../BlendModes.cginc"

		#pragma target 3.0			
		#pragma multi_compile BmDarken BmMultiply BmColorBurn BmLinearBurn BmDarkerColor BmLighten BmScreen BmColorDodge BmLinearDodge BmLighterColor BmOverlay BmSoftLight BmHardLight BmVividLight BmLinearLight BmPinLight BmHardMix BmDifference BmExclusion BmSubtract BmDivide BmHue BmSaturation BmColor BmLuminosity
		#pragma surface ComputeSurface Lambert vertex:ComputeVertex

		fixed4 _Color;
		sampler2D _MainTex;
		sampler2D _GrabTexture;

		struct Input
		{
			float2 uv_MainTex;
			float4 GrabUV;
		};

		void ComputeVertex(inout appdata_full vertexInput, out Input vertexOutput)
		{
			UNITY_INITIALIZE_OUTPUT(Input, vertexOutput);
			float4 hpos = UnityObjectToClipPos(vertexInput.vertex);
			vertexOutput.GrabUV = ComputeGrabScreenPos(hpos);
		}

		void ComputeSurface(Input vertexOutput, inout SurfaceOutput surfaceOutput)
		{
			fixed4 texColor = tex2D(_MainTex, vertexOutput.uv_MainTex) * _Color;
			fixed4 grabColor = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(vertexOutput.GrabUV));

			#include "../BlendOps.cginc"

			surfaceOutput.Albedo = lerp(grabColor, blendResult.rgb, texColor.a);
		}

		ENDCG

	}

	FallBack "Diffuse"
	CustomEditor "BmMaterialEditor"
}
