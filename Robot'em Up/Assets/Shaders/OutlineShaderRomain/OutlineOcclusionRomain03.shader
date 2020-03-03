// Upgrade NOTE: replaced 'PositionFog()' with transforming position into clip space.
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'
// Upgrade NOTE: replaced 'glstate.matrix.invtrans.modelview[0]' with 'UNITY_MATRIX_IT_MV'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/OutlineOcclusionRomain03"
{
	Properties{
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Outline("Outline width", Range(.002, 0.03)) = .005
		_ToonShade("ToonShader Cubemap(RGB)", CUBE) = "white" { Texgen CubeNormal }
	}

		Category{
			#warning Upgrade NOTE: SubShader commented out; uses Unity 2.x per-pixel lighting. You should rewrite shader into a Surface Shader.
/*SubShader {
				Pass {
		// Fallback case for non-pixel-lit situations.
		Blend Off
		Name "BASE"
		Tags {"LightMode" = "VertexOrNone"}
		SetTexture[_MainTex] { constantColor[_Color] Combine texture * constant }
		SetTexture[_ToonShade] { combine texture * previous DOUBLE, previous }
	}
	Pass {
		Blend Off
		Name "PPL"
		Tags { "LightMode" = "Pixel" }
		ColorMask RGBA
		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members uvA,uvB)
#pragma exclude_renderers d3d11
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_builtin
		#pragma fragmentoption ARB_fog_exp2
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		struct v2f {
			float4 pos : SV_POSITION;
			LIGHTING_COORDS
			float2  uvA;
			float3  uvB;
		};

		uniform float4 _MainTex_ST;

		v2f vert(appdata_tan v) {
			v2f o;
			o.pos = UnityObjectToClipPos (v.vertex);
			o.uvA = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uvB = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
			TRANSFER_VERTEX_TO_FRAGMENT(o);
			return o;
		}

		uniform sampler2D _MainTex;
		uniform samplerCUBE _ToonShade;
		uniform float4 _Color;

		float4 frag(v2f i) : COLOR {
			half4 texcol = tex2D(_MainTex, i.uvA) * _Color;
			half4 lighting = LIGHT_ATTENUATION(i) * texCUBE(_ToonShade, i.uvB);

			return half4((texcol * lighting * 2).rgb, texcol.a);
		}
		ENDCG
	}
}*/
	}
		Fallback "VertexLit"
}
