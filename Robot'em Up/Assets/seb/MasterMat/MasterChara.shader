// Upgrade NOTE: upgraded instancing buffer 'MasterChara_Toon' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MasterChara_Toon"
{
	Properties
	{
		_ASEOutlineColor( "Outline Color", Color ) = (0.1132075,0.09131362,0.09131362,0)
		_ASEOutlineWidth( "Outline Width", Float ) = 0.04
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Normal("Height", 2D) = "white" {}
		_MetalTint01("Metal Tint 01", Color) = (0,0,0,0)
		_RustColor("Rust Color", Color) = (0,0,0,0)
		_DirtColor("Dirt Color", Color) = (0,0,0,0)
		_MetalTint02("Metal Tint 02", Color) = (0,0,0,0)
		_MetalTint03("Metal Tint 03", Color) = (0,0,0,0)
		[HDR]_EmissiveCOlor("Emissive COlor", Color) = (6.09044,0.5189164,0,0)
		_Rimpower("Rim power", Range( 0 , 2)) = 0
		_RimOffset("Rim Offset", Float) = 1
		_RimColor("Rim Color", Color) = (0,0,0,0)
		_ShadowColor("Shadow Color", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		#pragma multi_compile_instancing
		UNITY_INSTANCING_BUFFER_START(MasterChara_Toon)
			UNITY_DEFINE_INSTANCED_PROP( half4, _ASEOutlineColor )
#define _ASEOutlineColor_arr MasterChara_Toon
			UNITY_DEFINE_INSTANCED_PROP(half, _ASEOutlineWidth)
#define _ASEOutlineWidth_arr MasterChara_Toon
		UNITY_INSTANCING_BUFFER_END(MasterChara_Toon)
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz *= ( 1 + UNITY_ACCESS_INSTANCED_PROP( _ASEOutlineWidth_arr, _ASEOutlineWidth ));
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o ) { o.Emission = UNITY_ACCESS_INSTANCED_PROP( _ASEOutlineColor_arr, _ASEOutlineColor ).rgb; o.Alpha = 1; }
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
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
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
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

		uniform sampler2D _TextureSample0;
		uniform sampler2D _Normal;
		uniform float4 _ShadowColor;
		uniform float _RimOffset;
		uniform float _Rimpower;
		uniform float4 _RimColor;

		UNITY_INSTANCING_BUFFER_START(MasterChara_Toon)
			UNITY_DEFINE_INSTANCED_PROP(float4, _MetalTint01)
#define _MetalTint01_arr MasterChara_Toon
			UNITY_DEFINE_INSTANCED_PROP(float4, _MetalTint02)
#define _MetalTint02_arr MasterChara_Toon
			UNITY_DEFINE_INSTANCED_PROP(float4, _MetalTint03)
#define _MetalTint03_arr MasterChara_Toon
			UNITY_DEFINE_INSTANCED_PROP(float4, _TextureSample0_ST)
#define _TextureSample0_ST_arr MasterChara_Toon
			UNITY_DEFINE_INSTANCED_PROP(float4, _RustColor)
#define _RustColor_arr MasterChara_Toon
			UNITY_DEFINE_INSTANCED_PROP(float4, _DirtColor)
#define _DirtColor_arr MasterChara_Toon
			UNITY_DEFINE_INSTANCED_PROP(float4, _EmissiveCOlor)
#define _EmissiveCOlor_arr MasterChara_Toon
		UNITY_INSTANCING_BUFFER_END(MasterChara_Toon)

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
			float4 _MetalTint01_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint01_arr, _MetalTint01);
			float4 lerpResult22 = lerp( float4( 0,0,0,0 ) , _MetalTint01_Instance , i.vertexColor.r);
			float4 _MetalTint02_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint02_arr, _MetalTint02);
			float4 lerpResult24 = lerp( lerpResult22 , _MetalTint02_Instance , i.vertexColor.g);
			float4 _MetalTint03_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint03_arr, _MetalTint03);
			float4 lerpResult27 = lerp( lerpResult24 , _MetalTint03_Instance , i.vertexColor.b);
			float4 _TextureSample0_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_TextureSample0_ST_arr, _TextureSample0_ST);
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST_Instance.xy + _TextureSample0_ST_Instance.zw;
			float4 tex2DNode1 = tex2D( _TextureSample0, uv_TextureSample0 );
			float4 _RustColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_RustColor_arr, _RustColor);
			float4 lerpResult7 = lerp( ( lerpResult27 + tex2DNode1.r ) , _RustColor_Instance , tex2DNode1.b);
			float4 _DirtColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_DirtColor_arr, _DirtColor);
			float4 lerpResult9 = lerp( lerpResult7 , _DirtColor_Instance , tex2DNode1.g);
			float4 Albedo61 = lerpResult9;
			float2 temp_output_2_0_g1 = Albedo61.rg;
			float2 break6_g1 = temp_output_2_0_g1;
			float temp_output_25_0_g1 = ( pow( 1.0 , 3.0 ) * 0.1 );
			float2 appendResult8_g1 = (float2(( break6_g1.x + temp_output_25_0_g1 ) , break6_g1.y));
			float4 tex2DNode14_g1 = tex2D( _Normal, temp_output_2_0_g1 );
			float temp_output_4_0_g1 = 2.0;
			float3 appendResult13_g1 = (float3(1.0 , 0.0 , ( ( tex2D( _Normal, appendResult8_g1 ).g - tex2DNode14_g1.g ) * temp_output_4_0_g1 )));
			float2 appendResult9_g1 = (float2(break6_g1.x , ( break6_g1.y + temp_output_25_0_g1 )));
			float3 appendResult16_g1 = (float3(0.0 , 1.0 , ( ( tex2D( _Normal, appendResult9_g1 ).g - tex2DNode14_g1.g ) * temp_output_4_0_g1 )));
			float3 normalizeResult22_g1 = normalize( cross( appendResult13_g1 , appendResult16_g1 ) );
			float3 temp_output_78_0 = normalizeResult22_g1;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult40 = dot( normalize( (WorldNormalVector( i , temp_output_78_0 )) ) , ase_worldlightDir );
			float NormalLightdir45 = dotResult40;
			float2 temp_cast_3 = (NormalLightdir45).xx;
			float2 uv_TexCoord52 = i.uv_texcoord * temp_cast_3;
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult43 = dot( normalize( (WorldNormalVector( i , temp_output_78_0 )) ) , ase_worldViewDir );
			float normalviewdir46 = dotResult43;
			float4 shawdow51 = ( saturate( ( saturate( ( ( uv_TexCoord52.x - 0.5 ) / 0.2 ) ) + ( normalviewdir46 * 0.5 ) ) ) * ( Albedo61 * _ShadowColor ) );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			UnityGI gi71 = gi;
			float3 diffNorm71 = ase_worldNormal;
			gi71 = UnityGI_Base( data, 1, diffNorm71 );
			float3 indirectDiffuse71 = gi71.indirect.diffuse + diffNorm71 * 0.0001;
			float4 lighting70 = ( shawdow51 * ( ase_lightColor * float4( ( indirectDiffuse71 + ase_lightAtten ) , 0.0 ) ) );
			float4 RimLight100 = ( ( pow( ( 1.0 - saturate( ( _RimOffset + normalviewdir46 ) ) ) , _Rimpower ) * ( NormalLightdir45 * ase_lightAtten ) ) * ( ase_lightColor * _RimColor ) );
			c.rgb = ( lighting70 + RimLight100 ).rgb;
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
			float4 _MetalTint01_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint01_arr, _MetalTint01);
			float4 lerpResult22 = lerp( float4( 0,0,0,0 ) , _MetalTint01_Instance , i.vertexColor.r);
			float4 _MetalTint02_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint02_arr, _MetalTint02);
			float4 lerpResult24 = lerp( lerpResult22 , _MetalTint02_Instance , i.vertexColor.g);
			float4 _MetalTint03_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint03_arr, _MetalTint03);
			float4 lerpResult27 = lerp( lerpResult24 , _MetalTint03_Instance , i.vertexColor.b);
			float4 _TextureSample0_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_TextureSample0_ST_arr, _TextureSample0_ST);
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST_Instance.xy + _TextureSample0_ST_Instance.zw;
			float4 tex2DNode1 = tex2D( _TextureSample0, uv_TextureSample0 );
			float4 _RustColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_RustColor_arr, _RustColor);
			float4 lerpResult7 = lerp( ( lerpResult27 + tex2DNode1.r ) , _RustColor_Instance , tex2DNode1.b);
			float4 _DirtColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_DirtColor_arr, _DirtColor);
			float4 lerpResult9 = lerp( lerpResult7 , _DirtColor_Instance , tex2DNode1.g);
			o.Albedo = lerpResult9.rgb;
			float4 _EmissiveCOlor_Instance = UNITY_ACCESS_INSTANCED_PROP(_EmissiveCOlor_arr, _EmissiveCOlor);
			float4 lerpResult34 = lerp( float4( 0,0,0,0 ) , _EmissiveCOlor_Instance , i.vertexColor.a);
			o.Emission = lerpResult34.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred 

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
0;0;1920;1019;9108.757;2237.554;6.774844;True;True
Node;AmplifyShaderEditor.CommentaryNode;31;-2166.528,-1585.033;Inherit;False;991.446;633.9523;set on VC RGB Mask;7;27;11;21;24;22;26;29;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;21;-2114.079,-1440.632;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-2116.533,-1281.717;Inherit;False;InstancedProperty;_MetalTint01;Metal Tint 01;3;0;Create;True;0;0;False;0;0,0,0,0;0.5,0.5,0.5,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;26;-1882.768,-1535.034;Inherit;False;InstancedProperty;_MetalTint02;Metal Tint 02;7;0;Create;True;0;0;False;0;0,0,0,0;0.503,0.503,0.503,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;22;-1807.56,-1300.032;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;29;-1653.135,-1158.082;Inherit;False;InstancedProperty;_MetalTint03;Metal Tint 03;8;0;Create;True;0;0;False;0;0,0,0,0;0.65,0.65,0.65,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;24;-1603.824,-1439.762;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;27;-1359.087,-1329.561;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;36;-942.8578,-1149.443;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;-1366.444,-444.1589;Inherit;False;InstancedProperty;_RustColor;Rust Color;4;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1407.914,-848.4495;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;-1;4980db732e661d240845afb74dd2d240;5154740d7dc305944a079290743705b3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;38;-716.6532,-534.0937;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;-823.2985,-880.9362;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;7;-632.8118,-721.0495;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;15;-770.0453,-440.0053;Inherit;False;InstancedProperty;_DirtColor;Dirt Color;5;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;9;-422.8117,-634.0495;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;56.81072,-575.3554;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-3995.455,-100.5687;Inherit;False;61;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;47;-3395.438,-293.4839;Inherit;False;813.8065;370.7;normal.light;4;40;41;39;45;;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;78;-3768.02,69.1073;Inherit;True;NormalCreate;1;;1;e12f7ae19d416b942820e3932b56220f;0;4;1;SAMPLER2D;;False;2;FLOAT2;0,0;False;3;FLOAT;1;False;4;FLOAT;2;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;39;-3345.438,-243.4838;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;41;-3343.129,-68.30581;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;40;-3048.038,-199.6839;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;-2786.631,-217.7448;Inherit;True;NormalLightdir;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;48;-3403.856,214.7599;Inherit;False;708.2981;370.405;normal.viewdir;4;43;44;42;46;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;60;-2503.439,-175.5668;Inherit;False;1915.656;592.7352;Comment;15;50;51;64;65;81;56;90;58;86;53;55;52;49;114;117;shadow;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;42;-3353.856,264.7599;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;49;-2453.439,194.2932;Inherit;False;45;NormalLightdir;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;44;-3345.818,408.1651;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;52;-2427.721,-96.53755;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;43;-3118.818,310.165;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-2943.558,307.4998;Inherit;True;normalviewdir;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;55;-2112,-128;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;111;-3158.786,1162.418;Inherit;False;2332.762;718.1646;RimLight;15;95;94;96;99;97;98;93;102;103;100;106;110;108;109;107;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-2166.819,125.0371;Inherit;False;46;normalviewdir;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;53;-1856,-96;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;58;-1632,-64;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-1891.702,118.299;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-2850.586,1212.418;Inherit;False;Property;_RimOffset;Rim Offset;12;0;Create;True;0;0;False;0;1;-0.32;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-3108.786,1280.308;Inherit;False;46;normalviewdir;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-1330,208;Inherit;False;61;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;94;-2693.586,1269.418;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;114;-1153.245,272.813;Inherit;False;Property;_ShadowColor;Shadow Color;14;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;56;-1495,-59;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;75;-2528,544;Inherit;False;1271.125;475.9821;Comment;8;66;69;71;72;73;74;67;70;lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;81;-1264,-64;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;96;-2534.585,1272.418;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;117;-967.0713,129.2333;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;71;-2448,800;Inherit;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightAttenuation;72;-2480,912;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-2523.585,1450.418;Inherit;False;Property;_Rimpower;Rim power;11;0;Create;True;0;0;False;0;0;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;-2149.671,1593.713;Inherit;False;45;NormalLightdir;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;-960,0;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;97;-2349.585,1324.418;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;109;-2145.202,1667.788;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-2176,832;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;98;-2183.585,1348.418;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;-1890.202,1606.788;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-800,-16;Inherit;False;shawdow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;113;-1646.082,1724.287;Inherit;False;Property;_RimColor;Rim Color;13;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightColorNode;67;-2176,672;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LightColorNode;102;-1654.149,1544.582;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;66;-1920,592;Inherit;False;51;shawdow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;-1436.149,1597.582;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1872,752;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-1514.202,1332.788;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;-1206.271,1393.312;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-1680,656;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;100;-1069.024,1389.39;Inherit;False;RimLight;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-1504,656;Inherit;False;lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;-43.47601,303.5757;Inherit;False;100;RimLight;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-62.58569,176.27;Inherit;False;70;lighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;121;-1937.92,1280.15;Inherit;False;219;183;Test Roughness;1;120;;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;33;-489.1566,-431.735;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;35;-491.0973,-271.0457;Inherit;False;InstancedProperty;_EmissiveCOlor;Emissive COlor;9;1;[HDR];Create;True;0;0;False;0;6.09044,0.5189164,0,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-1367.546,-515.0319;Inherit;False;InstancedProperty;_GrayscaleContrat;GrayscaleContrat;6;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;50;-1872,208;Inherit;True;Property;_TextureSample1;Texture Sample 1;10;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;34;-177.4321,-329.1704;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;16;-1056.063,-611.7008;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-1887.92,1330.15;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-1037.312,-874.6497;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-899.7062,-605.6591;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;118;-3722.689,-306.4236;Inherit;False;Constant;_Vector0;Vector 0;14;0;Create;True;0;0;False;0;127,127,255;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;112;191.5819,238.1848;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;387.7347,1.562029;Float;False;True;2;ASEMaterialInspector;0;0;CustomLighting;MasterChara_Toon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;True;0.04;0.1132075,0.09131362,0.09131362,0;VertexScale;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;1;11;0
WireConnection;22;2;21;1
WireConnection;24;0;22;0
WireConnection;24;1;26;0
WireConnection;24;2;21;2
WireConnection;27;0;24;0
WireConnection;27;1;29;0
WireConnection;27;2;21;3
WireConnection;36;0;27;0
WireConnection;38;0;14;0
WireConnection;32;0;36;0
WireConnection;32;1;1;1
WireConnection;7;0;32;0
WireConnection;7;1;38;0
WireConnection;7;2;1;3
WireConnection;9;0;7;0
WireConnection;9;1;15;0
WireConnection;9;2;1;2
WireConnection;61;0;9;0
WireConnection;78;2;77;0
WireConnection;39;0;78;0
WireConnection;40;0;39;0
WireConnection;40;1;41;0
WireConnection;45;0;40;0
WireConnection;42;0;78;0
WireConnection;52;0;49;0
WireConnection;43;0;42;0
WireConnection;43;1;44;0
WireConnection;46;0;43;0
WireConnection;55;0;52;1
WireConnection;53;0;55;0
WireConnection;58;0;53;0
WireConnection;90;0;86;0
WireConnection;94;0;95;0
WireConnection;94;1;93;0
WireConnection;56;0;58;0
WireConnection;56;1;90;0
WireConnection;81;0;56;0
WireConnection;96;0;94;0
WireConnection;117;0;65;0
WireConnection;117;1;114;0
WireConnection;64;0;81;0
WireConnection;64;1;117;0
WireConnection;97;0;96;0
WireConnection;73;0;71;0
WireConnection;73;1;72;0
WireConnection;98;0;97;0
WireConnection;98;1;99;0
WireConnection;108;0;107;0
WireConnection;108;1;109;0
WireConnection;51;0;64;0
WireConnection;103;0;102;0
WireConnection;103;1;113;0
WireConnection;74;0;67;0
WireConnection;74;1;73;0
WireConnection;110;0;98;0
WireConnection;110;1;108;0
WireConnection;106;0;110;0
WireConnection;106;1;103;0
WireConnection;69;0;66;0
WireConnection;69;1;74;0
WireConnection;100;0;106;0
WireConnection;70;0;69;0
WireConnection;50;1;49;0
WireConnection;34;1;35;0
WireConnection;34;2;33;4
WireConnection;16;0;1;1
WireConnection;16;1;17;0
WireConnection;2;0;27;0
WireConnection;2;1;1;1
WireConnection;20;0;17;0
WireConnection;20;1;16;0
WireConnection;112;0;76;0
WireConnection;112;1;101;0
WireConnection;0;0;9;0
WireConnection;0;2;34;0
WireConnection;0;13;112;0
ASEEND*/
//CHKSM=4CDF124ED9259CE8F5391B2E006913C2E92AFB0C