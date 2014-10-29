using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(TypogenicText)), CanEditMultipleObjects]
public class TypogenicTextEditor : Editor
{
	protected SerializedProperty m_Font;
	protected SerializedProperty m_Text;
	protected SerializedProperty m_Size;
	protected SerializedProperty m_Leading;
	protected SerializedProperty m_Tracking;
	protected SerializedProperty m_ParagraphSpacing;
	protected SerializedProperty m_WordWrap;
	protected SerializedProperty m_Alignment;
	protected SerializedProperty m_FillMode;
	protected SerializedProperty m_ColorTopLeft;
	protected SerializedProperty m_ColorTopRight;
	protected SerializedProperty m_ColorBottomLeft;
	protected SerializedProperty m_ColorBottomRight;
	protected SerializedProperty m_GenerateNormals;
	protected SerializedProperty m_Stationary;
	protected SerializedProperty m_EnableClickSupport;
	protected SerializedProperty m_DrawGlyphBoundsGizmos;

	Vector2 scrollText;

	void OnEnable()
	{
		m_Font = serializedObject.FindProperty("Font");
		m_Text = serializedObject.FindProperty("Text");
		m_Size = serializedObject.FindProperty("Size");
		m_Leading = serializedObject.FindProperty("Leading");
		m_Tracking = serializedObject.FindProperty("Tracking");
		m_ParagraphSpacing = serializedObject.FindProperty("ParagraphSpacing");
		m_WordWrap = serializedObject.FindProperty("WordWrap");
		m_Alignment = serializedObject.FindProperty("Alignment");
		m_FillMode = serializedObject.FindProperty("FillMode");
		m_ColorTopLeft = serializedObject.FindProperty("ColorTopLeft");
		m_ColorTopRight = serializedObject.FindProperty("ColorTopRight");
		m_ColorBottomLeft = serializedObject.FindProperty("ColorBottomLeft");
		m_ColorBottomRight = serializedObject.FindProperty("ColorBottomRight");
		m_GenerateNormals = serializedObject.FindProperty("GenerateNormals");
		m_Stationary = serializedObject.FindProperty("Stationary");
		m_EnableClickSupport = serializedObject.FindProperty("EnableClickSupport");
		m_DrawGlyphBoundsGizmos = serializedObject.FindProperty("DrawGlyphBoundsGizmos");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.PropertyField(m_Font);
		EditorGUILayout.PropertyField(m_GenerateNormals);
		EditorGUILayout.PropertyField(m_EnableClickSupport);

		if (m_EnableClickSupport.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(m_DrawGlyphBoundsGizmos);
			EditorGUILayout.PropertyField(m_Stationary, new GUIContent("Static"));
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.PrefixLabel(String.Format("Text (w: {0:F2}, h: {1:F2})", ((TypogenicText)target).Width, ((TypogenicText)target).Height));
		scrollText = EditorGUILayout.BeginScrollView(scrollText, GUILayout.MinHeight(85f), GUILayout.MaxHeight(200f));
		m_Text.stringValue = EditorGUILayout.TextArea(m_Text.stringValue, GUILayout.MinHeight(85f), GUILayout.MaxHeight(200f));
		EditorGUILayout.EndScrollView();
		EditorGUILayout.PropertyField(m_Size, new GUIContent("Character Size"));
		EditorGUILayout.PropertyField(m_Tracking, new GUIContent("Character Spacing (Tracking)"));
		EditorGUILayout.PropertyField(m_Leading, new GUIContent("Line Spacing (Leading)"));
		EditorGUILayout.PropertyField(m_ParagraphSpacing);
		EditorGUILayout.PropertyField(m_Alignment);
		EditorGUILayout.PropertyField(m_WordWrap);

		EditorGUILayout.PropertyField(m_FillMode);

		switch (m_FillMode.enumValueIndex)
		{
			case (int)TFillMode.SingleColor:
				EditorGUILayout.PropertyField(m_ColorTopLeft, new GUIContent("Color (RGB + A)"));
				break;
			case (int)TFillMode.VerticalGradient:
				EditorGUILayout.PropertyField(m_ColorTopLeft, new GUIContent("Top Color (RGB + A)"));
				EditorGUILayout.PropertyField(m_ColorBottomLeft, new GUIContent("Bottom Color (RGB + A)"));
				break;
			case (int)TFillMode.HorizontalGradient:
				EditorGUILayout.PropertyField(m_ColorTopLeft, new GUIContent("Left Color (RGB + A)"));
				EditorGUILayout.PropertyField(m_ColorBottomLeft, new GUIContent("Right Color (RGB + A)"));
				break;
			case (int)TFillMode.QuadGradient:
				EditorGUILayout.PropertyField(m_ColorTopLeft, new GUIContent("Top Left Color (RGB + A)"));
				EditorGUILayout.PropertyField(m_ColorTopRight, new GUIContent("Top Right Color (RGB + A)"));
				EditorGUILayout.PropertyField(m_ColorBottomLeft, new GUIContent("Bottom Left Color (RGB + A)"));
				EditorGUILayout.PropertyField(m_ColorBottomRight, new GUIContent("Bottom Right Color (RGB + A)"));
				break;
			default:
				break;
		}

		if (serializedObject.ApplyModifiedProperties() || Event.current.commandName == "UndoRedoPerformed")
		{
			foreach (TypogenicText t in targets)
			{
				if (t.enabled && t.gameObject.activeInHierarchy && PrefabUtility.GetPrefabType(target) != PrefabType.Prefab)
					t.RebuildMesh();
			}
		}
	}

	void OnSceneGUI()
	{
		TypogenicText src = (TypogenicText)target;

		if (src.WordWrap > 0f)
		{
			Vector3 alignmentOffset = Vector3.zero;
			if (src.Alignment == TTextAlignment.Center) alignmentOffset = new Vector3(-src.WordWrap * 0.5f, 0f, 0f);
			else if (src.Alignment == TTextAlignment.Right) alignmentOffset = new Vector3(-src.WordWrap, 0f, 0f);
				
			Vector3 v1 = src.transform.TransformPoint(alignmentOffset);
			Vector3 v2 = src.transform.TransformPoint(alignmentOffset + new Vector3(src.WordWrap, 0f, 0f));
			Vector3 v3 = src.transform.TransformPoint(alignmentOffset + new Vector3(0f, -src.Height, 0f));
			Vector3 v4 = src.transform.TransformPoint(alignmentOffset + new Vector3(src.WordWrap, -src.Height, 0f));

			Handles.color = Color.yellow;
			Handles.DrawLine(v1, v2);
			Handles.DrawLine(v1, v3);
			Handles.DrawLine(v4, v3);
			Handles.DrawLine(v2, v4);
		}
	}

	[MenuItem("GameObject/Create Other/Typogenic Text", false, 1500)]
	public static void CreateNewTypogenicText()
	{
		GameObject gameObject = new GameObject("New Typogenic Text");
		gameObject.AddComponent<TypogenicText>();
		gameObject.GetComponent<MeshRenderer>().castShadows = false;
		gameObject.GetComponent<MeshRenderer>().receiveShadows = false;
		Selection.objects = new GameObject[1] { gameObject };
		EditorApplication.ExecuteMenuItem("GameObject/Move To View");

		#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		#else
		Undo.RegisterCreatedObjectUndo(gameObject, "Created New Typogenic Text");
		#endif
	}
}
