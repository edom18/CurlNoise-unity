Shader "Custom/ParticleShader"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "ForceNoShadowCasting"="True" }
        LOD 200

        CGINCLUDE
        #include "UnityCG.cginc"
        #include "UnityStandardShadow.cginc"

        struct Particle
        {
            int id;
            int active;
            float3 position;
            float3 velocity;
            float3 color;
            float scale;
            float baseScale;
            float time;
            float lifeTime;
        };

        StructuredBuffer<Particle> _Particles;

        int _IdOffset;

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv1 : TEXCOORD1;
        };

        struct v2f
        {
            float4 position : SV_POSITION;
            float3 normal : NORMAL;
            float3 color : TEXCOORD0;
            float2 uv1 : TEXCOORD1;
        };

        inline int getId(float2 uv1)
        {
            return (int)(uv1.x + 0.5) + _IdOffset;
        }

        v2f vert(appdata v)
        {
            Particle p = _Particles[getId(v.uv1)];
            v.vertex.xyz *= p.scale * p.baseScale;
            v.vertex.xyz += p.position;

            v2f o;
            o.uv1 = v.uv1;
            o.position = UnityObjectToClipPos(v.vertex);
            o.color = p.color;
            o.normal = v.normal;
            return o;
        }

        float4 frag(v2f i) : SV_Target
        {
            return float4(i.color, 1.0);
        }
        ENDCG

        //Pass
        //{
        //    Stencil
        //    {
        //        Ref 10
        //        Comp Equal
        //        Pass IncrSat
        //    }

        //    ColorMask 0

        //    ZWrite On
        //    ZTest LEqual
        //    Cull Off

        //    CGPROGRAM
        //    #pragma target 3.0
        //    #pragma vertex vert
        //    #pragma fragment frag
        //    ENDCG
        //}

        Pass
        {
            ZWrite On
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
    FallBack "Diffuse"
}
