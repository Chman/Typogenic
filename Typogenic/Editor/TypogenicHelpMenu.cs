using UnityEngine;
using UnityEditor;

public class TypogenicHelpMenu : Editor
{
	static string pathManual = "/Typogenic/Documentation/index.html";

	[MenuItem("Help/Typogenic Manual", false, 0)]
	public static void MenuManual()
	{
		Application.OpenURL("file://" + Application.dataPath + pathManual);
	}
}
