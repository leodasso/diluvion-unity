Shader "VPaint/Lit/Blend 3 Bumped Textures" {
	Properties {
		_Color ("Base Color", Color) = (1,1,1,1)
		
		_Texture1 ("Texture 1 (RGB: Red Channel) (A: Spec)", 2D) = "white" {}
		_Texture1Bump ("Texture 1 Bump", 2D) = "bump" {}
		
		_Texture2 ("Texture 2 (RGB: Blue Channel) (A: Spec)", 2D) = "white" {}
		_Texture2Bump ("Texture 2 Bump", 2D) = "bump" {}
		
		_Texture3 ("Texture 3 (RGB: Green Channel) (A: Spec)", 2D) = "white" {}
		_Texture3Bump ("Texture 3 Bump", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 3.0

		half4 _Color;
		sampler2D _Texture1;
		sampler2D _Texture1Bump;
		
		sampler2D _Texture2;
		sampler2D _Texture2Bump;

		sampler2D _Texture3;
		sampler2D _Texture3Bump;

		struct Input {
			half2 uv_Texture1;
			half2 uv_Texture1Bump;
			
			half2 uv_Texture2;
			half2 uv_Texture2Bump;
			
			half2 uv_Texture3;
			half2 uv_Texture3Bump;
			
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			float4 color = IN.color;
			
			half4 t1 = tex2D (_Texture1, IN.uv_Texture1);
			half4 t2 = tex2D (_Texture2, IN.uv_Texture2);
			half4 t3 = tex2D (_Texture3, IN.uv_Texture3);
			
			half4 t1b = tex2D (_Texture1Bump, IN.uv_Texture1Bump);
			half4 t2b = tex2D (_Texture2Bump, IN.uv_Texture2Bump);
			half4 t3b = tex2D (_Texture3Bump, IN.uv_Texture3Bump);
			
			half4 cum = t1 * color.r + t2 * color.g + t3 * color.b;
			half4 nrm = t1b * color.r + t2b * color.g + t3b * color.b;
			fixed fac = color.r + color.g + color.b;
			
			if(fac != 0)
			{
				cum /= fac;
			}
			cum = lerp(_Color, cum, fac);
			nrm = lerp(half4(0,0,0,0), nrm, fac);
			
			o.Albedo = cum.rgb;
			o.Specular = cum.a;
			o.Normal = UnpackNormal(normalize(nrm));
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
