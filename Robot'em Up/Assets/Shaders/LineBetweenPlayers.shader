// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LineBetweenPlayers"
{
	Properties
	{
		_CurrentEnergyAmout("CurrentEnergyAmout", Range( 0 , 1)) = 0
		_BreakingLinkProgression("BreakingLinkProgression", Range( 0 , 1)) = 0
		_EmissiveIntensityWhenCharged("EmissiveIntensityWhenCharged", Float) = 1
		_ColorWhenCharged("ColorWhenCharged", Color) = (0,0.90622,1,1)
		_ColorWhenFullyCharged("ColorWhenFullyCharged", Color) = (0,0.90622,1,1)
		_ColorWhenNotCharged("ColorWhenNotCharged", Color) = (0,0.4644156,0.8301887,1)
		_ColorWhenBreaking("ColorWhenBreaking", Color) = (0,0.4644156,0.8301887,1)
		_DepthFadeDistance("DepthFadeDistance", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float4 screenPosition42;
			float2 uv_texcoord;
		};

		uniform float4 _ColorWhenNotCharged;
		uniform float _CurrentEnergyAmout;
		uniform float4 _ColorWhenFullyCharged;
		uniform float4 _ColorWhenCharged;
		uniform float _EmissiveIntensityWhenCharged;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _DepthFadeDistance;
		uniform float4 _ColorWhenBreaking;
		uniform float _BreakingLinkProgression;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 vertexPos42 = ase_vertex3Pos;
			float4 ase_screenPos42 = ComputeScreenPos( UnityObjectToClipPos( vertexPos42 ) );
			o.screenPosition42 = ase_screenPos42;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ifLocalVar41 = 0;
			if( _CurrentEnergyAmout >= 1.0 )
				ifLocalVar41 = _ColorWhenFullyCharged;
			else
				ifLocalVar41 = _ColorWhenCharged;
			float4 temp_output_17_0 = ( ifLocalVar41 * _EmissiveIntensityWhenCharged );
			float4 ase_screenPos42 = i.screenPosition42;
			float4 ase_screenPosNorm42 = ase_screenPos42 / ase_screenPos42.w;
			ase_screenPosNorm42.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm42.z : ase_screenPosNorm42.z * 0.5 + 0.5;
			float screenDepth42 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm42.xy ));
			float distanceDepth42 = abs( ( screenDepth42 - LinearEyeDepth( ase_screenPosNorm42.z ) ) / ( _DepthFadeDistance ) );
			float clampResult48 = clamp( ( 1.0 - distanceDepth42 ) , 0.0 , 1.0 );
			float4 lerpResult44 = lerp( _ColorWhenNotCharged , temp_output_17_0 , clampResult48);
			float temp_output_9_0 = (i.uv_texcoord).x;
			float temp_output_3_0 = ( 1.0 - i.uv_texcoord.x );
			float temp_output_20_0 = ( 1.0 - step( ( ( (0.0 + (temp_output_9_0 - 0.0) * (1.0 - 0.0) / (0.5 - 0.0)) * step( temp_output_9_0 , 0.5 ) ) + ( (0.0 + (temp_output_3_0 - 0.0) * (1.0 - 0.0) / (0.5 - 0.0)) * step( temp_output_3_0 , 0.5 ) ) ) , ( 1.0 - _CurrentEnergyAmout ) ) );
			float4 lerpResult16 = lerp( lerpResult44 , temp_output_17_0 , temp_output_20_0);
			float4 lerpResult35 = lerp( lerpResult16 , _ColorWhenBreaking , _BreakingLinkProgression);
			o.Emission = lerpResult35.rgb;
			float lerpResult49 = lerp( _ColorWhenNotCharged.a , _ColorWhenCharged.a , clampResult48);
			float lerpResult19 = lerp( lerpResult49 , _ColorWhenCharged.a , temp_output_20_0);
			float lerpResult38 = lerp( lerpResult19 , _ColorWhenBreaking.a , _BreakingLinkProgression);
			o.Alpha = lerpResult38;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
114;65;1309;774;1412.51;1470.395;2.0704;True;True
Node;AmplifyShaderEditor.CommentaryNode;39;-3000.306,-370.4296;Inherit;False;1478.185;1247.923;GRADIENT LINEAR;12;1;9;3;23;32;26;28;22;24;27;29;30;GRADIENT LINEAR;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-2950.306,66.05917;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;32;-2311.142,149.236;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-2332.502,762.4936;Inherit;False;Constant;_SemiStep;SemiStep;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;9;-2648.786,-173.9314;Inherit;True;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;3;-2638.385,437.7053;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;22;-2338.002,546.2933;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;28;-2339.24,290.4164;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;26;-2306.206,-320.4296;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;24;-2313.307,-67.64113;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-1615.666,-1213.031;Inherit;False;Property;_DepthFadeDistance;DepthFadeDistance;7;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;47;-1815.498,-1331.083;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;-1078.439,-375.4681;Inherit;False;Property;_CurrentEnergyAmout;CurrentEnergyAmout;0;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-1354.652,-1113.793;Inherit;False;Property;_ColorWhenCharged;ColorWhenCharged;3;0;Create;True;0;0;False;0;0,0.90622,1,1;0,0.90622,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;42;-1315.429,-1317.006;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-2023.097,-149.0649;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-2036.34,464.0162;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;40;-1281.286,-619.9403;Inherit;False;Property;_ColorWhenFullyCharged;ColorWhenFullyCharged;4;0;Create;True;0;0;False;0;0,0.90622,1,1;1,0.7668697,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-1757.121,-1.204329;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;21;-961.0188,1.577385;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-475.7235,-548.616;Inherit;False;Property;_EmissiveIntensityWhenCharged;EmissiveIntensityWhenCharged;2;0;Create;True;0;0;False;0;1;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;46;-956.0193,-1267.211;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;41;-553.4933,-720.4274;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;-1440.785,-1508.839;Inherit;False;Property;_ColorWhenNotCharged;ColorWhenNotCharged;5;0;Create;True;0;0;False;0;0,0.4644156,0.8301887,1;0,0.4644156,0.8301887,0.2784314;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;48;-746.3055,-1217.278;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;5;-941.9573,-99.47221;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-335.1216,-676.3909;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;49;171.3466,-646.3768;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;44;-546.3723,-1233.46;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;20;-724.3349,-97.75731;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;16;210.8062,-996.2709;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;19;399.2822,-382.7029;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;755.0261,-963.2097;Inherit;False;Property;_BreakingLinkProgression;BreakingLinkProgression;1;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;36;-878.0569,-1651.573;Inherit;False;Property;_ColorWhenBreaking;ColorWhenBreaking;6;0;Create;True;0;0;False;0;0,0.4644156,0.8301887,1;0.8313726,0.1241253,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;35;792.8082,-1530.038;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;38;1066.052,-653.6329;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1238.327,-896.8408;Float;False;True;2;ASEMaterialInspector;0;0;Standard;LineBetweenPlayers;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;1;0
WireConnection;3;0;1;1
WireConnection;22;0;3;0
WireConnection;22;1;23;0
WireConnection;28;0;3;0
WireConnection;26;0;9;0
WireConnection;24;0;9;0
WireConnection;24;1;32;0
WireConnection;42;1;47;0
WireConnection;42;0;45;0
WireConnection;27;0;26;0
WireConnection;27;1;24;0
WireConnection;29;0;28;0
WireConnection;29;1;22;0
WireConnection;30;0;27;0
WireConnection;30;1;29;0
WireConnection;21;0;7;0
WireConnection;46;0;42;0
WireConnection;41;0;7;0
WireConnection;41;2;40;0
WireConnection;41;3;40;0
WireConnection;41;4;15;0
WireConnection;48;0;46;0
WireConnection;5;0;30;0
WireConnection;5;1;21;0
WireConnection;17;0;41;0
WireConnection;17;1;18;0
WireConnection;49;0;14;4
WireConnection;49;1;15;4
WireConnection;49;2;48;0
WireConnection;44;0;14;0
WireConnection;44;1;17;0
WireConnection;44;2;48;0
WireConnection;20;0;5;0
WireConnection;16;0;44;0
WireConnection;16;1;17;0
WireConnection;16;2;20;0
WireConnection;19;0;49;0
WireConnection;19;1;15;4
WireConnection;19;2;20;0
WireConnection;35;0;16;0
WireConnection;35;1;36;0
WireConnection;35;2;33;0
WireConnection;38;0;19;0
WireConnection;38;1;36;4
WireConnection;38;2;33;0
WireConnection;0;2;35;0
WireConnection;0;9;38;0
ASEEND*/
//CHKSM=9A0E299326B57284EBCA4290CABF73235467814D