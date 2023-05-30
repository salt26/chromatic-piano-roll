using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Sanford.Multimedia.Midi;

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

    [HideInInspector]
    public float scaleValue;
    [HideInInspector]
    public float scrollValue;

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
        rangeSlider.MinRangeSize = Mathf.Clamp01((XScale - 100000f) / Math.Max(400000f, EndTiming / 17 - 100000f));
        scaleValue = Mathf.Clamp01((XScale - 100000f) / Math.Max(400000f, EndTiming / 17 - 100000f));
        rangeSlider.LowValue = 0;
        rangeSlider.HighValue = scaleValue;
    }

    void Update()
    {
        scrollValue = (rangeSlider.HighValue + rangeSlider.LowValue) / 2f;
        mainCamera.transform.localPosition = new Vector3(Mathf.Lerp(4.5f, EndTiming / XScale - 4.5f, scrollValue), 0f, -10f);
    }

    public void UpdateXScale()
    {
        scaleValue = rangeSlider.HighValue - rangeSlider.LowValue;
        XScale = Mathf.Lerp(100000f, Math.Max(500000f, EndTiming / 17f), scaleValue);
        foreach (Note note in notes)
        {
            note.UpdateNote();
        }
    }

    public void UpdateRangeSliderValues(float lowValue, float highValue, float scaleValue)
    {
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
        rangeSlider.LowValue = lowValue;
        rangeSlider.HighValue = highValue;
    }
}
