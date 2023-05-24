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

    private List<Note> notes = new();
    private long endTiming;

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
        endTiming = 0;
        JArray jArray = JArray.Parse(notesJson.text);
        foreach (JObject noteJson in jArray)
        {
            GameObject g = Instantiate(notePrefab, notesParent.transform);
            Note note = g.GetComponent<Note>();
            note.Initialize(noteJson);
            notes.Add(note);
            endTiming = Math.Max(endTiming, note.endTiming);
        }

    }

    void Update()
    {
        mainCamera.transform.localPosition = new Vector3(Mathf.Lerp(8.5f, endTiming / XScale - 8.5f, scrollSlider.value), 0f, -10f);
    }

    public void UpdateXScale()
    {
        XScale = Mathf.Lerp(100000f, Math.Max(100000f, endTiming / 17f), scaleSlider.value);
        foreach (Note note in notes)
        {
            note.UpdateNote();
        }
    }
}
