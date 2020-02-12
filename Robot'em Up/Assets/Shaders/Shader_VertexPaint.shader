// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SH_VertexPaint"
{
	Properties
	{
		_Human_Ground("Human_Ground", 2D) = "white" {}
		_Robot_Ground("Robot_Ground", 2D) = "white" {}
		_Other_Ground("Other_Ground", 2D) = "white" {}
		[Toggle]_Human_Colour("Human_Colour", Float) = 1
		[Toggle]_Robot_Colour("Robot_Colour", Float) = 1
		[Toggle]_Other_Colour("Other_Colour", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _Human_Colour;
		uniform sampler2D _Human_Ground;
		uniform float4 _Human_Ground_ST;
		uniform float _Other_Colour;
		uniform sampler2D _Other_Ground;
		uniform float4 _Other_Ground_ST;
		uniform float _Robot_Colour;
		uniform sampler2D _Robot_Ground;
		uniform float4 _Robot_Ground_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color7 = IsGammaSpace() ? float4(1,0.5,0,0) : float4(1,0.2140411,0,0);
			float2 uv_Human_Ground = i.uv_texcoord * _Human_Ground_ST.xy + _Human_Ground_ST.zw;
			float4 color1 = IsGammaSpace() ? float4(1,0,0.3087969,0) : float4(1,0,0.07767044,0);
			float4 break5 = i.vertexColor;
			float4 lerpResult6 = lerp( color7 , ( _Human_Colour )?( color1 ):( tex2D( _Human_Ground, uv_Human_Ground ) ) , break5.r);
			float2 uv_Other_Ground = i.uv_texcoord * _Other_Ground_ST.xy + _Other_Ground_ST.zw;
			float4 color2 = IsGammaSpace() ? float4(0.5355395,1,0,0) : float4(0.2484229,1,0,0);
			float4 lerpResult8 = lerp( lerpResult6 , ( _Other_Colour )?( color2 ):( tex2D( _Other_Ground, uv_Other_Ground ) ) , break5.g);
			float2 uv_Robot_Ground = i.uv_texcoord * _Robot_Ground_ST.xy + _Robot_Ground_ST.zw;
			float4 color3 = IsGammaSpace() ? float4(0,0.2865825,1,0) : float4(0,0.06677034,1,0);
			float4 lerpResult9 = lerp( lerpResult8 , ( _Robot_Colour )?( color3 ):( tex2D( _Robot_Ground, uv_Robot_Ground ) ) , break5.b);
			o.Albedo = lerpResult9.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
-100;45;1582;893;2067.218;437.2447;1.460573;True;True
Node;AmplifyShaderEditor.TexturePropertyNode;11;-1528.088,-375.3202;Inherit;True;Property;_Human_Ground;Human_Ground;0;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ColorNode;1;-1271.5,-182.8;Inherit;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;False;0;1,0,0.3087969,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;16;-1530.765,6.329605;Inherit;True;Property;_Other_Ground;Other_Ground;2;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.VertexColorNode;4;-1528.001,-630.3693;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-1271.988,-376.6201;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-919.7992,-631.693;Inherit;False;Constant;_Color3;Color 3;0;0;Create;True;0;0;False;0;1,0.5,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;5;-1272.003,-630.3693;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ToggleSwitchNode;12;-922.2856,-246.621;Inherit;False;Property;_Human_Colour;Human_Colour;3;0;Create;True;0;0;False;0;1;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;13;-1530.878,451.7484;Inherit;True;Property;_Robot_Ground;Robot_Ground;1;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;17;-1274.665,5.029594;Inherit;True;Property;_TextureSample2;Texture Sample 1;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-1273.286,232.8124;Inherit;False;Constant;_Color1;Color 1;0;0;Create;True;0;0;False;0;0.5355395,1,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-632.7,-247.2996;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;14;-1274.778,451.4484;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-1272.053,646.0424;Inherit;False;Constant;_Color2;Color 2;0;0;Create;True;0;0;False;0;0,0.2865825,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;18;-923.0393,136.0297;Inherit;False;Property;_Other_Colour;Other_Colour;4;0;Create;True;0;0;False;0;1;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;8;-375.4144,-118.7455;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;15;-923.1517,580.9879;Inherit;False;Property;_Robot_Colour;Robot_Colour;3;0;Create;True;0;0;False;0;1;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;9;-153.0142,8.142588;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;ASEMaterialInspector;0;0;Standard;SH_VertexPaint;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;11;0
WireConnection;5;0;4;0
WireConnection;12;0;10;0
WireConnection;12;1;1;0
WireConnection;17;0;16;0
WireConnection;6;0;7;0
WireConnection;6;1;12;0
WireConnection;6;2;5;0
WireConnection;14;0;13;0
WireConnection;18;0;17;0
WireConnection;18;1;2;0
WireConnection;8;0;6;0
WireConnection;8;1;18;0
WireConnection;8;2;5;1
WireConnection;15;0;14;0
WireConnection;15;1;3;0
WireConnection;9;0;8;0
WireConnection;9;1;15;0
WireConnection;9;2;5;2
WireConnection;0;0;9;0
ASEEND*/
//CHKSM=6E87AF0496CD44E664E5E0E1E0F3AEC69B026AE3