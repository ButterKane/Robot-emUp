// Upgrade NOTE: upgraded instancing buffer 'NewAmplifyShader' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "New Amplify Shader"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_MetalTint01("Metal Tint 01", Color) = (0,0,0,0)
		_RustColor("Rust Color", Color) = (0,0,0,0)
		_DirtColor("Dirt Color", Color) = (0,0,0,0)
		_MetalTint02("Metal Tint 02", Color) = (0,0,0,0)
		_MetalTint03("Metal Tint 03", Color) = (0,0,0,0)
		[HDR]_EmissiveCOlor("Emissive COlor", Color) = (6.09044,0.5189164,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample0;

		UNITY_INSTANCING_BUFFER_START(NewAmplifyShader)
			UNITY_DEFINE_INSTANCED_PROP(float4, _MetalTint01)
#define _MetalTint01_arr NewAmplifyShader
			UNITY_DEFINE_INSTANCED_PROP(float4, _MetalTint02)
#define _MetalTint02_arr NewAmplifyShader
			UNITY_DEFINE_INSTANCED_PROP(float4, _MetalTint03)
#define _MetalTint03_arr NewAmplifyShader
			UNITY_DEFINE_INSTANCED_PROP(float4, _TextureSample0_ST)
#define _TextureSample0_ST_arr NewAmplifyShader
			UNITY_DEFINE_INSTANCED_PROP(float4, _RustColor)
#define _RustColor_arr NewAmplifyShader
			UNITY_DEFINE_INSTANCED_PROP(float4, _DirtColor)
#define _DirtColor_arr NewAmplifyShader
			UNITY_DEFINE_INSTANCED_PROP(float4, _EmissiveCOlor)
#define _EmissiveCOlor_arr NewAmplifyShader
		UNITY_INSTANCING_BUFFER_END(NewAmplifyShader)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 _MetalTint01_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint01_arr, _MetalTint01);
			float4 lerpResult22 = lerp( float4( 0,0,0,0 ) , _MetalTint01_Instance , i.vertexColor.r);
			float4 _MetalTint02_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint02_arr, _MetalTint02);
			float4 lerpResult24 = lerp( lerpResult22 , _MetalTint02_Instance , i.vertexColor.g);
			float4 _MetalTint03_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetalTint03_arr, _MetalTint03);
			float4 lerpResult27 = lerp( lerpResult24 , _MetalTint03_Instance , i.vertexColor.b);
			float4 _TextureSample0_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_TextureSample0_ST_arr, _TextureSample0_ST);
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST_Instance.xy + _TextureSample0_ST_Instance.zw;
			float4 tex2DNode1 = tex2D( _TextureSample0, uv_TextureSample0 );
			float4 _RustColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_RustColor_arr, _RustColor);
			float4 lerpResult7 = lerp( ( lerpResult27 + tex2DNode1.r ) , _RustColor_Instance , tex2DNode1.b);
			float4 _DirtColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_DirtColor_arr, _DirtColor);
			float4 lerpResult9 = lerp( lerpResult7 , _DirtColor_Instance , tex2DNode1.g);
			o.Albedo = lerpResult9.rgb;
			float4 _EmissiveCOlor_Instance = UNITY_ACCESS_INSTANCED_PROP(_EmissiveCOlor_arr, _EmissiveCOlor);
			float4 lerpResult34 = lerp( float4( 0,0,0,0 ) , _EmissiveCOlor_Instance , i.vertexColor.a);
			o.Emission = lerpResult34.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
22;15;2492;1301;3416.14;1355.557;1.861208;True;True
Node;AmplifyShaderEditor.CommentaryNode;31;-1906.826,-1017.56;Inherit;False;991.446;633.9523;set on VC RGB Mask;7;27;11;21;24;22;26;29;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;11;-1859.826,-687.2427;Inherit;False;InstancedProperty;_MetalTint01;Metal Tint 01;1;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;21;-1858.372,-893.5576;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;22;-1569.853,-705.5576;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;26;-1623.061,-967.5602;Inherit;False;InstancedProperty;_MetalTint02;Metal Tint 02;5;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;29;-1393.428,-590.608;Inherit;False;InstancedProperty;_MetalTint03;Metal Tint 03;6;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;24;-1344.117,-872.2885;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-1149.738,-202.7414;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;27;-1099.38,-762.0867;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;-585.2235,-243.6281;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;-777.1703,90.14893;Inherit;False;InstancedProperty;_RustColor;Rust Color;2;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;33;-884.2837,730.341;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;35;-933.3431,559.9474;Inherit;False;InstancedProperty;_EmissiveCOlor;Emissive COlor;7;1;[HDR];Create;True;0;0;False;0;6.09044,0.5189164,0,0;6.09044,0.5189164,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;7;-335.7369,-62.74142;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;15;-433.1703,104.1489;Inherit;False;InstancedProperty;_DirtColor;Dirt Color;3;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-1095.471,227.2762;Inherit;False;InstancedProperty;_GrayscaleContrat;GrayscaleContrat;4;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;16;-845.9875,-58.39272;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-799.2367,-237.3416;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-646.6313,-31.35103;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;-184.7369,3.258575;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;34;-465.3431,541.9474;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;ASEMaterialInspector;0;0;Standard;New Amplify Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;1;11;0
WireConnection;22;2;21;1
WireConnection;24;0;22;0
WireConnection;24;1;26;0
WireConnection;24;2;21;2
WireConnection;27;0;24;0
WireConnection;27;1;29;0
WireConnection;27;2;21;3
WireConnection;32;0;27;0
WireConnection;32;1;1;1
WireConnection;7;0;32;0
WireConnection;7;1;14;0
WireConnection;7;2;1;3
WireConnection;16;0;1;1
WireConnection;16;1;17;0
WireConnection;2;0;27;0
WireConnection;2;1;1;1
WireConnection;20;0;17;0
WireConnection;20;1;16;0
WireConnection;9;0;7;0
WireConnection;9;1;15;0
WireConnection;9;2;1;2
WireConnection;34;1;35;0
WireConnection;34;2;33;4
WireConnection;0;0;9;0
WireConnection;0;2;34;0
ASEEND*/
//CHKSM=B2867073B93676CC3679D5DBF41B897C96B80E81