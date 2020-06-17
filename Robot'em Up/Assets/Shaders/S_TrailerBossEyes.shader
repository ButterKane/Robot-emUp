// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_TrailerBossEyes"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainColor("MainColor", Color) = (1,0.5231712,0,1)
		_SecondaryColor("SecondaryColor", Color) = (1,0.5231712,0,1)
		_EmissiveIntensity("EmissiveIntensity", Float) = 1
		_EmissiveIntensity1("EmissiveIntensity", Float) = 1
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Dissolve("Dissolve", Range( 0 , 1)) = 0
		_CircleSize("CircleSize", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _SecondaryColor;
		uniform float _EmissiveIntensity1;
		uniform float4 _MainColor;
		uniform float _EmissiveIntensity;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float _Dissolve;
		uniform float _CircleSize;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv0_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float2 panner15 = ( 1.0 * _Time.y * float2( 1,3 ) + uv0_TextureSample0);
			float cos17 = cos( _Time.y );
			float sin17 = sin( _Time.y );
			float2 rotator17 = mul( panner15 - float2( 0.5,0.5 ) , float2x2( cos17 , -sin17 , sin17 , cos17 )) + float2( 0.5,0.5 );
			float clampResult9 = clamp( ( tex2D( _TextureSample0, rotator17 ).r + (-1.0 + (_Dissolve - 0.0) * (2.0 - -1.0) / (1.0 - 0.0)) ) , 0.0 , 1.0 );
			float4 lerpResult4 = lerp( ( _SecondaryColor * _EmissiveIntensity1 ) , ( _MainColor * _EmissiveIntensity ) , clampResult9);
			o.Emission = lerpResult4.rgb;
			o.Alpha = 1;
			clip( ceil( ( ( ( 1.0 - length( (float2( -1,-1 ) + (i.uv_texcoord - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) ) ) - 1.0 ) + _CircleSize ) ) - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
114;65;1309;774;1737.838;697.8604;1.973236;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-1467.901,693.6031;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;16;-2209.678,-112.01;Inherit;False;0;5;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;11;-1202.701,694.9034;Inherit;True;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;15;-1920.671,-104.0978;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,3;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;18;-1755.407,34.36617;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;17;-1696.857,-105.9531;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1620.29,160.0728;Inherit;False;Property;_Dissolve;Dissolve;6;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;12;-923.2018,700.1031;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-1492.545,-132.7371;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;False;0;-1;None;c43b86301d5c2a946b7e575dc1c9a46e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;13;-751.6018,698.8035;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;8;-1258.165,128.6172;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-815.4967,254.9396;Inherit;False;Property;_EmissiveIntensity;EmissiveIntensity;3;0;Create;True;0;0;False;0;1;16;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-825.1997,63.14349;Inherit;False;Property;_MainColor;MainColor;1;0;Create;True;0;0;False;0;1,0.5231712,0,1;1,0.1135959,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;31;-782.8898,-279.3362;Inherit;False;Property;_EmissiveIntensity1;EmissiveIntensity;4;0;Create;True;0;0;False;0;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-553.0697,830.1153;Inherit;False;Property;_CircleSize;CircleSize;7;0;Create;True;0;0;False;0;0;0.2813363;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;19;-568.0837,723.7654;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;6;-1126.065,-99.58288;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;30;-792.5928,-471.1323;Inherit;False;Property;_SecondaryColor;SecondaryColor;2;0;Create;True;0;0;False;0;1,0.5231712,0,1;1,0.9151791,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-401.678,721.2632;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-551.8899,-337.3363;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-584.4968,196.9395;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;9;-898.2385,-85.66367;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;4;-91.82862,-16.22575;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CeilOpNode;14;-268.2163,686.9753;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;167.0574,-58.51511;Float;False;True;2;ASEMaterialInspector;0;0;Standard;S_TrailerBossEyes;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;False;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;11;0;10;0
WireConnection;15;0;16;0
WireConnection;17;0;15;0
WireConnection;17;2;18;0
WireConnection;12;0;11;0
WireConnection;5;1;17;0
WireConnection;13;0;12;0
WireConnection;8;0;7;0
WireConnection;19;0;13;0
WireConnection;6;0;5;1
WireConnection;6;1;8;0
WireConnection;20;0;19;0
WireConnection;20;1;21;0
WireConnection;32;0;30;0
WireConnection;32;1;31;0
WireConnection;2;0;1;0
WireConnection;2;1;3;0
WireConnection;9;0;6;0
WireConnection;4;0;32;0
WireConnection;4;1;2;0
WireConnection;4;2;9;0
WireConnection;14;0;20;0
WireConnection;0;2;4;0
WireConnection;0;10;14;0
ASEEND*/
//CHKSM=9FA9120ABFDBFF664D662E98617E6FE386418323