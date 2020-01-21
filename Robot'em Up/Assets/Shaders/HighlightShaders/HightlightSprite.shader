// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Highlight/HighlightSprite"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_EmissiveOffset("EmissiveOffset", Range(0,1)) = 0
		_EmissiveWidth("EmissiveWidth", Range(0,1)) = 0
		_EmissiveIntensity("EmissiveIntensity", Range(0, 10)) = 0
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
		{
			Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest Always
			Blend One OneMinusSrcAlpha

			Pass
		{
			CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma multi_compile _ PIXELSNAP_ON
	#include "UnityCG.cginc"
		float _EmissiveIntensity;
		sampler2D _MainTex;
		sampler2D _AlphaTex;
		float _AlphaSplitEnabled;
		float _EmissiveOffset;
		float _EmissiveWidth;
		float4 _HighlightColor;
		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord  : TEXCOORD0;
		};

		fixed4 _Color;

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			OUT.vertex = UnityObjectToClipPos(IN.vertex);
			OUT.texcoord = IN.texcoord;
			OUT.color = IN.color * _HighlightColor;
	#ifdef PIXELSNAP_ON
			OUT.vertex = UnityPixelSnap(OUT.vertex);
	#endif

			return OUT;
		}

		fixed4 SampleSpriteTexture(float2 uv)
		{
			fixed4 color = tex2D(_MainTex, uv);

	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
			if (_AlphaSplitEnabled)
				color.a = tex2D(_AlphaTex, uv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED




			float lowLevel = _EmissiveOffset - _EmissiveWidth;
			float highLevel = _EmissiveOffset + _EmissiveWidth;
			float currentDistanceProjection = (uv.x + uv.y) / 2;
			if (currentDistanceProjection > lowLevel && currentDistanceProjection < highLevel) {
				float whitePower = 1 - (abs(currentDistanceProjection - _EmissiveOffset) / _EmissiveWidth);
				color.rgb += color.a * whitePower * _EmissiveIntensity;
			}

			return color;
		}

		fixed4 frag(v2f IN) : SV_Target
		{
			fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
			c.rgb *= c.a;

		return c;
		}
			ENDCG
		}
		}
}