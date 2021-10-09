Shader "VPaint/Lit/Blend 3 Textures" {
	Properties {
		_Color ("Base Color", Color) = (1,1,1,1)
		_Texture1 ("Texture 1", 2D) = "white" {}
		_Texture2 ("Texture 2", 2D) = "white" {}
		_Texture3 ("Texture 3", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		half4 _Color;
		sampler2D _Texture1;
		sampler2D _Texture2;
		sampler2D _Texture3;

		struct Input {
			half2 uv_Texture1;
			half2 uv_Texture2;
			half2 uv_Texture3;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			float4 color = IN.color;
			
			half4 t1 = tex2D (_Texture1, IN.uv_Texture1);
			half4 t2 = tex2D (_Texture2, IN.uv_Texture2);
			half4 t3 = tex2D (_Texture3, IN.uv_Texture3);
			
			half4 cum = t1 * color.r + t2 * color.g + t3 * color.b;
			fixed fac = color.r + color.g + color.b;
			
			if(fac != 0) cum /= fac;		
			cum = lerp(_Color, cum, fac);
			
			o.Albedo = cum.rgb;
			o.Alpha = cum.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}