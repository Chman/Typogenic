using UnityEngine;
using UnityEditor;
using System.IO;

public class TypogenicFontWizard : ScriptableWizard
{
	public Texture2D Atlas;
	public TextAsset FontXML;
	public bool CreateMaterial = true;
	public bool PrepareTextures = true;

	[MenuItem("Assets/Create/Typogenic Font")]
	public static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<TypogenicFontWizard>("Create Typogenic Font", "Create");
	}

	void OnWizardCreate()
	{
		if (Atlas == null || FontXML == null)
		{
			Debug.LogError("Make sure Atlas and FontXML aren't null");
			return;
		}

		if (PrepareTextures)
		{
			string atlasPath = AssetDatabase.GetAssetPath(Atlas);
			TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(atlasPath);
			importer.textureType = TextureImporterType.Advanced;
			importer.mipmapEnabled = false;
			importer.anisoLevel = 4;
			importer.filterMode = FilterMode.Bilinear;
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.maxTextureSize = 4096;
			importer.textureFormat = TextureImporterFormat.Alpha8;
			AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);
		}

		TypogenicFont asset = ScriptableObject.CreateInstance<TypogenicFont>();
		asset.Atlas = Atlas;
		asset.FontXML = FontXML;
		asset.Apply();
		CreateAsset(asset, FontXML.name + " Data.asset");

		if (CreateMaterial)
		{
			Material material = new Material(Shader.Find("Typogenic/Unlit Font"));
			material.mainTexture = Atlas;
			CreateAsset(material, FontXML.name + ".mat");
		}

		AssetDatabase.SaveAssets();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}

	void CreateAsset(Object obj, string name)
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);

		if (path == "")
			path = "Assets";
		else if (Path.GetExtension(path) != "")
			path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name);

		AssetDatabase.CreateAsset(obj, assetPathAndName);
	}
}
