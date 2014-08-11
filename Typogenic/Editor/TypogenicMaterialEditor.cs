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
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		FetchProperties();

		if (!isVisible)
			return;

		EditorGUIUtility.fieldWidth = 64f;
		GUI.changed = false;

		Material material = (Material)target;
		string[] inKeywords = material.shaderKeywords;
		List<string> outKeywords = new List<string>();

		// GLOBAL_MULTIPLIER
		Color globalMultiplier = ColorProperty(properties["_GlobalMultiplierColor"], "Global Color Multiplier (RGB + Alpha)");
		outKeywords.Add((globalMultiplier == Color.white) ? "GLOBAL_MULTIPLIER_OFF" : "GLOBAL_MULTIPLIER_ON");

		TextureProperty(properties["_MainTex"], "Main Texture (Alpha8)", false);

		if (properties.ContainsKey("_FillTex"))
			TextureProperty(properties["_FillTex"], "Fill Texture (RGB + A)", true);

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

			EditorGUI.indentLevel--;
		}

		// GLOW
		EditorGUILayout.Space();
		bool glowing = inKeywords.Contains("GLOW_ON");
		EditorGUI.BeginChangeCheck();
		glowing = EditorGUILayout.Toggle("Glow", glowing);
		if (EditorGUI.EndChangeCheck())
			RegisterPropertyChangeUndo("Glow");
		outKeywords.Add(glowing ? "GLOW_ON" : "GLOW_OFF");

		if (glowing)
		{
			EditorGUI.indentLevel++;

			ColorProperty(properties["_GlowColor"], "Color (RGB + Alpha)");
			RangeProperty(properties["_GlowStart"], "Start");
			RangeProperty(properties["_GlowEnd"], "End");

			EditorGUI.indentLevel--;
		}

		// SHADOWS
		if (properties.ContainsKey("_ShadowCutoff"))
			RangeProperty(properties["_ShadowCutoff"], "Shadow Cutoff");

		material.shaderKeywords = outKeywords.ToArray();

		if (GUI.changed)
		{
			PropertiesChanged();
			EditorUtility.SetDirty(material);
		}
	}

	void FetchProperties()
	{
		properties.Clear();
		MaterialProperty[] props = GetMaterialProperties(new Object[] { (Material)target });

		foreach (MaterialProperty property in props)
			properties.Add(property.name, property);
	}
}
