Shader "Unlit/water"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _ShallowWater("ShallowColor", Color) = (1.0, 1.0, 1.0, 1.0)
        _DeepWater("DeepColor", Color) = (1.0, 1.0, 1.0, 1.0)

        _SurfaceNoise("SurfaceNoise", 2D) = "white"{}
        _MoveSpeed("MoveSpeed", Range(0, 1)) = 0.5
        _WaterAlpha("WaterAlpha", Range(0, 1)) = 0.5

        _FoamDistance("FoamDistance", Range(0, 10)) = 0.4
        _FoamColor("FoamColor", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags 
        {
            //"RenderType"="Opaque"
            "RenderType" = "Transparent"
            "RenderPipeLine" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        //LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha//透明度混合
            //HLSLPROGRAM
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // make fog work
            //#pragma multi_compile_fog

            //#include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal"

            struct appdata
            {
                //float4 vertex : POSITION;
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                float2 noiseUV:TEXCOORD0;
                float4 screenPosition:TEXCOORD1;
                //UNITY_FOG_COORDS(1)
                float4 positionCS : SV_POSITION;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;
            float _MoveSpeed;

            float _WaterAlpha;//透明度

            float _FoamDistance;
            float3 _FoamColor;

            float3 _ShallowWater;
            float3 _DeepWater;
            
            //float4 ComputeScreenPos(float4 positionCS)
            //{
            //    float4 o = positionCS * 0.5f;
            //    o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
            //    o.zw = positionCS.zw;
            //    return o;
            //}

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS);
                o.screenPosition = ComputeScreenPos(positionInputs.positionCS);
                o.positionCS = positionInputs.positionCS;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                //通过纹理采样 计算屏幕深度
                float screenRawDepth = SampleSceneDepth(i.screenPosition.xy / i.screenPosition.w);
                //深度纹理采样结果转换到视图空间的深度值
                float sceneEyeDepth = LinearEyeDepth(screenRawDepth, _ZBufferParams);
                //最终得到水的深度
                float waterDepth = sceneEyeDepth - i.screenPosition.w;
                //拿到水的颜色
                //float3 waterColor = lerp(_ShallowWater, _DeepWater, waterDepth);
                float3 waterColor = lerp(_DeepWater, _ShallowWater, waterDepth);
                //流动
                float surfaceNoiseSample = tex2D(_SurfaceNoise, i.noiseUV + _Time.y * _MoveSpeed * 0.1).r;
                //浮沫
                float foam = saturate(waterDepth / _FoamDistance);
                float surfaceNoise = smoothstep(0, foam, surfaceNoiseSample);

                // sample the texture
                //half4 col = tex2D(_MainTex, i.uv);
                half4 col = half4(waterColor + surfaceNoise * _FoamColor, _WaterAlpha);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            //fixed4 frag (v2f i) : SV_Target
            //{
            //    // sample the texture
            //    fixed4 col = tex2D(_MainTex, i.uv);
            //    // apply fog
            //    //UNITY_APPLY_FOG(i.fogCoord, col);
            //    return col;
            //}
            ENDHLSL
        }
    }
}
