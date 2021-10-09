Shader "VPaint/Lit/VertexColors*Diffuse" {
	Properties {
		_MainTexture ("Main Texture (RGB)", 2D) = "white"	
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTexture;

		struct Input {			
			half2 uv_MainTexture;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			
			o.Albedo = tex2D(_MainTexture, IN.uv_MainTexture) * IN.color;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}