Shader "VPaint/Cutout/Lit/VertexColors*Diffuse" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTexture ("Main Texture (RGB)", 2D) = "white"
		_Cutoff ("Alpha cutoff", float) = 0.5
	}
	SubShader {
		Tags { 
			"RenderType"="TransparentCutout" 
			"Queue"="AlphaTest"
		}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alphatest:_Cutoff

		sampler2D _MainTexture;
		fixed4 _Color;
	
		struct Input {			
			half2 uv_MainTexture;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 col = tex2D(_MainTexture, IN.uv_MainTexture) * IN.color * _Color;
			o.Albedo = col;
			o.Alpha = col.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}