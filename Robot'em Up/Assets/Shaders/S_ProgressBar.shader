// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "New Amplify Shader"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Progression("Progression", Range( 0 , 1)) = 1
		_Appearance("Appearance", Range( 0 , 1)) = 1
		_EmissiveIntensity("EmissiveIntensity", Float) = 1
		_LinkChargedColor("LinkChargedColor", Color) = (1,0.9190924,0,1)
		_LinkUnchargedColor("LinkUnchargedColor", Color) = (0,0,0,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _LinkUnchargedColor;
		uniform float4 _LinkChargedColor;
		uniform float _Progression;
		uniform float _EmissiveIntensity;
		uniform float _Appearance;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_7_0 = ( ( 1.0 - i.uv_texcoord.x ) - 1.0 );
			float4 lerpResult18 = lerp( _LinkUnchargedColor , _LinkChargedColor , ceil( ( temp_output_7_0 + _Progression ) ));
			o.Emission = ( lerpResult18 * _EmissiveIntensity ).rgb;
			o.Alpha = 1;
			clip( ceil( ( temp_output_7_0 + _Appearance ) ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
68;59;1309;780;540.9716;272.1709;1.3563;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-755,-53;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;2;-515,-36;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;7;-333,-28;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-450,200;Inherit;False;Property;_Progression;Progression;1;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-174,-26;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-112,-200;Inherit;False;Property;_LinkChargedColor;LinkChargedColor;4;0;Create;True;0;0;False;0;1,0.9190924,0,1;0,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-112,-385;Inherit;False;Property;_LinkUnchargedColor;LinkUnchargedColor;5;0;Create;True;0;0;False;0;0,0,0,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;-454.9247,406.472;Inherit;False;Property;_Appearance;Appearance;2;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;9;-39,-27;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;383,111;Inherit;False;Property;_EmissiveIntensity;EmissiveIntensity;3;0;Create;True;0;0;False;0;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;218,-100;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-141.1247,392.672;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;429,-12;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CeilOpNode;22;-1.163891,393.7727;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;657,-100;Float;False;True;2;ASEMaterialInspector;0;0;Standard;New Amplify Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;1
WireConnection;7;0;2;0
WireConnection;8;0;7;0
WireConnection;8;1;5;0
WireConnection;9;0;8;0
WireConnection;18;0;19;0
WireConnection;18;1;15;0
WireConnection;18;2;9;0
WireConnection;20;0;7;0
WireConnection;20;1;21;0
WireConnection;10;0;18;0
WireConnection;10;1;11;0
WireConnection;22;0;20;0
WireConnection;0;2;10;0
WireConnection;0;10;22;0
ASEEND*/
//CHKSM=75D26CCD9E058D52E5B64C2AD2EEEAAAACD97853