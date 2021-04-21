Shader "Custom/VertexShader"
{
    Properties
    {
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }

    SubShader{
        Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
        //Tags { "RenderType" = "Opaque" }
        Pass {
            CGPROGRAM
            #include "UnityStandardBRDF.cginc"
             
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            struct v2g // vertex to geometry
            {
                float4 pos : SV_POSITION;
                float3 vertex : TEXCOORD1;
                float4 vertexColor : TEXCOORD0;
            };

            struct g2f {
                float4 pos: SV_POSITION;
                float3 normal : TEXCOORD1;
                float4 vertexColor : TEXCOORD0;
            };

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;

            v2g vert(appdata_full v) {
                v2g o;
                o.vertex = v.vertex;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.vertexColor = v.color.rgba;
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                g2f o;

                // Compute the Normal
                float3 vecA = IN[1].vertex - IN[0].vertex;
                float3 vecB = IN[2].vertex - IN[0].vertex;
                float3 normal = cross(vecA, vecB);
                o.normal = normalize(mul(normal, (float3x3) unity_WorldToObject));

                o.vertexColor = IN[0].vertexColor;

                for (int i = 0; i < 3; i++) {
                    o.pos = IN[i].pos;
                    triStream.Append(o);
                }
            }

            half4 frag(g2f i) : COLOR{
                /*float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 lightColor = _LightColor0.rgb;
                float3 diffuse = lightColor * DotClamped(lightDir, i.normal);*/

                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float nl = max(0, dot(i.normal, lightDir));
                float4 diff = nl * _LightColor0;
                diff.rgb += ShadeSH9(half4(i.normal, 1));

                float4 col = i.vertexColor;
                col.rgb *= diff;
                return col;
            }
            ENDCG
        }
        
        

    }
    Fallback "Diffuse"
}
