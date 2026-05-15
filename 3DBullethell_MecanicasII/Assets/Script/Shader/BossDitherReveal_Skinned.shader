Shader "Custom/BossDitherReveal_Skinned"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _MainTex ("Main Texture", 2D) = "white" {}
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
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            fixed4 _BaseColor;

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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uvBase : TEXCOORD0;
                float2 uvMain : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                fixed4 color : COLOR;
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
                o.uvBase = TRANSFORM_TEX(v.uv, _BaseMap);
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 colBase = tex2D(_BaseMap, i.uvBase) * _BaseColor;
                fixed4 colMain = tex2D(_MainTex, i.uvMain) * _Color;

                fixed4 col = colBase;

                // fallback si el material usa _MainTex en vez de _BaseMap
                if (colBase.a <= 0.001 && colMain.a > 0.001)
                    col = colMain;

                if (col.rgb == fixed3(1,1,1) && colMain.rgb != fixed3(1,1,1))
                    col = colMain;

                col *= i.color;

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