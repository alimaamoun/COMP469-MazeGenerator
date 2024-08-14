Shader "Custom/OutlinedShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline width", Range (.002, 0.03)) = .005
    }
    SubShader
    {
        Tags {"RenderType"="Opaque"}
        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        fixed4 _OutlineColor;
        float _Outline;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _OutlineColor;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG

        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front

            ZWrite On
            ZTest LEqual
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            uniform float _Outline;
            uniform float4 _OutlineColor;

            v2f vert(appdata_t v)
            {
                // just make a copy of incoming vertex data but scaled according to normal direction
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 norm = mul((float3x3) UNITY_MATRIX_IT_MV, v.normal);
                float2 offset = normalize(norm.xy) * _Outline;
                o.pos.xy += offset * o.pos.z * _Outline;
                o.color = _OutlineColor;
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                return i.color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
