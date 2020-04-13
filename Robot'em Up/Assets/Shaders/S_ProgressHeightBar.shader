// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ProgressHeightBar"
{
	Properties
	{
		_MinHeight("MinHeight", Float) = 10
		_MaxHeight("MaxHeight", Float) = 31
		_ProgressionSlider("ProgressionSlider", Range( 0 , 1)) = 0
		_UnchargedLink("UnchargedLink", Color) = (1,0.25,0.25,1)
		_ChargedLink("ChargedLink", Color) = (0.3066038,0.9792062,1,1)
		_ChargedColorIntensity("ChargedColorIntensity", Float) = 1
		_UnchargedColorIntensity("UnchargedColorIntensity", Float) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _UnchargedLink;
		uniform float4 _ChargedLink;
		uniform float _MinHeight;
		uniform float _MaxHeight;
		uniform float _ProgressionSlider;
		uniform float _UnchargedColorIntensity;
		uniform float _ChargedColorIntensity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float clampResult6 = clamp( ( ( ( ase_worldPos.y - _MinHeight ) / _MaxHeight ) - _ProgressionSlider ) , 0.0 , 1.0 );
			float temp_output_14_0 = ( 1.0 - ceil( clampResult6 ) );
			float4 lerpResult23 = lerp( _UnchargedLink , _ChargedLink , temp_output_14_0);
			o.Albedo = lerpResult23.rgb;
			float4 lerpResult22 = lerp( ( _UnchargedLink * _UnchargedColorIntensity ) , ( _ChargedLink * _ChargedColorIntensity ) , temp_output_14_0);
			o.Emission = lerpResult22.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
94;144;1569;875;1847.936;237.543;1.449761;True;True
Node;AmplifyShaderEditor.CommentaryNode;15;-1553.477,272.205;Inherit;False;1238.554;386.7272;LerpValue;10;1;2;4;3;9;5;8;6;13;14;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1;-1503.477,322.205;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;2;-1470.191,473.4111;Inherit;False;Property;_MinHeight;MinHeight;0;0;Create;True;0;0;False;0;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;4;-1302.191,372.4111;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-1300.191,475.4111;Inherit;False;Property;_MaxHeight;MaxHeight;1;0;Create;True;0;0;False;0;31;31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1106.133,480.1427;Inherit;False;Property;_ProgressionSlider;ProgressionSlider;2;0;Create;True;0;0;False;0;0;0.9637296;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;5;-1136.191,371.4111;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;8;-986.3344,377.9323;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;6;-795.389,374.4663;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;16;-904.561,-304.771;Inherit;False;Property;_UnchargedLink;UnchargedLink;3;0;Create;True;0;0;False;0;1,0.25,0.25,1;1,0.1084906,0.1084906,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;-899.6048,-122.7123;Inherit;False;Property;_UnchargedColorIntensity;UnchargedColorIntensity;6;0;Create;True;0;0;False;0;1;2.06;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;-906.7407,-19.47098;Inherit;False;Property;_ChargedLink;ChargedLink;4;0;Create;True;0;0;False;0;0.3066038,0.9792062,1,1;0.1462264,0.9257588,1,0.9843137;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CeilOpNode;13;-632.0115,376.4584;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-899.8971,158.892;Inherit;False;Property;_ChargedColorIntensity;ChargedColorIntensity;5;0;Create;True;0;0;False;0;1;2.31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;14;-501.9228,389.9588;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-583.9602,-200.9681;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-610.2517,16.28302;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;22;-260.005,-21.31244;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;23;-213.2046,-291.7122;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;135.4,-15.8;Float;False;True;2;ASEMaterialInspector;0;0;Standard;ProgressHeightBar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;1;2
WireConnection;4;1;2;0
WireConnection;5;0;4;0
WireConnection;5;1;3;0
WireConnection;8;0;5;0
WireConnection;8;1;9;0
WireConnection;6;0;8;0
WireConnection;13;0;6;0
WireConnection;14;0;13;0
WireConnection;18;0;16;0
WireConnection;18;1;21;0
WireConnection;19;0;17;0
WireConnection;19;1;20;0
WireConnection;22;0;18;0
WireConnection;22;1;19;0
WireConnection;22;2;14;0
WireConnection;23;0;16;0
WireConnection;23;1;17;0
WireConnection;23;2;14;0
WireConnection;0;0;23;0
WireConnection;0;2;22;0
ASEEND*/
//CHKSM=6D33A04264AF8BB2F79EE4FA4DBCFE5DCC8C85EB