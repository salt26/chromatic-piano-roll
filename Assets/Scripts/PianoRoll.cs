using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.UI;
using Sanford.Multimedia.Midi;

public class PianoRoll : MonoBehaviour
{
    public static PianoRoll pr;

    public TextAsset notesJson;
    public AudioClip audioClip;
    public GameObject notePrefab;
    public GameObject notesParent;
    public int colorPaletteIndex;
    public Camera mainCamera;
    public Slider scrollSlider;
    public Slider scaleSlider;

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
        scaleSlider.value = Mathf.Clamp01((XScale - 100000f) / Math.Max(400000f, EndTiming / 17 - 100000f));
    }

    void Update()
    {
        mainCamera.transform.localPosition = new Vector3(Mathf.Lerp(4.5f, EndTiming / XScale - 4.5f, scrollSlider.value), 0f, -10f);
    }

    public void UpdateXScale()
    {
        XScale = Mathf.Lerp(100000f, Math.Max(500000f, EndTiming / 17f), scaleSlider.value);
        foreach (Note note in notes)
        {
            note.UpdateNote();
        }
    }
}
