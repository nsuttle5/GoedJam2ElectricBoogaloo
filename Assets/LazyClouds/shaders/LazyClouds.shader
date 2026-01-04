Shader "LemonSpawn/LazyClouds_URP"
{
    Properties
    {
        _MainTex ("Noise (R)", 2D) = "white" {}

        ls_time ("LS Time (adds to _Time.y)", Float) = 0
        ls_cloudscale ("Cloud Scale", Float) = 1
        ls_cloudscattering ("Cloud Scattering", Float) = 1
        ls_cloudintensity ("Cloud Intensity", Float) = 1
        ls_cloudsharpness ("Cloud Sharpness", Float) = 2
        ls_shadowscale ("Shadow Scale", Float) = 1
        ls_distScale ("Distance Scale", Float) = 1
        ls_cloudthickness ("Cloud Thickness", Float) = 1
        ls_cloudcolor ("Cloud Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+100"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        LOD 400
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            // URP keywords for main light/shadows (optional but nice)
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float ls_time;
            float ls_cloudscale;
            float ls_cloudscattering;
            float ls_cloudintensity;
            float ls_cloudsharpness;
            float ls_shadowscale;
            float ls_distScale;
            float ls_cloudthickness;
            float4 ls_cloudcolor;

            float GetTime()
            {
                // lets you drive ls_time from script but still animate if you don't
                return _Time.y + ls_time;
            }

            float SampleNoise(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;
            }

            float getCloud(float2 uv, float scale, float disp)
            {
                float y = 0.0;

                const int NN = 5;
                float t = GetTime();

                // Perlin-ish octaves (your original offsets preserved)
                [unroll]
                for (int i = 0; i < NN; i++)
                {
                    float fi = (float)i;
                    float k = scale * fi + 0.11934;

                    float2 ofs = float2(
                        0.1234 * fi * t * 0.015 - 0.04234 * fi * fi * t * 0.015 + 0.9123559 + 0.23411 * fi,
                        0.31342 + 0.5923 * fi + disp
                    );

                    y += SampleNoise(k * uv + ofs);
                }

                // Normalize (kept close to original behavior)
                y /= (0.5 * NN);

                // Guard against divide-by-zero
                y = max(y, 1e-4);

                return saturate(pow(ls_cloudscattering / y, ls_cloudsharpness));
            }

            // returns cloud value, outputs normal to n
            float getNormal(float2 uv, float scale, float dst, out float3 n, float nscale, float disp)
            {
                n = float3(0, 0, 0);

                float height = getCloud(uv, scale, disp);

                const int N = 5;

                [unroll]
                for (int i = 0; i < N; i++)
                {
                    float a0 = (float)i * 2.0 * PI / (float)N;
                    float a1 = (float)(i + 1) * 2.0 * PI / (float)N;

                    float2 du1 = float2(dst * cos(a0), dst * sin(a0));
                    float2 du2 = float2(dst * cos(a1), dst * sin(a1));

                    float hx = getCloud(uv + du1, scale, disp);
                    float hy = getCloud(uv + du2, scale, disp);

                    float3 d2 = float3(0, height * nscale, 0) - float3(du1.x, hx * nscale, du1.y);
                    float3 d1 = float3(0, height * nscale, 0) - float3(du2.x, hy * nscale, du2.y);

                    n += normalize(cross(d1, d2));
                }

                n = normalize(n);
                return height;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionWS  = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS    = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
{
    float3 worldPos = IN.positionWS;

    float2 uv = worldPos.xz * 0.00005;

    float3 N;
    float x = getNormal(
        uv,
        1.7 * ls_cloudscale * 0.1,
        0.005 * ls_shadowscale,
        N,
        0.05 * ls_shadowscale,
        worldPos.y * 0.001 + GetTime() * 0.0002
    );

    // HARD CLAMP so it cannot go black
    x = saturate(x);

    // FORCE VISIBILITY
    float alpha = saturate(ls_cloudthickness * x);
    if (alpha < 0.01) discard;

    // Basic lighting (fixed URP direction)
    Light light = GetMainLight();
    float3 lightDir = normalize(-light.direction);

    float NdotL = saturate(dot(N, lightDir));

    float3 color =
        ls_cloudcolor.rgb *
        (0.4 + 0.6 * NdotL) *
        x;

    return half4(color, alpha);
}


            ENDHLSL
        }
    }

    Fallback Off
}
