Shader "VPaint/Lit/Transparent/Blend 4 Textures" {
	Properties {
		_Color ("Base Color", Color) = (1,1,1,1)
		_Texture1 ("Texture 1 (Red Channel)", 2D) = "white" {}
		_Texture2 ("Texture 2 (Blue Channel)", 2D) = "white" {}
		_Texture3 ("Texture 3 (Green Channel)", 2D) = "white" {}
		_Texture4 ("Texture 4 (Alpha Channel)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha

		half4 _Color;
		sampler2D _Texture1;
		sampler2D _Texture2;
		sampler2D _Texture3;
		sampler2D _Texture4;

		struct Input {
			half2 uv_Texture1;
			half2 uv_Texture2;
			half2 uv_Texture3;
			half2 uv_Texture4;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			float4 color = IN.color;
			
			half4 t1 = tex2D (_Texture1, IN.uv_Texture1);
			half4 t2 = tex2D (_Texture2, IN.uv_Texture2);
			half4 t3 = tex2D (_Texture3, IN.uv_Texture3);
			half4 t4 = tex2D (_Texture4, IN.uv_Texture4);
			
			half4 cum = t1 * color.r + t2 * color.g + t3 * color.b + t4 * color.a;
			fixed fac = color.r + color.g + color.b + color.a;
			
			if(fac != 0) cum /= fac;
			cum = lerp(_Color, cum, fac);
			
			o.Albedo = cum.rgb;
			o.Alpha = cum.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}