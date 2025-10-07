Shader "Custom/S_2DCloud"
{
    Properties
    {
        _FlameMask ("Flame Mask", 2D) = "white" {}
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Range(0, 100)) = 5
        _Iterations ("Iterations", Range(1, 10)) = 4
        _Persistence ("Persistence", Range(0, 1)) = 0.5
        _Lacunarity ("Lacunarity", Range(0, 2.5)) = 2
        _Frequency ("Frequency", Float) = 1
        _Speed ("Cloud speed", Range(0,10)) = 1
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            ZWrite On
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Includes/lygia/generative/fbm.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _FlameMask;
            sampler2D _NoiseTexture;
            float _NoiseScale;
            int _Iterations;
            float _Persistence;
            float _Lacunarity;
            float _Frequency;
            float _Speed;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = TransformObjectToHClip(IN.vertex);
                OUT.uv = IN.uv;

                return OUT;
            }

            float getNoise(float2 uv)
            {
                float value = 0;
                float a = 1;
                float f = _Frequency;

                for(int i=0; i<_Iterations; i++)
                {
                    value += fbm(f * uv) * a;
                    a *= _Persistence;
                    f *= _Lacunarity;
                }

                return value * _Frequency;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float4 fbmNoise1 = 0;
                fbmNoise1.x = getNoise(IN.uv * _NoiseScale + _Time.y * 0.03 * _Speed);
                fbmNoise1.y = getNoise(IN.uv * _NoiseScale + _Time.y * 0.03 * _Speed);
                fbmNoise1.z = getNoise(IN.uv * _NoiseScale + _Time.y * 0.03 * _Speed);
                fbmNoise1.w = getNoise(IN.uv * _NoiseScale + _Time.y * 0.03 * _Speed);

                float noise = fbmNoise1.x + fbmNoise1.y + fbmNoise1.z + fbmNoise1.w;
                noise = clamp(noise, 0, 1);

                return noise;
            }
            ENDHLSL
        }
    }
}
