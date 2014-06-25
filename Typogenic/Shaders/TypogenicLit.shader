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

		sampler2D _MainTex;
		half _Smoothness;
		half _Thickness;

		// OUTLINED
		half4 _OutlineColor;
		half _OutlineThickness;

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

			// OUTLINED
			#if OUTLINED_ON

			half outlineAlpha = smoothstep(_OutlineThickness - smoothing, _OutlineThickness + smoothing, dist);
			half4 outline = half4(_OutlineColor.rgb, _OutlineColor.a * outlineAlpha);
			half4 color = half4(IN.color.rgb, IN.color.a * alpha);
			half4 finalColor = lerp(outline, color, alpha);
			o.Albedo = finalColor.rgb;
			o.Alpha = finalColor.a;

			#endif

			#if OUTLINED_OFF

			o.Albedo = IN.color.rgb;
			o.Alpha = IN.color.a * alpha;

			#endif
		}
		ENDCG
	} 
	
	FallBack off
	CustomEditor "TypogenicMaterialEditor"
}
