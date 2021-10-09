Shader "VPaint/Lit/Vertex Colors" {
	Properties {}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input {			
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.color;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
