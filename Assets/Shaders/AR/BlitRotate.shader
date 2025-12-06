Shader "Custom/URPBlitRotateFull"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MirrorX("Mirror X", Float) = 0
        _Rotate90("Rotate 90", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Overlay"
        }

        Pass
        {
            Name "BlitPass"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _MirrorX;
            float _Rotate90;

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            float2 ApplyMirror(float2 uv)
            {
                if (_MirrorX > 0.5)
                    uv.x = 1.0 - uv.x;
                return uv;
            }

            float2 ApplyRotate90(float2 uv)
            {
                if (_Rotate90 > 0.5)
                    return float2(uv.y, 1.0 - uv.x);
                
                return uv;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                uv = ApplyMirror(uv);
                uv = ApplyRotate90(uv);

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }

            ENDHLSL
        }
    }
}
