// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ZoneOcclusionBorder"
{
	Properties
	{
		//Color of the sphere. Fadeout is an extra modifier for fade agressiveness.
			_Color("Color", Color) = (1,1,1,1)
			_Fadeout("Fadeout", Float) = 4
	}
		SubShader
	{
		//Enable transparency:
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

		//Only one pass needed:
			Pass
			{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				fixed4 _Color;
				half _Fadeout;

				//We need position and color by default.
				struct VertShaderOutput
				{
					float4 Position : SV_POSITION;
					float4 WorldPos: POSITION1;
					float3 Normal : NORMAL;
				};

				VertShaderOutput vert(appdata_base v)
				{
					VertShaderOutput o;
					o.Position = UnityObjectToClipPos(v.vertex);
					o.WorldPos = v.vertex;
					//Here we calculate intensity of the color. Closer to the edge, more intensity:
					o.Normal = v.normal;

					return o;

				}

				fixed4 frag(VertShaderOutput input) : SV_Target
				{
					float intensity;

					float3 direction = normalize(ObjSpaceViewDir(input.WorldPos));
					intensity = 1 / dot(direction, input.Normal);

					//Color by intensity
					float4 color = (_Color * intensity);
					color.a = intensity / _Fadeout;
					//Finally, return color modified by Color's Alpha (modified by script).
						return color * _Color.a;
					}

					ENDCG
				}
	}
		FallBack "Diffuse"
}
