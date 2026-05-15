Shader "Custom/BossDitherReveal_Mesh"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        _RevealScreenPos ("Reveal Screen Pos", Vector) = (0.5,0.5,0,0)
        _RevealRadius ("Reveal Radius", Float) = 0
        _RevealSoftness ("Reveal Softness", Float) = 0.04
        _MinAlpha ("Minimum Visible Amount", Range(0.05, 1)) = 0.35
        _DitherPixelScale ("Dither Pixel Scale", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
            "IgnoreProjector"="True"
        }

        Cull Off
        ZWrite On

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;

        float4 _RevealScreenPos;
        float _RevealRadius;
        float _RevealSoftness;
        float _MinAlpha;
        float _DitherPixelScale;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        float Bayer4x4(float2 pixel)
        {
            int x = (int)fmod(pixel.x, 4);
            int y = (int)fmod(pixel.y, 4);

            float value = 0;

            if (y == 0)
            {
                if (x == 0) value = 0;
                else if (x == 1) value = 8;
                else if (x == 2) value = 2;
                else value = 10;
            }
            else if (y == 1)
            {
                if (x == 0) value = 12;
                else if (x == 1) value = 4;
                else if (x == 2) value = 14;
                else value = 6;
            }
            else if (y == 2)
            {
                if (x == 0) value = 3;
                else if (x == 1) value = 11;
                else if (x == 2) value = 1;
                else value = 9;
            }
            else
            {
                if (x == 0) value = 15;
                else if (x == 1) value = 7;
                else if (x == 2) value = 13;
                else value = 5;
            }

            return (value + 0.5) / 16.0;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float aspect = _ScreenParams.x / _ScreenParams.y;

            float2 diff = screenUV - _RevealScreenPos.xy;
            diff.x *= aspect;

            float dist = length(diff);

            float reveal = 1.0 - smoothstep(
                _RevealRadius - _RevealSoftness,
                _RevealRadius,
                dist
            );

            if (reveal > 0.001)
            {
                float pixelScale = max(_DitherPixelScale, 1.0);
                float2 pixel = floor((screenUV * _ScreenParams.xy) / pixelScale);

                float dither = Bayer4x4(pixel);

                // reveal = 1 significa zona interior del círculo.
                // _MinAlpha controla cuánto queda visible dentro.
                float keepChance = lerp(1.0, _MinAlpha, reveal);

                if (dither > keepChance)
                    clip(-1);
            }

            o.Albedo = col.rgb;
            o.Alpha = col.a;
            o.Metallic = 0;
            o.Smoothness = 0.35;
        }
        ENDCG
    }

    FallBack "Diffuse"
}