using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TypogenicFont))]
public class TypogenicFontEditor : Editor
{
	TypogenicFont srcFont;

	void OnEnable()
	{
		srcFont = (TypogenicFont)target;
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("Glyph count: " + srcFont.Glyphs.Count);
		EditorGUILayout.LabelField("Kerning pairs: " + srcFont.KerningPairs);
	}
}
