Shader "Custom/OutlineSingleLine"
{
	Properties
	{
		_Color("Main Color", Color) = (0.1,0.7,0.5,0)
		_MainTex("Texture", 2D) = "white" {}
		_OutlineColor("Outline color", Color) = (0,0,0,1)
		_OutlineWidth("Outlines width", Range(0.0, 2.0)) = 1.1
	}

		CGINCLUDE
#include "UnityCG.cginc"

			struct appdata
		{
			float4 vertex : POSITION;
		};

		struct v2f
		{
			float4 pos : POSITION;
		};

		uniform float _OutlineWidth;
		uniform float4 _OutlineColor;
		uniform sampler2D _MainTex;
		uniform float4 _Color;

		ENDCG

			SubShader
		{
			Tags{ "Queue" = "Transparent" }

			Pass //Outline
			{
				ZWrite Off
				ZTest Greater
				Cull Back
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				v2f vert(appdata v)
				{
					appdata original = v;
					v.vertex.xyz += _OutlineWidth * normalize(v.vertex.xyz);

					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					return o;

				}

				half4 frag(v2f i) : COLOR
				{
					return _OutlineColor;
				}

				ENDCG
			}

			Pass //Main Color (supposed to appear transparent)
					// ACCCHUALLY You have to make it so this part takes the front color and reproduces it.
					// front color is what is capted with the depth Buffer (ZTest). ZTest LEqual means that i something is found closer to camera than the rendered shader, the shader isn't rendered. 
			{
				ZWrite Off
				ZTest LEqual

					Cull Off
					CGPROGRAM

					#pragma vertex vert
					#pragma fragment frag

					v2f vert(appdata v)
					{
						appdata original = v;
						v.vertex.xyz -= normalize(v.vertex.xyz) * 0.05;

						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);
						return o;

					}

					half4 frag(v2f i) : COLOR
					{
						return _Color;
					}

					ENDCG
				}

			Tags{ "Queue" = "Geometry"}

			CGPROGRAM
			#pragma surface surf Lambert

			struct Input {
				float2 uv_MainTex;
			};

			void surf(Input IN, inout SurfaceOutput o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Alpha = 0;
			}
			ENDCG
		}
			Fallback "Diffuse"
}
