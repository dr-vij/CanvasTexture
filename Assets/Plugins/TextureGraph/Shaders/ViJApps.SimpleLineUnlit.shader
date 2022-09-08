Shader "ViJApps.SimpleLineUnlit"
{
    Properties
    {
        _Color ("_Color", Color) = (0, 0, 0, 1)
        _Thickness("_Thickness", Float) = 0
        _FromToCoord("_FromToCoord", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

         LOD 100

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"        

            uniform half4 _Color;
            uniform half _Thickness;
            uniform half3x3 _TransformationMatrix;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return _Color;
            }

            float line_segment(in half2 p, in half2 a, in half2 b)
            {
	            half2 ba = b - a;
	            half2 pa = p - a;
	            half h = clamp(dot(pa, ba) / dot(ba, ba), 0., 1.);
	            return length(pa - h * ba);
            }

            ENDHLSL
        }
    }
}
