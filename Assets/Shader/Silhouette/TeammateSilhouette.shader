//AI
// 壁に隠れている部分だけを描画するシルエットシェーダー(URP用)
// Pass 0 (Mask)      : 画面に見えている部分のStencilに印を付ける(色は描かない)
// Pass 1 (Silhouette): 壁などに遮られている部分のうち、印の無い場所だけ描く
// ※ Render Objectsの「Override Material」で Pass Index 0 と 1 をそれぞれ別Featureで実行する
Shader "Custom/TeammateSilhouette"
{
    Properties
    {
        [MainColor] _BaseColor ("Silhouette Color", Color) = (0.2, 0.8, 1.0, 0.6)
        [MainTexture] _BaseMap ("Silhouette Texture", 2D) = "white" {}
        [Toggle] _UseScreenUV ("Use Screen UV", Float) = 0
        _ScreenTiling ("Screen UV Tiling", Float) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _UseScreenUV;
            half _ScreenTiling;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        Varyings Vert(Attributes input)
        {
            Varyings output;
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            return output;
        }
        ENDHLSL

        // Pass 0: 見えている部分にStencilで印を付ける
        Pass
        {
            Name "Mask"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            ColorMask 0

            Stencil
            {
                Ref 2
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            half4 Frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // Pass 1: 遮られている部分(奥にある部分)だけシルエットを描く
        Pass
        {
            Name "Silhouette"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Greater

            Stencil
            {
                Ref 2
                Comp NotEqual
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                if (_UseScreenUV > 0.5)
                {
                    // 画面固定のパターン(縦横比を補正)
                    uv = input.positionCS.xy / _ScaledScreenParams.xy;
                    uv.x *= _ScaledScreenParams.x / _ScaledScreenParams.y;
                    uv *= _ScreenTiling;
                }

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                return tex * _BaseColor;
            }
            ENDHLSL
        }
    }
}
