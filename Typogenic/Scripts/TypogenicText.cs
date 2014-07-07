using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum TTextAlignment
{
	Left,
	Center,
	Right
}

public enum TFillMode
{
	SingleColor,
	VerticalGradient,
	HorizontalGradient,
	QuadGradient,
	ProjectedTexture,
	StretchedTexture
}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("")]
public class TypogenicText : MonoBehaviour
{
	public TypogenicFont Font;
	public string Text = "Hello, World !";
	public float Size = 16.0f;
	public float Leading = 0f;
	public float Tracking = 0f;
	public float ParagraphSpacing = 0f;
	public float WordWrap = 0f;
	public TTextAlignment Alignment = TTextAlignment.Left;
	public TFillMode FillMode = TFillMode.SingleColor;
	public Color ColorTopLeft = Color.white;
	public Color ColorTopRight = Color.white;
	public Color ColorBottomLeft = Color.white;
	public Color ColorBottomRight = Color.white;
	public bool GenerateNormals = true;

	public float Width { get; protected set; }
	public float Height { get; protected set; }

	public Mesh Mesh { get { return m_Mesh; } }

	protected Mesh m_Mesh;
	protected List<Vector3> m_Vertices = new List<Vector3>();
	protected List<Vector2> m_UVs = new List<Vector2>();
	protected List<Vector2> m_UVs2 = new List<Vector2>();
	protected List<Color> m_Colors = new List<Color>();
	protected List<int> m_Indices = new List<int>();

	// Not the best way to track changes but Unity can't serialize properties,
	// so it'll do the job just fine for now.
	string _text;
	float _size;
	float _leading;
	float _tracking;
	float _paragraphSpacing;
	float _wordWrap;
	TTextAlignment _alignment;
	TFillMode _fillMode;
	Color _colorTopLeft;
	Color _colorTopRight;
	Color _colorBottomLeft;
	Color _colorBottomRight;
	bool _generateNormals;

	public bool AutoRebuild = true;
	public bool IsDirty
	{
		get
		{
			if (Text != _text) return true;
			else if (Size != _size) return true;
			else if (Leading != _leading) return true;
			else if (Tracking != _tracking) return true;
			else if (ParagraphSpacing != _paragraphSpacing) return true;
			else if (WordWrap != _wordWrap) return true;
			else if (Alignment != _alignment) return true;
			else if (FillMode != _fillMode) return true;
			else if (ColorTopLeft != _colorTopLeft) return true;
			else if (ColorTopRight != _colorTopRight) return true;
			else if (ColorBottomLeft != _colorBottomLeft) return true;
			else if (ColorBottomRight != _colorBottomRight) return true;
			else if (GenerateNormals != _generateNormals) return true;

			return false;
		}
	}

	void OnEnable()
	{
		GetComponent<MeshFilter>().mesh = m_Mesh = new Mesh();
		m_Mesh.name = "Text Mesh";
		m_Mesh.hideFlags = HideFlags.HideAndDontSave;
		RebuildMesh();
	}

	void OnDisable()
	{
		if (Application.isEditor)
		{
			GetComponent<MeshFilter>().mesh = null;
			DestroyImmediate(m_Mesh);
		}
	}

	void Reset()
	{
		RebuildMesh();
	}

	void LateUpdate()
	{
		if (AutoRebuild && IsDirty)
			RebuildMesh();
	}

	public void Set(string text = null, float? size = null, float? leading = null, float? tracking = null, float? paragraphSpacing = null, TTextAlignment? alignement = null, float? wordWrap = null)
	{
		Text = text ?? Text;
		Size = size ?? Size;
		Leading = leading ?? Leading;
		Tracking = tracking ?? Tracking;
		ParagraphSpacing = paragraphSpacing ?? ParagraphSpacing;
		Alignment = alignement ?? Alignment;
		WordWrap = wordWrap ?? WordWrap;
	}

	public void RebuildMesh()
	{
		if (Font == null)
			return;

		_text = Text;
		_size = Size;
		_leading = Leading;
		_tracking = Tracking;
		_paragraphSpacing = ParagraphSpacing;
		_wordWrap = WordWrap;
		_alignment = Alignment;
		_fillMode = FillMode;
		_colorTopLeft = ColorTopLeft;
		_colorTopRight = ColorTopRight;
		_colorBottomLeft = ColorBottomLeft;
		_colorBottomRight = ColorBottomRight;
		_generateNormals = GenerateNormals;

		m_Mesh.Clear();

		m_Vertices.Clear();
		m_UVs.Clear();
		m_UVs2.Clear();
		m_Colors.Clear();
		m_Indices.Clear();

		if (IsTextNullOrEmpty())
			return;

		Width = 0f;
		Height = 0f;
		float cursorX = 0f, cursorY = 0f;
		string[] lines = Regex.Split(Text, @"\r?\n|\r");

		if (WordWrap <= 0)
		{
			foreach (string line in lines)
			{
				if (Alignment == TTextAlignment.Left)
					cursorX = 0f;
				else if (Alignment == TTextAlignment.Center)
					cursorX = GetStringWidth(line) / -2f;
				else if (Alignment == TTextAlignment.Right)
					cursorX = -GetStringWidth(line);

				BlitString(line, cursorX, cursorY);
				cursorY += Font.LineHeight * Size + Leading + ParagraphSpacing;
			}
		}
		else
		{
			List<int> vertexPointers = new List<int>();

			foreach (string line in lines)
			{
				string[] words = line.Split(' ');
				cursorX = 0;

				foreach (string w in words)
				{
					string word = w;

					if (Alignment == TTextAlignment.Right)
						word = " " + word;
					else
						word += " ";

					float wordWidth = GetStringWidth(word);

					if (cursorX + wordWidth > WordWrap)
					{
						OffsetStringPosition(vertexPointers, cursorX);
						vertexPointers.Clear();

						cursorX = 0;
						cursorY += Font.LineHeight * Size + Leading;
					}

					cursorX = BlitString(word, cursorX, cursorY, vertexPointers);
				}

				OffsetStringPosition(vertexPointers, cursorX);
				vertexPointers.Clear();
				cursorY += Font.LineHeight * Size + Leading + ParagraphSpacing;
			}
		}

		Height = cursorY;

		m_Mesh.vertices = m_Vertices.ToArray();
		m_Mesh.uv = m_UVs.ToArray();
		m_Mesh.triangles = m_Indices.ToArray();
		m_Mesh.colors = null;
		m_Mesh.uv2 = null;
		m_Mesh.normals = null;

		if (FillMode == TFillMode.StretchedTexture || FillMode == TFillMode.ProjectedTexture)
			m_Mesh.uv2 = m_UVs2.ToArray();
		else
			m_Mesh.colors = m_Colors.ToArray();

		if (GenerateNormals)
		{
			Vector3[] normals = new Vector3[m_Vertices.Count];

			for (int i = 0; i < m_Vertices.Count; i++)
				normals[i] = new Vector3(0f, 0f, -1f);

			m_Mesh.normals = normals;
		}

		m_Mesh.RecalculateBounds();
	}

	bool IsTextNullOrEmpty()
	{
		if (string.IsNullOrEmpty(Text))
			return true;

		return string.IsNullOrEmpty(Text.Trim());
	}

	float BlitString(string str, float cursorX, float cursorY, List<int> vertexPointers = null)
	{
		TGlyph prevGlyph = null;

		foreach (char c in str)
		{
			int charCode = (int)c;
			TGlyph glyph = Font.Glyphs.Get(charCode);

			if (glyph == null)
				continue;

			if (charCode == 32)
			{
				cursorX += glyph.xAdvance * Size + Tracking;
				continue;
			}

			float kerning = 0f;

			if (prevGlyph != null)
				kerning = prevGlyph.GetKerning(charCode);

			if (vertexPointers != null)
				vertexPointers.Add(m_Vertices.Count);

			BlitQuad(
					new Rect(
							cursorX + glyph.xOffset * Size + kerning,
							cursorY + glyph.yOffset * Size,
							glyph.rect.width * Size,
							glyph.rect.height * Size
						),
					glyph
				);

			cursorX += glyph.xAdvance * Size + Tracking + kerning;
			prevGlyph = glyph;
		}

		if (cursorX > Width)
			Width = cursorX;

		return cursorX;
	}

	float GetStringWidth(string str)
	{
		TGlyph prevGlyph = null;
		float width = 0f;

		foreach (char c in str)
		{
			int charCode = (int)c;
			TGlyph glyph = Font.Glyphs.Get(charCode);

			if (glyph == null)
				continue;

			float kerning = 0f;

			if (prevGlyph != null)
				kerning = prevGlyph.GetKerning(charCode);

			width += glyph.xAdvance * Size + Tracking + kerning;
			prevGlyph = glyph;
		}

		return width;
	}

	void OffsetStringPosition(List<int> vertexPointers, float offsetX)
	{
		if (Alignment == TTextAlignment.Right)
		{
			foreach (int p in vertexPointers)
			{
				Vector3 v;
				v = m_Vertices[p    ]; v.x -= offsetX; m_Vertices[p    ] = v;
				v = m_Vertices[p + 1]; v.x -= offsetX; m_Vertices[p + 1] = v;
				v = m_Vertices[p + 2]; v.x -= offsetX; m_Vertices[p + 2] = v;
				v = m_Vertices[p + 3]; v.x -= offsetX; m_Vertices[p + 3] = v;
			}
		}
		else if (Alignment == TTextAlignment.Center)
		{
			float halfOffsetX = offsetX / 2f;

			foreach (int p in vertexPointers)
			{
				Vector3 v;
				v = m_Vertices[p    ]; v.x -= halfOffsetX; m_Vertices[p    ] = v;
				v = m_Vertices[p + 1]; v.x -= halfOffsetX; m_Vertices[p + 1] = v;
				v = m_Vertices[p + 2]; v.x -= halfOffsetX; m_Vertices[p + 2] = v;
				v = m_Vertices[p + 3]; v.x -= halfOffsetX; m_Vertices[p + 3] = v;
			}
		}
	}

	void BlitQuad(Rect quad, TGlyph glyph)
	{
		int index = m_Vertices.Count;
		Rect uvs = glyph.rect;

		m_Vertices.Add(new Vector3(quad.x, -quad.y, 0f));
		m_Vertices.Add(new Vector3(quad.x + quad.width, -quad.y, 0f));
		m_Vertices.Add(new Vector3(quad.x + quad.width, -quad.y - quad.height, 0f));
		m_Vertices.Add(new Vector3(quad.x, -quad.y - quad.height, 0f));

		m_UVs.Add(new Vector2(uvs.x / Font.HScale, 1 - uvs.y / Font.VScale));
		m_UVs.Add(new Vector2((uvs.x + uvs.width) / Font.HScale, 1 - uvs.y / Font.VScale));
		m_UVs.Add(new Vector2((uvs.x + uvs.width) / Font.HScale, 1 - (uvs.y + uvs.height) / Font.VScale));
		m_UVs.Add(new Vector2(uvs.x / Font.HScale, 1 - (uvs.y + uvs.height) / Font.VScale));

		switch (FillMode)
		{
			case TFillMode.SingleColor:
				m_Colors.Add(ColorTopLeft);
				m_Colors.Add(ColorTopLeft);
				m_Colors.Add(ColorTopLeft);
				m_Colors.Add(ColorTopLeft);
				break;
			case TFillMode.VerticalGradient:
				m_Colors.Add(ColorTopLeft);
				m_Colors.Add(ColorTopLeft);
				m_Colors.Add(ColorBottomLeft);
				m_Colors.Add(ColorBottomLeft);
				break;
			case TFillMode.HorizontalGradient:
				m_Colors.Add(ColorTopLeft);
				m_Colors.Add(ColorBottomLeft);
				m_Colors.Add(ColorBottomLeft);
				m_Colors.Add(ColorTopLeft);
				break;
			case TFillMode.QuadGradient:
				m_Colors.Add(ColorTopLeft);
				m_Colors.Add(ColorTopRight);
				m_Colors.Add(ColorBottomRight);
				m_Colors.Add(ColorBottomLeft);
				break;
			case TFillMode.StretchedTexture:
				m_UVs2.Add(new Vector2(0f, 1f));
				m_UVs2.Add(new Vector2(1f, 1f));
				m_UVs2.Add(new Vector2(1f, 0f));
				m_UVs2.Add(new Vector2(0f, 0f));
				break;
			case TFillMode.ProjectedTexture:
				float h = uvs.height / Font.LineHeight;
				float w = uvs.width / Font.LineHeight;
				m_UVs2.Add(new Vector2(glyph.xOffset, h - glyph.yOffset));
				m_UVs2.Add(new Vector2(w - glyph.xOffset, h - glyph.yOffset));
				m_UVs2.Add(new Vector2(w - glyph.xOffset, glyph.yOffset));
				m_UVs2.Add(new Vector2(glyph.xOffset, glyph.yOffset));
				break;
			default:
				break;
		}

		m_Indices.Add(index);
		m_Indices.Add(index + 1);
		m_Indices.Add(index + 2);
		m_Indices.Add(index);
		m_Indices.Add(index + 2);
		m_Indices.Add(index + 3);
	}
}
