Shader "Typogenic/Unlit Textured Shadowcaster Font"
{
	Properties
	{
		_MainTex ("Base (Alpha8)", 2D) = "white" {}
		_FillTex ("Fill Texture (RGBA)", 2D) = "white" {}
		_Smoothness ("Smoothness / Antialiasing (Float)", Float) = 0.85
		_Thickness ("Thickness (Float)", Range(1.0, 0.05)) = 0.5

		// OUTLINED
		_OutlineColor ("Outline Color (RGBA)", Color) = (0, 0, 0, 1)
		_OutlineThickness ("Outline Thickness (Float)", Range(1.0, 0.1)) = 0.25

		// GLOW
		_GlowColor ("Glow Color (RGBA)", Color) = (0, 0, 0, 1)
		_GlowStart ("Glow Start", Range(0.0, 1.0)) = 0.1
		_GlowEnd ("Glow End", Range(0.0, 1.0)) = 0.9

		// GLOBAL_MULTIPLIER
		_GlobalMultiplierColor ("Global Color Multiplier (RGBA)", Color) = (1, 1, 1, 1)

		// SHADOW CUTOFF
		_ShadowCutoff ("Shadow Cutoff",Range(0.01, 1.0)) = .7
	}
	SubShader
	{
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
			Offset 1, 1

			Fog { Mode Off }
			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl
			#pragma target 3.0
			#pragma multi_compile OUTLINED_ON OUTLINED_OFF
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			float4 _MainTex_ST;
			sampler2D _MainTex;
			float4 _FillTex_ST;
			sampler2D _FillTex;
			half _OutlineThickness;
			half _ShadowCutoff;
			half _Smoothness;
			half _Thickness;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			}; 

			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2  uv  : TEXCOORD1;
				float2  uv2 : TEXCOORD2;
			};

			v2f vert(appdata v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.texcoord1, _FillTex);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				half dist = tex2D(_MainTex, i.uv).a;
				half smoothing = fwidth(dist) * _Smoothness;
				half alpha = smoothstep(_Thickness - smoothing, _Thickness + smoothing, dist);
				half4 color = tex2D(_FillTex, i.uv2);

				alpha *= color.a;

				#if OUTLINED_ON
				half outlineAlpha = smoothstep(_OutlineThickness - smoothing, _OutlineThickness + smoothing, dist);
				outlineAlpha *= color.a;

				if (outlineAlpha > _ShadowCutoff)
				alpha = outlineAlpha;
				#endif

				clip(alpha - _ShadowCutoff);

				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 200
		ZWrite Off

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl
			#pragma target 3.0
			#pragma multi_compile OUTLINED_ON OUTLINED_OFF
			#pragma multi_compile GLOW_ON GLOW_OFF
			#pragma multi_compile GLOBAL_MULTIPLIER_ON GLOBAL_MULTIPLIER_OFF
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _FillTex;
			half4 _FillTex_ST;
			half _Smoothness;
			half _Thickness;

			// OUTLINED
			half4 _OutlineColor;
			half _OutlineThickness;

			// GLOW
			half4 _GlowColor;
			half _GlowStart;
			half _GlowEnd;
			
			// GLOBAL_MULTIPLIER
			half4 _GlobalMultiplierColor;

			struct vertexInput
			{
				half4 vertex : POSITION;
				half2 texcoord0 : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
			};

			struct fragmentInput
			{
				half4 position : SV_POSITION;
				half2 texcoord0 : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
			};

			fragmentInput vert(vertexInput i)
			{
				fragmentInput o;
				o.position = mul(UNITY_MATRIX_MVP, i.vertex);
				o.texcoord0 = i.texcoord0;
				o.texcoord1 = TRANSFORM_TEX(i.texcoord1, _FillTex);
				return o;
			}

			half4 frag(fragmentInput i) : COLOR
			{
				half dist = tex2D(_MainTex, i.texcoord0).a;
				half4 color = tex2D(_FillTex, i.texcoord1);
				half smoothing = fwidth(dist) * _Smoothness;
				half alpha = smoothstep(_Thickness - smoothing, _Thickness + smoothing, dist);
				half4 finalColor = half4(color.rgb, color.a * alpha);

				// OUTLINED
				#if OUTLINED_ON

				half outlineAlpha = smoothstep(_OutlineThickness - smoothing, _OutlineThickness + smoothing, dist);
				half4 outline = half4(_OutlineColor.rgb, _OutlineColor.a * outlineAlpha);
				finalColor = lerp(outline, finalColor, alpha);

				#endif

				// GLOW
				#if GLOW_ON

				half glowAlpha = smoothstep(_GlowStart, _GlowEnd, dist);
				finalColor = lerp(half4(_GlowColor.rgb, _GlowColor.a * glowAlpha), finalColor, finalColor.a);

				#endif
				
				// GLOBAL_MULTIPLIER
				#if GLOBAL_MULTIPLIER_ON

				return finalColor * _GlobalMultiplierColor;

				#endif

				#if GLOBAL_MULTIPLIER_OFF

				return finalColor;

				#endif
			}

			ENDCG
		}
	}

	FallBack off
	CustomEditor "TypogenicMaterialEditor"
}
