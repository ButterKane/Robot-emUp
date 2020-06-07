// Upgrade NOTE: upgraded instancing buffer 'MM_Folliage' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MM_Folliage"
{
	Properties
	{
		_GradientBlend("Gradient Blend", Color) = (0.4822432,0.7830189,0.2253026,0)
		_MainFoliageColor("Main Foliage Color", Color) = (0.322723,0.5471698,0.2038982,0)
		_HueShiftIntensity("HueShift Intensity", Float) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.2
		_T_Grass_test("T_Grass_test", 2D) = "white" {}
		_OutlineColor("Outline Color", Color) = (0.2338911,0.4528302,0.1901032,1)
		_Min_Clamp("Min_Clamp", Float) = 0.4
		_Float3("Float 3", Float) = 0.5
		_Max_Clamp("Max_Clamp", Float) = 0.25
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
		};

		uniform float _HueShiftIntensity;
		uniform float4 _MainFoliageColor;
		uniform sampler2D _T_Grass_test;
		uniform float4 _GradientBlend;
		uniform float _Min_Clamp;
		uniform float _Max_Clamp;
		uniform float _Float3;
		uniform float _Cutoff = 0.2;

		UNITY_INSTANCING_BUFFER_START(MM_Folliage)
			UNITY_DEFINE_INSTANCED_PROP(float4, _T_Grass_test_ST)
#define _T_Grass_test_ST_arr MM_Folliage
			UNITY_DEFINE_INSTANCED_PROP(float4, _OutlineColor)
#define _OutlineColor_arr MM_Folliage
		UNITY_INSTANCING_BUFFER_END(MM_Folliage)


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color153 = IsGammaSpace() ? float4(0.6792453,0.6294477,0.259523,0) : float4(0.418999,0.3540061,0.05477216,0);
			float3 normalizeResult2_g4 = normalize( float3(1,1,1) );
			float4 transform101 = mul(unity_ObjectToWorld,float4( 0,0,0,1 ));
			float simplePerlin2D113 = snoise( ( ( (i.vertexColor).rgb * 255.0 ) + 0.5 ).xy*transform101.x );
			float clampResult123 = clamp( ( ( simplePerlin2D113 * simplePerlin2D113 ) * simplePerlin2D113 ) , -1.0 , 1.0 );
			float3 temp_cast_2 = (0.0).xxx;
			float3 temp_output_6_0_g4 = _MainFoliageColor.rgb;
			float3 rotatedValue3_g4 = RotateAroundAxis( temp_cast_2, temp_output_6_0_g4, normalizeResult2_g4, ( clampResult123 * _HueShiftIntensity ) );
			float4 _T_Grass_test_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_T_Grass_test_ST_arr, _T_Grass_test_ST);
			float2 uv_T_Grass_test = i.uv_texcoord * _T_Grass_test_ST_Instance.xy + _T_Grass_test_ST_Instance.zw;
			float4 tex2DNode142 = tex2D( _T_Grass_test, uv_T_Grass_test );
			float4 lerpResult152 = lerp( color153 , float4( ( rotatedValue3_g4 + temp_output_6_0_g4 ) , 0.0 ) , tex2DNode142.b);
			float4 _OutlineColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_OutlineColor_arr, _OutlineColor);
			float4 lerpResult125 = lerp( lerpResult152 , _OutlineColor_Instance , tex2DNode142.r);
			float temp_output_143_0 = ( 1.0 - i.uv_texcoord.y );
			float clampResult97 = clamp( simplePerlin2D113 , _Min_Clamp , _Max_Clamp );
			float temp_output_87_0 = ( ( temp_output_143_0 - clampResult97 ) / _Float3 );
			float4 lerpResult16 = lerp( lerpResult125 , _GradientBlend , saturate( temp_output_87_0 ));
			o.Albedo = lerpResult16.rgb;
			o.Alpha = 1;
			clip( tex2DNode142.g - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
416;658;2080;1035;2603.486;902.4775;1.296818;True;True
Node;AmplifyShaderEditor.VertexColorNode;64;-3510.444,351.8416;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;66;-3344.486,418.9437;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;255;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;115;-3347.416,347.2323;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-3030.616,353.2976;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-3007.784,443.3439;Inherit;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-2823.905,354.9437;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;101;-2955.167,517.6931;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;113;-2538.899,347.0522;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;80;-2210.581,403.5222;Inherit;False;False;True;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;75;-2235.197,245.1886;Inherit;False;True;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;82;-1998.581,293.5223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;81;-2222.581,503.5222;Inherit;False;False;False;True;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-1843.581,390.5223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;86;-1254.748,377.2609;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;149;-1844.584,797.5414;Inherit;False;Property;_Max_Clamp;Max_Clamp;11;0;Create;True;0;0;False;0;0.25;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;123;-1431.759,335.2849;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;121;-1992.651,44.32124;Inherit;False;Property;_HueShiftIntensity;HueShift Intensity;2;0;Create;True;0;0;True;0;0;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;-1845.584,687.5414;Inherit;False;Property;_Min_Clamp;Min_Clamp;9;0;Create;True;0;0;False;0;0.4;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;97;-1590.023,574.9896;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.4;False;2;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;143;-1014.749,408.0672;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-1296.171,-76.1423;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;58;-1697.009,-224.4231;Inherit;False;Property;_MainFoliageColor;Main Foliage Color;1;0;Create;True;0;0;False;0;0.322723,0.5471698,0.2038982,0;0.3476679,0.3882353,0.1686275,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;117;-883.8434,-219.3026;Inherit;False;MF_HueShift;-1;;4;97212708e7754c44a836a0939a5d71d4;0;2;6;FLOAT3;0,0,0;False;8;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;153;-1665.124,-523.2711;Inherit;False;Constant;_Fake_Shadow_Color;Fake_Shadow_Color;12;0;Create;True;0;0;False;0;0.6792453,0.6294477,0.259523,0;0.8962264,0.8443869,0.4354308,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;142;-1532.894,-762.1298;Inherit;True;Property;_T_Grass_test;T_Grass_test;7;0;Create;True;0;0;False;0;-1;None;aa317375c19cf294281674d7fd3724c0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;85;-845.3207,550.6447;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;-909.5132,714.937;Inherit;False;Property;_Float3;Float 3;10;0;Create;True;0;0;False;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;144;-840.2388,-424.5952;Inherit;False;InstancedProperty;_OutlineColor;Outline Color;8;0;Create;True;0;0;False;0;0.2338911,0.4528302,0.1901032,1;0.4166037,0.552,0.1596981,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;152;-418.6927,-613.2442;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;87;-679.0549,617.8879;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;88;-407.0553,370.8879;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;125;-230.6248,-341.7649;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;17;-812.6351,-6.489923;Inherit;False;Property;_GradientBlend;Gradient Blend;0;0;Create;True;0;0;False;0;0.4822432,0.7830189,0.2253026,0;0.5786397,0.7137255,0.2697883,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;145;-826.5132,350.937;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;127;239.4868,342.0289;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;16;-75.83296,-175.0549;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;128;398,342;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;129;538,342;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;137;-1745.996,-1055.423;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;146;-670.5132,296.937;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;139;-1541.996,-968.4227;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;-1830.796,-801.7226;Inherit;False;InstancedProperty;_outlinesoftness;outline softness;6;0;Create;True;0;0;False;0;0;1.31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;138;-2054.796,-948.9228;Inherit;False;InstancedProperty;_outlinewidth;outline width;5;0;Create;True;0;0;False;0;0.2;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;133;725.4868,297.0289;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;132;58.48682,341.0289;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;126;-168.5132,294.0289;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;136;-2107.098,-1173.892;Inherit;True;Property;_testgraycale;testgraycale;4;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;141;-1412.576,-924.7307;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;135;-165.6538,408.1263;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;147;-521.5132,272.937;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;473.7087,-565.2352;Float;False;True;2;ASEMaterialInspector;0;0;Standard;MM_Folliage;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.2;True;True;0;False;TransparentCutout;;AlphaTest;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;3;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;115;0;64;0
WireConnection;65;0;115;0
WireConnection;65;1;66;0
WireConnection;67;0;65;0
WireConnection;67;1;68;0
WireConnection;113;0;67;0
WireConnection;113;1;101;0
WireConnection;80;0;113;0
WireConnection;75;0;113;0
WireConnection;82;0;75;0
WireConnection;82;1;80;0
WireConnection;81;0;113;0
WireConnection;83;0;82;0
WireConnection;83;1;81;0
WireConnection;123;0;83;0
WireConnection;97;0;113;0
WireConnection;97;1;150;0
WireConnection;97;2;149;0
WireConnection;143;0;86;2
WireConnection;120;0;123;0
WireConnection;120;1;121;0
WireConnection;117;6;58;0
WireConnection;117;8;120;0
WireConnection;85;0;143;0
WireConnection;85;1;97;0
WireConnection;152;0;153;0
WireConnection;152;1;117;0
WireConnection;152;2;142;3
WireConnection;87;0;85;0
WireConnection;87;1;148;0
WireConnection;88;0;87;0
WireConnection;125;0;152;0
WireConnection;125;1;144;0
WireConnection;125;2;142;1
WireConnection;145;0;143;0
WireConnection;127;0;132;0
WireConnection;16;0;125;0
WireConnection;16;1;17;0
WireConnection;16;2;88;0
WireConnection;128;0;127;0
WireConnection;129;0;128;0
WireConnection;137;0;136;1
WireConnection;137;1;138;0
WireConnection;146;0;145;0
WireConnection;139;0;137;0
WireConnection;139;1;140;0
WireConnection;133;2;129;0
WireConnection;132;0;126;2
WireConnection;141;0;139;0
WireConnection;147;0;146;0
WireConnection;147;1;87;0
WireConnection;0;0;16;0
WireConnection;0;10;142;2
ASEEND*/
//CHKSM=0DDED65FAE79727006B61CF9613C7E7ABC943EBA