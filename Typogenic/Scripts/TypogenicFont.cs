using UnityEngine;
using System;
using System.Xml;

// Dumb hack to bypass Unity serialization limitations on generics
[Serializable]
public class GlyphDictionary : SerializedDictionary<int, TGlyph> { }
[Serializable]
public class KerningDictionary : SerializedDictionary<int, float> { }

[Serializable]
public class TGlyph
{
	public Rect rect;
	public float xOffset;
	public float yOffset;
	public float xAdvance;
	public KerningDictionary kerning;

	public TGlyph(Rect rect, float xOffset, float yOffset, float xAdvance)
	{
		this.rect = rect;
		this.xOffset = xOffset;
		this.yOffset = yOffset;
		this.xAdvance = xAdvance;
	}

	public float GetKerning(int previousChar)
	{
		float k = 0f;

		if (kerning == null)
			return k;

		kerning.TryGetValue(previousChar, out k);
		return k;
	}
}

[Serializable]
public class TypogenicFont : ScriptableObject
{
	public Texture2D Atlas;
	public TextAsset FontXML;

	public GlyphDictionary Glyphs;
	public int KerningPairs;
	public float BaseSize;
	public float LineHeight;
	public float HScale;
	public float VScale;

	public void Apply()
	{
		if (Atlas == null || FontXML == null)
		{
			Debug.LogError("Make sure Atlas and FontXML aren't null");
			return;
		}

		XmlDocument xmlData = new XmlDocument();
		xmlData.LoadXml(FontXML.text);

		XmlNode fontNode = xmlData.FirstChild;

		if (fontNode.Name != "font")
			Debug.LogError("Invalid font xml");

		Glyphs = new GlyphDictionary();
		BaseSize = 1f;
		LineHeight = 1f;
		VScale = 1f;
		HScale = 1f;

		if (Atlas.width > Atlas.height)      HScale = Atlas.width / Atlas.height;
		else if (Atlas.width < Atlas.height) VScale = Atlas.height / Atlas.width;

		foreach (XmlNode node in fontNode.ChildNodes)
		{
			if (node.Name == "info")
				BaseSize = Convert.ToSingle(attribute(node, "size"));
			else if (node.Name == "common")
				LineHeight = Convert.ToSingle(attribute(node, "lineHeight")) / Atlas.height * VScale;
			else if (node.Name == "chars")
				parseChars(node);
			else if (node.Name == "kernings")
				parseKernings(node);
		}
	}

	void parseChars(XmlNode charsNode)
	{
		foreach (XmlNode node in charsNode.ChildNodes)
		{
			int charCode = Convert.ToInt32(attribute(node, "id"));
			float x = Convert.ToSingle(attribute(node, "x")) / Atlas.width * HScale;
			float y = Convert.ToSingle(attribute(node, "y")) / Atlas.height * VScale;
			float width = Convert.ToSingle(attribute(node, "width")) / Atlas.width * HScale;
			float height = Convert.ToSingle(attribute(node, "height")) / Atlas.height * VScale;
			float xOffset = Convert.ToSingle(attribute(node, "xoffset")) / Atlas.width * HScale;
			float yOffset = Convert.ToSingle(attribute(node, "yoffset")) / Atlas.height * VScale;
			float xAdvance = Convert.ToSingle(attribute(node, "xadvance")) / Atlas.width * HScale;

			Glyphs.Set(charCode, new TGlyph(new Rect(x, y, width, height), xOffset, yOffset, xAdvance));
		}
	}

	void parseKernings(XmlNode kerningsNode)
	{
		foreach (XmlNode node in kerningsNode.ChildNodes)
		{
			int first = Convert.ToInt32(attribute(node, "first"));
			int second = Convert.ToInt32(attribute(node, "second"));
			float amount = Convert.ToSingle(attribute(node, "amount")) / Atlas.width * HScale;
			TGlyph glyph = Glyphs.Get(first);
			KerningDictionary kerning = glyph.kerning;

			if (kerning == null)
			{
				kerning = new KerningDictionary();
				glyph.kerning = kerning;
			}

			kerning.Set(second, amount);
			KerningPairs++;
		}
	}

	string attribute(XmlNode node, string name)
	{
		XmlAttribute attr = (XmlAttribute)node.Attributes.GetNamedItem(name);
		return attr == null ? "" : attr.Value;
	}
}
