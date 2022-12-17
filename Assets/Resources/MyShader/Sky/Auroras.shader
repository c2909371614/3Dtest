Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _SkyColor("SkyColor", Color) = (0.4, 0.4, 0.4, 1)

        _SkyCurvature("SkyCurvature", Range(0, 10)) = 0.4
        _AurorasTiling("AurorasTiling", Range(0.1, 10)) = 0.4
        [IntRange] _RayMarchStep("RayMarchStep", Range(1, 128)) = 64

        _RayMarchDistance("RayMarchDistance", Range(0.01, 1)) = 2.5
        _AurorasTex("AurorasTexture", 2D) = "white"{}
        _AurorasColor("AurorasColor", Color) = (0.4, 0.4, 0.4, 1)

        _AurorasIntensity("AurorasIntensity", Range(0.1, 20)) = 3
        _AurorasAttenuation("AurorasAttenuation", Range(0, 0.99)) = 0.4

        _AurorasNoiseTex("AurorasNoiseTex", 2D) = "white"{}
        _AurorasSpeed("AurorasSpeed", Range(0.1, 1)) = 0.1

        _StarNoiseTex("StarNoiseTex", 2D) = "white"{}

        _StarShinningSpeed("StarShinningSpeed", Range(0, 1)) = 0.1
        _StarCount("StarCount", Range(0, 1)) = 0.3

        _SkyLineSize("SkyLineSize", Range(0, 1)) = 0.06
        _SkyLineBasePow("SkyLineBasePow", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float3 worldPos:TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            float3 _SkyColor;

            float _SkyCurvature;
            float _AurorasTiling;
            float _RayMarchStep;

            float _RayMarchDistance;
            sampler2D _AurorasTex;
            float4 _AurorasTex_ST;

            float3 _AurorasColor;
            float _AurorasIntensity;
            float _AurorasAttenuation;

            sampler2D _AurorasNoiseTex;
            float4 _AurorasNoiseTex_ST;
            float _AurorasSpeed;

            sampler2D _StarNoiseTex;
            float4 _StarNoiseTex_ST;

            float _StarShinningSpeed;
            float _StarCount;

            float _SkyLineSize;
            float _SkyLineBasePow;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(v.vertex, unity_ObjectToWorld);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);

                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // -------星星------
                //return tex2D(_StarNoiseTex, TRANSFORM_TEX(i.uv, _StarNoiseTex)).r;
                const float starTime = _Time.y * _StarShinningSpeed;
                //计算叠加区间的两层星星UV
                const float2 beginMove = floor(starTime) * 0.3;
                const float2 endMove = ceil(starTime) * 0.3;//向上取整
                const float2 beginUV = i.uv + beginMove;
                const float2 endUV = i.uv + endMove;
                //采样两层星星的值
                float beginNoise = tex2D(_StarNoiseTex, TRANSFORM_TEX(beginUV, _StarNoiseTex)).r;
                float endNoise = tex2D(_StarNoiseTex, TRANSFORM_TEX(endUV, _StarNoiseTex)).r;
                //减少星星
                beginNoise = saturate(beginNoise - (1 - _StarCount)) / _StarCount;
                endNoise = saturate(endNoise - (1 - _StarCount)) / _StarCount;
                const float fracStarTime = frac(starTime);
                //混合两层星星
                float starColor = saturate(beginNoise - fracStarTime) + saturate(endNoise - (1 - fracStarTime));

                //// sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
                float3 color = 0;
                //计算ray march信息
                //每个像素发射射线
                float3 rayDriginal = 0;
                float3 totalDir = i.worldPos - rayDriginal;
                float3 rayDir = normalize(totalDir);

                //扩展球面来计算march的起始点
                //天空曲率
                float skyCurvatureFactor = rcp(rayDir.y + _SkyCurvature);
                //本质为模拟地球大气 无数条射线向外发射 就会形成一个球面
                float3 basicRayPlane = rayDir * skyCurvatureFactor * _AurorasTiling;
                //march 起点
                float3 rayMarchBegin = rayDriginal + basicRayPlane;
                //一步的大小
                float stepSize = rcp(_RayMarchStep);
                float3 avgColor = 0; 
                for(float ii = 0; ii < _RayMarchStep; ii += 1)
                {
                    float curStep = stepSize * ii;
                    //初始几次采样贡献更大， 用二次函数着重初始采样
                    curStep = curStep * curStep;
                    //当前步进距离
                    float curDistance = curStep * _RayMarchDistance;
                    //步进后的位置
                    float3 curPos = rayMarchBegin + rayDir * curDistance * skyCurvatureFactor;
                    float2 uv = float2(-curPos.x, curPos.z);
                    //计算扰动uv
                    float2 warp_vec = tex2D(_AurorasNoiseTex, TRANSFORM_TEX((uv*2 + _Time.y * _AurorasSpeed), _AurorasNoiseTex));
                    //采样当前噪声强度
                    float curAuroras = tex2D(_AurorasTex, TRANSFORM_TEX((uv + warp_vec * 0.1), _AurorasTex)).r;

                    //强度衰减
                    curAuroras = curAuroras * saturate(1 - pow(curDistance, 1 - _AurorasAttenuation));

                    //极光色彩累计计算
                    float3 curColor = sin((_AurorasColor * 2 - 1) * 0.5 + 0.5);
                    //去两步色彩的平均值 是颜色更趋近于本色
                    avgColor = (avgColor + curColor) / 2;
                    color += avgColor * curAuroras * stepSize; 
                    //return curAuroras;
                }
                //强度
                color *= _AurorasIntensity;
                //混合天际线
                color *= saturate(rayDir.y / _SkyLineSize + _SkyLineBasePow);
                color += _SkyColor;
                //星星
                color = color + starColor * 0.9;
                return float4(color, 1);
            }
            ENDCG
        }
    }
}
