// This shader works with URP 7.1.x and above
Shader "Iridescent"
{
    Properties
    {
        _NoiseTex ("Nosie Texture", 3D) = "white" {}
        _AnisoTex ("Anisotropy Mask", 2D) = "white" {}

        IridescentDistortionScale ("Iridescent Distortion Scale", Float) = 0.05
        IridescentEffectStrength ("Iridescent Effect Strength", Range(0.0, 1.0)) = 1.0
        IridescentFresnelStrength ("Iridescent Fresnel Strength", Range(0.0, 1.0)) = 0.25
        IridescentRange ("Iridescent Range", Range(0.0, 2.0)) = 1.0
        IridescentTint ("Iridescent Tint", Color) = (0.0, 0.178, 1.0)

        AnisoRoughness ("Anisotropy Roughness", Range(0.0, 1.0)) = 0.25
        AnisoEffectStrength ("Anisotropy Effect Strength", Range(0.0, 1.0)) = 0.0

        Metalness ("Metalness", Range(0.0, 1.0)) = 1.0
        Roughness ("Roughness", Range(0.0, 1.0)) = 0.0

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        // Blending state
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector]_ReceiveShadows("Receive Shadows", Float) = 1.0
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalRenderPipeline"
            "IgnoreProjector" = "True"
        }

        LOD 300

        Pass
        {
            Name "StandardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _OCCLUSIONMAP
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

            sampler3D _NoiseTex;
            sampler2D _AnisoTex;
            float Roughness;
            float Metalness;
            float IridescentDistortionScale;
            float IridescentEffectStrength;
            float IridescentFresnelStrength;
            float IridescentRange;
            float3 IridescentTint;
            float AnisoRoughness;
            float AnisoEffectStrength;

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
                float2 uvLM         : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float2 uvLM                     : TEXCOORD1;
                float4 positionWSAndFogFactor   : TEXCOORD2; // xyz: positionWS, w: vertex fog factor
                half3  normalWS                 : TEXCOORD3;

                #if _NORMALMAP
                half3 tangentWS                 : TEXCOORD4;
                half3 bitangentWS               : TEXCOORD5;
                #endif

                #ifdef _MAIN_LIGHT_SHADOWS
                float4 shadowCoord              : TEXCOORD6; // compute shadow coord per-vertex for the main light
                #endif
                float4 positionCS               : SV_POSITION;
            };

            struct CoreData
            {
                float3 PixelNormalWS;
                float3 ViewDirWS;
                float3 PositionWS;
                float3 ObjectPositionWS;
            };

            struct AnisotropySettings
            {
                float AnisoMask;
                float AnisoStrength;
                float2 UVs;
                float2 AnisoScale;
                //float EffectMultiplier;
                //float2 UVs;

                float3 AnisoTangentNormals()
                {
                    float dither_pattern = AnisoMask;
                    float2 xy = lerp(-1.0f, 1.0f, dither_pattern) * (normalize(UVs) * 0.7f);
                    float3 normals = float3(xy, 0.0f);
                    return normals + (float3(0.0f, 0.0f, 1.0f) * AnisoStrength);
                }

                float2 GetUVs()
                {
                    return UVs * AnisoScale;
                }
            };

            struct IridescentSettings
            {
                float FresnelStrength;
                float3 Tint;
                float Range;
                float DistortionStrength;
                float EffectStrength;
                CoreData CD;

                float3 Lerp3(float3 a, float3 b, float3 c, float x)
                {
                    float3 ab = lerp(a, b, saturate(2.0f * x));
                    float3 ab_c = lerp(ab, c, saturate((2.0f * x) - 1.0f));
                    return ab_c;
                }

                float3 ReflectionVector(float3 CustomWorldNormal)
                {
                    float3 target_normal = CustomWorldNormal;
                    return -CD.ViewDirWS + target_normal * dot(target_normal, CD.ViewDirWS) * 2.0f;
                }

                float Fresnel(float ExponentIn, float BaseRefractionIn, float3 NormalIn)
                {
                    float fresnel = 1.0f - pow(1.0f - dot(NormalIn, CD.ViewDirWS), ExponentIn);
                    return saturate(fresnel);
                }

                float3 HueShift(float3 Color, float Shift)
                {
                    // See: https://www.shadertoy.com/view/MsjXRt
                    float3 P = float3(0.55735.xxx) * dot(float3(0.55735.xxx), Color);
                    float3 U = Color - P;
                    float3 V = cross(float3(0.55735.xxx), U);
                    Color = U * cos(Shift * 6.2832) + V * sin(Shift * 6.2832) + P;
                    return Color;
                }

                float4 Calculate()
                {
                    float3 normals = CD.PixelNormalWS;
                    float fresnel_mask = Fresnel((1.0f / FresnelStrength), 0.0f, normals);
                    fresnel_mask = pow(fresnel_mask, 1.0f);

                    float3 reflection_vector = ReflectionVector(normalize(CD.PositionWS - CD.ObjectPositionWS)) * DistortionStrength;
                    float4 noise_sample = tex3D(_NoiseTex, reflection_vector);
                    float noise_mask = noise_sample.r +  (1.0f - fresnel_mask);

                    float percent_a = (Range * 0.33f) * -1.0f;
                    float percent_b = (Range * 0.33f);
                    float3 hue_a = HueShift(Tint, percent_a);
                    float3 hue_b = HueShift(Tint, percent_b);

                    float3 final_tint = Lerp3(hue_a, Tint, hue_b, noise_mask);

                    return float4(final_tint, 1.0f);
                }

            };

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                output.uv = input.uv;
                output.uvLM = input.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                output.positionWSAndFogFactor = float4(vertexInput.positionWS, fogFactor);
                output.normalWS = vertexNormalInput.normalWS;
                #ifdef _NORMALMAP
                    output.tangentWS = vertexNormalInput.tangentWS;
                    output.bitangentWS = vertexNormalInput.bitangentWS;
                #endif
                #ifdef _MAIN_LIGHT_SHADOWS
                    output.shadowCoord = GetShadowCoord(vertexInput);
                #endif
                    output.positionCS = vertexInput.positionCS;
                return output;
            }


            half4 LitPassFragment(Varyings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(input.uv, surfaceData);

                surfaceData.smoothness = 1.0 - Roughness;
                surfaceData.metallic = Metalness;

                // Aniso Calculation
                AnisotropySettings as = (AnisotropySettings)0;
                as.AnisoScale = float2(3.0f, 20.0f);
                as.AnisoStrength = 1.0f;
                as.UVs = input.uv;
                as.AnisoMask = tex2D(_AnisoTex, as.GetUVs()) * AnisoEffectStrength;

                float3 aniso_normals = as.AnisoTangentNormals();
                surfaceData.normalTS = aniso_normals;
                surfaceData.smoothness = lerp(surfaceData.smoothness, 1.0f - AnisoRoughness, as.AnisoMask);

                // end

                #if _NORMALMAP
                    half3 normalWS = TransformTangentToWorld(surfaceData.normalTS,
                    half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                #else
                    half3 normalWS = input.normalWS;
                #endif
                normalWS = normalize(normalWS);

                #ifdef LIGHTMAP_ON
                    half3 bakedGI = SampleLightmap(input.uvLM, normalWS);
                #else
                    half3 bakedGI = SampleSH(normalWS);
                #endif

                float3 positionWS = input.positionWSAndFogFactor.xyz;
                half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - positionWS);

                // Iridescence Calculation

                CoreData cd = (CoreData)0;
                cd.PixelNormalWS = normalWS;
                cd.ViewDirWS = viewDirectionWS;
                cd.PositionWS = positionWS;
                cd.ObjectPositionWS = mul(unity_ObjectToWorld , float4(0,0,0,1)).xyz;

                IridescentSettings is = (IridescentSettings)0;
                is.FresnelStrength = IridescentFresnelStrength;
                is.Tint = IridescentTint;
                is.Range = IridescentRange;
                is.DistortionStrength = IridescentDistortionScale;
                is.EffectStrength = IridescentEffectStrength;
                is.CD = cd;
                surfaceData.albedo.xyz = is.Calculate().xyz;

                // end

                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

                #ifdef _MAIN_LIGHT_SHADOWS
                    Light mainLight = GetMainLight(input.shadowCoord);
                #else
                    Light mainLight = GetMainLight();
                #endif
                half3 color = GlobalIllumination(brdfData, bakedGI, surfaceData.occlusion, normalWS, viewDirectionWS);
                color += LightingPhysicallyBased(brdfData, mainLight, normalWS, viewDirectionWS);

                #ifdef _ADDITIONAL_LIGHTS
                    int additionalLightsCount = GetAdditionalLightsCount();
                    for (int i = 0; i < additionalLightsCount; ++i)
                    {
                        Light light = GetAdditionalLight(i, positionWS);

                        color += LightingPhysicallyBased(brdfData, light, normalWS, viewDirectionWS);
                    }
                #endif

                color += surfaceData.emission;

                float fogFactor = input.positionWSAndFogFactor.w;

                color = MixFog(color, fogFactor);
                return half4(color, surfaceData.alpha);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }
}