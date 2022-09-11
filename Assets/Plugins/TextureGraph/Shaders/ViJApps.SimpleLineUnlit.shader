Shader "ViJApps.SimpleLineUnlit"
{
    Properties
    {
        _Color ("_Color", Color) = (0, 0, 0, 1)
        _Thickness("_Thickness", Float) = 0
        _FromToCoord("_FromToCoord", Vector) = (0, 0, 0, 0)

        //TODO: Optimaze to matrices;
        _Trs2dCol0("_Trs2dCol0", Vector) = (1, 0, 0, 0)
        _Trs2dCol1("_Trs2dCol0", Vector) = (0, 1, 0, 0)
        _Trs2dCol2("_Trs2dCol0", Vector) = (0, 0, 1, 0)
     }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
     //   Tags { "RenderType"="Transparent" }
    //   Tags { "Queue" = "Transparent" }

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
            uniform half4 _FromToCoord;
            
            uniform float3 _Trs2dCol0;
            uniform float3 _Trs2dCol1;
            uniform float3 _Trs2dCol2;
            uniform half3x3 _TransformationMatrix;

            //TAKES 3 colum vectors and returns float3x3 mtrx
            float3x3 ColumnsToMatrices(in float3 c1, in float3 c2, in float3 c3)
            {
                return float3x3(c1, c2, c3);
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
                _TransformationMatrix = ColumnsToMatrices(_Trs2dCol0, _Trs2dCol1, _Trs2dCol2);
                float distance = line_segment(i.localPos.xy, _FromToCoord.xy, _FromToCoord.zw);
                float isLine = step(distance, _Thickness);
                return lerp(float4(0,0,0,0), _Color, isLine);
            }

            ENDHLSL
        }
    }
}
