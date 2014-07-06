using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TypogenicMaterialEditor : MaterialEditor
{
	Dictionary<string, MaterialProperty> properties;

	public override void OnEnable()
	{
		base.OnEnable();

		properties = new Dictionary<string, MaterialProperty>();
		MaterialProperty[] props = GetMaterialProperties(new Object[] { (Material)target });

		foreach (MaterialProperty property in props)
			properties.Add(property.name, property);
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if (!isVisible)
			return;

		Material material = (Material)target;
		string[] inKeywords = material.shaderKeywords;
		List<string> outKeywords = new List<string>();

		// GLOBAL_MULTIPLIER
		Color globalMultiplier = ColorProperty(properties["_GlobalMultiplierColor"], "Global Color Multiplier (RGB + Alpha)");
		outKeywords.Add((globalMultiplier == Color.white) ? "GLOBAL_MULTIPLIER_OFF" : "GLOBAL_MULTIPLIER_ON");

		TextureProperty(properties["_MainTex"], "Main Texture (Alpha8)", false);
		FloatProperty(properties["_Smoothness"], "Smoothness (Antialiasing)");
		properties["_Smoothness"].floatValue = Mathf.Max(0f, properties["_Smoothness"].floatValue);
		RangeProperty(properties["_Thickness"], "Thickness");

		// OUTLINED
		EditorGUILayout.Space();
		bool outlined = inKeywords.Contains("OUTLINED_ON");
		EditorGUI.BeginChangeCheck();
		outlined = EditorGUILayout.Toggle("Outlines", outlined);
		if (EditorGUI.EndChangeCheck())
			RegisterPropertyChangeUndo("Outlines");
		outKeywords.Add(outlined ? "OUTLINED_ON" : "OUTLINED_OFF");

		if (outlined)
		{
			EditorGUI.indentLevel++;

			ColorProperty(properties["_OutlineColor"], "Color (RGB + Alpha)");
			RangeProperty(properties["_OutlineThickness"], "Thickness");

			// OUTLINED_BLUR
			bool blur = inKeywords.Contains("OUTLINED_BLUR_ON");
			EditorGUI.BeginChangeCheck();
			blur = EditorGUILayout.Toggle("Blur", blur);
			if (EditorGUI.EndChangeCheck())
				RegisterPropertyChangeUndo("Blur");
			outKeywords.Add(blur ? "OUTLINED_BLUR_ON" : "OUTLINED_BLUR_OFF");

			if (blur)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.HelpBox("Don't forget to play with the outline tickness parameter as well, the following values are automatically clamped to avoid visual artifacts.", MessageType.Info);
				RangeProperty(properties["_OutlineBlurLow"], "Low Threshold");
				RangeProperty(properties["_OutlineBlurHigh"], "High Threshold");
				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel--;
		}

		material.shaderKeywords = outKeywords.ToArray();
		PropertiesChanged();
		EditorUtility.SetDirty(material);
	}
}
