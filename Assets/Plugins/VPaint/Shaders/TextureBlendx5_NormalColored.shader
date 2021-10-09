Shader "VPaint/Lit/Blendx5 Bump,Spec,T4Color" {
	Properties {
		_BaseTexture ("Base Texture (RGB: Diffuse) (A: Spec)", 2D) = "white" {}
		_BaseTextureBump ("Base Texture Bump", 2D) = "bump" {}
		
		_Texture1 ("Metal (RGB: Red Channel) (A: Spec)", 2D) = "white" {}
		_Texture1Bump ("Metal Bump", 2D) = "bump" {}
		
		_Texture2 ("Rust (RGB: Blue Channel) (A: Spec)", 2D) = "white" {}
		_Texture2Bump ("Rust Bump", 2D) = "bump" {}
		
		_Texture3 ("Dirt (RGB: Green Channel) (A: Spec)", 2D) = "white" {}
		_Texture3Bump ("Dirt Bump", 2D) = "bump" {}
		
		_Texture4 ("Paint (RGB: Alpha Channel) (A: Spec)", 2D) = "white" {}
		_Texture4Bump ("Paint Bump", 2D) = "bump" {}
		_Texture4Color ("Paint Color", Color) = (1,1,1,1)
		
		_Gloss ("Gloss", Range(0,1)) = 0.5
		_Spec ("Specularity", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 3.0

		sampler2D _BaseTexture;
		sampler2D _BaseTextureBump;
		
		sampler2D _Texture1;
		sampler2D _Texture1Bump;
		
		sampler2D _Texture2;
		sampler2D _Texture2Bump;

		sampler2D _Texture3;
		sampler2D _Texture3Bump;
		
		sampler2D _Texture4;
		sampler2D _Texture4Bump;
		
		half4 _Texture4Color;
		fixed _Gloss;
		fixed _Spec;

		struct Input {
			half2 uv_BaseTexture;
			half2 uv_Texture1;			
			half2 uv_Texture2;			
			half2 uv_Texture3;			
			half2 uv_Texture4;
			
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			float4 color = IN.color;
			
			half4 t0 = tex2D (_BaseTexture, IN.uv_BaseTexture);
			half4 t0b = tex2D (_BaseTextureBump, IN.uv_BaseTexture);
			
			half4 t1 = tex2D (_Texture1, IN.uv_Texture1);
			half4 t2 = tex2D (_Texture2, IN.uv_Texture2);
			half4 t3 = tex2D (_Texture3, IN.uv_Texture3);
			half4 t4 = tex2D (_Texture4, IN.uv_Texture4);
			t4 *= _Texture4Color;
			
			half4 t1b = tex2D (_Texture1Bump, IN.uv_Texture1);
			half4 t2b = tex2D (_Texture2Bump, IN.uv_Texture2);
			half4 t3b = tex2D (_Texture3Bump, IN.uv_Texture3);
			half4 t4b = tex2D (_Texture4Bump, IN.uv_Texture4);
			
			half4 cum = t1 * color.r + t2 * color.g + t3 * color.b + t4 * color.a;
			half4 nrm = t1b * color.r + t2b * color.g + t3b * color.b + t4b * color.a;
			fixed fac = color.r + color.g + color.b + color.a;
			
			if(fac != 0)
			{
				cum /= fac;
				nrm /= fac;
			}
			cum = lerp(t0, cum, fac);
			
			o.Albedo = cum.rgb;
			o.Specular = cum.a * _Spec;
			o.Normal = lerp(UnpackNormal(t0b), UnpackNormal(nrm), fac);
			o.Gloss = _Gloss;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
