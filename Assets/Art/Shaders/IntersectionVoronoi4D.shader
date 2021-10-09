// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/IntersectionVoronoi4D"
{
    Properties
    {
       
        _RegularColor("Main Color", Color) = (1, 1, 1, .0) //Color when not intersecting
        _HighlightColor("Highlight Color", Color) = (1, 1, 1, .5) //Color when intersecting
        _HighlightThresholdMax("Highlight Threshold Max", Range(0,5)) = 1 //Max difference for intersections
		_HighlightThickness("Highlight thickness", Range(0,1)) = 0.01 //Max difference for intersections
		_Frequency("Frequency", Range(0,100)) = 5.0
		_Lacunarity("Lacunarity", Range(0,20)) = 1.0
		_Gain("Gain", Range(0,30)) = 1
		_Jitter("Jitter", Range(0,10)) = 1.0
	
	
	}
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent"  }
		LOD 300

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front
            Fog{Mode Off}

 
            CGPROGRAM           
            #pragma vertex vert
			#pragma target 4.0
            #pragma fragment frag    
			#pragma glsl 
			#include "UnityCG.cginc"
			#include "Assets/Art/Shaders/CGINC/ImprovedVoronoiNoise3D.cginc"
            #define OCTAVES 3
 
            uniform sampler2D _CameraDepthTexture; //Depth Texture
            uniform float4 _RegularColor;
            uniform float4 _HighlightColor;
            uniform float _HighlightThresholdMax;
			uniform float _HighlightThickness;
			
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD1; //Screen position of pos
				float4 uv : TEXCOORD;
            };
 
            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.projPos = ComputeScreenPos(o.pos);
                o.projPos.z = COMPUTE_DEPTH_01;
				//Where to draw the Noise
				o.uv  = float4( v.vertex.xyz, _Time.x); //use model space, not world space for noise uvs
 
                return o;
            }	
 
            half4 frag(v2f i) : COLOR
            {
                float4 finalColor = _RegularColor;
 
                //Get the distance to the camera from the depth buffer for this point
                float sceneZ = Linear01Depth (tex2Dproj(_CameraDepthTexture,
                                                         UNITY_PROJ_COORD(i.projPos)).r);
 
                //Actual distance to the camera
                float partZ = i.projPos.z;
 
                //If the two are similar, then there is an object intersecting with our object
                float diff = abs(sceneZ - partZ)/(_HighlightThickness*_HighlightThickness);
                  
                  //Figure out _Time propagation here
				float3 objectPos =i.uv*sceneZ;

				//float n = fBm_F0(i.uv, OCTAVES);
				float n = fBm_F1_F0(objectPos, OCTAVES);

                if(diff <= _HighlightThresholdMax&&partZ<0.95f)
                {
                    finalColor = lerp(_RegularColor,_HighlightColor,
                                      float4(diff*n, diff*n, diff*n, diff*n));
                }
 
                half4 c;
                c.r = finalColor.r*n;
                c.g = finalColor.g*n;
                c.b = finalColor.b*n;
                c.a = finalColor.a*n;
 
                return c;
            }
 
            ENDCG
        }
    }
    FallBack "VertexLit"
}