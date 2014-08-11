using UnityEngine;

public struct TypogenicGlyphClickEvent
{
	public TypogenicText source;
	public Vector3 point;
	public int index;

	public TypogenicGlyphClickEvent(TypogenicText clickSource, Vector3 clickPoint, int clickIndex)
	{
		source = clickSource;
		point = clickPoint;
		index = clickIndex;
	}
}
