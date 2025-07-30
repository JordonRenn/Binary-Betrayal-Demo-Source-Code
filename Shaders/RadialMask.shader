Shader "UI/RadialMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Angle ("Angle", Range(0, 360)) = 30
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" } // Ensure it renders before other UI elements
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil {
                Ref 1
                Comp Always
                Pass Replace
                ZFail Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Angle;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.uv);
                float2 uv = i.uv - 0.5;

                // Rotate UV coordinates by 90 degrees clockwise
                float temp = uv.x;
                uv.x = uv.y;
                uv.y = -temp;

                float angle = atan2(uv.y, uv.x) * 57.2958; // Convert radians to degrees
                if (angle < 0) angle += 360;
                if (angle > _Angle) discard;
                return color;
            }
            ENDCG
        }
    }
}
