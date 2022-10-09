Shader "ViJApps.SimpleEllipseUnlit"
{
    Properties
    {
        _AbFillStroke("_AbFillStroke", Vector) = (1, 1, 1.1, 1.1)
        _FillColor ("_FillColor", Color) = (1, 1, 1, 1)
        _StrokeColor("_StrokeColor", Color) = (0, 0, 0, 1)
        _Center("_Center", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
        }
        Tags
        {
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always
        ZWrite Off

        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Math.hlsl"

            //Figure data
            uniform half4 _AbFillStroke;
            uniform half4 _FillColor;
            uniform half4 _StrokeColor;
            uniform half3 _Center;
            uniform float3x3 _InverseAspectMatrix;
            uniform float4x4 _TransformMatrix;
            uniform half _Aspect;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 texCoord: TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 texCoord : TEXCOORD0;
                float3 localPos: COLOR0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.localPos = v.vertex.xyz;
                o.texCoord = v.texCoord;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 p = TransformPoint(_TransformMatrix, i.localPos.xy);
                float2 abFill = _AbFillStroke.xy;
                float2 abStroke = _AbFillStroke.zw;

                float fillDistance = sdEllipse(p, abFill);
                float strokeDistance = sdfSubtract(sdEllipse(p, abStroke), fillDistance);

                float isFill = step(fillDistance, 0);
                float isStroke = step(strokeDistance, 0);

                half4 result = lerp(float4(0, 0, 0, 0), _StrokeColor, isStroke);
                result = lerp(result, _FillColor, isFill);
                return result;
            }
            ENDHLSL
        }
    }
}