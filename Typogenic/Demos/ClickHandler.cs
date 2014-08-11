using UnityEngine;
using System.Text.RegularExpressions;

public class ClickHandler : MonoBehaviour
{
	public string nonBreakingPunctuation = "-'";

	TypogenicText textObject;

	void Start()
	{
		textObject = GetComponent<TypogenicText>();
	}
	
	void OnGlyphClicked(TypogenicGlyphClickEvent e)
	{
		Debug.Log(string.Format(
			"Clicked {0} at '{1}' in '{2}' at index {3}",
			e.source.name,
			characterAt(e.index),
			wordAt(e.index),
			e.index
		));
	}

	public char characterAt(int index)
	{
		return textObject.Text[index];
	}

	bool isWordCharacter(char c) 
	{
		if (char.IsLetterOrDigit(c))
			return true;

		// Preserve escape sequences for now, then strip them at the end.
		if (c.Equals('\\'))
			return true;

		if (nonBreakingPunctuation.IndexOf(c) > -1)
			return true;

		return false;
	}

	public string wordAt(int index)
	{
		int start = index, end = index;
		string text = textObject.Text;

		if (!isWordCharacter(text[start]))
		{
			// Clicked a non-word glyph - just return the character as-is.
			return text[start].ToString();
		}

		while (start > 0 && isWordCharacter(text[start - 1]))
			start--;

		while (end < text.Length - 1 && isWordCharacter(text[end + 1]))
			end++;

		text = text.Substring(start, end - start + 1);

		// Strip out control codes last, so words with control codes
		// embedded within them are still detected as a single word.
		text = Regex.Replace(text, "\\\\[0-9]", "");

		return text;
	}
}
