// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VPaint/VertexColorLerpTangents" 
{
	Properties{
		_Interp ("Interolation", Range(0,1)) = 0.5
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			fixed _Interp;
			
			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
				fixed4 tangent;
			};
			
			struct appdata {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				fixed4 tangent: TANGENT;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.tangent = v.tangent;
				return o;
			}
			
			fixed4 frag (v2f o) : COLOR
			{
				return lerp(o.color, o.tangent, _Interp);
			}
			ENDCG
		}
	} 
}
