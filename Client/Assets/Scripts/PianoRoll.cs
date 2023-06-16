using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class PianoRoll : MonoBehaviour
{
    public static PianoRoll pr;

    public TextAsset notesJson;
    public GameObject notePrefab;
    public GameObject notesParent;
    public int colorPaletteIndex;
    public Camera mainCamera;
    public RangeSlider rangeSlider;

    [HideInInspector]
    public List<Note> notes = new();

    public long EndTiming { get; private set; }

    [SerializeField]
    private float _xScale = 500000f;

    public float XScale
    {
        get
        {
            return _xScale;
        }
        set
        {
            if (value <= 0f) _xScale = 500000f;
            else _xScale = value;
        }
    }

    public const float MAX_OFFSET = 8.5f;
    public const float MIN_OFFSET = 4.5f;

    [HideInInspector]
    public float scaleValue;
    [HideInInspector]
    public float scrollValue;
    [HideInInspector]
    public float scrollOffset;

    void Awake()
    {
        if (pr is not null && pr != this)
        {
            Destroy(gameObject);
            return;
        }
        pr = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        EndTiming = 0;
        JArray jArray = JArray.Parse(notesJson.text);
        foreach (JObject noteJson in jArray)
        {
            GameObject g = Instantiate(notePrefab, notesParent.transform);
            Note note = g.GetComponent<Note>();
            note.Initialize(noteJson);
            notes.Add(note);
            EndTiming = Math.Max(EndTiming, note.endTiming);
        }
        rangeSlider.MinRangeSize = Mathf.Clamp01((XScale - 100000f) / Math.Max(400000f, EndTiming / (MIN_OFFSET * 2f) - 100000f));
        scaleValue = rangeSlider.MinRangeSize;
        rangeSlider.LowValue = 0;
        rangeSlider.HighValue = scaleValue;
        scrollValue = (rangeSlider.LowValue + rangeSlider.HighValue) / 2f;
        mainCamera.transform.localPosition = new Vector3(EndTiming / XScale * scrollValue, 0f, -10f);
        UpdateXScale();
    }

    void Update()
    {
        //float onset = (rangeSlider.HighValue + rangeSlider.LowValue) / 2f;
        //scrollValue = (rangeSlider.HighValue + rangeSlider.LowValue) / 2f;
        //float offset = (rangeSlider.HighValue - rangeSlider.LowValue - rangeSlider.MinRangeSize) / (1 - rangeSlider.MinRangeSize) * 4f + 4.5f;
        //scrollValue = (((rangeSlider.MinRangeSize + rangeSlider.LowValue) / rangeSlider.MinRangeSize * 4f + 4.5f) + ((rangeSlider.HighValue - rangeSlider.MinRangeSize) / (1 - rangeSlider.MinRangeSize) * 4f + 4.5f)) / 2f;
        //scrollValue = (rangeSlider.LowValue + rangeSlider.HighValue) / 2f;
        //mainCamera.transform.localPosition = new Vector3(EndTiming / XScale * scrollValue, 0f, -10f);
    }

    public void UpdateXScale()
    {
        print("UpdateXScale");
        scaleValue = rangeSlider.HighValue - rangeSlider.LowValue;
        scrollOffset = Mathf.Lerp(MIN_OFFSET, MAX_OFFSET, Mathf.Pow(Mathf.Clamp01(rangeSlider.HighValue - rangeSlider.LowValue - rangeSlider.MinRangeSize) / (1 - rangeSlider.MinRangeSize), 0.5f));
        float oldXScale = XScale;
        XScale = Mathf.Lerp(100000f, Math.Max(500000f, EndTiming / (scrollOffset * 2f)), scaleValue);
        if (!Approximately(oldXScale, XScale))
        {
            print("UpdateAllNotes");
            UpdateAllNotes();
        }
    }

    public void UpdateAllNotes()
    {
        foreach (Note note in notes)
        {
            note.UpdateNote();
        }
    }

    public void UpdateRangeSliderValues(float lowValue, float highValue, float scaleValue)
    {
        if (lowValue > highValue)
        {
            (highValue, lowValue) = (lowValue, highValue);
        }

        if (lowValue < rangeSlider.MinValue && highValue <= rangeSlider.MaxValue)
        {
            lowValue = rangeSlider.MinValue;
            highValue = scaleValue;
            this.scrollValue = (lowValue + highValue) / 2f;
            this.scaleValue = scaleValue;
        }
        else if (highValue > rangeSlider.MaxValue && lowValue >= rangeSlider.MinValue)
        {
            highValue = rangeSlider.MaxValue;
            lowValue = rangeSlider.MaxValue - scaleValue;
            this.scrollValue = (lowValue + highValue) / 2f;
            this.scaleValue = scaleValue;
        }
        else if (lowValue < rangeSlider.MinValue && highValue > rangeSlider.MaxValue)
        {
            lowValue = rangeSlider.MinValue;
            highValue = rangeSlider.MaxValue;
            this.scrollValue = 0.5f;
            this.scaleValue = 1f;
        }
        rangeSlider.SetValueWithoutNotify(lowValue, highValue);
        this.scrollValue = (lowValue + highValue) / 2f;
        this.scaleValue = highValue - lowValue;
        rangeSlider.OnValueChanged.Invoke(lowValue, highValue);
    }

    private static bool Approximately(float a, float b)
    {
        return Mathf.Abs(b - a) < Mathf.Max(1E-12f * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)), Mathf.Epsilon * 8f);
    }
}
