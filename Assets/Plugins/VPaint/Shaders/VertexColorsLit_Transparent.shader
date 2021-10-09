Shader "VPaint/Lit/Transparent/Vertex Colors" {
	Properties {}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input {			
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.color;
			o.Alpha = IN.color.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
