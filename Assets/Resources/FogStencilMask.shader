// Writes stencil 1 where the revealed area is, and nothing else. FogRenderer draws the fog wherever the stencil isn't 1.
// Lives in Resources so it's included in builds, since it's only found by name.
Shader "Hidden/Mini-Civ/FogStencilMask" {
    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass {
            ColorMask 0
            ZWrite Off
            ZTest Always
            Cull Off
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 vert (float4 vertex : POSITION) : SV_POSITION {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag () : SV_Target {
                return 0;
            }
            ENDCG
        }
    }
}
