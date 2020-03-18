//This version of the shader does not support shadows, but it does support transparent outlines

Shader "HighLight/HightlightObject"
{
	Properties
	{
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_FirstOutlineColor2("First outline Color", Color) = (0.5,0.5,0.5,1)
		_HighlightColor2("Highlight Color", Color) = (0.5,0.5,0.5,1)
		_SecondOutlineColor2("Second outline Color", Color) = (0.5,0.5,0.5,1)

		_MainTex("Texture", 2D) = "white" {}

		_FirstOutlineWidth2("Outlines width", Range(0.0, 2.0)) = 0.15

		_SecondOutlineWidth2("Outlines width", Range(0.0, 2.0)) = 0.025

		_Angle("Switch shader on angle", Range(0.0, 180.0)) = 89
	}

		CGINCLUDE
#include "UnityCG.cginc"

			struct appdata {
			float4 vertex : POSITION;
			float4 normal : NORMAL;
		};

		uniform float4 _FirstOutlineColor2;
		uniform float _FirstOutlineWidth2;
		uniform float4 _HighlightColor2;

		uniform float4 _SecondOutlineColor2;
		uniform float _SecondOutlineWidth2;

		float _MinEmissiveIntensity;
		float _MaxEmissiveIntensity;
		float _EmissiveLerpSpeed;
		float _EmissiveMultiplier;

		uniform sampler2D _MainTex;
		uniform float4 _Color;
		uniform float _Angle;

		ENDCG

			SubShader{
			//First outline
			Pass{
				Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off
				ZTest Always
				Cull Back
				CGPROGRAM

				struct v2f {
					float4 pos : SV_POSITION;
				};

				#pragma vertex vert
				#pragma fragment frag

				v2f vert(appdata v) {
					appdata original = v;

					float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
					//This shader consists of 2 ways of generating outline that are dynamically switched based on demiliter angle
					//If vertex normal is pointed away from object origin then custom outline generation is used (based on scaling along the origin-vertex vector)
					//Otherwise the old-school normal vector scaling is used
					//This way prevents weird artifacts from being created when using either of the methods
					if (degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) {
						v.vertex.xyz += normalize(v.normal.xyz) * _FirstOutlineWidth2;
					}
	else {
	   v.vertex.xyz += scaleDir * _FirstOutlineWidth2;
   }

   v2f o;
   o.pos = UnityObjectToClipPos(v.vertex);
   return o;
}

half4 frag(v2f i) : COLOR{
	return _FirstOutlineColor2;
}

ENDCG
}


//Second outline
Pass{
	Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	ZWrite Off
	ZTest Always
	Cull Back
	CGPROGRAM

	struct v2f {
		float4 pos : SV_POSITION;
	};

	#pragma vertex vert
	#pragma fragment frag

	v2f vert(appdata v) {
		appdata original = v;

		float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
		//This shader consists of 2 ways of generating outline that are dynamically switched based on demiliter angle
		//If vertex normal is pointed away from object origin then custom outline generation is used (based on scaling along the origin-vertex vector)
		//Otherwise the old-school normal vector scaling is used
		//This way prevents weird artifacts from being created when using either of the methods
		if (degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) {
			v.vertex.xyz += normalize(v.normal.xyz) * _SecondOutlineWidth2;
		}
	else {
		v.vertex.xyz += scaleDir * _SecondOutlineWidth2;
	}

	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	return o;
	}

	half4 frag(v2f i) : COLOR{
		return _SecondOutlineColor2;
	}

	ENDCG
}

//Surface shader
Tags{ "Queue" = "Transparent" }

CGPROGRAM
#pragma surface surf Lambert noshadow

struct Input {
	float2 uv_MainTex;
	float4 color : COLOR;
	float _intensityMultiplier;
};

void surf(Input IN, inout SurfaceOutput  o) {
	float _intensityMultiplier = (sin(_Time * _EmissiveLerpSpeed) + 1) / 2;
	float finalEmission = _MinEmissiveIntensity + ((_MaxEmissiveIntensity - _MinEmissiveIntensity) * _intensityMultiplier);
	finalEmission = lerp(1, finalEmission, _EmissiveMultiplier);
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _HighlightColor2;
	o.Albedo = c.rgb * finalEmission;
	o.Alpha = c.a;
}
ENDCG
		}
			Fallback "Diffuse"
}