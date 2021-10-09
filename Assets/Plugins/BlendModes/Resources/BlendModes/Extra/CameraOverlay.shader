// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BlendModes/Extra/CameraOverlay" 
{
	Properties 
	{
		_MainTex ("Screen Blended", 2D) = "" {}
		_OverlayTex ("Overlay Texture", 2D) = "grey" {}
		_OverlayTintColor ("Overlay Tint", Color) = (1,1,1,1)
	}
	
	SubShader 
	{
		ZTest Always 
		Cull Off 
		ZWrite Off
		ColorMask RGB 
		
		Pass 
		{  
			CGPROGRAM
			
			#include "UnityCG.cginc"
			#include "../BlendModes.cginc"

			#pragma target 3.0			
			#pragma multi_compile BmDarken BmMultiply BmColorBurn BmLinearBurn BmDarkerColor BmLighten BmScreen BmColorDodge BmLinearDodge BmLighterColor BmOverlay BmSoftLight BmHardLight BmVividLight BmLinearLight BmPinLight BmHardMix BmDifference BmExclusion BmSubtract BmDivide BmHue BmSaturation BmColor BmLuminosity
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			fixed4 _OverlayTintColor;
			sampler2D _OverlayTex;
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			half4 _UV_Transform = half4(1, 0, 0, 1);
			float _IsNormalBlendMode = 1;

			struct VertexOutput 
			{
				float4 ScreenPos : SV_POSITION;
				float2 ScreenUV[2] : TEXCOORD0;
			};
			
			VertexOutput ComputeVertex(appdata_img vertexInput)
			{
				VertexOutput vertexOutput;
				
				vertexOutput.ScreenPos = UnityObjectToClipPos(vertexInput.vertex);

				vertexOutput.ScreenUV[0] = float2(
					dot(vertexInput.texcoord.xy, _UV_Transform.xy),
					dot(vertexInput.texcoord.xy, _UV_Transform.zw)
				);
		
				#if UNITY_UV_STARTS_AT_TOP
				if(_MainTex_TexelSize.y < 0.0)
					vertexOutput.ScreenUV[0].y = 1.0 - vertexOutput.ScreenUV[0].y;
				#endif
		
				vertexOutput.ScreenUV[1] =  vertexInput.texcoord.xy;	
				return vertexOutput;
			}
			
			fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
			{
				fixed4 texColor = tex2D(_OverlayTex, vertexOutput.ScreenUV[0]) * _OverlayTintColor;
				fixed4 grabColor = tex2D(_MainTex, vertexOutput.ScreenUV[1]);

				#include "../BlendOps.cginc"

				blendResult = lerp(grabColor, blendResult, texColor.a);
				fixed4 normalBlendResult = lerp(grabColor, texColor, texColor.a);

				return lerp(blendResult, normalBlendResult, _IsNormalBlendMode); 
			}
			
			ENDCG
		}
	}

	Fallback off
}
