// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'


shader "Noise/TessellationFixed4D" 
{
    Properties 
    {
        _Tess ("Tessellation", float) = 4
        _Displacement ("Displacement", float) = 3
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", color) = (1,1,1,0)
		_Frequency("Frequency", float) = 10.0
		_Lacunarity("Lacunarity", float) = 2.0
		_Gain("Gain", float) = 0.5
		_Jitter("Jitter", Range(0,1)) = 1.0
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Lambert addshadow fullforwardshadows vertex:disp tessellate:tessFixed nolightmap
        #pragma target 5.0
        #include "ImprovedVoronoiNoise4D.cginc"
        #define OCTAVES 1
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _DispTex;
        fixed4 _Color;
        float _Tess;
        float _Displacement;
        
        struct appdata 
        {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
        };
        
        struct Input 
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        float4 tessFixed()
        {
            return _Tess;
        }

        void disp(inout appdata v)
        {
            //float d = fBm_F0(float4(v.vertex.xyz, _Time.x), OCTAVES) * _Displacement;
            
            float d = fBm_F1_F0(float4(v.vertex.xyz, _Time.x), OCTAVES) * _Displacement;
            
            v.vertex.xyz += v.normal * d;
        }
        
        void surf(Input IN, inout SurfaceOutput o) 
        {
       		//Must use object pos not world pos for noise uv's
        	float3 objectPos = mul(unity_WorldToObject,float4(IN.worldPos,1)).xyz;
        	
        	//float noise = fBm_F0(float4(objectPos, _Time.x), OCTAVES);
        	
        	float noise = fBm_F1_F0(float4(objectPos, _Time.x), OCTAVES);
        	
            o.Albedo = noise * _Color.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}