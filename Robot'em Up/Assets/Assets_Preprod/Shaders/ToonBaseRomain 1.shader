Shader "RomainBaseShader"
{
	Properties
	{
		[Header(Main)]
		_MainTex("Main Texture Albedo", 2D) = "white" {}
		_BumpMap("Normal Texture", 2D) = "white" {}
		_NormalValue("Normal Value", Float) = 0

		[Space]

		_AMR("AO + Rougness + Emit", 2D) = "white" {}

		[Header(ColorVariation)]
		_Color("Tint Color", Color) = (1,1,1,1)
		_SaturationValue("Saturation Value", Float) = 0
		[HDR]
		_EmitColor("Emition Color", Color) = (1,1,1,1)

		[Header(Shadow)]
	    _AmbientColor("Shadow Color", Color) = (0.4,0.4,0.4,1)
		_CelMin("ShadowMin", Float) = 0
		_CelMax("ShadowMax", Float) = 0.01

		[Header(Specular)]
		[HDR]
		_SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
		_Glossiness("Glossiness", Float) = 32

		[Header(RimOutline)]
		[HDR]
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
		[Space]
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
		_OutlineAmount("Outline Amount", Range(0, 1)) = 0.716

		[Header(Extra)]
		_AOValue("AO Value", Float) = 0.01
		_RougnessValue("Rougness Value", Float) = 0.01
	}
	SubShader
	{
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
				//"PassFlags" = "OnlyDirectional"
			}
			Lighting On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile_shadowcaster
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;				
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float3 normal : NORMAL1;
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD1;	
				SHADOW_COORDS(2)
				half3 tspace0 : TEXCOORD3; // tangent.x, bitangent.x, normal.x
				half3 tspace1 : TEXCOORD4; // tangent.y, bitangent.y, normal.y
				half3 tspace2 : TEXCOORD5; // tangent.z, bitangent.z, normal.z
				UNITY_FOG_COORDS(6)
				half4 worldPos : TEXCOORD7;
				half4 attenUV : TEXCOORD8;
			};

			sampler2D _MainTex, _BumpMap, _AMR;
			float4 _MainTex_ST, _BumpMap_ST, _AMR_ST;
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);		
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.normal = v.normal;
				
				half3 wNormal = UnityObjectToWorldNormal(v.normal);

				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
				// compute bitangent from cross product of normal and tangent
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 wBitangent = cross(wNormal, wTangent) * tangentSign;


				o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
				o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
				o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
				
				TRANSFER_SHADOW(o)
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}
			
			float4 _Color;

			float4 _AmbientColor;

			float4 _SpecularColor, _EmitColor;
			float _Glossiness, _NormalValue, _AOValue, _RougnessValue;

			float4 _RimColor, _OutlineColor;
			float _RimAmount, _OutlineAmount, _CelMin, _CelMax;
			float _RimThreshold, _OutlineThreshold, _SaturationValue;

			float attenUV(float lightAtten0, float3 _4LightPos, float3 _worldPos) : SV_Target{
				float range = (0.005 * sqrt(1000000 - lightAtten0)) / sqrt(lightAtten0);
				return distance(_4LightPos, _worldPos) / range;
			}

			float atten(float _attenUV) : SV_Target{
				return saturate(1.0 / (5.0 + 25.0*_attenUV*_attenUV) * saturate((1 - _attenUV) * 5.0));
			}

			float4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);

				// _WorldSpaceLightPos0 is a vector pointing the OPPOSITE
				// direction of the main directional light.
				float NdotL = dot(_WorldSpaceLightPos0, normal);

				// Samples the shadow map, returning a value in the 0...1 range,
				// where 0 is in the shadow, and 1 is not.
				float shadow = SHADOW_ATTENUATION(i);
				// Partition the intensity into light and dark, smoothly interpolated
				// between the two to avoid a jagged break.
			//	float lightIntensity = smoothstep(0, 0.01, NdotL * (shadow));

				// NORMAL /////////////////////
				half2 uv_BumpMap = TRANSFORM_TEX(i.uv, _BumpMap);
				half uv_AO = tex2D(_AMR,i.uv).r;
				half uv_SpecRough = tex2D(_AMR,i.uv).g;
				half uv_Emissive = tex2D(_AMR,i.uv).b;

				half3 tnormal = UnpackNormal(tex2D(_BumpMap, uv_BumpMap));
				// transform normal from tangent to world space
				half3 worldNormal;
				worldNormal.x = dot(i.tspace0, tnormal);
				worldNormal.y = dot(i.tspace1, tnormal);
				worldNormal.z = dot(i.tspace2, tnormal);

				half NormalDiffuse = saturate(lerp(1, dot(_WorldSpaceLightPos0.xyz - worldNormal * 0.1, worldNormal), _NormalValue));
				// NORMAL /////////////////////

				float lightIntensity = smoothstep(_CelMin, _CelMax, NdotL*(shadow ))*NormalDiffuse;    // calculate shadow shape
				
				// Multiply by the main directional light's intensity and color.
				float4 light = lightIntensity * _LightColor0 + lerp(_AmbientColor, _LightColor0, lightIntensity);// calculate shadow color

				// Point Light magic calculation /////////////////////////
				float4 _atten;
				_atten.x = atten(i.attenUV.x);
				_atten.y = atten(i.attenUV.y);
				_atten.z = atten(i.attenUV.z);
				_atten.w = atten(i.attenUV.w);

				light.rgb += unity_LightColor[0].rgb * (0.5 / distance(float3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x), i.worldPos.xyz)) * _atten.x;
				light.rgb += unity_LightColor[1].rgb * (0.5 / distance(float3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y), i.worldPos.xyz)) * _atten.y;
				light.rgb += unity_LightColor[2].rgb * (0.5 / distance(float3(unity_4LightPosX0.z, unity_4LightPosY0.z, unity_4LightPosZ0.z), i.worldPos.xyz)) * _atten.z;
				light.rgb += unity_LightColor[3].rgb * (0.5 / distance(float3(unity_4LightPosX0.w, unity_4LightPosY0.w, unity_4LightPosZ0.w), i.worldPos.xyz)) * _atten.w;
				// Point Light magic calculation /////////////////////////


				// Calculate specular reflection.
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector);
				// Multiply _Glossiness by itself to allow artist to use smaller
				// glossiness values in the inspector.
				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				uv_SpecRough = lerp(1, uv_SpecRough, _RougnessValue);
				float4 specular = specularIntensitySmooth * _SpecularColor * (uv_SpecRough*uv_SpecRough*5);
				
				// Calculate rim lighting.
				float rimDot = 1 - dot(viewDir, normal);
				// We only want rim to appear on the lit side of the surface,
				// so multiply it by NdotL, raised to a power to smoothly blend it.
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColor;

				float outlineIntensity = rimDot;
				outlineIntensity = smoothstep(_OutlineAmount - 0.01, _OutlineAmount + 0.01, outlineIntensity)*_OutlineColor;

				float4 sample = lerp(lerp(dot(tex2D(_MainTex, i.uv),float3(0.299, 0.587, 0.114)), tex2D(_MainTex, i.uv),_SaturationValue), _RimColor,rim);
				sample = lerp(light, sample, saturate(lightIntensity+0.4));

				float4 FinalColor = (light + (unity_AmbientSky * unity_AmbientEquator *unity_AmbientGround) + specular + rim) * _Color * sample * saturate(1 - outlineIntensity) *(pow(uv_AO, _AOValue)) * (1+ uv_Emissive*_EmitColor);
				UNITY_APPLY_FOG(i.fogCoord, FinalColor); // Apply fog to the rgb
				return FinalColor;
			}
			ENDCG
		}
		// Shadow casting support.
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}