Shader "Custom/S_Liquid_vrFix"
{
    Properties
    {
        [Header(General)]
        _FillAmount("Fill Amount", Float) = 0
        _Foam("Foam", Float) = 0.0
        _Transparency("Transparency", Range(0,1)) = 0
        
        [Header(Liquid Colors)]
        _LiquidColor("Liquid Color", Color) = (0,0.7,0.9)
        _BackColor("Back Color", Color) = (0,0.6,0.6)
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        
        [Header(Rim)]
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimWidth("Rim Width", Float) = 0.2
        _RimPower("Rim Power", Float) = 0.0

        [Header(Wobble)]
        _WobbleSpeedScale("Wobble Speed Scale", Float) = 0.4
        
        [Header(Bubbles)]
        _BubbleColor("Bubble Color", Color) = (1, 1, 1, 1)
        _UVScale("UV Scale", Range(0, 15)) = 3
        _ScrollSpeed("Scroll Speed", Range(0, 10)) = 1
        _BubbleRadius("Bubble Radius", Range(0,1)) = 0.1
        _NoiseOffset("Noise Offset", Range(0,1)) = 0.0
        _BubbleSmoothness("Bubble Smoothness", Range(0, 0.1)) = 0.01
        _BubbleInnerRadius("Bubble Inner Radius", Range(0, 1)) = 0.01

    }
    SubShader
    {
        Tags { "Queue"="Geometry" }
        LOD 100
        
        //ZWrite On
        Cull Off
        AlphaToMask On
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Includes/noiseSimplex.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID /// new
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD3;
                float3 fillEdge : TEXCOORD1;
                float3 viewDir : TEXCOORD2;

                
                UNITY_VERTEX_OUTPUT_STEREO /// new
            };

            float4 _LiquidColor;
            float4 _BackColor;
            float4 _FoamColor;
            float4 _RimColor;
            float _FillAmount;
            float _Foam;
            float _Transparency;

            float _RimWidth;
            float _RimPower;
            
            float _WobbleX;
            float _WobbleZ;
            float _WobbleSpeedScale;

            int _UVScale;
            float4 _BubbleColor;
            float _BubbleRadius;
            float _ScrollSpeed;
            float _NoiseOffset;
            float _BubbleSmoothness;
            float _BubbleThreshold;
            float _BubbleInnerRadius;


            float4 RotateAroundYInDegrees(float4 vertex, float degrees)
	        {
		        float alpha = degrees * UNITY_PI / 180.0;
		        float sina, cosa;
		        sincos(alpha, sina, cosa);
		        float2x2 m = float2x2(cosa, -sina, sina, cosa);
		        return float4(mul(m, vertex.xz), vertex.yw).xzyw;
	        }

            float invLerp(float from, float to, float value){
              return (value - from) / (to - from);
            }
            
            float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value){
              float rel = invLerp(origFrom, origTo, value);
              return lerp(targetFrom, targetTo, rel);
            }

            float drawBubble(float2 pos, float2 center)
            {
                float dist = distance(pos, center);
                return smoothstep(_BubbleRadius - _BubbleInnerRadius - _BubbleSmoothness, _BubbleRadius - _BubbleInnerRadius, dist) - step(_BubbleRadius, dist);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                
                UNITY_SETUP_INSTANCE_ID(v); /// new
                UNITY_INITIALIZE_OUTPUT(v2f,o); /// new
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); /// new

                o.vertex = UnityObjectToClipPos(v.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex.xyz);

                // Rotate the liquid. First coordinate is X (worldPos.xz), so it rotates around X axis.
                float3 worldPosX = RotateAroundYInDegrees(float4(worldPos.xz,0,0),90);
                // Rotate the liquid. First coordinate is Z (worldPos.zx), so it rotates around Z axis.
                float3 worldPosZ = RotateAroundYInDegrees(float4(worldPos.zx,0,0),90);
                
                // Add rotated vertex value to current position.
                // We are not physically changing vertex position. We just calculate offset for current
                // vertex. That offset gets added to current position and we use it to clip.
                float3 worldPosWithWobble = worldPos + (worldPosX * _WobbleX) + (worldPosZ * _WobbleZ);
                
                o.fillEdge = worldPosWithWobble + _FillAmount;
                o.normal = v.normal;
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));

                o.uv = v.uv;
                
                return o;
            }

            float4 frag (v2f i, float facing : VFACE) : SV_Target
            {
                float4 col = _LiquidColor;
                
                // Where does the liquid end
                float cutoffTop = step(i.fillEdge.y, 0);
                // Cutoff from top to form a foam
                float cutoffFoam = step(_Foam, i.fillEdge.y);
                // Find the intersection
                float foam = cutoffTop * cutoffFoam;
                // Color the foam where intersected
                float4 foamColored = foam * _FoamColor;
                col += foamColored;

                
                // Rim light dot product
                float viewDirDotProduct = 1 - pow(dot(i.viewDir, i.normal), _RimPower);
                float rimLight = max(0,viewDirDotProduct - _RimWidth);
                // Add rim light to final color
                col += _RimColor * rimLight;
                

                // Clip where the liquid ends
                clip(cutoffTop - 0.01); // 0.01 so that values = 0 wouldn't draw

                
                // GENERATING BUBBLES
                // BUBBLES LAYER 1 MIDDLE
                
                // Scale UV
                float2 uv = i.uv * _UVScale;
                
                // Divide UV into _UVScale
                uv.y -= _Time.y * _ScrollSpeed;
                float2 cellUV = floor(uv);
                
                // Get offset for that cell
                float offset = snoise(cellUV + _NoiseOffset);
                float2 bubbleUV = (cellUV + offset);

                // Draw bubble
                float bubble = drawBubble(uv, bubbleUV);
                
                float4 bubbleColored = _BubbleColor * bubble;

                col += bubbleColored;


                // BUBBLES LAYER 2 CLOSER(SAME AS ABOVE)
                uv = i.uv * _UVScale * 0.75;
                uv.y -= _Time.y * _ScrollSpeed * 0.9;
                cellUV = floor(uv);
                offset = snoise(cellUV + _NoiseOffset);
                bubbleUV = (cellUV + offset);
                bubble = drawBubble(uv, bubbleUV) * 1.5;
                bubbleColored = _BubbleColor * bubble;
                col += bubbleColored;

                // BUBBLES LAYER 3 FURTHER (SAME AS ABOVE)
                uv = i.uv * _UVScale * 2;
                uv.y -= _Time.y * _ScrollSpeed;
                cellUV = floor(uv);
                offset = snoise(cellUV + _NoiseOffset);
                bubbleUV = (cellUV + offset);
                bubble = drawBubble(uv, bubbleUV) * 0.3;
                bubbleColored = _BubbleColor * bubble;
                col += bubbleColored;
                
                
                return facing > 0 ? col : _BackColor;
            }
            ENDCG
        }
    }
}
