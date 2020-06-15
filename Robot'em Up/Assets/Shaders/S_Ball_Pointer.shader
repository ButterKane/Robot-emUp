// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_Ball_Pointer"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_CircleThickness("CircleThickness", Range( 0 , 1)) = 0.1051673
		_MainColor("MainColor", Color) = (0,0.8595505,1,1)
		_SecondaryColor("SecondaryColor", Color) = (0,0.8595505,1,1)
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_RotatorSpeed("RotatorSpeed", Float) = 0
		_NoiseTilingX("NoiseTilingX", Float) = 0
		_NoiseTilingY("NoiseTilingY", Float) = 0
		_SecondaryColorEmissiveIntensity("SecondaryColorEmissiveIntensity", Float) = 0
		_MainColorEmissiveIntensity("MainColorEmissiveIntensity", Float) = 0
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
		uniform float _SecondaryColorEmissiveIntensity;
		uniform float4 _MainColor;
		uniform float _MainColorEmissiveIntensity;
		uniform sampler2D _TextureSample0;
		uniform float _NoiseTilingX;
		uniform float _NoiseTilingY;
		uniform float _RotatorSpeed;
		uniform float _CircleThickness;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult38 = (float2(_NoiseTilingX , _NoiseTilingY));
			float2 uv_TexCoord37 = i.uv_texcoord * appendResult38;
			float cos35 = cos( ( _RotatorSpeed * _Time.y ) );
			float sin35 = sin( ( _RotatorSpeed * _Time.y ) );
			float2 rotator35 = mul( uv_TexCoord37 - float2( 0.5,0.5 ) , float2x2( cos35 , -sin35 , sin35 , cos35 )) + float2( 0.5,0.5 );
			float4 lerpResult34 = lerp( ( _SecondaryColor * _SecondaryColorEmissiveIntensity ) , ( _MainColor * _MainColorEmissiveIntensity ) , tex2D( _TextureSample0, rotator35 ).r);
			o.Emission = lerpResult34.rgb;
			o.Alpha = 1;
			float temp_output_6_0 = ( 1.0 - length( ( i.uv_texcoord - float2( 0.5,0.5 ) ) ) );
			float clampResult28 = clamp( ( floor( ( temp_output_6_0 + 0.5 ) ) - floor( ( temp_output_6_0 + ( ( 1.0 - _CircleThickness ) / 2.0 ) ) ) ) , 0.0 , 1.0 );
			clip( clampResult28 - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
114;65;1309;774;1072.695;440.115;1.912271;True;True
Node;AmplifyShaderEditor.CommentaryNode;31;-863.8036,803.6149;Inherit;False;1668.213;617.2856;Circle Thicness;13;4;12;5;23;6;15;17;24;18;25;27;28;3;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-827.5009,879.2973;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-354.5406,1253.622;Inherit;False;Property;_CircleThickness;CircleThickness;1;0;Create;True;0;0;False;0;0.1051673;0.1051673;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;4;-595.4675,876.346;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-778.3547,374.0516;Inherit;False;Property;_NoiseTilingX;NoiseTilingX;6;0;Create;True;0;0;False;0;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-791.3547,493.0516;Inherit;False;Property;_NoiseTilingY;NoiseTilingY;7;0;Create;True;0;0;False;0;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;5;-385.6444,885.0095;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;23;-85.9903,1260.851;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-371.3547,544.0516;Inherit;False;Property;_RotatorSpeed;RotatorSpeed;5;0;Create;True;0;0;False;0;0;5.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;38;-588.3547,428.0516;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;42;-362.355,627.0516;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;6;-192.8629,888.5146;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;15;79.54799,1266.652;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;17;54.40956,905.9506;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;37;-421.3547,418.0516;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;24;79.75963,1166.601;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-133.355,542.0516;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-57.86526,-93.29335;Inherit;False;Property;_SecondaryColorEmissiveIntensity;SecondaryColorEmissiveIntensity;8;0;Create;True;0;0;False;0;0;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;35;-137.9979,413.3219;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FloorOpNode;25;218.8596,1167.901;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;18;193.5095,907.2505;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;30;-3.157822,78.51968;Inherit;False;Property;_MainColor;MainColor;2;0;Create;True;0;0;False;0;0,0.8595505,1,1;0,0.7582536,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;33;14.26982,-271.4193;Inherit;False;Property;_SecondaryColor;SecondaryColor;3;0;Create;True;0;0;False;0;0,0.8595505,1,1;0.4009434,1,0.9033288,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;46;-55.85369,275.2725;Inherit;False;Property;_MainColorEmissiveIntensity;MainColorEmissiveIntensity;9;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;32;75.41611,389.8325;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;False;0;-1;None;223a3af3c6f4cf24eb06bbfe77aafdb7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;27;400.2096,1004.751;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;301.5638,180.932;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;297.1347,-168.2933;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;28;548.4095,1007.351;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;34;689.3513,388.7292;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;922.5245,363.4843;Float;False;True;2;ASEMaterialInspector;0;0;Standard;S_Ball_Pointer;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;False;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;3;0
WireConnection;5;0;4;0
WireConnection;23;0;12;0
WireConnection;38;0;39;0
WireConnection;38;1;40;0
WireConnection;6;0;5;0
WireConnection;15;0;23;0
WireConnection;17;0;6;0
WireConnection;37;0;38;0
WireConnection;24;0;6;0
WireConnection;24;1;15;0
WireConnection;41;0;36;0
WireConnection;41;1;42;0
WireConnection;35;0;37;0
WireConnection;35;2;41;0
WireConnection;25;0;24;0
WireConnection;18;0;17;0
WireConnection;32;1;35;0
WireConnection;27;0;18;0
WireConnection;27;1;25;0
WireConnection;45;0;30;0
WireConnection;45;1;46;0
WireConnection;43;0;33;0
WireConnection;43;1;44;0
WireConnection;28;0;27;0
WireConnection;34;0;43;0
WireConnection;34;1;45;0
WireConnection;34;2;32;1
WireConnection;0;2;34;0
WireConnection;0;10;28;0
ASEEND*/
//CHKSM=D4433BC9F64696016B61768063138449EE7797DE