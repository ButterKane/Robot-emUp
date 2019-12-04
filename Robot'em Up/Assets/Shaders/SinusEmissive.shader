// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SinusEmissive"
{
	Properties
	{
		_AlbedoColor("AlbedoColor", Color) = (0.9716981,0.04125132,0.04125132,1)
		_EmissiveColor("EmissiveColor", Color) = (0.9716981,0,0,1)
		_SinusSpeed("SinusSpeed", Float) = 0
		_EmissiveIntensity("EmissiveIntensity", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			half filler;
		};

		uniform float4 _AlbedoColor;
		uniform float _SinusSpeed;
		uniform float4 _EmissiveColor;
		uniform float _EmissiveIntensity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _AlbedoColor.rgb;
			float mulTime9 = _Time.y * _SinusSpeed;
			o.Emission = ( (0.0 + (sin( mulTime9 ) - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) * _EmissiveColor * _EmissiveIntensity ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
200;155;1442;694;1610.349;47.84839;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;4;-1105,136.5;Inherit;False;Property;_SinusSpeed;SinusSpeed;2;0;Create;True;0;0;False;0;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;9;-943,141.5;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;10;-770,140.5;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;8;-636,306.5;Inherit;False;Property;_EmissiveColor;EmissiveColor;1;0;Create;True;0;0;False;0;0.9716981,0,0,1;0.9716981,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;11;-623,138.5;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-628.3486,492.1516;Inherit;False;Property;_EmissiveIntensity;EmissiveIntensity;3;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-383,190.5;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1;-340,-44.5;Inherit;False;Property;_AlbedoColor;AlbedoColor;0;0;Create;True;0;0;False;0;0.9716981,0.04125132,0.04125132,1;0.9716981,0.04125132,0.04125132,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;ASEMaterialInspector;0;0;Standard;SinusEmissive;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;4;0
WireConnection;10;0;9;0
WireConnection;11;0;10;0
WireConnection;6;0;11;0
WireConnection;6;1;8;0
WireConnection;6;2;12;0
WireConnection;0;0;1;0
WireConnection;0;2;6;0
ASEEND*/
//CHKSM=F27AC4CF9827FA6341C1A4D71102D6E5E7423238