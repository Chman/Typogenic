Shader "Typogenic/Lit Font"
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
		
		CGPROGRAM
		#pragma surface surf Lambert alpha
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

		struct Input
		{
			half2 uv_MainTex;
			half4 color : Color;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			half dist = tex2D(_MainTex, IN.uv_MainTex).a;

			half smoothing = fwidth(dist) * _Smoothness;
			half alpha = smoothstep(_Thickness - smoothing, _Thickness + smoothing, dist);

			half3 finalAlbedo;
			half finalAlpha;

			// OUTLINED
			#if OUTLINED_ON
			
			#if OUTLINED_GLOW_ON
			half outlineAlpha = smoothstep(max(0.0, _OutlineThickness - smoothing - _OutlineGlowLow), min(1.0, _OutlineThickness + smoothing + _OutlineGlowHigh), dist);
			#endif
			#if OUTLINED_GLOW_OFF
			half outlineAlpha = smoothstep(_OutlineThickness - smoothing, _OutlineThickness + smoothing, dist);
			#endif

			half4 outline = half4(_OutlineColor.rgb, _OutlineColor.a * outlineAlpha);
			half4 color = half4(IN.color.rgb, IN.color.a * alpha);
			half4 finalColor = lerp(outline, color, alpha);
			finalAlbedo = finalColor.rgb;
			finalAlpha = finalColor.a;

			#endif

			#if OUTLINED_OFF

			finalAlbedo = IN.color.rgb;
			finalAlpha = IN.color.a * alpha;

			#endif

			// GLOBAL_MULTIPLIER
			#if GLOBAL_MULTIPLIER_ON
				
			o.Albedo = finalAlbedo * _GlobalMultiplierColor.rgb;
			o.Alpha = finalAlpha * _GlobalMultiplierColor.a;

			#endif

			#if GLOBAL_MULTIPLIER_OFF
				
			o.Albedo = finalAlbedo;
			o.Alpha = finalAlpha;

			#endif
		}
		ENDCG
	} 
	
	FallBack off
	CustomEditor "TypogenicMaterialEditor"
}
