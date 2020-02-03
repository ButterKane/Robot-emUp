// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HitBoxMelee"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_ContrastValue("ContrastValue", Float) = 0
		_WhiteOutline("WhiteOutline", Color) = (1,1,1,1)
		_EmissiveIntensity("EmissiveIntensity", Float) = 1
		_StepValue("StepValue", Float) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _WhiteOutline;
		uniform float _EmissiveIntensity;
		uniform float _ContrastValue;
		uniform float _StepValue;
		uniform float _Cutoff = 0.5;


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Emission = ( _WhiteOutline * _EmissiveIntensity ).rgb;
			o.Alpha = 1;
			float2 temp_output_2_0 = ( 1.0 - i.uv_texcoord );
			float4 temp_cast_1 = (( step( (i.uv_texcoord).x , _StepValue ) * step( (i.uv_texcoord).y , _StepValue ) * step( (temp_output_2_0).x , _StepValue ) * step( (temp_output_2_0).y , _StepValue ) )).xxxx;
			float4 clampResult11 = clamp( ( 1.0 - CalculateContrast(_ContrastValue,temp_cast_1) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
			clip( (clampResult11).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
1920;0;1920;1019;1718.253;459.5916;1.3;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-944.0129,134.6691;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;2;-885.5146,405.069;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-339.5657,363.0729;Inherit;False;Property;_StepValue;StepValue;4;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;6;-656.7136,618.2695;Inherit;True;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;4;-647.6138,226.9692;Inherit;True;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;5;-652.814,423.269;Inherit;True;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;3;-651.5141,41.06917;Inherit;True;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;20;-328.7285,92.13301;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;21;-320.0583,241.6918;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;22;-314.6393,447.6062;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;23;-309.2207,636.1807;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;143.1095,243.2652;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;359.9586,359.7464;Inherit;False;Property;_ContrastValue;ContrastValue;1;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;8;367.9586,257.7465;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;10;557.9589,256.7465;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;475.5687,-34.80209;Inherit;False;Property;_WhiteOutline;WhiteOutline;2;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;11;718.9588,257.7465;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;16;691.5689,78.29801;Inherit;False;Property;_EmissiveIntensity;EmissiveIntensity;3;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;768.2689,-16.6021;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;13;622.4691,373.3982;Inherit;True;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;962.7756,-65.31551;Float;False;True;2;ASEMaterialInspector;0;0;Standard;HitBoxMelee;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;False;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;0
WireConnection;6;0;2;0
WireConnection;4;0;1;0
WireConnection;5;0;2;0
WireConnection;3;0;1;0
WireConnection;20;0;3;0
WireConnection;20;1;24;0
WireConnection;21;0;4;0
WireConnection;21;1;24;0
WireConnection;22;0;5;0
WireConnection;22;1;24;0
WireConnection;23;0;6;0
WireConnection;23;1;24;0
WireConnection;7;0;20;0
WireConnection;7;1;21;0
WireConnection;7;2;22;0
WireConnection;7;3;23;0
WireConnection;8;1;7;0
WireConnection;8;0;9;0
WireConnection;10;0;8;0
WireConnection;11;0;10;0
WireConnection;15;0;14;0
WireConnection;15;1;16;0
WireConnection;13;0;11;0
WireConnection;0;2;15;0
WireConnection;0;10;13;0
ASEEND*/
//CHKSM=043597E1E6FD988BE210F640A46C295D92315D74