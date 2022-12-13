Shader "Unlit/ToonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}   

        _OutLineWidth("Outline Width", Range(0.01, 1)) = 0.01
        _OutLineColor("Outline Color", Color) = (0.5, 0.5, 0.5, 1)

        _RampStart("RampStart", Range(0.1, 1)) = 0.3
        _RampSize("RampSize", Range(0, 1)) = 0.1
        [IntRange] _RampStep("RampStep", Range(1, 10)) = 1
        _RampSmooth("RampSmooth", Range(0.01, 1)) = 0.1
        _DarkColor("DarkColor", Color) = (0.4, 0.4, 0.4, 1)
        _LightColor("LightColor", Color) = (0.8, 0.8, 0.8, 1)

        _SpecPow("SpecPow", Range(0, 1)) = 0.1//�����
        _SpecularColor("SpecularColor", Color) = (1.0, 1.0, 1.0, 1)
        _SpecIntensity("SpecIntensity", Range(0, 1)) = 0
        _SpecSmooth("SpecSmooth", Range(0, 0.5)) = 0.1

        _RimColor("RimColor", Color) = (1.0, 1.0, 1.0, 1)
        _RimThreshold("RimThreshold", Range(0,1)) = 0.45
        _RimSmooth("RimSmooth", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal:NORMAL;//���շ���
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //���ߺ�����λ��
                float3 worldNormal:TEXCOORD1;
                float3 worldPos:TEXCOORD2;

                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _RampStart;
            float _RampSize;
            float _RampStep;
            float _RampSmooth;
            float3 _DarkColor;
            float3 _LightColor;

            float _SpecPow;

            float3 _SpecularColor;
            float _SpecIntensity;
            float _SpecSmooth;

            float3 _RimColor;
            float _RimThreshold;
            float _RimSmooth;

            float linearstep(float min, float max, float t)
            {
                return saturate((t- min) / (max - min));//�޶�ȡֵ��0��1��
            }            

            //float lerp(float a, float b, float w) {
            //    return a + w*(b-a);
            //}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                ////�ü�ת����
                //float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                ////ѹy��
                //float y = worldPos.y - (worldPos.y - _Bottom) * _Value;
                ////��������
                //float3 tempWorld = float3(worldPos.x, y, worldPos.z);
                ////����ת�ü�
                //o.vertex = UnityWorldToClipPos(tempWorld);
                
                //����
                o.worldPos =mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                //����
                float3 normal = normalize(i.worldNormal);
                //����
                float3 worldLightDir = UnityWorldSpaceLightDir(i.worldPos);
                //����������С
                float NoL = dot(i.worldNormal, worldLightDir);//(-1,1)
                //half-lambert����ֵ
                float halfLambert = NoL * 0.5 + 0.5;//(0,1)
                
                //������
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                //����half������ʹ��Blinn-phone����߹�
                float3 halfDir = normalize(viewDir + worldLightDir);
                //����NoH���ڼ���߹�
                float NoH = dot(normal, halfDir);
                //�߹�����ֵ
                float blinnPhone = pow(max(0, NoH), _SpecPow * 128.0);
                //�߹�ɫ��
                float3 specularColor = smoothstep(0.7 - _SpecSmooth / 2, 0.7 + _SpecSmooth / 2, blinnPhone) * _SpecularColor * _SpecIntensity;
                //return blinnPhone;

                //NoV���ڼ����Ե��
                float NoV = dot(i.worldNormal, viewDir);
                //�����Ե������ֵ
                float rim = (1 - max(0, NoV)) * NoL;
                float3 rimColor = smoothstep(_RimThreshold - _RimSmooth / 2, _RimThreshold + _RimSmooth / 2, rim) * _RimColor;
                //return rim;

                //ͨ������ֵ��������ramp
                float ramp = linearstep(_RampStart, _RampStart + _RampSize, halfLambert);//(0, 1)
                //ɫ�׷ֲ�
                float step = ramp * _RampStep;//ʹÿ��ɫ�״�СΪ1���������
                float gridStep = floor(step);
                float smoothStep = smoothstep(gridStep, gridStep + _RampSmooth, step) + gridStep;//��ͻ�
                ramp = smoothStep / _RampStep;
                //����rampɫ��
                float3 rampColor = lerp(_DarkColor, _LightColor, ramp);
                rampColor *= col;
                //�����ɫ���߹⣬��Ե��
                float3 finalColor = saturate(rampColor + specularColor + rimColor);
                //return rampColor;
                return float4(finalColor, 1);
                //ramp *= col;
                //return ramp
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col * halfLambert;
            }
            ENDCG
        }

        //���
        Pass
        {
        Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                //����
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;



            //�������
            float _OutLineWidth;
            //������ɫ
            float _OutLineColor;

            v2f vert (appdata v)
            {
                v2f o;
                
                float4 newVertex = float4(v.vertex.xyz + normalize(v.normal) * _OutLineWidth * 0.05, 1);

                o.vertex = UnityObjectToClipPos(newVertex); //�ü��ռ�
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                //// sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
                return _OutLineColor;
            }
            ENDCG
        }
    }
}
