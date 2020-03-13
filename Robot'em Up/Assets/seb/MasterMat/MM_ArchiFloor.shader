// Upgrade NOTE: upgraded instancing buffer 'MM_ArchiFLoor' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MM_ArchiFLoor"
{
	Properties
	{
		_NoiseMark("NoiseMark", 2D) = "white" {}
		_NoiseFFloor("NoiseFFloor", 2D) = "white" {}
		_NoiuseMarkingTiling("Noiuse Marking Tiling", Float) = 1
		_NoiseFloorTiling("Noise Floor Tiling", Float) = 1
		_Marktexture("Mark texture", 2D) = "white" {}
		_MarkingCOlor("Marking COlor", Color) = (0.745283,0.6245149,0,0)
		_FlorCOlor("Flor COlor", Color) = (0.509434,0.509434,0.509434,0)
		_NoiseMarkInt("Noise Mark Int", Float) = 0
		_NoiseMarkSharpness("Noise Mark Sharpness", Float) = 1
		_noiseFloorint("noise Floor int", Float) = 0
		_NoiseFloorSharpness("Noise Floor Sharpness", Float) = 1
		_51612536_553196668424338_6330084407579246592_n("51612536_553196668424338_6330084407579246592_n", 2D) = "white" {}
		_DetailMaskCOlor("Detail Mask COlor", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform sampler2D _NoiseFFloor;
		uniform float4 _DetailMaskCOlor;
		uniform sampler2D _51612536_553196668424338_6330084407579246592_n;
		uniform float4 _MarkingCOlor;
		uniform sampler2D _Marktexture;
		uniform sampler2D _NoiseMark;

		UNITY_INSTANCING_BUFFER_START(MM_ArchiFLoor)
			UNITY_DEFINE_INSTANCED_PROP(float4, _FlorCOlor)
#define _FlorCOlor_arr MM_ArchiFLoor
			UNITY_DEFINE_INSTANCED_PROP(float4, _51612536_553196668424338_6330084407579246592_n_ST)
#define _51612536_553196668424338_6330084407579246592_n_ST_arr MM_ArchiFLoor
			UNITY_DEFINE_INSTANCED_PROP(float, _NoiseFloorTiling)
#define _NoiseFloorTiling_arr MM_ArchiFLoor
			UNITY_DEFINE_INSTANCED_PROP(float, _NoiseFloorSharpness)
#define _NoiseFloorSharpness_arr MM_ArchiFLoor
			UNITY_DEFINE_INSTANCED_PROP(float, _noiseFloorint)
#define _noiseFloorint_arr MM_ArchiFLoor
			UNITY_DEFINE_INSTANCED_PROP(float, _NoiuseMarkingTiling)
#define _NoiuseMarkingTiling_arr MM_ArchiFLoor
			UNITY_DEFINE_INSTANCED_PROP(float, _NoiseMarkInt)
#define _NoiseMarkInt_arr MM_ArchiFLoor
			UNITY_DEFINE_INSTANCED_PROP(float, _NoiseMarkSharpness)
#define _NoiseMarkSharpness_arr MM_ArchiFLoor
		UNITY_INSTANCING_BUFFER_END(MM_ArchiFLoor)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float _NoiseFloorTiling_Instance = UNITY_ACCESS_INSTANCED_PROP(_NoiseFloorTiling_arr, _NoiseFloorTiling);
			float _NoiseFloorSharpness_Instance = UNITY_ACCESS_INSTANCED_PROP(_NoiseFloorSharpness_arr, _NoiseFloorSharpness);
			float4 _FlorCOlor_Instance = UNITY_ACCESS_INSTANCED_PROP(_FlorCOlor_arr, _FlorCOlor);
			float _noiseFloorint_Instance = UNITY_ACCESS_INSTANCED_PROP(_noiseFloorint_arr, _noiseFloorint);
			float4 lerpResult33 = lerp( saturate( ( tex2D( _NoiseFFloor, ( (ase_worldPos).xz / _NoiseFloorTiling_Instance ) ) / _NoiseFloorSharpness_Instance ) ) , _FlorCOlor_Instance , _noiseFloorint_Instance);
			float4 _51612536_553196668424338_6330084407579246592_n_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_51612536_553196668424338_6330084407579246592_n_ST_arr, _51612536_553196668424338_6330084407579246592_n_ST);
			float2 uv_51612536_553196668424338_6330084407579246592_n = i.uv_texcoord * _51612536_553196668424338_6330084407579246592_n_ST_Instance.xy + _51612536_553196668424338_6330084407579246592_n_ST_Instance.zw;
			float4 lerpResult38 = lerp( ( lerpResult33 * _FlorCOlor_Instance ) , _DetailMaskCOlor , tex2D( _51612536_553196668424338_6330084407579246592_n, uv_51612536_553196668424338_6330084407579246592_n ).r);
			float4 temp_cast_0 = (tex2D( _Marktexture, i.uv_texcoord ).r).xxxx;
			float _NoiuseMarkingTiling_Instance = UNITY_ACCESS_INSTANCED_PROP(_NoiuseMarkingTiling_arr, _NoiuseMarkingTiling);
			float cos41 = cos( 0.5 );
			float sin41 = sin( 0.5 );
			float2 rotator41 = mul( ( (ase_worldPos).xz / _NoiuseMarkingTiling_Instance ) - float2( 0,0 ) , float2x2( cos41 , -sin41 , sin41 , cos41 )) + float2( 0,0 );
			float _NoiseMarkInt_Instance = UNITY_ACCESS_INSTANCED_PROP(_NoiseMarkInt_arr, _NoiseMarkInt);
			float _NoiseMarkSharpness_Instance = UNITY_ACCESS_INSTANCED_PROP(_NoiseMarkSharpness_arr, _NoiseMarkSharpness);
			float4 lerpResult49 = lerp( lerpResult38 , _MarkingCOlor , saturate( ( ( temp_cast_0 - ( tex2D( _NoiseMark, rotator41 ) * _NoiseMarkInt_Instance ) ) / _NoiseMarkSharpness_Instance ) ));
			o.Albedo = lerpResult49.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
236;548;1762;1431;2522.445;679.4081;1.637305;True;True
Node;AmplifyShaderEditor.CommentaryNode;44;-3359.039,-35.54653;Inherit;False;2350.157;623.9853;Noise and Color // Road Marking;16;2;6;3;5;42;41;27;24;4;8;23;25;21;22;28;17;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;46;-3233.912,-928.2343;Inherit;False;2082.506;864.4288; Noise and Color // Concrete;15;38;14;36;34;19;35;31;33;18;40;37;10;11;12;13;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;2;-3308.004,274.5661;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;10;-3183.912,-844.8665;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;6;-3032.012,414.0956;Inherit;False;InstancedProperty;_NoiuseMarkingTiling;Noiuse Marking Tiling;2;0;Create;True;0;0;False;0;1;42.93;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;3;-3113.039,262.5661;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-2906.884,-705.3371;Inherit;False;InstancedProperty;_NoiseFloorTiling;Noise Floor Tiling;3;0;Create;True;0;0;False;0;1;45.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;11;-2978.912,-849.8665;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-2704.826,463.5626;Inherit;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;5;-2834.012,274.0957;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;13;-2708.884,-845.3374;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;41;-2581.862,320.5625;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0.25;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2488.062,-671.234;Inherit;False;InstancedProperty;_NoiseFloorSharpness;Noise Floor Sharpness;10;0;Create;True;0;0;False;0;1;2.23;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-2572.183,-875.7033;Inherit;True;Property;_NoiseFFloor;NoiseFFloor;1;0;Create;True;0;0;False;0;-1;a8d2541ae14a15e42b3830ac3a68a0d8;c43b86301d5c2a946b7e575dc1c9a46e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;-2170.855,473.4388;Inherit;False;InstancedProperty;_NoiseMarkInt;Noise Mark Int;7;0;Create;True;0;0;False;0;0;2.33;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-2361.341,245.5661;Inherit;True;Property;_NoiseMark;NoiseMark;0;0;Create;True;0;0;False;0;-1;a8d2541ae14a15e42b3830ac3a68a0d8;c43b86301d5c2a946b7e575dc1c9a46e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-2728.253,43.58109;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;34;-2212.617,-846.5344;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-1894.62,320.5912;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;8;-2319.883,14.45347;Inherit;True;Property;_Marktexture;Mark texture;4;0;Create;True;0;0;False;0;-1;1c089cfa53f963a4ebbc0a512b6d8443;3ec8dcd850907854a92c1ed810903cb5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-2431.309,-514.474;Inherit;False;InstancedProperty;_FlorCOlor;Flor COlor;6;0;Create;True;0;0;False;0;0.509434,0.509434,0.509434,0;0.6792453,0.6792453,0.6792453,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;35;-2075.062,-878.2343;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-2441.446,-596.0019;Inherit;False;InstancedProperty;_noiseFloorint;noise Floor int;9;0;Create;True;0;0;False;0;0;0.82;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;33;-1903.824,-787.3783;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1647.883,366.4535;Inherit;False;InstancedProperty;_NoiseMarkSharpness;Noise Mark Sharpness;8;0;Create;True;0;0;False;0;1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;21;-1695.883,254.4535;Inherit;False;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;22;-1358.812,254.4535;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;40;-1839.88,-477.3794;Inherit;False;Property;_DetailMaskCOlor;Detail Mask COlor;12;0;Create;True;0;0;False;0;0,0,0,0;0.245283,0.245283,0.245283,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;37;-1834.06,-293.8054;Inherit;True;Property;_51612536_553196668424338_6330084407579246592_n;51612536_553196668424338_6330084407579246592_n;11;0;Create;True;0;0;False;0;-1;b0198540b2f723b499ae6441d44aa369;b0198540b2f723b499ae6441d44aa369;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1734.172,-785.6153;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;38;-1335.407,-575.356;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;17;-1614.048,36.38109;Inherit;False;Property;_MarkingCOlor;Marking COlor;5;0;Create;True;0;0;False;0;0.745283,0.6245149,0,0;0.5471698,0.4578948,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;28;-1215.978,252.3135;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;49;-558.6998,19.13388;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-168.8454,24.62493;Float;False;True;2;ASEMaterialInspector;0;0;Standard;MM_ArchiFLoor;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;2;0
WireConnection;11;0;10;0
WireConnection;5;0;3;0
WireConnection;5;1;6;0
WireConnection;13;0;11;0
WireConnection;13;1;12;0
WireConnection;41;0;5;0
WireConnection;41;2;42;0
WireConnection;14;1;13;0
WireConnection;4;1;41;0
WireConnection;34;0;14;0
WireConnection;34;1;36;0
WireConnection;23;0;4;0
WireConnection;23;1;24;0
WireConnection;8;1;27;0
WireConnection;35;0;34;0
WireConnection;33;0;35;0
WireConnection;33;1;19;0
WireConnection;33;2;31;0
WireConnection;21;0;8;1
WireConnection;21;1;23;0
WireConnection;22;0;21;0
WireConnection;22;1;25;0
WireConnection;18;0;33;0
WireConnection;18;1;19;0
WireConnection;38;0;18;0
WireConnection;38;1;40;0
WireConnection;38;2;37;1
WireConnection;28;0;22;0
WireConnection;49;0;38;0
WireConnection;49;1;17;0
WireConnection;49;2;28;0
WireConnection;0;0;49;0
ASEEND*/
//CHKSM=69CED5EB44D5C89739C13F76CE4D16FDC440828B