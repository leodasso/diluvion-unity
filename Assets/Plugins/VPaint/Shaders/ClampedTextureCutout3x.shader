Shader "VPaint/Clamped Blending/CutoutClamped" {
	Properties {
		_Texture1 ("Texture 1 (RGB), Alpha/Spec (A)", 2D) = "white" {}
		_Texture1_Normal ("Texture 1 Normal", 2D) = "bump" {}
		_Alpha1("AlphaSlider1", Range(0,1)) = 0.5
		_Spec1("SpecSlider1", Range(0.03,1)) = 0.5
		_Gloss1("Gloss1", Range(0.03,1)) =0.5
		
		_Texture2 ("Texture 2 (RGB), Alpha/Spec (A)", 2D) = "white" {}
		_Texture2_Normal ("Texture 2 Normal", 2D) = "bump" {}
		_Alpha2("AlphaSlider2", Range(0,1)) = 0.5
		_Spec2("SpecSlider2", Range(0.03,1)) =0.5
		_Gloss2("Gloss2", Range(0.03,1)) =0.5
		
		_Texture3 ("Texture 3 (RGB), Alpha/Spec (A)", 2D) = "white" {}
		_Texture3_Normal ("Texture 3 Normal", 2D) = "bump" {}
		_Alpha3("AlphaSlider3", Range(0,1)) = 0.5
		_Spec3("SpecSlider3",  Range(0.03,1)) =0.5
		_Gloss3("Gloss3", Range(0.03,1)) =0.5
		
		_AlphaTest("AlphaTest", Range(0,1)) = 0.5
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)		
		_Heightmaps ("T1 Height (R), T2 Height (G)", 2D) = "white" {}
	}
	//TODO CHECK THE SPEC
	SubShader {
		Tags { "Queue"="AlphaTest""RenderType"="TransparentCutout" }
//		ZWrite Off
//		ZTest LEqual
//		AlphaTest Greater [_AlphaTest]		
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong alphatest:_AlphaTest
		#pragma target 3.0

		sampler2D _Texture1;
		sampler2D _Texture1_Normal;
		fixed _Alpha1;
		fixed _Spec1;
		fixed _Gloss1;
										
		sampler2D _Texture2;
		sampler2D _Texture2_Normal;
		fixed _Alpha2;
		fixed _Spec2;
		fixed _Gloss2;
		
		sampler2D _Texture3;
		sampler2D _Texture3_Normal;
		fixed _Alpha3;
		fixed _Spec3;
		fixed _Gloss3;
		
		fixed _Shininess;
//		fixed _AlphaTest;
		sampler2D _Heightmaps;

		struct Input {
			//Cannot use modified texture coords because the heightmap shares the sample T1 & T2
			half2 uv_Texture1;			
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
		
			

			fixed4 vColor = IN.color;
		
			fixed4 tex1samp = tex2D (_Texture1, IN.uv_Texture1);
			float alpha1 = tex1samp.a;
//			clip(-(alpha1 + _Alpha1));
			fixed4 tex1nrm = tex2D (_Texture1_Normal, IN.uv_Texture1);
			
			fixed4 tex2samp = tex2D (_Texture2, IN.uv_Texture1);
			float alpha2 = tex2samp.a;
//			clip(-(alpha1 + _Alpha2));
			fixed4 tex2nrm = tex2D (_Texture2_Normal, IN.uv_Texture1);
			
			fixed4 tex3samp = tex2D (_Texture3, IN.uv_Texture1);
			float alpha3 = tex3samp.a;
//			clip(-(alpha1 + _Alpha3));
			fixed4 tex3nrm = tex2D (_Texture3_Normal, IN.uv_Texture1);
			
			fixed4 heightmaps = tex2D (_Heightmaps, IN.uv_Texture1);

			fixed interp1 = 
				pow(
					((1-heightmaps.r) * 0.5 + heightmaps.r * 2)
						* vColor.r,
					(1-vColor.a) * 20 + 1
				);
				
			fixed interp2 = 
				pow(
					((1-heightmaps.g) * 0.5 + heightmaps.g * 2) 
						* vColor.g,
					(1-vColor.a) * 20 + 1
				);
				
			fixed interp3 = saturate( interp1 - interp2 );
			fixed interp = saturate( lerp(interp2, interp1, interp3) );
			
			o.Albedo = 
				lerp( 
					tex1samp.rgb,
					lerp( tex3samp.rgb, tex2samp.rgb, interp3 ),
					interp 
				);
		
			o.Gloss = 
				lerp( 
					tex1samp.a*_Spec1, 
					lerp( tex3samp.a*_Spec3, tex2samp.a*_Spec2, interp3 ), 
					interp
				);
				
			o.Alpha = 	
				lerp( 
					alpha1+_Alpha1, 
					lerp(alpha3+_Alpha3, alpha2+_Alpha2, interp3), 
					interp
				);
				
			o.Specular = 
				lerp( 
					_Gloss1, 
					lerp(_Gloss3,_Gloss2, interp3), 
					interp
				);
				
			o.Normal = 
				UnpackNormal(
					lerp(
						tex1nrm, 
						lerp( tex3nrm, tex2nrm, interp3), 
						interp
					)
				);
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
