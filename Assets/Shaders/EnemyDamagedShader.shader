Shader "Custom/RadialWaves"
{
    Properties
    {
        _ParticleMap ("Particle Map", 2D) = "White" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _ParticleMap;
            float4 Time;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float timeFactor = sin(_Time.y * 5.0) + 1.0;
                float texVal = tex2D(_ParticleMap, i.uv).r;
                float angle = clamp((timeFactor + texVal) * 8.0, 0.0, 5.0) * 3.14159265;
                float col = sin(angle);
                return fixed4(col, col, col, 1.0);
            }
            ENDCG
        }
    }
}
