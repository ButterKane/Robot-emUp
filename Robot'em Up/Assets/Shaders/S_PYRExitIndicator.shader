// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_PYRExitIndicator"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_PannerSpeed("PannerSpeed", Float) = 0
		_TilingX("TilingX", Float) = 1
		_MainColor("MainColor", Color) = (0,0,0,0)
		_MainColor1("SecondaryColor", Color) = (0,0,0,0)
		_AlphaAdder("AlphaAdder", Range( 0 , 1)) = 0.1
		_MinIntensity("MinIntensity", Float) = 0
		_MaxIntensity("MaxIntensity", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _MainColor1;
		uniform float4 _MainColor;
		uniform sampler2D _TextureSample0;
		uniform float _PannerSpeed;
		uniform float _TilingX;
		uniform float _AlphaAdder;
		uniform float _MinIntensity;
		uniform float _MaxIntensity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult5 = (float2(_PannerSpeed , 0.0));
			float2 appendResult16 = (float2(_TilingX , 1.0));
			float2 uv_TexCoord14 = i.uv_texcoord * appendResult16;
			float2 panner2 = ( 1.0 * _Time.y * appendResult5 + uv_TexCoord14);
			float clampResult11 = clamp( ( tex2D( _TextureSample0, panner2 ).r + _AlphaAdder ) , 0.0 , 1.0 );
			float4 lerpResult17 = lerp( _MainColor1 , _MainColor , clampResult11);
			float lerpResult13 = lerp( _MinIntensity , _MaxIntensity , clampResult11);
			o.Albedo = ( lerpResult17 * lerpResult13 ).rgb;
			o.Alpha = clampResult11;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
114;65;1309;774;687.2146;614.0509;1.3;True;True
Node;AmplifyShaderEditor.RangedFloatNode;15;-1522.343,38.05709;Inherit;False;Property;_TilingX;TilingX;2;0;Create;True;0;0;False;0;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-1139.079,131.9456;Inherit;False;Property;_PannerSpeed;PannerSpeed;1;0;Create;True;0;0;False;0;0;-0.94;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;16;-1336.343,36.05709;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-1187.151,14.55246;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;5;-953.0787,129.9456;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;2;-944.0787,9.945539;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;1;-748.0118,-20.02006;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;-1;b8c01fea7fb09f04580ad11e45ea3dad;1edd9b1f06baf814a819c5d75a2a677a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-613.4158,-126.5874;Inherit;False;Property;_AlphaAdder;AlphaAdder;5;0;Create;True;0;0;False;0;0.1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-470.0159,11.8129;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;11;-347.1159,8.912897;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;8;-282.5635,-497.1015;Inherit;False;Property;_MainColor;MainColor;3;0;Create;True;0;0;False;0;0,0,0,0;1,0.03301889,0.03301889,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-276.6018,-689.8671;Inherit;False;Property;_MainColor1;SecondaryColor;4;0;Create;True;0;0;False;0;0,0,0,0;0.735849,0.007595105,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;20;-250.5623,-260.9294;Inherit;False;Property;_MinIntensity;MinIntensity;6;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-252.1699,-175.7235;Inherit;False;Property;_MaxIntensity;MaxIntensity;7;0;Create;True;0;0;False;0;0;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;17;50.63018,-504.231;Inherit;False;3;0;COLOR;1,0,0,0;False;1;COLOR;3,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;13;44.10538,-202.5892;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;3;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;282.3841,-76.96153;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;508.7999,-53.79999;Float;False;True;2;ASEMaterialInspector;0;0;Standard;S_PYRExitIndicator;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;16;0;15;0
WireConnection;14;0;16;0
WireConnection;5;0;4;0
WireConnection;2;0;14;0
WireConnection;2;2;5;0
WireConnection;1;1;2;0
WireConnection;9;0;1;1
WireConnection;9;1;10;0
WireConnection;11;0;9;0
WireConnection;17;0;19;0
WireConnection;17;1;8;0
WireConnection;17;2;11;0
WireConnection;13;0;20;0
WireConnection;13;1;21;0
WireConnection;13;2;11;0
WireConnection;12;0;17;0
WireConnection;12;1;13;0
WireConnection;0;0;12;0
WireConnection;0;9;11;0
ASEEND*/
//CHKSM=38EAA8D76E508DECE42FBC8C517361C7F2772889