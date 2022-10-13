Shader "Hidden/BlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            // 采样尺寸比例
            float4 _MainTex_TexelSize;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 col = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(-1, -1)).rgb * 1;
                col += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(0, -1)).rgb * 2;
                col += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(1, -1)).rgb * 1;
                col += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(-1, 0)).rgb * 2;
                col += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(0, 0)).rgb * 4;
                col += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(1, 0)).rgb * 2;
                col += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(-1, 1)).rgb * 1;
                col += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(0, 1)).rgb * 2;
                col += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * fixed2(1, 1)).rgb * 1;
                col = col / 16;

                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}