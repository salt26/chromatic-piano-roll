using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using UnityEngine.UIElements;

public class PianoRoll : MonoBehaviour
{
    public static PianoRoll pr;

    public GameObject notePrefab;
    public GameObject notesParent;
    public int colorPaletteIndex;
    public Camera mainCamera;
    public RangeSlider rangeSlider;
    public TMP_Dropdown musicDropdown;
    public TMP_Dropdown colorDropdown;
    public GameObject loadingPanel;
    public List<TextAsset> jsonData = new();

    [HideInInspector]
    public List<Note> notes = new();

    public bool IsReady { get; private set; }

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

    [SerializeField]
    private float initialXScale = 500000f;

    public const float MAX_OFFSET = 8.5f;
    public const float MIN_OFFSET = 4.5f;

    [HideInInspector]
    public float scaleValue;
    [HideInInspector]
    public float scrollValue;
    [HideInInspector]
    public float scrollOffset;

    private Dictionary<string, JArray> jsonFiles;

    void Awake()
    {
        IsReady = false;
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
        loadingPanel.SetActive(true);
        // TODO 파일 목록 표시
        jsonFiles = new Dictionary<string, JArray>();
        List<string> jsonNames = new List<string>();
        /*
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/Data/JSON");
        foreach (FileInfo file in di.GetFiles())
        {
            if (file.Extension.Equals(".json"))
            {
                jsonFiles.Add(file.Name.Substring(0, file.Name.LastIndexOf('.')),
                    JArray.Parse(File.ReadAllText(file.FullName)));
                jsonNames.Add(file.Name.Substring(0, file.Name.LastIndexOf('.')));
            }
        }
        */
        foreach (TextAsset ta in jsonData)
        {
            jsonFiles.Add(ta.name, JArray.Parse(ta.text));
            jsonNames.Add(ta.name);
            break;
        }
        musicDropdown.ClearOptions();
        musicDropdown.AddOptions(jsonNames);
        musicDropdown.value = 0;
        Initialize();
    }

    void Update()
    {
        //float onset = (rangeSlider.HighValue + rangeSlider.LowValue) / 2f;
        //scrollValue = (rangeSlider.HighValue + rangeSlider.LowValue) / 2f;
        //float offset = (rangeSlider.HighValue - rangeSlider.LowValue - rangeSlider.MinRangeSize) / (1 - rangeSlider.MinRangeSize) * 4f + 4.5f;
        //scrollValue = (((rangeSlider.MinRangeSize + rangeSlider.LowValue) / rangeSlider.MinRangeSize * 4f + 4.5f) + ((rangeSlider.HighValue - rangeSlider.MinRangeSize) / (1 - rangeSlider.MinRangeSize) * 4f + 4.5f)) / 2f;
        scrollValue = (rangeSlider.LowValue + rangeSlider.HighValue) / 2f;
        mainCamera.transform.localPosition = new Vector3(EndTiming / XScale * scrollValue, 0f, -10f);
    }

    void Initialize()
    {
        IsReady = false;
        loadingPanel.SetActive(true);
        GetComponent<PlayMusic>().Stop();
        EndTiming = 0;
        //JArray jArray = JArray.Parse(notesJson.text);
        if (notes == null)
        {
            notes = new();
        }
        foreach (Note note in notes)
        {
            Destroy(note.gameObject);
        }
        notes.Clear();

        if (musicDropdown.options.Count <= musicDropdown.value || musicDropdown.value < 0)
        {
            Debug.LogError("Music index is out of range!");
            musicDropdown.value = 0;
        }

        bool b = jsonFiles.TryGetValue(musicDropdown.options[musicDropdown.value].text, out JArray jArray);
        if (!b || jArray == null)
        {
            Debug.LogError("Cannot find the music file!");
            return;
        }

        foreach (JObject noteJson in jArray)
        {
            GameObject g = Instantiate(notePrefab, notesParent.transform);
            Note note = g.GetComponent<Note>();
            note.Initialize(noteJson);
            notes.Add(note);
            EndTiming = Math.Max(EndTiming, note.endTiming);
        }
        rangeSlider.MinRangeSize = Mathf.Clamp01((initialXScale - 100000f) / Math.Max(400000f, EndTiming / (MIN_OFFSET * 2f) - 100000f));
        scaleValue = 1f;
        //scaleValue = rangeSlider.MinRangeSize;
        rangeSlider.LowValue = 0;
        //rangeSlider.HighValue = scaleValue;
        rangeSlider.HighValue = 1;
        scrollValue = (rangeSlider.LowValue + rangeSlider.HighValue) / 2f;
        UpdateXScale();
        mainCamera.transform.localPosition = new Vector3(EndTiming / initialXScale * scrollValue, 0f, -10f);
        loadingPanel.SetActive(false);
        IsReady = true;
    }

    public void InitializeWithCustomMidi(string preprocessedMidi)
    {
        IsReady = false;
        loadingPanel.SetActive(true);
        GetComponent<PlayMusic>().Stop();
        EndTiming = 0;
        //JArray jArray = JArray.Parse(notesJson.text);
        if (notes == null)
        {
            notes = new();
        }
        foreach (Note note in notes)
        {
            Destroy(note.gameObject);
        }
        notes.Clear();

        /*
        if (musicDropdown.options.Count <= musicDropdown.value || musicDropdown.value < 0)
        {
            Debug.LogError("Music index is out of range!");
            musicDropdown.value = 0;
        }

        bool b = jsonFiles.TryGetValue(musicDropdown.options[musicDropdown.value].text, out JArray jArray);
        if (!b || jArray == null)
        {
            Debug.LogError("Cannot find the music file!");
            return;
        }
        */

        JArray jArray = JArray.Parse(preprocessedMidi);

        foreach (JObject noteJson in jArray)
        {
            GameObject g = Instantiate(notePrefab, notesParent.transform);
            Note note = g.GetComponent<Note>();
            note.Initialize(noteJson);
            notes.Add(note);
            EndTiming = Math.Max(EndTiming, note.endTiming);
        }
        rangeSlider.MinRangeSize = Mathf.Clamp01((initialXScale - 100000f) / Math.Max(400000f, EndTiming / (MIN_OFFSET * 2f) - 100000f));
        scaleValue = 1f;
        //scaleValue = rangeSlider.MinRangeSize;
        rangeSlider.LowValue = 0;
        //rangeSlider.HighValue = scaleValue;
        rangeSlider.HighValue = 1;
        scrollValue = (rangeSlider.LowValue + rangeSlider.HighValue) / 2f;
        UpdateXScale();
        mainCamera.transform.localPosition = new Vector3(EndTiming / initialXScale * scrollValue, 0f, -10f);
        loadingPanel.SetActive(false);
        IsReady = true;
    }

    public void UpdateXScale()
    {
        scaleValue = rangeSlider.HighValue - rangeSlider.LowValue;
        scrollOffset = Mathf.Lerp(MIN_OFFSET, MAX_OFFSET, Mathf.Pow(Mathf.Clamp01(rangeSlider.HighValue - rangeSlider.LowValue - rangeSlider.MinRangeSize) / (1 - rangeSlider.MinRangeSize), 0.5f));
        float oldXScale = XScale;
        XScale = Mathf.Lerp(100000f, Math.Max(500000f, EndTiming / (scrollOffset * 2f)), scaleValue);
        if (!Mathf.Approximately(oldXScale, XScale))
        {
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

    public void UpdateColor()
    {
        if (!IsReady) return;
        colorPaletteIndex = colorDropdown.value;
        UpdateAllNotes();
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

    public void ChangeMusic()
    {
        if (GetComponent<PlayMusic>().IsPlaying) return;
        Initialize();
    }

    private static bool Approximately(float a, float b)
    {
        return Mathf.Abs(b - a) < Mathf.Max(1E-12f * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)), Mathf.Epsilon * 8f);
    }
}
