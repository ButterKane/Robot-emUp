// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HitBoxExplosion"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.59
		_WhiteOutline("WhiteOutline", Color) = (1,1,1,1)
		_WhiteEmissiveIntensity("WhiteEmissiveIntensity", Float) = 1
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_RotationSpeed("RotationSpeed", Float) = 1
		_Thickness("Thickness", Float) = 0.07
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _WhiteOutline;
		uniform float _WhiteEmissiveIntensity;
		uniform float _Thickness;
		uniform sampler2D _TextureSample0;
		uniform float _RotationSpeed;
		uniform float _Cutoff = 0.59;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Emission = ( _WhiteOutline * _WhiteEmissiveIntensity ).rgb;
			o.Alpha = 1;
			float temp_output_2_0 = length( (float2( -1,-1 ) + (i.uv_texcoord - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) );
			float cos12 = cos( ( _RotationSpeed * _Time.y ) );
			float sin12 = sin( ( _RotationSpeed * _Time.y ) );
			float2 rotator12 = mul( i.uv_texcoord - float2( 0.5,0.5 ) , float2x2( cos12 , -sin12 , sin12 , cos12 )) + float2( 0.5,0.5 );
			clip( ( floor( ( temp_output_2_0 + _Thickness ) ) * tex2D( _TextureSample0, rotator12 ) * ( 1.0 - floor( temp_output_2_0 ) ) ).r - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
1920;7;1920;988;1552.377;515.9946;1.536982;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-1536.799,142.652;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;15;-1533.129,500.1357;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-1535.082,425.7271;Inherit;False;Property;_RotationSpeed;RotationSpeed;4;0;Create;True;0;0;False;0;1;0.13;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;1;-1220.476,-271.4187;Inherit;True;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-521.2762,-35.37942;Inherit;False;Property;_Thickness;Thickness;5;0;Create;True;0;0;False;0;0.07;0.13;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;2;-948.7756,-262.3188;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-1345.428,470.0459;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;12;-1204.2,124.052;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;5;-481.7794,-148.6066;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;17;-346.1775,167.7225;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-775.5685,96.4368;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;False;0;-1;2cc59ec0f46c96a4bac84d39f9b0979f;2cc59ec0f46c96a4bac84d39f9b0979f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;-446.4296,-214.1684;Inherit;False;Property;_WhiteEmissiveIntensity;WhiteEmissiveIntensity;2;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;6;-666.1279,-324.8031;Inherit;False;Property;_WhiteOutline;WhiteOutline;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;16;-184.5684,163.4243;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;8;68.5014,-142.7461;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-373.4275,-306.6031;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;51.23661,74.78634;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;350.84,-156.5861;Float;False;True;2;ASEMaterialInspector;0;0;Standard;HitBoxExplosion;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.59;True;False;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;1;0;13;0
WireConnection;2;0;1;0
WireConnection;14;0;4;0
WireConnection;14;1;15;0
WireConnection;12;0;13;0
WireConnection;12;2;14;0
WireConnection;5;0;2;0
WireConnection;5;1;3;0
WireConnection;17;0;2;0
WireConnection;9;1;12;0
WireConnection;16;0;17;0
WireConnection;8;0;5;0
WireConnection;10;0;6;0
WireConnection;10;1;7;0
WireConnection;11;0;8;0
WireConnection;11;1;9;0
WireConnection;11;2;16;0
WireConnection;0;2;10;0
WireConnection;0;10;11;0
ASEEND*/
//CHKSM=87702D7CC65FCA22E74076EF0149AABF0770EDA9