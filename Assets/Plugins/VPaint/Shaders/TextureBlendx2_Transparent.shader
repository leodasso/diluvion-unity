Shader "VPaint/Lit/Transparent/Blend 2 Textures" {
	Properties {
		_Color ("Base Color", Color) = (1,1,1,1)
		_Texture1 ("Texture 1 (Red Channel)", 2D) = "white" {}
		_Texture2 ("Texture 2 (Blue Channel)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha

		half4 _Color;
		sampler2D _Texture1;
		sampler2D _Texture2;

		struct Input {
			half2 uv_Texture1;
			half2 uv_Texture2;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			float4 color = IN.color;
			
			half4 t1 = tex2D (_Texture1, IN.uv_Texture1);
			half4 t2 = tex2D (_Texture2, IN.uv_Texture2);
			
			half4 cum = t1 * color.r + t2 * color.g;
			fixed fac = color.r + color.g;
			
			if(fac != 0) cum /= fac;
			cum = lerp(_Color, cum, fac);
			
			o.Albedo = cum.rgb;
			o.Alpha = cum.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
