Shader "ViJApps.SimpleLineUnlit"
{
    Properties
    {
        _Color ("_Color", Color) = (0, 0, 0, 1)
        _Thickness("_Thickness", Float) = 0
        _FromToCoord("_FromToCoord", Vector) = (0, 0, 0, 0)
        _Ascpect("_Ascpect", Float) = 1
     }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Tags { "Queue" = "Transparent" }

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

            uniform half4 _Color;
            uniform half _Thickness;
            uniform half _Aspect;
            uniform half4 _FromToCoord;
            
            uniform half3x3 _InverseAspectMatrix;

            float3x3 ColumnsToMatrices(in float3 c1, in float3 c2, in float3 c3)
            {
                return float3x3(c1, c2, c3);
            }

            float3x3 ScaleMatrixFromAspect(in float aspect)
            {
                return float3x3(float3(aspect, 0, 0),float3(0, 1, 0), float3(0, 0, 1));
            }

            float3x3 InverseScaleMatrixFromAspect(in float aspect)
            {
                return float3x3(float3(1.0 / aspect, 0, 0),float3(0, 1, 0), float3(0, 0, 1));
            } 

            float2 TransformPoint(in float3x3 m, in float2 point2d)
            {
                return mul(m, float3(point2d.x, point2d.y, 1)).xy;
            }

            float2 TransformDirection(in float3x3 m, in float2 point2d)
            {
                return mul(m, float3(point2d.x, point2d.y, 0)).xy;
            }

            //SDF LINE
            float line_segment(in half2 p, in half2 a, in half2 b)
            {
	            half2 ba = b - a;
	            half2 pa = p - a;
	            half h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
	            return length(pa - h * ba);
            }

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 localPos: COLOR0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.localPos = v.vertex.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                _InverseAspectMatrix = InverseScaleMatrixFromAspect(_Aspect);

                float2 p = TransformPoint(_InverseAspectMatrix, i.localPos.xy);
                float2 from = TransformPoint(_InverseAspectMatrix, _FromToCoord.xy);
                float2 to = TransformPoint(_InverseAspectMatrix, _FromToCoord.zw);

                float distance = line_segment(p, from, to);
                float isLine = step(distance, _Thickness / 2);
                return lerp(float4(0,0,0,0), _Color, isLine);
            }

            ENDHLSL
        }
    }
}
