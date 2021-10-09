 Shader "Noise/TessellationFixed" 
 {
    Properties 
    {
    	_Tess ("Tessellation", float) = 4
    	_Displacement ("Displacement", float) = 0.3
    	_NormalStr ("Normal Str", float) = 0.1
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _DispTex ("Disp Texture", 2D) = "gray" {}
        _DispTexSize("Disp Texture Size", Vector) = (512.0,512.0,0,0)
        _Color ("Color", color) = (1,1,1,0)
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Lambert addshadow fullforwardshadows vertex:disp tessellate:tessFixed nolightmap
        #pragma target 5.0

        struct appdata 
        {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
        };

        sampler2D _DispTex, _MainTex;
        float _Displacement, _Tess, _NormalStr;
        float2 _DispTexSize;
        fixed4 _Color;

        void disp (inout appdata v)
        {
            float d = tex2Dlod(_DispTex, float4(v.texcoord.xy,0,0)).r * _Displacement;
            v.vertex.xyz += v.normal * d;
        }
                
        float4 tessFixed()
        {
            return _Tess;
        }

        struct Input 
        {
            float2 uv_MainTex;
        };

        float3 FindNormal(float2 uv, float2 u)
        {
            float ht0 = tex2D(_DispTex, uv + float2(-u.x, 0));
            float ht1 = tex2D(_DispTex, uv + float2(u.x, 0));
            float ht2 = tex2D(_DispTex, uv + float2(0, -u.y));
            float ht3 = tex2D(_DispTex, uv + float2(0, u.y));

            float3 va = normalize(float3(float2(_NormalStr, 0.0), ht1-ht0));
            float3 vb = normalize(float3(float2(0.0,_NormalStr), ht3-ht2));

           return cross(va,vb);
        }

        void surf(Input IN, inout SurfaceOutput o) 
        {
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            o.Albedo = c.rgb * _Color;
            o.Normal = FindNormal(IN.uv_MainTex, 1.0/_DispTexSize);
        }
        ENDCG
    }
    FallBack "Diffuse"
}