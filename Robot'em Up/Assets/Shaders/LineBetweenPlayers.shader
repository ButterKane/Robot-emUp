// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LineBetweenPlayers"
{
	Properties
	{
		_CurrentEnergyAmout("CurrentEnergyAmout", Range( 0 , 1)) = 0
		_BreakingLinkProgression("BreakingLinkProgression", Range( 0 , 1)) = 0
		_EmissiveIntensityWhenCharged("EmissiveIntensityWhenCharged", Float) = 1
		_ColorWhenCharged("ColorWhenCharged", Color) = (0,0.90622,1,1)
		_ColorWhenFullyCharged("ColorWhenFullyCharged", Color) = (0,0.90622,1,1)
		_ColorWhenNotCharged("ColorWhenNotCharged", Color) = (0,0.4644156,0.8301887,1)
		_ColorWhenBreaking("ColorWhenBreaking", Color) = (0,0.4644156,0.8301887,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _ColorWhenNotCharged;
		uniform float _CurrentEnergyAmout;
		uniform float4 _ColorWhenFullyCharged;
		uniform float4 _ColorWhenCharged;
		uniform float _EmissiveIntensityWhenCharged;
		uniform float4 _ColorWhenBreaking;
		uniform float _BreakingLinkProgression;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ifLocalVar41 = 0;
			if( _CurrentEnergyAmout >= 1.0 )
				ifLocalVar41 = _ColorWhenFullyCharged;
			else
				ifLocalVar41 = _ColorWhenCharged;
			float temp_output_9_0 = (i.uv_texcoord).x;
			float temp_output_3_0 = ( 1.0 - i.uv_texcoord.x );
			float temp_output_20_0 = ( 1.0 - step( ( ( (0.0 + (temp_output_9_0 - 0.0) * (1.0 - 0.0) / (0.5 - 0.0)) * step( temp_output_9_0 , 0.5 ) ) + ( (0.0 + (temp_output_3_0 - 0.0) * (1.0 - 0.0) / (0.5 - 0.0)) * step( temp_output_3_0 , 0.5 ) ) ) , ( 1.0 - _CurrentEnergyAmout ) ) );
			float4 lerpResult16 = lerp( _ColorWhenNotCharged , ( ifLocalVar41 * _EmissiveIntensityWhenCharged ) , temp_output_20_0);
			float4 lerpResult35 = lerp( lerpResult16 , _ColorWhenBreaking , _BreakingLinkProgression);
			o.Emission = lerpResult35.rgb;
			float lerpResult19 = lerp( _ColorWhenNotCharged.a , _ColorWhenCharged.a , temp_output_20_0);
			float lerpResult38 = lerp( _ColorWhenBreaking.a , lerpResult19 , _BreakingLinkProgression);
			o.Alpha = lerpResult38;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
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
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
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
1920;0;1920;1019;1824.396;1434.362;1.633331;True;True
Node;AmplifyShaderEditor.CommentaryNode;39;-2657.747,-835.9584;Inherit;False;1478.185;1247.923;GRADIENT LINEAR;12;1;9;3;23;32;26;28;22;24;27;29;30;GRADIENT LINEAR;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-2607.747,-399.4697;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;32;-1968.583,-316.2929;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1989.944,296.9646;Inherit;False;Constant;_SemiStep;SemiStep;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;9;-2306.227,-639.4603;Inherit;True;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;3;-2295.827,-27.82362;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;26;-1963.647,-785.9584;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;24;-1970.748,-533.17;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;22;-1995.444,80.7645;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;28;-1996.681,-175.1124;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-1680.538,-614.5938;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1121.43,-737.1;Inherit;False;Property;_CurrentEnergyAmout;CurrentEnergyAmout;1;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1693.781,-1.512609;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;21;-1004.01,-360.0544;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;40;-1018.281,-1077.67;Inherit;False;Property;_ColorWhenFullyCharged;ColorWhenFullyCharged;5;0;Create;True;0;0;False;0;0,0.90622,1,1;1,0.5820617,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-891.8644,-1265.527;Inherit;False;Property;_ColorWhenCharged;ColorWhenCharged;4;0;Create;True;0;0;False;0;0,0.90622,1,1;0,0.8783557,1,0.7568628;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-1414.562,-466.7332;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-452.9635,-718.0518;Inherit;False;Property;_EmissiveIntensityWhenCharged;EmissiveIntensityWhenCharged;3;0;Create;True;0;0;False;0;1;1.51;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;5;-984.9485,-461.104;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;41;-530.7333,-889.8633;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;-872.4103,-1449.492;Inherit;False;Property;_ColorWhenNotCharged;ColorWhenNotCharged;6;0;Create;True;0;0;False;0;0,0.4644156,0.8301887,1;0,0.4644156,0.8301887,0.6235294;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-312.3615,-845.8267;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;20;-767.3261,-459.3891;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;36;-878.0569,-1651.573;Inherit;False;Property;_ColorWhenBreaking;ColorWhenBreaking;7;0;Create;True;0;0;False;0;0,0.4644156,0.8301887,1;0.9622642,0.03177289,0.093562,0.9803922;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;16;97.37713,-848.1185;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;33;782.8047,-759.5007;Inherit;False;Property;_BreakingLinkProgression;BreakingLinkProgression;2;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;399.2822,-382.7029;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;35;892.3481,-884.1871;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;38;1066.052,-653.6329;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1238.327,-896.8408;Float;False;True;2;ASEMaterialInspector;0;0;Standard;LineBetweenPlayers;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;1;0
WireConnection;3;0;1;1
WireConnection;26;0;9;0
WireConnection;24;0;9;0
WireConnection;24;1;32;0
WireConnection;22;0;3;0
WireConnection;22;1;23;0
WireConnection;28;0;3;0
WireConnection;27;0;26;0
WireConnection;27;1;24;0
WireConnection;29;0;28;0
WireConnection;29;1;22;0
WireConnection;21;0;7;0
WireConnection;30;0;27;0
WireConnection;30;1;29;0
WireConnection;5;0;30;0
WireConnection;5;1;21;0
WireConnection;41;0;7;0
WireConnection;41;2;40;0
WireConnection;41;3;40;0
WireConnection;41;4;15;0
WireConnection;17;0;41;0
WireConnection;17;1;18;0
WireConnection;20;0;5;0
WireConnection;16;0;14;0
WireConnection;16;1;17;0
WireConnection;16;2;20;0
WireConnection;19;0;14;4
WireConnection;19;1;15;4
WireConnection;19;2;20;0
WireConnection;35;0;16;0
WireConnection;35;1;36;0
WireConnection;35;2;33;0
WireConnection;38;0;36;4
WireConnection;38;1;19;0
WireConnection;38;2;33;0
WireConnection;0;2;35;0
WireConnection;0;9;38;0
ASEEND*/
//CHKSM=0DDE56DE53591B2A892EAAA35294F915F098EFFC