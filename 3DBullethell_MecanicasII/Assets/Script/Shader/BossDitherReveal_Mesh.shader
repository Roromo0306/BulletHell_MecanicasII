Shader "Custom/BossDitherReveal_Mesh"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        _RevealScreenPos ("Reveal Screen Pos", Vector) = (0.5,0.5,0,0)
        _RevealRadius ("Reveal Radius", Float) = 0
        _RevealSoftness ("Reveal Softness", Float) = 0.04
        _MinAlpha ("Minimum Visible Amount", Range(0.05, 1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float4 _RevealScreenPos;
            float _RevealRadius;
            float _RevealSoftness;
            float _MinAlpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            float Bayer4x4(float2 pixel)
            {
                int x = (int)fmod(pixel.x, 4);
                int y = (int)fmod(pixel.y, 4);

                float value = 0;

                if (y == 0)
                {
                    if (x == 0) value = 0;
                    if (x == 1) value = 8;
                    if (x == 2) value = 2;
                    if (x == 3) value = 10;
                }
                else if (y == 1)
                {
                    if (x == 0) value = 12;
                    if (x == 1) value = 4;
                    if (x == 2) value = 14;
                    if (x == 3) value = 6;
                }
                else if (y == 2)
                {
                    if (x == 0) value = 3;
                    if (x == 1) value = 11;
                    if (x == 2) value = 1;
                    if (x == 3) value = 9;
                }
                else
                {
                    if (x == 0) value = 15;
                    if (x == 1) value = 7;
                    if (x == 2) value = 13;
                    if (x == 3) value = 5;
                }

                return (value + 0.5) / 16.0;
            }

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.pos);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                float2 screenUV = i.screenPos.xy / i.screenPos.w;

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
                    float2 pixel = floor(screenUV * _ScreenParams.xy);
                    float dither = Bayer4x4(pixel);

                    float keepChance = lerp(1.0, _MinAlpha, reveal);

                    if (dither > keepChance)
                        discard;
                }

                return col;
            }

            ENDCG
        }
    }
}