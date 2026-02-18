Shader "My Legacy Shaders/Particles/Alpha Blended Premultiply"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _MainTex ("Particle Texture", 2D) = "white" {}
        _InvFade ("Soft Particles Factor", Range(0.01, 3.0)) = 1.0
        _ZClipMin ("Z Clip Min", Float) = -10000
        _XClipMin ("X Clip Min", Float) = -10000
        _XClipMax ("X Clip Max", Float) = 10000
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent+25"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }
        Blend One OneMinusSrcAlpha
        ColorMask RGB
        Cull Off
        Lighting Off
        ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_particles
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _TintColor;
                float4 _MainTex_ST;
                float _ZClipMin;
                float _XClipMin;
                float _XClipMax;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    float2 worldPosXZ : TEXCOORD3;
                    UNITY_FOG_COORDS(1)
                    #ifdef SOFTPARTICLES_ON
                    float4 projPos : TEXCOORD2;
                    #endif
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.worldPosXZ = worldPos.xz;
                    #ifdef SOFTPARTICLES_ON
                    o.projPos = ComputeScreenPos(o.vertex);
                    COMPUTE_EYEDEPTH(o.projPos.z);
                    #endif
                    o.color = v.color * _TintColor;
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    return o;
                }

                UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
                float _InvFade;

                fixed4 frag(v2f i) : SV_Target
                {
                    clip(i.worldPosXZ.x - _XClipMin);
                    clip(_XClipMax - i.worldPosXZ.x);
                    clip(i.worldPosXZ.y - _ZClipMin);
                    #ifdef SOFTPARTICLES_ON
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                    float partZ = i.projPos.z;
                    float fade = saturate(_InvFade * (sceneZ - partZ));
                    i.color.a *= fade;
                    #endif

                    fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                    // Premultiply alpha: output (RGB * A, A) for correct Blend One OneMinusSrcAlpha
                    col.rgb *= col.a;
                    col.a = saturate(col.a);

                    UNITY_APPLY_FOG(i.fogCoord, col);
                    return col;
                }
                ENDCG
            }
        }
    }

    Fallback "Legacy Shaders/Particles/Alpha Blended"
}
