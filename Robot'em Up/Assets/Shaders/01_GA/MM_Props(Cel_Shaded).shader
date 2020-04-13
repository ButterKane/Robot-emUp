// Upgrade NOTE: upgraded instancing buffer 'MM_PropsCel_Shaded' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MM_Props(Cel_Shaded)"
{
	Properties
	{
		_ColorR("Color R", Color) = (1,0.3286127,0,0)
		_Normal("Height", 2D) = "white" {}
		_ShadowColor("Shadow Color", Color) = (0,0,0,0)
		_SpecCoverage("Spec Coverage", Range( 0 , 1)) = 0.1
		_SpecCOlor("Spec COlor", Color) = (0.3113827,0.6792453,0.2146671,0)
		_SpecTransition("Spec Transition", Range( 0 , 1)) = 0
		_OffsetRamp("Offset Ramp", Float) = 1
		_qMap("qMap", 2D) = "white" {}
		_ColorG("Color G", Color) = (0.6482062,0.8773585,0,0)
		_ColorB("Color B", Color) = (0,0.3807745,1,0)
		_CombinedMask("CombinedMask", 2D) = "white" {}
		_AOColorR("AO Color (R)", Color) = (0.5660378,0.3284087,0.3284087,0)
		_GradientColorG("Gradient Color (G)", Color) = (0.5750571,0.5849056,0.4607511,0)
		_CurvatureColorB("Curvature Color (B)", Color) = (0.5566576,0.2963688,0.5660378,0)
		_NoiseGradient("Noise Gradient", 2D) = "white" {}
		_NoiseIntensity("Noise Intensity", Float) = 0.5
		_GradientHeight("Gradient Height", Float) = 0
		_GradientSharpness("Gradient Sharpness", Float) = 0
		_GrayMapContrast("GrayMap Contrast", Range( 1 , 50)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _CombinedMask;
		uniform sampler2D _NoiseGradient;
		uniform float4 _ShadowColor;
		uniform sampler2D _qMap;
		uniform sampler2D _Normal;
		uniform float _OffsetRamp;

		UNITY_INSTANCING_BUFFER_START(MM_PropsCel_Shaded)
			UNITY_DEFINE_INSTANCED_PROP(float4, _CombinedMask_ST)
#define _CombinedMask_ST_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float4, _ColorR)
#define _ColorR_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float4, _ColorG)
#define _ColorG_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float4, _ColorB)
#define _ColorB_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float4, _AOColorR)
#define _AOColorR_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float4, _GradientColorG)
#define _GradientColorG_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float4, _CurvatureColorB)
#define _CurvatureColorB_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float4, _NoiseGradient_ST)
#define _NoiseGradient_ST_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float4, _SpecCOlor)
#define _SpecCOlor_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float, _GrayMapContrast)
#define _GrayMapContrast_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float, _GradientHeight)
#define _GradientHeight_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float, _GradientSharpness)
#define _GradientSharpness_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float, _NoiseIntensity)
#define _NoiseIntensity_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float, _SpecCoverage)
#define _SpecCoverage_arr MM_PropsCel_Shaded
			UNITY_DEFINE_INSTANCED_PROP(float, _SpecTransition)
#define _SpecTransition_arr MM_PropsCel_Shaded
		UNITY_INSTANCING_BUFFER_END(MM_PropsCel_Shaded)

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float4 _CurvatureColorB_Instance = UNITY_ACCESS_INSTANCED_PROP(_CurvatureColorB_arr, _CurvatureColorB);
			float4 _NoiseGradient_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_NoiseGradient_ST_arr, _NoiseGradient_ST);
			float2 uv_NoiseGradient = i.uv_texcoord * _NoiseGradient_ST_Instance.xy + _NoiseGradient_ST_Instance.zw;
			float _NoiseIntensity_Instance = UNITY_ACCESS_INSTANCED_PROP(_NoiseIntensity_arr, _NoiseIntensity);
			float4 temp_output_87_0_g1 = ( _CurvatureColorB_Instance * ( tex2D( _NoiseGradient, uv_NoiseGradient ) * _NoiseIntensity_Instance ) );
			float2 temp_output_2_0_g2 = temp_output_87_0_g1.rg;
			float2 break6_g2 = temp_output_2_0_g2;
			float temp_output_25_0_g2 = ( pow( 0.0 , 3.0 ) * 0.1 );
			float2 appendResult8_g2 = (float2(( break6_g2.x + temp_output_25_0_g2 ) , break6_g2.y));
			float4 tex2DNode14_g2 = tex2D( _Normal, temp_output_2_0_g2 );
			float temp_output_4_0_g2 = 5.0;
			float3 appendResult13_g2 = (float3(1.0 , 0.0 , ( ( tex2D( _Normal, appendResult8_g2 ).g - tex2DNode14_g2.g ) * temp_output_4_0_g2 )));
			float2 appendResult9_g2 = (float2(break6_g2.x , ( break6_g2.y + temp_output_25_0_g2 )));
			float3 appendResult16_g2 = (float3(0.0 , 1.0 , ( ( tex2D( _Normal, appendResult9_g2 ).g - tex2DNode14_g2.g ) * temp_output_4_0_g2 )));
			float3 normalizeResult22_g2 = normalize( cross( appendResult13_g2 , appendResult16_g2 ) );
			float3 temp_output_16_0_g1 = normalizeResult22_g2;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult8_g1 = dot( normalize( (WorldNormalVector( i , temp_output_16_0_g1 )) ) , ase_worldlightDir );
			float NormalLightdir9_g1 = dotResult8_g1;
			float2 temp_cast_2 = ((NormalLightdir9_g1*_OffsetRamp + _OffsetRamp)).xx;
			float4 shawdow66_g1 = ( temp_output_87_0_g1 * _ShadowColor * tex2D( _qMap, temp_cast_2 ) );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			UnityGI gi88_g1 = gi;
			float3 diffNorm88_g1 = ase_worldNormal;
			gi88_g1 = UnityGI_Base( data, 1, diffNorm88_g1 );
			float3 indirectDiffuse88_g1 = gi88_g1.indirect.diffuse + diffNorm88_g1 * 0.0001;
			float4 lighting57_g1 = ( shawdow66_g1 * ( ase_lightColor * float4( ( indirectDiffuse88_g1 + ase_lightAtten ) , 0.0 ) ) );
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult48_g1 = dot( ( float4( ase_worldViewDir , 0.0 ) + _WorldSpaceLightPos0 ) , float4( normalize( (WorldNormalVector( i , temp_output_16_0_g1 )) ) , 0.0 ) );
			float _SpecCoverage_Instance = UNITY_ACCESS_INSTANCED_PROP(_SpecCoverage_arr, _SpecCoverage);
			float smoothstepResult33_g1 = smoothstep( 1.0 , 1.12 , pow( dotResult48_g1 , _SpecCoverage_Instance ));
			float4 _SpecCOlor_Instance = UNITY_ACCESS_INSTANCED_PROP(_SpecCOlor_arr, _SpecCOlor);
			float _SpecTransition_Instance = UNITY_ACCESS_INSTANCED_PROP(_SpecTransition_arr, _SpecTransition);
			float4 lerpResult37_g1 = lerp( _SpecCOlor_Instance , ase_lightColor , _SpecTransition_Instance);
			float4 Spec42_g1 = ( ( ( smoothstepResult33_g1 * lerpResult37_g1 ) * 1.0 ) * ase_lightAtten );
			float4 temp_cast_6 = (0.0).xxxx;
			c.rgb = ( ( lighting57_g1 + Spec42_g1 ) - temp_cast_6 ).rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			float4 _CombinedMask_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_CombinedMask_ST_arr, _CombinedMask_ST);
			float2 uv_CombinedMask = i.uv_texcoord * _CombinedMask_ST_Instance.xy + _CombinedMask_ST_Instance.zw;
			float4 tex2DNode63 = tex2D( _CombinedMask, uv_CombinedMask );
			float _GrayMapContrast_Instance = UNITY_ACCESS_INSTANCED_PROP(_GrayMapContrast_arr, _GrayMapContrast);
			float4 _ColorR_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorR_arr, _ColorR);
			float4 lerpResult55 = lerp( float4( 0,0,0,0 ) , _ColorR_Instance , i.vertexColor.r);
			float4 _ColorG_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorG_arr, _ColorG);
			float4 lerpResult56 = lerp( lerpResult55 , _ColorG_Instance , i.vertexColor.g);
			float4 _ColorB_Instance = UNITY_ACCESS_INSTANCED_PROP(_ColorB_arr, _ColorB);
			float4 lerpResult50 = lerp( lerpResult56 , _ColorB_Instance , i.vertexColor.b);
			float4 _AOColorR_Instance = UNITY_ACCESS_INSTANCED_PROP(_AOColorR_arr, _AOColorR);
			float4 lerpResult71 = lerp( ( saturate( pow( tex2DNode63.a , _GrayMapContrast_Instance ) ) * lerpResult50 ) , _AOColorR_Instance , tex2DNode63.r);
			float4 _GradientColorG_Instance = UNITY_ACCESS_INSTANCED_PROP(_GradientColorG_arr, _GradientColorG);
			float _GradientHeight_Instance = UNITY_ACCESS_INSTANCED_PROP(_GradientHeight_arr, _GradientHeight);
			float _GradientSharpness_Instance = UNITY_ACCESS_INSTANCED_PROP(_GradientSharpness_arr, _GradientSharpness);
			float4 lerpResult73 = lerp( lerpResult71 , _GradientColorG_Instance , saturate( ( ( tex2DNode63.g - _GradientHeight_Instance ) / _GradientSharpness_Instance ) ));
			float4 _CurvatureColorB_Instance = UNITY_ACCESS_INSTANCED_PROP(_CurvatureColorB_arr, _CurvatureColorB);
			float4 lerpResult61 = lerp( lerpResult73 , _CurvatureColorB_Instance , tex2DNode63.b);
			o.Albedo = lerpResult61.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
199;241;2080;1048;2070.052;1024.663;1;True;True
Node;AmplifyShaderEditor.ColorNode;53;-3321.24,-1692.951;Inherit;False;InstancedProperty;_ColorR;Color R;0;0;Create;True;0;0;False;0;1,0.3286127,0,0;0.8045123,0.9528302,0.9419044,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;51;-3282.4,-1876.743;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;54;-2990.24,-1996.951;Inherit;False;InstancedProperty;_ColorG;Color G;10;0;Create;True;0;0;False;0;0.6482062,0.8773585,0,0;0.5176471,0.5647059,0.7254902,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;63;-3121.469,-1146.953;Inherit;True;Property;_CombinedMask;CombinedMask;12;0;Create;True;0;0;False;0;-1;482441b81c37dce48937c4e23fd1c0e0;482441b81c37dce48937c4e23fd1c0e0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;88;-2989.617,-1271.052;Inherit;False;InstancedProperty;_GrayMapContrast;GrayMap Contrast;33;0;Create;True;0;0;False;0;0;1.5;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;55;-2975.881,-1736.143;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-2534.752,-1001.734;Inherit;False;InstancedProperty;_GradientHeight;Gradient Height;31;0;Create;True;0;0;False;0;0;0.08;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;52;-3135.455,-1533.193;Inherit;False;InstancedProperty;_ColorB;Color B;11;0;Create;True;0;0;False;0;0,0.3807745,1,0;0.06728373,0.5283019,0.4409751,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;86;-2733.218,-1283.138;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;56;-2772.146,-1875.873;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;50;-2527.408,-1765.672;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-2243.29,-962.045;Inherit;False;InstancedProperty;_GradientSharpness;Gradient Sharpness;32;0;Create;True;0;0;False;0;0;0.65;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;80;-2216.571,-1121.546;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;87;-2549.01,-1341.122;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;77;-2569.77,-376.9625;Inherit;True;Property;_NoiseGradient;Noise Gradient;16;0;Create;True;0;0;False;0;-1;None;5459e43c5c725294fbf0881986364aa9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;81;-2019.669,-1043.292;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;72;-2307.981,-1442.687;Inherit;False;InstancedProperty;_AOColorR;AO Color (R);13;0;Create;True;0;0;False;0;0.5660378,0.3284087,0.3284087,0;0.8584906,0.4116866,0.2875134,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-2290.823,-1546.869;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-2288.075,-150.5104;Inherit;False;InstancedProperty;_NoiseIntensity;Noise Intensity;17;0;Create;True;0;0;False;0;0.5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;83;-1835.65,-1092.683;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;68;-2391.586,-858.6126;Inherit;False;InstancedProperty;_GradientColorG;Gradient Color (G);14;0;Create;True;0;0;False;0;0.5750571,0.5849056,0.4607511,0;0.8588236,0.4117647,0.2862745,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;71;-1681.308,-1459.871;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-2103.33,-382.4293;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;67;-2614.055,-663.8386;Inherit;False;InstancedProperty;_CurvatureColorB;Curvature Color (B);15;0;Create;True;0;0;False;0;0.5566576,0.2963688,0.5660378,0;0.5176471,0.5647059,0.7254902,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-1494.302,-457.3785;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;44;-5200.668,32.77019;Inherit;False;2350.157;623.9853;Noise and Color // Road Marking;16;2;6;3;5;42;41;27;24;4;8;23;25;21;22;28;17;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;46;-5097.776,-907.326;Inherit;False;2082.506;864.4288; Noise and Color // Concrete;15;38;35;33;18;40;37;34;36;31;19;11;13;12;10;14;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;73;-1557.505,-1121.007;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-4329.691,-602.9169;Inherit;False;InstancedProperty;_NoiseFloorSharpness;Noise Floor Sharpness;28;0;Create;True;0;0;False;0;1;2.23;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;3;-4954.668,330.8829;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-4569.882,111.8978;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;5;-4675.642,342.4126;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;17;-3455.678,104.6978;Inherit;False;Property;_MarkingCOlor;Marking COlor;23;0;Create;True;0;0;False;0;0.745283,0.6245149,0,0;0.5471698,0.4578948,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-4202.971,313.8829;Inherit;True;Property;_NoiseMark;NoiseMark;18;0;Create;True;0;0;False;0;-1;a8d2541ae14a15e42b3830ac3a68a0d8;c43b86301d5c2a946b7e575dc1c9a46e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-4272.938,-446.1572;Inherit;False;InstancedProperty;_FlorCOlor;Flor COlor;24;0;Create;True;0;0;False;0;0.509434,0.509434,0.509434,0;0.6792453,0.6792453,0.6792453,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;11;-4820.542,-781.5496;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-3736.251,388.908;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-4012.486,541.7555;Inherit;False;InstancedProperty;_NoiseMarkInt;Noise Mark Int;25;0;Create;True;0;0;False;0;0;2.33;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;-4161.513,82.77019;Inherit;True;Property;_Marktexture;Mark texture;22;0;Create;True;0;0;False;0;-1;1c089cfa53f963a4ebbc0a512b6d8443;3ec8dcd850907854a92c1ed810903cb5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;13;-4550.514,-777.0205;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;41;-4423.492,388.8794;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0.25;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;37;-3675.69,-225.4886;Inherit;True;Property;_51612536_553196668424338_6330084407579246592_n;51612536_553196668424338_6330084407579246592_n;29;0;Create;True;0;0;False;0;-1;b0198540b2f723b499ae6441d44aa369;b0198540b2f723b499ae6441d44aa369;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;21;-3537.513,322.7703;Inherit;False;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;61;-1337.522,-855.8241;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;22;-3200.441,322.7703;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-4873.642,482.4122;Inherit;False;InstancedProperty;_NoiuseMarkingTiling;Noiuse Marking Tiling;19;0;Create;True;0;0;False;0;1;42.93;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-3470.147,-715.8705;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;49;-2801.689,55.5244;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;35;-3916.692,-809.9175;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;34;-4054.247,-778.2175;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;28;-3057.607,320.6302;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;38;-3179.875,-507.0391;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;89;-1182.471,-483.6389;Inherit;False;MF_CellShading_CustomLighting;1;;1;a1954d31f308f184bbc7281f8cf9b30e;0;2;87;COLOR;0,0,0,0;False;86;FLOAT;0;False;1;COLOR;80
Node;AmplifyShaderEditor.RangedFloatNode;12;-4748.514,-637.0201;Inherit;False;InstancedProperty;_NoiseFloorTiling;Noise Floor Tiling;21;0;Create;True;0;0;False;0;1;45.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-4413.813,-807.3865;Inherit;True;Property;_NoiseFFloor;NoiseFFloor;20;0;Create;True;0;0;False;0;-1;a8d2541ae14a15e42b3830ac3a68a0d8;c43b86301d5c2a946b7e575dc1c9a46e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;31;-4283.076,-527.6848;Inherit;False;InstancedProperty;_noiseFloorint;noise Floor int;27;0;Create;True;0;0;False;0;0;0.82;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;10;-5025.542,-776.5496;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;2;-5149.632,342.883;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;33;-3745.455,-719.0615;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;40;-3681.51,-409.0626;Inherit;False;Property;_DetailMaskCOlor;Detail Mask COlor;30;0;Create;True;0;0;False;0;0,0,0,0;0.245283,0.245283,0.245283,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;42;-4546.456,531.8793;Inherit;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-3489.513,434.7702;Inherit;False;InstancedProperty;_NoiseMarkSharpness;Noise Mark Sharpness;26;0;Create;True;0;0;False;0;1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-589.048,-803.3093;Float;False;True;2;ASEMaterialInspector;0;0;CustomLighting;MM_Props(Cel_Shaded);False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;55;1;53;0
WireConnection;55;2;51;1
WireConnection;86;0;63;4
WireConnection;86;1;88;0
WireConnection;56;0;55;0
WireConnection;56;1;54;0
WireConnection;56;2;51;2
WireConnection;50;0;56;0
WireConnection;50;1;52;0
WireConnection;50;2;51;3
WireConnection;80;0;63;2
WireConnection;80;1;84;0
WireConnection;87;0;86;0
WireConnection;81;0;80;0
WireConnection;81;1;85;0
WireConnection;74;0;87;0
WireConnection;74;1;50;0
WireConnection;83;0;81;0
WireConnection;71;0;74;0
WireConnection;71;1;72;0
WireConnection;71;2;63;1
WireConnection;78;0;77;0
WireConnection;78;1;79;0
WireConnection;76;0;67;0
WireConnection;76;1;78;0
WireConnection;73;0;71;0
WireConnection;73;1;68;0
WireConnection;73;2;83;0
WireConnection;3;0;2;0
WireConnection;5;0;3;0
WireConnection;5;1;6;0
WireConnection;4;1;41;0
WireConnection;11;0;10;0
WireConnection;23;0;4;0
WireConnection;23;1;24;0
WireConnection;8;1;27;0
WireConnection;13;0;11;0
WireConnection;13;1;12;0
WireConnection;41;0;5;0
WireConnection;41;2;42;0
WireConnection;21;0;8;1
WireConnection;21;1;23;0
WireConnection;61;0;73;0
WireConnection;61;1;67;0
WireConnection;61;2;63;3
WireConnection;22;0;21;0
WireConnection;22;1;25;0
WireConnection;18;0;33;0
WireConnection;18;1;19;0
WireConnection;49;0;38;0
WireConnection;49;1;17;0
WireConnection;49;2;28;0
WireConnection;35;0;34;0
WireConnection;34;0;14;0
WireConnection;34;1;36;0
WireConnection;28;0;22;0
WireConnection;38;0;18;0
WireConnection;38;1;40;0
WireConnection;38;2;37;1
WireConnection;89;87;76;0
WireConnection;14;1;13;0
WireConnection;33;0;35;0
WireConnection;33;1;19;0
WireConnection;33;2;31;0
WireConnection;0;0;61;0
WireConnection;0;13;89;80
ASEEND*/
//CHKSM=4D2C2943EBE4D4FC1E7007E0E44FF501B2FE5B10