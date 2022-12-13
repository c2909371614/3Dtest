Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Value ("bottomLevel",Range(0, 1)) = 0
        _Bottom ("bottom", float) = 0
        _MainTex ("Texture", 2D) = "white" {}

         // �Ƚϳ�������������
        // ������������������������������������������������������������������������������������������������
        //_Integer ("����(�°�)", Integer) = 1
        _Int ("����(�ɰ�)", Int) = 1
        _Float ("������", Float) = 0.5
        _FloatRange ("������������", Range(0.0, 1.0)) = 0.5
        // Unity����������������, ����ֱ�����
        // ��white����RGBA��1,1,1,1��
        // ��black����RGBA��0,0,0,1��
        // ��gray����RGBA��0.5,0.5,0.5,1��
        // ��bump����RGBA��0.5,0.5,1,0.5��
        // ��red����RGBA��1,0,0,1��
        _Texture2D ("2D������ͼ", 2D) = "red" {}
        // �ַ������ջ�������Чֵ������Ĭ��Ϊ ��gray��
        _DefaultTexture2D ("2D������ͼ", 2D) = "" {}
        // Ĭ��ֵΪ ��gray����RGBA��0.5,0.5,0.5,1��
        _Texture3D ("3D������ͼ", 3D) = "" {}
        _Cubemap ("��������ͼ", Cube) = "" {}
        // Inspector����ʾ�ĸ������ĸ������ֶ�
        _Vector ("Example vector", Vector) = (0.25, 0.5, 0.5, 1)
        // Inspector����ʾʰɫ��ʰȡɫ��RGBAֵ
        _Color("ɫ��", Color) = (0.25, 0.5, 0.5, 1)
        // ������������������������������������������������������������������������������������������������
                                            
        // ����֮�� �������������Ծ���һ����ѡ���� ������֪Unity��δ�������
        // HDR����ʹɫ�����ȵ�ֵ����1
        [HDR]_HDRColor("HDRɫ��", Color) = (1,1,1,1)
        // Inspector���ش�����
        [HideInInspector]_Hide("��������~", Color) = (1,1,1,1)
        // Inspector���ش��������Ե�Scale Offset�ֶ�
        [NoScaleOffset]_HideScaleOffset("����ScaleOffset", 2D) = "" {}
        // ָʾ��������Ϊ������ͼ����������˲����ݵ������༭�������ʾ���档
        [Normal]_Normal("������ͼ", 2D) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100 //������Ⱦ��ϸ��

        Pass
        {
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

                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Value;
            float _Bottom;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //�ü�ת����
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                //ѹy��
                float y = worldPos.y - (worldPos.y - _Bottom) * _Value;
                //��������
                float3 tempWorld = float3(worldPos.x, y, worldPos.z);
                //����ת�ü�
                o.vertex = UnityWorldToClipPos(tempWorld);
                
                //����
                o.worldPos = worldPos;
                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                //����
                float3 WorldLightDir = UnityWorldSpaceLightDir(i.worldPos);
                //������С
                float Nol = dot(i.worldNormal, WorldLightDir);//(-1,1)
                //half-lambert����ֵ
                float halfLambert = Nol * 0.5 + 0.5;//(0,1)
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * halfLambert;
            }
            ENDCG
        }
    }
}
