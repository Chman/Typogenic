using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Based on http://forum.unity3d.com/threads/best-practices-for-generic-dictionary-serialization.215477/
/// Not as fast as a proper Dictionary, but will do the trick for now
/// </summary>
[Serializable]
public class SerializedDictionary<Key, Value>
{
	[SerializeField]
	private List<Key> keys = new List<Key>();

	[SerializeField]
	private List<Value> values = new List<Value>();

	public int Count { get { return keys.Count; } }

	public void Remove(Key key)
	{
		if (!keys.Contains(key))
			return;

		int index = keys.IndexOf(key);
		keys.RemoveAt(index);
		values.RemoveAt(index);
	}

	public bool TryGetValue(Key key, out Value value)
	{
		if (keys.Count != values.Count)
		{
			keys.Clear();
			values.Clear();
			value = default(Value);
			return false;
		}

		if (!keys.Contains(key))
		{
			value = default(Value);
			return false;
		}

		int index = keys.IndexOf(key);
		value = values[index];

		return true;
	}

	public Value Get(Key key)
	{
		if (!keys.Contains(key))
			return default(Value);

		int index = keys.IndexOf(key);
		return values[index];
	}

	public void Set(Key key, Value value)
	{
		if (!keys.Contains(key))
		{
			keys.Add(key);
			values.Add(value);
			return;
		}

		int index = keys.IndexOf(key);
		values[index] = value;
	}
}
