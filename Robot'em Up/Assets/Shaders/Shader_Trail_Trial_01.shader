// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shader_Trail_Trial_01"
{
	Properties
	{
		[Toggle(_KEYWORD0_ON)] _Keyword0("Keyword 0", Float) = 1
		_Texture0("Texture 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature _KEYWORD0_ON
		#pragma surface surf Unlit alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _Texture0;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 color1 = IsGammaSpace() ? float4(0,0.5550179,1,0) : float4(0,0.2685445,1,0);
			float4 color2 = IsGammaSpace() ? float4(0.03621269,0,0.5943396,0) : float4(0.00280284,0,0.3119799,0);
			float u6 = i.uv_texcoord.x;
			float4 lerpResult3 = lerp( color1 , color2 , ( u6 * 1.0 ));
			float mulTime10 = _Time.y * 1.2;
			float Time13 = mulTime10;
			float2 uv5 = i.uv_texcoord;
			float2 panner14 = ( Time13 * float2( 0.2,0 ) + uv5);
			float2 panner57 = ( Time13 * float2( -0.8,0 ) + ( uv5 + ( ( ( (tex2D( _Texture0, panner14 )).rg + -0.5 ) * 2.0 ) * 0.3 * u6 ) ));
			float2 panner37 = ( Time13 * float2( 0.2,0 ) + uv5);
			float clampResult40 = clamp( ( ( ( 1.0 - u6 ) + -0.55 ) * 2.0 ) , 0.25 , 0.75 );
			float clampResult50 = clamp( ( ( tex2D( _Texture0, panner37 ).g + clampResult40 ) - ( u6 * 0.9 ) ) , 0.0 , 1.0 );
			float clampResult31 = clamp( ( i.uv_texcoord.y * ( 1.0 - i.uv_texcoord.y ) * ( 1.0 - u6 ) * 6.0 ) , 0.0 , 1.0 );
			float Mask33 = ( clampResult31 * 1.0 );
			float temp_output_25_0 = ( tex2D( _Texture0, panner57 ).b * clampResult50 * i.vertexColor.a * Mask33 );
			#ifdef _KEYWORD0_ON
				float staticSwitch23 = temp_output_25_0;
			#else
				float staticSwitch23 = 1.0;
			#endif
			o.Emission = ( ( lerpResult3 * i.vertexColor * 4 ) * staticSwitch23 ).rgb;
			o.Alpha = temp_output_25_0;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
14;108;1881;893;4400.008;815.9436;2.997202;True;True
Node;AmplifyShaderEditor.RangedFloatNode;12;-3507.704,738.4348;Inherit;False;Constant;_TimeScale;TimeScale;0;0;Create;True;0;0;False;0;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-3349.484,611.5381;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;10;-3351.067,741.5669;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;13;-3094.114,743.1406;Inherit;False;Time;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;5;-3094.122,611.5311;Inherit;False;uv;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;42;-2837.26,360.2571;Inherit;True;Property;_Texture0;Texture 0;1;0;Create;True;0;0;False;0;4de56bde30c85674fa9c8a71afd3903e;4de56bde30c85674fa9c8a71afd3903e;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PannerNode;14;-2838.745,611.5293;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.2,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-3102.825,1027.047;Inherit;False;u;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;43;-2551.653,613.3331;Inherit;True;Property;_TextureSample1;Texture Sample 1;2;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;34;-2839.432,-28.41882;Inherit;False;6;u;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2871.63,1122.239;Inherit;False;Constant;_Float1;Float 1;1;0;Create;True;0;0;False;0;6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-2871.631,1028.13;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-2839.909,68.10817;Inherit;False;5;uv;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;38;-2581.163,228.9575;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;-2840.209,162.5173;Inherit;False;13;Time;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;27;-3097.487,932.453;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;44;-2195.71,612.2921;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-1939.648,743.312;Inherit;False;Constant;_FlowStrength;FlowStrength;3;0;Create;True;0;0;False;0;0.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;45;-1939.087,614.215;Inherit;False;ConstantBiasScale;-1;;4;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT2;0,0;False;1;FLOAT;-0.5;False;2;FLOAT;2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;-1943.447,837.5768;Inherit;False;6;u;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;39;-2388.761,228.9576;Inherit;False;ConstantBiasScale;-1;;5;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;-0.55;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;37;-2583.044,68.97552;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.2,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-2647.337,930.8846;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-1689.16,544.0596;Inherit;False;5;uv;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-1654.627,626.4212;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;41;-2388.758,35.25725;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;31;-2393.249,930.8845;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;40;-2130.06,228.9575;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.25;False;2;FLOAT;0.75;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;56;-1464.16,675.0596;Inherit;False;13;Time;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-2206.602,930.8845;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-1941.474,-25.94968;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;55;-1463.16,547.0596;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-1976.265,100.172;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;49;-1752.238,-27.46149;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;33;-1946.173,929.8846;Inherit;False;Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;15;-1019.503,135.6272;Inherit;False;6;u;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;57;-1273.16,549.0596;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.8,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-1016.754,581.4448;Inherit;False;33;Mask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;46;-1017.197,357.9849;Inherit;True;Property;_TextureSample2;Texture Sample 2;2;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;1;-890.1509,-248.2691;Inherit;False;Constant;_ColourStart;ColourStart;0;0;Create;True;0;0;False;0;0,0.5550179,1,0;0.8721447,0,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-825.0119,135.6272;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;21;-608.5615,96.41534;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-890.9096,-59.86013;Inherit;False;Constant;_ColourEnd;ColourEnd;0;0;Create;True;0;0;False;0;0.03621269,0,0.5943396,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;50;-1494.854,-27.46143;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;3;-605.0229,-248.3439;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-603.8565,264.2426;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;20;-606.9929,-7.104237;Inherit;False;Constant;_Emission;Emission;0;0;Create;True;0;0;False;0;4;0;0;1;INT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-605.4245,357.7189;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-313.6875,-124.74;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;INT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;23;-379.5634,261.1056;Inherit;False;Property;_Keyword0;Keyword 0;0;0;Create;True;0;0;False;0;0;1;1;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-150.5656,-55.72711;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;205.4707,-7.842391;Float;False;True;2;ASEMaterialInspector;0;0;Unlit;Shader_Trail_Trial_01;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;12;0
WireConnection;13;0;10;0
WireConnection;5;0;4;0
WireConnection;14;0;5;0
WireConnection;14;1;13;0
WireConnection;6;0;4;1
WireConnection;43;0;42;0
WireConnection;43;1;14;0
WireConnection;28;0;6;0
WireConnection;38;0;34;0
WireConnection;27;0;4;2
WireConnection;44;0;43;0
WireConnection;45;3;44;0
WireConnection;39;3;38;0
WireConnection;37;0;35;0
WireConnection;37;1;36;0
WireConnection;29;0;4;2
WireConnection;29;1;27;0
WireConnection;29;2;28;0
WireConnection;29;3;30;0
WireConnection;52;0;45;0
WireConnection;52;1;51;0
WireConnection;52;2;53;0
WireConnection;41;0;42;0
WireConnection;41;1;37;0
WireConnection;31;0;29;0
WireConnection;40;0;39;0
WireConnection;32;0;31;0
WireConnection;48;0;34;0
WireConnection;55;0;54;0
WireConnection;55;1;52;0
WireConnection;47;0;41;2
WireConnection;47;1;40;0
WireConnection;49;0;47;0
WireConnection;49;1;48;0
WireConnection;33;0;32;0
WireConnection;57;0;55;0
WireConnection;57;1;56;0
WireConnection;46;0;42;0
WireConnection;46;1;57;0
WireConnection;17;0;15;0
WireConnection;50;0;49;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;3;2;17;0
WireConnection;25;0;46;3
WireConnection;25;1;50;0
WireConnection;25;2;21;4
WireConnection;25;3;58;0
WireConnection;18;0;3;0
WireConnection;18;1;21;0
WireConnection;18;2;20;0
WireConnection;23;1;19;0
WireConnection;23;0;25;0
WireConnection;22;0;18;0
WireConnection;22;1;23;0
WireConnection;0;2;22;0
WireConnection;0;9;25;0
ASEEND*/
//CHKSM=B9ED2CA32E147D9E7C8C688F1377E63773A171BA