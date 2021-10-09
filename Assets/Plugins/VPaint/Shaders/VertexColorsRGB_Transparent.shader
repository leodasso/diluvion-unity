// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VPaint/Unlit/VertexColorsRGB_Transparent"
{
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};
			
			struct appdata {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f o) : COLOR
			{
				return o.color;
			}
			ENDCG
		}
	} 
}
