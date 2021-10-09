// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.24 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.24;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:True,rprd:True,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3643,x:32719,y:32712,varname:node_3643,prsc:2|diff-2418-OUT,spec-8757-OUT,gloss-2590-OUT,normal-6784-OUT,alpha-648-OUT,refract-7407-OUT;n:type:ShaderForge.SFN_Slider,id:3421,x:30747,y:33616,ptovrint:False,ptlb:Transparency,ptin:_Transparency,varname:node_3421,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:7407,x:32216,y:33892,varname:node_7407,prsc:2|A-6551-OUT,B-5293-OUT;n:type:ShaderForge.SFN_Tex2d,id:2479,x:31515,y:32296,ptovrint:False,ptlb:Normal Map,ptin:_NormalMap,varname:_Refraction,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True|UVIN-9416-UVOUT;n:type:ShaderForge.SFN_Lerp,id:6784,x:31910,y:32248,varname:node_6784,prsc:2|A-4537-OUT,B-2479-RGB,T-7768-OUT;n:type:ShaderForge.SFN_Vector3,id:4537,x:31515,y:32186,varname:node_4537,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_TexCoord,id:9416,x:31255,y:32296,varname:node_9416,prsc:2,uv:0;n:type:ShaderForge.SFN_ComponentMask,id:6551,x:32040,y:33747,varname:node_6551,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-2479-RGB;n:type:ShaderForge.SFN_Slider,id:5293,x:31725,y:33916,ptovrint:False,ptlb:Refraction,ptin:_Refraction,varname:_NormalIntensity_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.2,max:1;n:type:ShaderForge.SFN_Lerp,id:648,x:31321,y:33543,varname:node_648,prsc:2|A-4313-OUT,B-5984-OUT,T-3421-OUT;n:type:ShaderForge.SFN_Vector1,id:5984,x:30826,y:33454,varname:node_5984,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:4313,x:30826,y:33397,varname:node_4313,prsc:2,v1:1;n:type:ShaderForge.SFN_Slider,id:7768,x:31358,y:32493,ptovrint:False,ptlb:Normal Intensity,ptin:_NormalIntensity,varname:_Refraction_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Tex2d,id:5796,x:32050,y:31308,ptovrint:False,ptlb:Diffuse Map (Spec A),ptin:_DiffuseMapSpecA,varname:node_5796,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:8761,x:32050,y:31143,ptovrint:False,ptlb:Diffuse Color,ptin:_DiffuseColor,varname:node_8761,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:3291,x:32486,y:31286,varname:node_3291,prsc:2|A-8761-RGB,B-5796-RGB;n:type:ShaderForge.SFN_Slider,id:577,x:30589,y:32884,ptovrint:False,ptlb:Specular Intensity,ptin:_SpecularIntensity,varname:node_577,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:1;n:type:ShaderForge.SFN_Lerp,id:4209,x:31005,y:32844,varname:node_4209,prsc:2|A-1808-OUT,B-8066-OUT,T-577-OUT;n:type:ShaderForge.SFN_Vector1,id:1808,x:30783,y:32608,varname:node_1808,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:8066,x:30783,y:32683,varname:node_8066,prsc:2,v1:2;n:type:ShaderForge.SFN_Multiply,id:7596,x:31263,y:32779,varname:node_7596,prsc:2|A-1398-RGB,B-4209-OUT;n:type:ShaderForge.SFN_Color,id:1398,x:31057,y:32643,ptovrint:False,ptlb:Specular Color,ptin:_SpecularColor,varname:node_1398,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Slider,id:2590,x:30589,y:33005,ptovrint:False,ptlb:Glossiness,ptin:_Glossiness,varname:_SpecularIntensity_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:1;n:type:ShaderForge.SFN_Multiply,id:8757,x:32338,y:32848,varname:node_8757,prsc:2|A-5796-A,B-7596-OUT;n:type:ShaderForge.SFN_Slider,id:2152,x:30963,y:31936,ptovrint:False,ptlb:Reflection Edges,ptin:_ReflectionEdges,varname:node_9228,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:1;n:type:ShaderForge.SFN_Vector1,id:803,x:31120,y:31736,varname:node_803,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:7746,x:31120,y:31632,varname:node_7746,prsc:2,v1:10;n:type:ShaderForge.SFN_Lerp,id:49,x:31352,y:31764,varname:node_49,prsc:2|A-7746-OUT,B-803-OUT,T-2152-OUT;n:type:ShaderForge.SFN_Fresnel,id:3559,x:31492,y:31623,varname:node_3559,prsc:2|EXP-49-OUT;n:type:ShaderForge.SFN_RemapRange,id:2540,x:31710,y:31764,varname:node_2540,prsc:2,frmn:0,frmx:1,tomn:0,tomx:1|IN-3559-OUT;n:type:ShaderForge.SFN_Multiply,id:1017,x:31856,y:31598,varname:node_1017,prsc:2|A-3559-OUT,B-2540-OUT,C-3559-OUT,D-3559-OUT;n:type:ShaderForge.SFN_Add,id:1577,x:32246,y:31619,varname:node_1577,prsc:2|A-1457-OUT,B-1017-OUT;n:type:ShaderForge.SFN_Multiply,id:1457,x:31482,y:31216,varname:node_1457,prsc:2|A-6677-RGB,B-8157-OUT;n:type:ShaderForge.SFN_Lerp,id:8157,x:31102,y:31222,varname:node_8157,prsc:2|A-64-OUT,B-4805-OUT,T-2565-OUT;n:type:ShaderForge.SFN_Slider,id:2565,x:30719,y:31385,ptovrint:False,ptlb:Reflection Intensity,ptin:_ReflectionIntensity,varname:_ReflectionGlow_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3,max:1;n:type:ShaderForge.SFN_Vector1,id:4805,x:30826,y:31250,varname:node_4805,prsc:2,v1:2;n:type:ShaderForge.SFN_Vector1,id:64,x:30826,y:31180,varname:node_64,prsc:2,v1:0;n:type:ShaderForge.SFN_Cubemap,id:6677,x:30878,y:30929,ptovrint:False,ptlb:Cube map ,ptin:_Cubemap,varname:node_3326,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,pvfc:1|MIP-2669-OUT;n:type:ShaderForge.SFN_ConstantLerp,id:2669,x:30636,y:30929,varname:node_2669,prsc:2,a:0,b:6|IN-3835-OUT;n:type:ShaderForge.SFN_Slider,id:3835,x:30248,y:30988,ptovrint:False,ptlb:Blur Reflection,ptin:_BlurReflection,varname:node_5181,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:2418,x:32773,y:32017,varname:node_2418,prsc:2|A-3291-OUT,B-1577-OUT;proporder:8761-5796-1398-577-2590-2479-7768-5293-3421-6677-2152-2565-3835;pass:END;sub:END;*/

Shader "Ciconia Studio/Double Sided/Effects/Glass/Old/Advanced Reflection(Depth Buffer Off)" {
    Properties {
        _DiffuseColor ("Diffuse Color", Color) = (1,1,1,1)
        _DiffuseMapSpecA ("Diffuse Map (Spec A)", 2D) = "white" {}
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _SpecularIntensity ("Specular Intensity", Range(0, 1)) = 0.5
        _Glossiness ("Glossiness", Range(0, 1)) = 0.5
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalIntensity ("Normal Intensity", Range(0, 1)) = 1
        _Refraction ("Refraction", Range(0, 1)) = 0.2
        _Transparency ("Transparency", Range(0, 1)) = 1
        _Cubemap ("Cube map ", Cube) = "_Skybox" {}
        _ReflectionEdges ("Reflection Edges", Range(0, 1)) = 0.5
        _ReflectionIntensity ("Reflection Intensity", Range(0, 1)) = 0.3
        _BlurReflection ("Blur Reflection", Range(0, 1)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 3.0
            #pragma glsl
            uniform sampler2D _GrabTexture;
            uniform float _Transparency;
            uniform sampler2D _NormalMap; uniform float4 _NormalMap_ST;
            uniform float _Refraction;
            uniform float _NormalIntensity;
            uniform sampler2D _DiffuseMapSpecA; uniform float4 _DiffuseMapSpecA_ST;
            uniform float4 _DiffuseColor;
            uniform float _SpecularIntensity;
            uniform float4 _SpecularColor;
            uniform float _Glossiness;
            uniform float _ReflectionEdges;
            uniform float _ReflectionIntensity;
            uniform samplerCUBE _Cubemap;
            uniform float _BlurReflection;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 screenPos : TEXCOORD7;
                UNITY_FOG_COORDS(8)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD9;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.screenPos = o.pos;
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3 _NormalMap_var = UnpackNormal(tex2D(_NormalMap,TRANSFORM_TEX(i.uv0, _NormalMap)));
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + (_NormalMap_var.rgb.rg*_Refraction);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalLocal = lerp(float3(0,0,1),_NormalMap_var.rgb,_NormalIntensity);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _Glossiness;
                float specPow = exp2( gloss * 10.0+1.0);
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                d.boxMax[0] = unity_SpecCube0_BoxMax;
                d.boxMin[0] = unity_SpecCube0_BoxMin;
                d.probePosition[0] = unity_SpecCube0_ProbePosition;
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.boxMax[1] = unity_SpecCube1_BoxMax;
                d.boxMin[1] = unity_SpecCube1_BoxMin;
                d.probePosition[1] = unity_SpecCube1_ProbePosition;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _DiffuseMapSpecA_var = tex2D(_DiffuseMapSpecA,TRANSFORM_TEX(i.uv0, _DiffuseMapSpecA));
                float3 specularColor = (_DiffuseMapSpecA_var.a*(_SpecularColor.rgb*lerp(0.0,2.0,_SpecularIntensity)));
                float3 directSpecular = 1 * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 indirectSpecular = (gi.indirect.specular)*specularColor;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float4 node_3559 = pow(1.0-max(0,dot(normalDirection, viewDirection)),lerp(10.0,0.0,_ReflectionEdges));
                float3 diffuseColor = ((_DiffuseColor.rgb*_DiffuseMapSpecA_var.rgb)*((texCUBElod(_Cubemap,float4(viewReflectDirection,lerp(0,6,_BlurReflection))).rgb*lerp(0.0,2.0,_ReflectionIntensity))+(node_3559*(node_3559*1.0+0.0)*node_3559*node_3559)));
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,lerp(1.0,0.0,_Transparency)),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 3.0
            #pragma glsl
            uniform sampler2D _GrabTexture;
            uniform float _Transparency;
            uniform sampler2D _NormalMap; uniform float4 _NormalMap_ST;
            uniform float _Refraction;
            uniform float _NormalIntensity;
            uniform sampler2D _DiffuseMapSpecA; uniform float4 _DiffuseMapSpecA_ST;
            uniform float4 _DiffuseColor;
            uniform float _SpecularIntensity;
            uniform float4 _SpecularColor;
            uniform float _Glossiness;
            uniform float _ReflectionEdges;
            uniform float _ReflectionIntensity;
            uniform samplerCUBE _Cubemap;
            uniform float _BlurReflection;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 screenPos : TEXCOORD7;
                LIGHTING_COORDS(8,9)
                UNITY_FOG_COORDS(10)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.screenPos = o.pos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3 _NormalMap_var = UnpackNormal(tex2D(_NormalMap,TRANSFORM_TEX(i.uv0, _NormalMap)));
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + (_NormalMap_var.rgb.rg*_Refraction);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalLocal = lerp(float3(0,0,1),_NormalMap_var.rgb,_NormalIntensity);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _Glossiness;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _DiffuseMapSpecA_var = tex2D(_DiffuseMapSpecA,TRANSFORM_TEX(i.uv0, _DiffuseMapSpecA));
                float3 specularColor = (_DiffuseMapSpecA_var.a*(_SpecularColor.rgb*lerp(0.0,2.0,_SpecularIntensity)));
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 node_3559 = pow(1.0-max(0,dot(normalDirection, viewDirection)),lerp(10.0,0.0,_ReflectionEdges));
                float3 diffuseColor = ((_DiffuseColor.rgb*_DiffuseMapSpecA_var.rgb)*((texCUBElod(_Cubemap,float4(viewReflectDirection,lerp(0,6,_BlurReflection))).rgb*lerp(0.0,2.0,_ReflectionIntensity))+(node_3559*(node_3559*1.0+0.0)*node_3559*node_3559)));
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * lerp(1.0,0.0,_Transparency),0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 3.0
            #pragma glsl
            uniform sampler2D _DiffuseMapSpecA; uniform float4 _DiffuseMapSpecA_ST;
            uniform float4 _DiffuseColor;
            uniform float _SpecularIntensity;
            uniform float4 _SpecularColor;
            uniform float _Glossiness;
            uniform float _ReflectionEdges;
            uniform float _ReflectionIntensity;
            uniform samplerCUBE _Cubemap;
            uniform float _BlurReflection;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : SV_Target {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
                float4 _DiffuseMapSpecA_var = tex2D(_DiffuseMapSpecA,TRANSFORM_TEX(i.uv0, _DiffuseMapSpecA));
                float4 node_3559 = pow(1.0-max(0,dot(normalDirection, viewDirection)),lerp(10.0,0.0,_ReflectionEdges));
                float3 diffColor = ((_DiffuseColor.rgb*_DiffuseMapSpecA_var.rgb)*((texCUBElod(_Cubemap,float4(viewReflectDirection,lerp(0,6,_BlurReflection))).rgb*lerp(0.0,2.0,_ReflectionIntensity))+(node_3559*(node_3559*1.0+0.0)*node_3559*node_3559)));
                float3 specColor = (_DiffuseMapSpecA_var.a*(_SpecularColor.rgb*lerp(0.0,2.0,_SpecularIntensity)));
                float roughness = 1.0 - _Glossiness;
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
