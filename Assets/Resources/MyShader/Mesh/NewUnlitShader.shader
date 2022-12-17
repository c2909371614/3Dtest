Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Value ("bottomLevel",Range(0, 1)) = 0
        _Bottom ("bottom", float) = 0
        _MainTex ("Texture", 2D) = "white" {}

         // 比较常见的属性类型
        // ————————————————————————————————————————————————
        //_Integer ("整数(新版)", Integer) = 1
        _Int ("整数(旧版)", Int) = 1
        _Float ("浮点数", Float) = 0.5
        _FloatRange ("浮点数滑动条", Range(0.0, 1.0)) = 0.5
        // Unity包含以下内置纹理, 可以直接填充
        // “white”（RGBA：1,1,1,1）
        // “black”（RGBA：0,0,0,1）
        // “gray”（RGBA：0.5,0.5,0.5,1）
        // “bump”（RGBA：0.5,0.5,1,0.5）
        // “red”（RGBA：1,0,0,1）
        _Texture2D ("2D纹理贴图", 2D) = "red" {}
        // 字符串留空或输入无效值，则它默认为 “gray”
        _DefaultTexture2D ("2D纹理贴图", 2D) = "" {}
        // 默认值为 “gray”（RGBA：0.5,0.5,0.5,1）
        _Texture3D ("3D纹理贴图", 3D) = "" {}
        _Cubemap ("立方体贴图", Cube) = "" {}
        // Inspector会显示四个单独的浮点数字段
        _Vector ("Example vector", Vector) = (0.25, 0.5, 0.5, 1)
        // Inspector会显示拾色器拾取色彩RGBA值
        _Color("色彩", Color) = (0.25, 0.5, 0.5, 1)
        // ————————————————————————————————————————————————
                                            
        // 除此之外 属性声明还可以具有一个可选特性 用来告知Unity如何处理它们
        // HDR可以使色彩亮度的值超过1
        [HDR]_HDRColor("HDR色彩", Color) = (1,1,1,1)
        // Inspector隐藏此属性
        [HideInInspector]_Hide("看不见我~", Color) = (1,1,1,1)
        // Inspector隐藏此纹理属性的Scale Offset字段
        [NoScaleOffset]_HideScaleOffset("隐藏ScaleOffset", 2D) = "" {}
        // 指示纹理属性为法线贴图，如果分配了不兼容的纹理，编辑器则会显示警告。
        [Normal]_Normal("法线贴图", 2D) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100 //决定渲染精细度

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
                float3 normal:NORMAL;//光照法线
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //法线和世界位置
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
                //裁剪转世界
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                //压y轴
                float y = worldPos.y - (worldPos.y - _Bottom) * _Value;
                //世界坐标
                float3 tempWorld = float3(worldPos.x, y, worldPos.z);
                //世界转裁剪
                o.vertex = UnityWorldToClipPos(tempWorld);
                
                //光线
                o.worldPos = worldPos;
                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                //方向
                float3 WorldLightDir = UnityWorldSpaceLightDir(i.worldPos);
                //能量大小
                float Nol = dot(i.worldNormal, WorldLightDir);//(-1,1)
                //half-lambert亮度值
                float halfLambert = Nol * 0.5 + 0.5;//(0,1)
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * halfLambert;
            }
            ENDCG
        }
    }
}
