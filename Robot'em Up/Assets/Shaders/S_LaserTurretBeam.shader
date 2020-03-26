// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/S_LaserTurretBeam"
{
	Properties
	{
		_NoiseTexture1("NoiseTexture1", 2D) = "white" {}
		_LaserColor("LaserColor", Color) = (1,0.9863347,0,0)
		_MinEmissiveMultiplier("MinEmissiveMultiplier", Float) = 0.8
		_MinFresnelScale("MinFresnelScale", Float) = 0.8
		_MaxEmissiveMultiplier("MaxEmissiveMultiplier", Float) = 1.2
		_MaxFresnelScale("MaxFresnelScale", Float) = 1.2
		_LaserAntiColor("LaserAntiColor", Color) = (0,0,0,0)
		_Noise1PannerSpeed("Noise1PannerSpeed", Vector) = (1,0.2,0,0)
		_Noise2PannerSpeed("Noise2PannerSpeed", Vector) = (1,0.2,0,0)
		_Noise1Tiling("Noise1Tiling", Vector) = (1,1,0,0)
		_Noise2Tiling("Noise2Tiling", Vector) = (1,1,0,0)
		_TimeScale("TimeScale", Float) = 1
		_TimeScaleForFresnel("TimeScaleForFresnel", Float) = 1
		_NoiseTexture2("NoiseTexture2", 2D) = "white" {}
		_FresnelPower("FresnelPower", Float) = 5
		_LerpAdder("LerpAdder", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _LaserAntiColor;
		uniform float4 _LaserColor;
		uniform float _TimeScale;
		uniform float _MinEmissiveMultiplier;
		uniform float _MaxEmissiveMultiplier;
		uniform sampler2D _NoiseTexture2;
		uniform float2 _Noise2PannerSpeed;
		uniform float2 _Noise2Tiling;
		uniform sampler2D _NoiseTexture1;
		uniform float2 _Noise1PannerSpeed;
		uniform float2 _Noise1Tiling;
		uniform float _TimeScaleForFresnel;
		uniform float _MinFresnelScale;
		uniform float _MaxFresnelScale;
		uniform float _FresnelPower;
		uniform float _LerpAdder;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord29 = i.uv_texcoord * _Noise2Tiling;
			float2 panner31 = ( 1.0 * _Time.y * _Noise2PannerSpeed + uv_TexCoord29);
			float2 uv_TexCoord7 = i.uv_texcoord * _Noise1Tiling;
			float2 panner8 = ( 1.0 * _Time.y * _Noise1PannerSpeed + uv_TexCoord7);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV33 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode33 = ( 0.0 + (_MinFresnelScale + (sin( ( _Time.y * _TimeScaleForFresnel ) ) - -1.0) * (_MaxFresnelScale - _MinFresnelScale) / (1.0 - -1.0)) * pow( 1.0 - fresnelNdotV33, _FresnelPower ) );
			float clampResult14 = clamp( ( ( ( tex2D( _NoiseTexture2, panner31 ).r + tex2D( _NoiseTexture1, panner8 ).r ) / 2.0 ) + fresnelNode33 + _LerpAdder ) , 0.0 , 1.0 );
			float4 lerpResult2 = lerp( _LaserAntiColor , ( _LaserColor * (_MinEmissiveMultiplier + (sin( ( _Time.y * _TimeScale ) ) - -1.0) * (_MaxEmissiveMultiplier - _MinEmissiveMultiplier) / (1.0 - -1.0)) ) , clampResult14);
			o.Emission = lerpResult2.rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

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
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
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
				o.worldNormal = worldNormal;
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
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
1920;38;1920;970;1678.292;-240.5563;1;True;True
Node;AmplifyShaderEditor.Vector2Node;28;-1724.649,521.6855;Inherit;False;Property;_Noise2Tiling;Noise2Tiling;10;0;Create;True;0;0;False;0;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;10;-1725.97,207.1308;Inherit;False;Property;_Noise1Tiling;Noise1Tiling;9;0;Create;True;0;0;False;0;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-1552.354,189.7403;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;9;-1530.579,331.765;Inherit;False;Property;_Noise1PannerSpeed;Noise1PannerSpeed;7;0;Create;True;0;0;False;0;1,0.2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;30;-1527.849,647.7289;Inherit;False;Property;_Noise2PannerSpeed;Noise2PannerSpeed;8;0;Create;True;0;0;False;0;1,0.2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-1551.033,504.2951;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;37;-1348.26,904.0029;Inherit;False;Property;_TimeScaleForFresnel;TimeScaleForFresnel;12;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;38;-1277.783,817.8646;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;8;-1289.354,192.7403;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;31;-1288.033,507.295;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-1112.706,820.1414;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;18;-1560.274,-225.5726;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;-1050.697,415.9183;Inherit;True;Property;_NoiseTexture2;NoiseTexture2;13;0;Create;True;0;0;False;0;-1;eb2ce2b29cde3d04f8790e66afe4b484;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;43;-1075.626,961.7942;Inherit;False;Property;_MinFresnelScale;MinFresnelScale;3;0;Create;True;0;0;False;0;0.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-1073.084,1042.268;Inherit;False;Property;_MaxFresnelScale;MaxFresnelScale;5;0;Create;True;0;0;False;0;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;40;-976.0897,823.5569;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1547.751,-150.4343;Inherit;False;Property;_TimeScale;TimeScale;11;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1059,199.5;Inherit;True;Property;_NoiseTexture1;NoiseTexture1;0;0;Create;True;0;0;False;0;-1;a8d2541ae14a15e42b3830ac3a68a0d8;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;34;-753.5394,653.0696;Inherit;False;Property;_FresnelPower;FresnelPower;14;0;Create;True;0;0;False;0;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-1395.197,-223.2959;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;41;-931.8381,605.171;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0.8;False;4;FLOAT;1.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-628.26,315.9223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1366.489,-36.01074;Inherit;False;Property;_MaxEmissiveMultiplier;MaxEmissiveMultiplier;4;0;Create;True;0;0;False;0;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-1369.031,-116.4846;Inherit;False;Property;_MinEmissiveMultiplier;MinEmissiveMultiplier;2;0;Create;True;0;0;False;0;0.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;27;-477.26,330.9223;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-572.3658,717.468;Inherit;False;Property;_LerpAdder;LerpAdder;15;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;19;-1258.581,-219.8803;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;33;-566.9395,525.3692;Inherit;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-323.8528,493.8586;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-968,-39.5;Inherit;False;Property;_LaserColor;LaserColor;1;0;Create;True;0;0;False;0;1,0.9863347,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;22;-978.3811,-215.1112;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0.8;False;4;FLOAT;1.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-565,-57.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;6;-676.4079,-294.219;Inherit;False;Property;_LaserAntiColor;LaserAntiColor;6;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;14;-142.7776,480.5862;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;2;-359,-76.5;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;ASEMaterialInspector;0;0;Standard;Custom/S_LaserTurretBeam;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;10;0
WireConnection;29;0;28;0
WireConnection;8;0;7;0
WireConnection;8;2;9;0
WireConnection;31;0;29;0
WireConnection;31;2;30;0
WireConnection;39;0;38;0
WireConnection;39;1;37;0
WireConnection;25;1;31;0
WireConnection;40;0;39;0
WireConnection;1;1;8;0
WireConnection;20;0;18;0
WireConnection;20;1;21;0
WireConnection;41;0;40;0
WireConnection;41;3;43;0
WireConnection;41;4;42;0
WireConnection;26;0;25;1
WireConnection;26;1;1;1
WireConnection;27;0;26;0
WireConnection;19;0;20;0
WireConnection;33;2;41;0
WireConnection;33;3;34;0
WireConnection;13;0;27;0
WireConnection;13;1;33;0
WireConnection;13;2;36;0
WireConnection;22;0;19;0
WireConnection;22;3;5;0
WireConnection;22;4;23;0
WireConnection;4;0;3;0
WireConnection;4;1;22;0
WireConnection;14;0;13;0
WireConnection;2;0;6;0
WireConnection;2;1;4;0
WireConnection;2;2;14;0
WireConnection;0;2;2;0
ASEEND*/
//CHKSM=1DE662AD44C17517C12C96502414480AD216DFBF