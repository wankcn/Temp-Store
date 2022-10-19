Shader "Unlit/01MiniShader"
{
    Properties
    {

        // 默认贴图为白色
        _MainTex("MainTex",2D) = "white"{}
        // _参数名，显示文本，参数类型，默认值
        _Folat("Float",Float) = 0.0
        _Range("Range",Range(1,6)) = 0.0
        _Vector("Vector",Vector) = (1,1,1,1)
        _Color("Color",color) = (1,1,1,1)

    }

    Subshader
    {
        Pass
        {

            CGPROGRAM
            // 定义顶点shader和片元shader
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 拿数据
            struct appdata
            {
                // 模型顶点坐标
                float4 vertex:POSITION;
                // UV
                float2 uv:TEXCOORD0;
                // 法线
                // float4 normal:NORMAL;
                // // 顶点色
                // float4 color :COLOR;
            };

            // 输出结构体
            struct v2f
            {
                // 输出在裁剪空间下的顶点坐标
                float4 pos:SV_POSITION;
                float2 uv:TEXCOORD0; // 储存器 插值
            };

            float4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                // init输出数据
                v2f o;
                // float4 post_world = mul(unity_ObjectToWorld, v.vertex); // 模型空间转换为世界空间
                // float4 post_view = mul(UNITY_MATRIX_V, post_world);     // 世界空间转换为相机空间
                // float4 post_clip = mul(UNITY_MATRIX_P, post_view);      // 转到裁剪空间
                // o.pos = post_clip;
                // o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; // xy缩放，zw便宜
                return o;
            }


            float4 frag(v2f i):SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG

        }
    }
}