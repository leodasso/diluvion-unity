Shader "VPaint/Clamped Blending/Clamped2xBlend_Normal" {
	Properties {
		_Texture1 ("1: Diffuse (RGB), Height (A)", 2D) = "white" {}
		_Texture1_Spec ("1: Specular (RGB)", 2D) = "white" {}
		_Texture1_Normal ("1: Normal", 2D) = "bump" {}
		
		_Texture2 ("2: Diffuse (RGB), Height (A)", 2D) = "white" {}
		_Texture2_Spec ("2: Specular (RGB)", 2D) = "white" {}
		_Texture2_Normal ("2 Normal", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _Texture1;
		sampler2D _Texture1_Spec;
		sampler2D _Texture1_Normal;
		
		sampler2D _Texture2;
		sampler2D _Texture2_Spec;
		sampler2D _Texture2_Normal;

		struct Input {
			half2 uv_Texture1;
			half2 uv_Texture1_Spec;
			half2 uv_Texture1_Normal;
			
			half2 uv_Texture2;
			half2 uv_Texture2_Spec;
			half2 uv_Texture2_Normal;
			
			fixed4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 vColor = IN.color;
		
			fixed4 tex1samp = tex2D (_Texture1, IN.uv_Texture2);
			fixed4 tex2samp = tex2D (_Texture2, IN.uv_Texture2);

			fixed interp =
				pow(
					saturate(
						((1-tex2samp.a) * 0.5 + tex2samp.a * 2) * vColor.r
					),
					(1-vColor.g) * 20 + 1
				);
			
			fixed tex1spec = tex2D (_Texture1_Spec, IN.uv_Texture1_Spec).x;
			fixed tex2spec = tex2D (_Texture2_Spec, IN.uv_Texture2_Spec).x;
			
			fixed4 tex1nrm = tex2D (_Texture1_Normal, IN.uv_Texture1_Normal);
			fixed4 tex2nrm = tex2D (_Texture2_Normal, IN.uv_Texture2_Normal);
			
			o.Albedo = lerp(tex1samp, tex2samp, interp).xyz; 
			o.Specular = lerp(tex1spec, tex2spec, interp);
			o.Normal = UnpackNormal( lerp( tex1nrm, tex2nrm, interp ) ); 
			o.Gloss = 1;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
