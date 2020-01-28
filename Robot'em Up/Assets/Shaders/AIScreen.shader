// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AIScreen"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Noise1("Noise1", 2D) = "white" {}
		_AdditiveEmissive("AdditiveEmissive", Float) = 0
		_SinFrequency("SinFrequency", Float) = 0
		_Noise1PannerSpeed("Noise1PannerSpeed", Vector) = (0,0,0,0)
		_Noise2PannerSpeed("Noise2PannerSpeed", Vector) = (0,0,0,0)
		_Noise2("Noise2", 2D) = "white" {}
		_CenterRadius("CenterRadius", Float) = 0
		_MainTextAdditive("MainTextAdditive", Float) = 0
		_BugColor("BugColor", Color) = (1,0,0,1)
		_RedColorOpacity("RedColorOpacity", Float) = 0
		_RedEmissiveIntensity("RedEmissiveIntensity", Float) = 0
		_RedPartOffset("RedPartOffset", Vector) = (0,0,0,0)
		_RedCircleFrequency("RedCircleFrequency", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _MainTextAdditive;
		uniform float4 _BugColor;
		uniform float _RedEmissiveIntensity;
		uniform float2 _RedPartOffset;
		uniform float _RedCircleFrequency;
		uniform float _RedColorOpacity;
		uniform float _AdditiveEmissive;
		uniform float _SinFrequency;
		uniform sampler2D _Noise1;
		uniform float2 _Noise1PannerSpeed;
		uniform float _CenterRadius;
		uniform sampler2D _Noise2;
		uniform float2 _Noise2PannerSpeed;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv_TexCoord81 = i.uv_texcoord + _RedPartOffset;
			float clampResult86 = clamp( ( length( (float2( -1,-1 ) + (uv_TexCoord81 - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) ) - (0.8 + (sin( ( _Time.y * _RedCircleFrequency ) ) - -1.0) * (1.34 - 0.8) / (1.0 - -1.0)) ) , 0.0 , 1.0 );
			float4 lerpResult80 = lerp( ( tex2D( _MainTex, uv_MainTex ) + _MainTextAdditive ) , ( _BugColor * _RedEmissiveIntensity ) , ( clampResult86 * _RedColorOpacity ));
			float4 clampResult72 = clamp( lerpResult80 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
			float2 panner31 = ( 1.0 * _Time.y * _Noise1PannerSpeed + i.uv_texcoord);
			float clampResult65 = clamp( ( length( (float2( -1,-1 ) + (i.uv_texcoord - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) ) - _CenterRadius ) , 0.0 , 1.0 );
			float2 panner53 = ( 1.0 * _Time.y * _Noise2PannerSpeed + i.uv_texcoord);
			float temp_output_52_0 = ( tex2D( _Noise1, panner31 ).r * ( clampResult65 * tex2D( _Noise2, panner53 ).r ) );
			o.Emission = ( ( clampResult72 * ( ( _AdditiveEmissive * (0.0 + (sin( ( _Time.y * _SinFrequency ) ) - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) ) + 1.0 ) ) + ( temp_output_52_0 * 0.5 ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
2109;95;1442;771;1640.927;1493.719;2.175283;True;True
Node;AmplifyShaderEditor.RangedFloatNode;97;-872.7139,-216.5262;Inherit;False;Property;_RedCircleFrequency;RedCircleFrequency;14;0;Create;True;0;0;False;0;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;92;-822.2983,-294.9576;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;91;-1198.938,-537.7613;Inherit;False;Property;_RedPartOffset;RedPartOffset;13;0;Create;True;0;0;False;0;0,0;0,0.14;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;94;-642.2983,-295.9576;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-979.8787,-559.0599;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;95;-508.2982,-295.9576;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;82;-707.7595,-558.9199;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-1041.295,712.7828;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;42;42.53512,972.4753;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;38.53512,1055.475;Inherit;False;Property;_SinFrequency;SinFrequency;3;0;Create;True;0;0;False;0;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;96;-364.3763,-333.8011;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0.8;False;4;FLOAT;1.34;False;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;83;-519.7594,-559.9199;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;62;-769.1758,712.9227;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;54;-1020.992,338.2973;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LengthOpNode;63;-581.1758,711.9227;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;85;-366.0008,-560.5743;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;69;-793.2875,460.0351;Inherit;False;Property;_Noise2PannerSpeed;Noise2PannerSpeed;5;0;Create;True;0;0;False;0;0,0;0.15,0.15;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;67;-443.4848,825.7438;Inherit;False;Property;_CenterRadius;CenterRadius;8;0;Create;True;0;0;False;0;0;0.23;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;222.5351,971.4753;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;43;356.5352,971.4753;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;77;-271.5682,-896.9927;Inherit;False;Property;_BugColor;BugColor;10;0;Create;True;0;0;False;0;1,0,0,1;1,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;64;-427.4172,711.2683;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-304.5014,-710.9755;Inherit;False;Property;_RedEmissiveIntensity;RedEmissiveIntensity;12;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;53;-742.2343,339.6504;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.15,0.15;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;86;-163.3288,-559.2582;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;29;125.9823,-1068.716;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;-1;None;1925c8f870938e14592191bbc4b2c340;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;32;-798.0534,43.55257;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;78;-63.96868,-430.5469;Inherit;False;Property;_RedColorOpacity;RedColorOpacity;11;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;45;-782.754,161.0407;Inherit;False;Property;_Noise1PannerSpeed;Noise1PannerSpeed;4;0;Create;True;0;0;False;0;0,0;0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;73;216.3646,-859.0931;Inherit;False;Property;_MainTextAdditive;MainTextAdditive;9;0;Create;True;0;0;False;0;0;0.06;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;19.5276,-773.6908;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;70;477.3566,-1038.145;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;31;-543.2562,116.8357;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;5,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;51;-357.6428,311.7034;Inherit;True;Property;_Noise2;Noise2;7;0;Create;True;0;0;False;0;-1;None;c43b86301d5c2a946b7e575dc1c9a46e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;36;500.4571,933.6317;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;216.0155,817.0781;Inherit;False;Property;_AdditiveEmissive;AdditiveEmissive;2;0;Create;True;0;0;False;0;0;0.77;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;144.6094,-563.8927;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;65;-224.7452,712.5844;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;500.4575,821.8315;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;30;-359.9287,91.25505;Inherit;True;Property;_Noise1;Noise1;1;0;Create;True;0;0;False;0;-1;43f0f936bc7be0e43bcbc192b485f5a8;43f0f936bc7be0e43bcbc192b485f5a8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-6.851593,316.7017;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;80;649.6208,-654.2284;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-19.26167,97.99541;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;72;871.2934,-189.4094;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;819.5348,819.4753;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;1039.148,-41.41443;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;707.4148,168.6953;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;416.6224,143.8222;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5019608;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;50;405.9251,247.0378;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;412.1204,323.6567;Inherit;False;Property;_NoiseOpacity;NoiseOpacity;6;0;Create;True;0;0;False;0;0;0.47;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;74;1226.363,65.51258;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1458.385,-77.7388;Float;False;True;2;ASEMaterialInspector;0;0;Standard;AIScreen;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Translucent;0.5;True;True;0;False;Opaque;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;94;0;92;0
WireConnection;94;1;97;0
WireConnection;81;1;91;0
WireConnection;95;0;94;0
WireConnection;82;0;81;0
WireConnection;96;0;95;0
WireConnection;83;0;82;0
WireConnection;62;0;61;0
WireConnection;63;0;62;0
WireConnection;85;0;83;0
WireConnection;85;1;96;0
WireConnection;40;0;42;0
WireConnection;40;1;41;0
WireConnection;43;0;40;0
WireConnection;64;0;63;0
WireConnection;64;1;67;0
WireConnection;53;0;54;0
WireConnection;53;2;69;0
WireConnection;86;0;85;0
WireConnection;89;0;77;0
WireConnection;89;1;90;0
WireConnection;70;0;29;0
WireConnection;70;1;73;0
WireConnection;31;0;32;0
WireConnection;31;2;45;0
WireConnection;51;1;53;0
WireConnection;36;0;43;0
WireConnection;88;0;86;0
WireConnection;88;1;78;0
WireConnection;65;0;64;0
WireConnection;37;0;34;0
WireConnection;37;1;36;0
WireConnection;30;1;31;0
WireConnection;66;0;65;0
WireConnection;66;1;51;1
WireConnection;80;0;70;0
WireConnection;80;1;89;0
WireConnection;80;2;88;0
WireConnection;52;0;30;1
WireConnection;52;1;66;0
WireConnection;72;0;80;0
WireConnection;44;0;37;0
WireConnection;33;0;72;0
WireConnection;33;1;44;0
WireConnection;75;0;52;0
WireConnection;46;0;52;0
WireConnection;46;1;50;0
WireConnection;50;0;49;0
WireConnection;74;0;33;0
WireConnection;74;1;75;0
WireConnection;0;2;74;0
ASEEND*/
//CHKSM=341EF4C51F701FD8FDA6D30DF28B1805EBCC90BC