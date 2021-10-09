// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VPaint/Unlit/VertexColorsDiffuse"
{
	Properties {
		_MainTexture ("Main Texture", 2D) = "white"
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTexture;
			half2 _MainTexture_ST;
			
			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
				half2 texCoord : TEXCOORD;
			};
			
			struct appdata {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				half2 texCoord : TEXCOORD;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texCoord = v.texCoord;
				return o;
			}
			
			fixed4 frag (v2f o) : COLOR
			{
				return tex2D(_MainTexture, o.texCoord) * o.color;
			}
			ENDCG
		}
	} 
}
