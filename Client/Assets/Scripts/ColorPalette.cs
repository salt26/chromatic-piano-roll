using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SerializableCollections;

public class ColorPalette : MonoBehaviour
{
    public List<ColorCodeDictionary> colorPalettes;

	public Color colorForNoteHighlight;

	// ["#77D977", "#A877D9", "#D9D977", "#77A8D9", "#D97777", "#77D9A8", "#D977D9", "#A8D977", "#7777D9", "#D9A877", "#77D9D9", "#D977A8"]

	public static Color ChangeAlpha(Color original, float alpha)
    {
		return new Color(original.r, original.g, original.b, Mathf.Clamp(alpha, 0f, 1f));
    }
}

[Serializable]
public class ColorCodeTuple : SerializableKeyValuePair<Note.Pitch, Color>
{
	public ColorCodeTuple(Note.Pitch item1, Color item2) : base(item1, item2) { }
}

[Serializable]
public class ColorCodeDictionary : SerializableDictionary<Note.Pitch, Color>
{
	[SerializeField] private List<ColorCodeTuple> _pairs = new List<ColorCodeTuple>();

	protected override List<SerializableKeyValuePair<Note.Pitch, Color>> _keyValuePairs
	{
		get
		{
			var list = new List<SerializableKeyValuePair<Note.Pitch, Color>>();
			foreach (var pair in _pairs)
			{
				list.Add(new SerializableKeyValuePair<Note.Pitch, Color>(pair.Key, pair.Value));
			}
			return list;
		}

		set
		{
			_pairs.Clear();
			foreach (var kvp in value)
			{
				_pairs.Add(new ColorCodeTuple(kvp.Key, kvp.Value));
			}
		}
	}
}