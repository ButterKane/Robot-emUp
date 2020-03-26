Shader "Fur/UltraMegaGrass"
{
	Properties
	{
		[Header(Textures)]
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Texture pattern d'herbe", 2D) = "white" { }
		_Noise("_Noise actually can be texture color", 2D) = "white" {} //new
		_Distortation("_Distortation swirl wind", 2D) = "white" {} //new
		_NoGrassTex("No Grass Texture", 2D) = "white" { }
		_Ramp("Toon Ramp (RGB)", 2D) = "gray" {}

		[Header(Herbe)]
		_FurThinness("Fur Thinness", Range(0.01, 100)) = 1
		_FurShading("Fur Shading", Range(0.0, 1)) = 0.25
		_BoolProjector("Bool Projector", Float) = 1
		_ForceWind("_ForceWind", Float) = 1
		_SpeedWind("_SpeedWind", Float) = 1

		[Header(Color)]
		_RimColor("Rim Color", Color) = (0, 0, 0, 1)
		_RimPower("Rim Power", Range(0.0, 8.0)) = 6.0
		_RimMin("Rim Min", Range(0,1)) = 0.0
		_RimMax("Rim Max", Range(0,1)) = 1.0
		_HColor("Highlight Color", Color) = (0.6,0.6,0.6,1.0)
		_SColor("Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_DiffTint("Diffuse Tint", Color) = (0.7,0.8,1,1)
		_NoirColor("_NoirColor", Color) = (0.3,0.3,0.3,1.0)
		_PJShadowColor("_PJShadowColor", Color) = (0.0,0.0,0.0,1.0)
	}

					SubShader
				{
			//Tags {"IgnoreProjector" = "true"}
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf ToonyColorsCustom fullforwardshadows vertex:vert addshadow
			
			// Use shader model 3.0 target, to get nicer looking lighting and don't go over it
			#pragma target 3.0
			sampler2D _MainTex;
			sampler2D _Noise;
			sampler2D _NoGrassTex;


			struct Input {
				float2 uv_MainTex;
				float2 uv_Noise;
				float4 pos: SV_POSITION;
				half4 uv: TEXCOORD0;
				float3 worldNormal: TEXCOORD1;
				float3 worldPos: TEXCOORD2;
				float4 vertColor : COLOR;
			};

			//sampler2D _CameraDepthTexture; //Depth Texture
			uniform sampler2D _GlobalEffectRT;
			uniform float3 _Position;
			sampler2D _FurTex;
			half4 _FurTex_ST;
			uniform float _OrthographicCamSize;
			fixed _FurThinness;
			fixed _FurShading;
			float _BoolProjector;

			fixed4 _RimColor;
			half _RimPower;
			fixed4 _Color, _PJShadowColor;
			fixed _RimMin;
			fixed _RimMax;

			fixed4 _HColor;
			fixed4 _SColor;
			fixed4 _NoirColor;
			sampler2D _Ramp;
			fixed4 _DiffTint;
			float _ForceWind;
			float _SpeedWind;

			sampler2D _Distortation;

			struct SurfaceOutputStandard 
			{
				fixed3 Albedo;
				fixed3 Normal;
				fixed3 Emission;
				fixed Alpha;
			};

			inline half4 LightingToonyColorsCustom(inout SurfaceOutputStandard s, half3 lightDir, half3 viewDir, half atten)
			{
				s.Normal = normalize(s.Normal);
				fixed ndl = max(0, dot(s.Normal, lightDir)*0.5 + 0.5);
				fixed3 ramp = tex2D(_Ramp, fixed2(ndl, ndl));
	#if !(POINT) && !(SPOT)
				ramp *= atten;
	#endif
				_SColor = lerp(_HColor, _SColor, _SColor.a);	//Shadows intensity through alpha
				ramp = lerp(_SColor.rgb, _HColor.rgb, ramp);
				fixed3 wrappedLight = saturate(_DiffTint.rgb + saturate(dot(s.Normal, lightDir)));
				ramp *= wrappedLight;
				fixed4 c;
				c.rgb = s.Albedo * _LightColor0.rgb * ramp;
				c.a = s.Alpha;
	#if (POINT || SPOT)
				c.rgb *= atten;
	#endif
				return c;
			}

			void vert(inout appdata_full v)
			{
				
			}

			void surf(Input IN, inout SurfaceOutputStandard o) 
			{
				float2 uv = IN.worldPos.xz - _Position.xz;
				uv = uv / (_OrthographicCamSize * 2);
				uv += 0.5;

				float ripples = (0.25 - tex2D(_GlobalEffectRT, uv).b)*_BoolProjector;
				float ripples2 = (tex2D(_GlobalEffectRT, uv).r)*_BoolProjector;
				float AntiRipple = (tex2D(_GlobalEffectRT, uv).r);
				float ripples3 = (0 - tex2D(_GlobalEffectRT, uv).r);

				fixed3 worldNormal = normalize(IN.worldNormal);
				//fixed3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
				fixed3 worldView = normalize(_WorldSpaceCameraPos.xyz - IN.worldPos.xyz);
				//fixed3 worldHalf = normalize(worldView + worldLight);
				half rim = 1.0 - saturate(dot(worldView, worldNormal));
				rim = smoothstep(_RimMin, _RimMax, rim);
				fixed3 albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;
				albedo -= (pow(1 - IN.vertColor, 3)) * _FurShading;

				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				fixed4 d = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				ripples3 *= AntiRipple*_BoolProjector;
				
				float2 dis = tex2D(_Distortation, IN.uv_MainTex  *0.6 + _Time.xx*3* _SpeedWind);
				float displacementStrengh = (0.6* (((sin(_Time.y+dis*5) + sin(_Time.y*0.5 + 1.051)) / 5.0) + 0.15*dis)*(1 - clamp(tex2D(_GlobalEffectRT, uv).b * 5,0,2))); //hmm math
				dis = dis * displacementStrengh*(IN.vertColor.r*1.3)*_ForceWind*(1 - clamp(tex2D(_GlobalEffectRT, uv).b * 5, 0, 2));
				fixed4 col = tex2D(_MainTex, IN.uv_MainTex *2.0 + dis.xy);
				float3 noise2 = tex2D(_Noise, IN.uv_Noise);
				fixed3 NoGrass = tex2D(_NoGrassTex, IN.uv_Noise);

				fixed alpha = step(1 - ((col.x + noise2.x) * _FurThinness)*(NoGrass.r + 0.3)*saturate(ripples*1+ 1)*saturate(ripples*1 + 1), (1 - IN.vertColor.r*(ripples + 1))*(NoGrass.r + 0.8)*_FurThinness);
				if (alpha*(ripples3 + 1) - (IN.vertColor.r) <= 0)discard;

				col.xyz = noise2* noise2;
				col.xyz *= saturate(lerp(_NoirColor, 1, pow(IN.vertColor.r, 1.1))+ _FurShading*(ripples*0.33+1) +(1- NoGrass.r)*1);
				o.Albedo = col * _Color*(ripples*-0.1 + 1);
				o.Albedo += fixed4(_RimColor.rgb * pow(rim, _RimPower), 1.0);
				o.Albedo *= 1 - (ripples2*(1 - saturate(IN.vertColor.r - 0.7)))*_BoolProjector;
				o.Albedo = lerp(_PJShadowColor,o.Albedo, 1 - (ripples2*_BoolProjector));
			}
			ENDCG
		//UsePass "Legacy Shaders/VertexLit/SHADOWCASTER" wtf is this?
				}
					Fallback "Transparent/Cutout/Diffuse"
}

