// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CircleCompletion"
{
	Properties
	{
		_AddToCompleteCircle("AddToCompleteCircle", Range( 0 , 1)) = 1
		_EmissiveColor("EmissiveColor", Color) = (1,1,1,1)
		_CircleThickness("CircleThickness", Range( 0 , 1)) = 0.16
		_EmissiveMultiplier("EmissiveMultiplier", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _EmissiveColor;
		uniform float _EmissiveMultiplier;
		uniform float _AddToCompleteCircle;
		uniform float _CircleThickness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Emission = ( _EmissiveColor * _EmissiveMultiplier ).rgb;
			float2 temp_output_2_0 = (float2( -1,-1 ) + (i.uv_texcoord - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 )));
			float2 break8 = temp_output_2_0;
			float temp_output_3_0 = length( temp_output_2_0 );
			float clampResult28 = clamp( ( ( atan2( break8.y , break8.x ) + (-3.6 + (_AddToCompleteCircle - 0.0) * (4.3 - -3.6) / (1.0 - 0.0)) ) * ( ( 1.0 - floor( temp_output_3_0 ) ) * floor( ( temp_output_3_0 + _CircleThickness ) ) ) ) , 0.0 , 1.0 );
			o.Alpha = ( _EmissiveColor.a * clampResult28 );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
94;144;1569;875;1327.196;513.9006;1;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-1065.303,-362.3965;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;2;-817.3038,-360.3965;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-457.3428,379.9539;Inherit;False;Property;_CircleThickness;CircleThickness;2;0;Create;True;0;0;False;0;0.16;0.27;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;3;-594.1999,135.3233;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-235.2743,-89.1612;Inherit;False;Property;_AddToCompleteCircle;AddToCompleteCircle;0;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;5;-293.0338,135.9957;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-326.3428,264.9539;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;8;-621.269,-362.2938;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.FloorOpNode;20;-161.3428,282.9539;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;16;-154.9245,-261.4172;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-3.6;False;4;FLOAT;4.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ATan2OpNode;10;-365.7754,-363.7579;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;6;-157.0336,51.99567;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-136.1242,-361.1477;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;12.65723,202.9539;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;281.1142,132.5525;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;163.1323,-486.9024;Inherit;False;Property;_EmissiveColor;EmissiveColor;1;0;Create;True;0;0;False;0;1,1,1,1;0.06875221,0.9716981,0.1370319,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;519.4082,-308.0125;Inherit;False;Property;_EmissiveMultiplier;EmissiveMultiplier;3;0;Create;True;0;0;False;0;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;28;441.8657,130.1681;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;602.414,114.5525;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;603.9084,-434.1124;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;858.0876,-80.62288;Float;False;True;2;ASEMaterialInspector;0;0;Standard;CircleCompletion;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;0
WireConnection;3;0;2;0
WireConnection;5;0;3;0
WireConnection;21;0;3;0
WireConnection;21;1;18;0
WireConnection;8;0;2;0
WireConnection;20;0;21;0
WireConnection;16;0;14;0
WireConnection;10;0;8;1
WireConnection;10;1;8;0
WireConnection;6;0;5;0
WireConnection;13;0;10;0
WireConnection;13;1;16;0
WireConnection;23;0;6;0
WireConnection;23;1;20;0
WireConnection;26;0;13;0
WireConnection;26;1;23;0
WireConnection;28;0;26;0
WireConnection;27;0;15;4
WireConnection;27;1;28;0
WireConnection;25;0;15;0
WireConnection;25;1;24;0
WireConnection;0;2;25;0
WireConnection;0;9;27;0
ASEEND*/
//CHKSM=E04A4B3A4E3769A1512915C174128B1D61D9B93A