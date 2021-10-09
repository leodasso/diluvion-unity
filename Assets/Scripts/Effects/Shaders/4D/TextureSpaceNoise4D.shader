
Shader "Noise/TextureSpaceNoise4D" 
{
    Properties 
    {
		_Frequency("Frequency", float) = 10.0
		_Lacunarity("Lacunarity", float) = 2.0
		_Gain("Gain", float) = 0.5
		_Jitter("Jitter", Range(0,1)) = 1.0
    }
	SubShader 
	{
    	Pass 
    	{

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#include "ImprovedVoronoiNoise4D.cginc"
			
			#define OCTAVES 1
		
			struct v2f 
			{
    			float4  pos : SV_POSITION;
    			float3  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
    			v2f OUT;
    			OUT.uv = v.vertex.xyz;
    			//To render in texture space the uvs in the range -1 to 1 are used as the position
    			OUT.pos = float4(v.texcoord.x*2.0-1.0, v.texcoord.y*-2.0+1.0, 0.0, 1.0);
    			return OUT;
			}
			
			float4 frag(v2f IN) : COLOR
			{
			
				//float noise = fBm_F0(float4(IN.uv, _Time.x), OCTAVES);
			
				float noise = fBm_F1_F0(float4(IN.uv, _Time.x), OCTAVES);
			
				return float4(noise.xxx,1);
			}
			
			ENDCG

    	}
	}
}