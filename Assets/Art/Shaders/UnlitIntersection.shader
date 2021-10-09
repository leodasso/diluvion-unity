// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Intersection Glow"
{
	Properties
	{
		_Color ("Color", Color) = (1,0,0,1)
		_IntersectionMax("Intersection Max", Range(0.01,300)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		//Blend One One // additive blending for a simple "glow" effect
		Cull Off // render backfaces as well
		ZWrite Off // don't write into the Z-buffer, this effect shouldn't block objects
		ZTest LEqual
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 3.0
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				
			};

			struct v2f
			{
				float4 screenPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
				half depth : DEPTH;
			};

            float _IntersectionMax;
			sampler2D _CameraDepthTexture; // automatically set up by Unity. Contains the scene's depth buffer
			fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.depth = COMPUTE_DEPTH_01;
				return o;
			}

			fixed4 frag (v2f i) : Color
			{
				//Get the distance to the camera from the depth buffer for this point
                float sceneZ = 1-Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                //Actual distance to the camera
                float fragZ = 1-i.depth;
                float factor = abs(fragZ-sceneZ);
                float4 c;
                
                if(factor< _IntersectionMax)
                {                
                    c.r = factor*2;
                    c.g = factor*2;
                    c.b = factor*2;
                    c.a = 1;
                } 
                else
                {
                    c.r = 0;
                    c.g = 0;
                    c.b = 0;
                    c.a = 0;
                }
               

				return _Color*c;
			}
			ENDCG
		}
	}
}