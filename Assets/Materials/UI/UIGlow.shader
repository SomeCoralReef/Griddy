Shader "Custom/UIGlow"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _GlowColor ("Glow Color", Color) = (1,0,0,1)
        _GlowIntensity ("Glow Intensity", Range(0,5)) = 0
        _GlowSize ("Glow Size (px)", Range(0,10)) = 3

        // Stencil properties to behave like UI/Default (for Masks)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // x=1/width, y=1/height
            fixed4 _Color;

            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowSize; // in px (we’ll convert to texel offset)

            float _UseUIAlphaClip;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            // Sample alpha around the sprite to create a soft halo
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv) * i.color;
                fixed baseA = baseCol.a;

                // Early out if no glow requested
                if (_GlowIntensity <= 0.0001)
                {
                    // Optional alpha clip (for masking crispness)
                    if (_UseUIAlphaClip > 0.5 && baseA <= 0) discard;
                    return baseCol;
                }

                // Convert pixel size to UV offset
                float2 texel = _MainTex_TexelSize.xy * _GlowSize;

                // 8-direction kernel
                float2 k[8];
                k[0] = float2(-texel.x, 0);
                k[1] = float2( texel.x, 0);
                k[2] = float2(0, -texel.y);
                k[3] = float2(0,  texel.y);
                k[4] = float2(-texel.x, -texel.y);
                k[5] = float2(-texel.x,  texel.y);
                k[6] = float2( texel.x, -texel.y);
                k[7] = float2( texel.x,  texel.y);

                // Accumulate neighbor alpha
                float neighbor = 0;
                [unroll] for (int n = 0; n < 8; n++)
                {
                    neighbor += tex2D(_MainTex, i.uv + k[n]).a;
                }
                neighbor /= 8.0;

                // Glow strongest where there isn't already opaque color
                float glowMask = saturate(neighbor - baseA);
                fixed4 glow = _GlowColor;
                glow.a = glowMask * _GlowIntensity;
                glow.rgb *= glow.a;

                fixed4 outCol = baseCol + glow * (1 - baseA);

                if (_UseUIAlphaClip > 0.5 && outCol.a <= 0) discard;
                return outCol;
            }
            ENDCG
        }
    }
    FallBack Off
}