Shader "Typogenic/Unlit Font"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Smoothness ("Smoothness / Antialiasing (Float)", Float) = 0.85
		_Thickness ("Thickness (Float)", Range(1.0, 0.05)) = 0.5

		// OUTLINED
		_OutlineColor ("Outline Color (RGBA)", Color) = (0, 0, 0, 1)
		_OutlineThickness ("Outline Thickness (Float)", Range(1.0, 0.1)) = 0.25

		// OUTLINED_GLOW
		_OutlineGlowLow ("Glow Low Threshold", Range(0.0, 1.0)) = 0.0
		_OutlineGlowHigh ("Glow High Threshold", Range(0.0, 1.0)) = 1.0

		// GLOBAL_MULTIPLIER
		_GlobalMultiplierColor ("Global Color Multiplier (RGBA)", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
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
			#pragma multi_compile OUTLINED_GLOW_ON OUTLINED_GLOW_OFF
			#pragma multi_compile GLOBAL_MULTIPLIER_ON GLOBAL_MULTIPLIER_OFF

			sampler2D _MainTex;
			half _Smoothness;
			half _Thickness;

			// OUTLINED
			half4 _OutlineColor;
			half _OutlineThickness;

			// OUTLINED_GLOW
			half _OutlineGlowLow;
			half _OutlineGlowHigh;
			
			// GLOBAL_MULTIPLIER
			half4 _GlobalMultiplierColor;

			struct vertexInput
			{
				half4 vertex : POSITION;
				half2 texcoord0 : TEXCOORD0;
				half4 color : COLOR;
			};

			struct fragmentInput
			{
				half4 position : SV_POSITION;
				half2 texcoord0 : TEXCOORD0;
				half4 color : COLOR;
			};

			fragmentInput vert(vertexInput i)
			{
				fragmentInput o;
				o.position = mul(UNITY_MATRIX_MVP, i.vertex);
				o.texcoord0 = i.texcoord0;
				o.color = i.color;
				return o;
			}

			half4 frag(fragmentInput i) : COLOR
			{
				half dist = tex2D(_MainTex, i.texcoord0).a;

				half smoothing = fwidth(dist) * _Smoothness;
				half alpha = smoothstep(_Thickness - smoothing, _Thickness + smoothing, dist);

				half4 finalColor;

				// OUTLINED
				#if OUTLINED_ON

				#if OUTLINED_GLOW_ON
				half outlineAlpha = smoothstep(max(0.0, _OutlineThickness - smoothing - _OutlineGlowLow), min(1.0, _OutlineThickness + smoothing + _OutlineGlowHigh), dist);
				#endif
				#if OUTLINED_GLOW_OFF
				half outlineAlpha = smoothstep(_OutlineThickness - smoothing, _OutlineThickness + smoothing, dist);
				#endif

				half4 outline = half4(_OutlineColor.rgb, _OutlineColor.a * outlineAlpha);
				half4 color = half4(i.color.rgb, i.color.a * alpha);
				finalColor =  lerp(outline, color, alpha);

				#endif

				#if OUTLINED_OFF
				
				finalColor = half4(i.color.rgb, i.color.a * alpha);

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
