Shader "Typogenic/Lit Textured Font"
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
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha nolightmap nodirlightmap
		#pragma glsl
		#pragma target 3.0
		#pragma multi_compile OUTLINED_ON OUTLINED_OFF
		#pragma multi_compile GLOW_ON GLOW_OFF
		#pragma multi_compile GLOBAL_MULTIPLIER_ON GLOBAL_MULTIPLIER_OFF

		sampler2D _MainTex;
		sampler2D _FillTex;
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

		struct Input
		{
			half2 uv_MainTex;
			half2 uv2_FillTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			half dist = tex2D(_MainTex, IN.uv_MainTex).a;
			half4 color = tex2D(_FillTex, IN.uv2_FillTex);
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
				
			o.Albedo = finalColor.rgb * _GlobalMultiplierColor.rgb;
			o.Alpha = finalColor.a * _GlobalMultiplierColor.a;

			#endif

			#if GLOBAL_MULTIPLIER_OFF
				
			o.Albedo = finalColor.rgb;
			o.Alpha = finalColor.a;

			#endif
		}
		ENDCG
	} 
	
	FallBack off
	CustomEditor "TypogenicMaterialEditor"
}
