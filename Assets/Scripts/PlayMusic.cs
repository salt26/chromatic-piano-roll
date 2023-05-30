using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NFluidsynth;
using System.IO;

public class PlayMusic : MonoBehaviour
{
    private Settings settings;
    private Synth syn;
    private AudioDriver audioDriver;

    public long playbackPosition = 0;
    public List<NoteOffset> offsetBuffer;

    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Button cameraLockButton;

    public GameObject playbackBar;

    private bool IsReady { get; set; }

    private bool canStart = false;
    public bool HasStart
    {
        get
        {
            return canStart && IsReady;
        }
        private set
        {
            canStart = value;
        }
    }

    private bool IsPlaying { get; set; }

    private bool _hasCameraLocked;
    public bool HasCameraLocked {
        get
        {
            return _hasCameraLocked;
        }
        private set
        {
            _hasCameraLocked = value;
            if (_hasCameraLocked)
            {
                cameraLockButton.GetComponent<Image>().color = new Color(0.8814773f, 0.8867924f, 0.489409f);
            }
            else
            {
                cameraLockButton.GetComponent<Image>().color = new Color(1f, 1f, 1f);
            }
        }
    }

    void Awake()
    {
        IsReady = false;
        HasStart = false;
        IsPlaying = false;
        HasCameraLocked = false;

        //outDevice = new OutputDevice(0);

        #region For synthesizing MIDI with a SoundFont (NFluidsynth)

        settings = new Settings();
        settings[ConfigurationKeys.SynthAudioChannels].IntValue = 2;

        syn = new Synth(settings);
        try
        {
            syn.LoadSoundFont("Assets/Resources/FluidR3_GM.sf2", true);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError(e.StackTrace);
            return;
        }
        for (int i = 0; i < 16; i++)
        {
            syn.SoundFontSelect(i, 0);
        }

        audioDriver = new AudioDriver(settings, syn);

        #endregion

        offsetBuffer = new();

        IsReady = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (IsReady && !HasStart)
        {
            HasStart = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!HasStart) return;

        long oldPosition = playbackPosition;

        if (IsPlaying && playbackPosition <= PianoRoll.pr.EndTiming)
        {
            playbackPosition += (long)(Time.deltaTime * 1000000f);
            playbackBar.transform.localPosition = new Vector3(playbackPosition / PianoRoll.pr.XScale, 0f, 0f);
            playButton.gameObject.SetActive(false);
            playButton.interactable = false;
            pauseButton.interactable = true;
            pauseButton.gameObject.SetActive(true);
        }
        else
        {
            pauseButton.gameObject.SetActive(false);
            pauseButton.interactable = false;
            playButton.interactable = true;
            playButton.gameObject.SetActive(true);
        }

        if (HasCameraLocked)
        {
            PianoRoll.pr.mainCamera.transform.localPosition = new Vector3(playbackPosition / PianoRoll.pr.XScale + 4.5f, 0f, -10f);
            //PianoRoll.pr.scrollSlider.interactable = false;

            float lowValue = PianoRoll.pr.scrollValue - PianoRoll.pr.scaleValue / 2f;
            float highValue = PianoRoll.pr.scrollValue + PianoRoll.pr.scaleValue / 2f;
            PianoRoll.pr.UpdateRangeSliderValues(lowValue, highValue, Mathf.Clamp01(playbackPosition / (PianoRoll.pr.EndTiming - 9f * PianoRoll.pr.XScale)));
        }
        else
        {
            //PianoRoll.pr.scrollSlider.interactable = true;
        }

        int death = 0;
        int num = offsetBuffer.Count;
        foreach (NoteOffset offset in offsetBuffer.Where(x => x.endTiming < playbackPosition))
        {
            offset.note.HighlightOff();
            try
            {
                syn.NoteOff(offset.channel, offset.notePosition);
            }
            catch (FluidSynthInteropException e)
            {
                Debug.Log(e);
            }
            death++;
        }
        offsetBuffer.RemoveAll(x => x.endTiming < playbackPosition);

        if (IsPlaying && playbackPosition <= PianoRoll.pr.EndTiming)
        {
            foreach (Note note in PianoRoll.pr.notes.Where(x => x.startTiming >= oldPosition && x.startTiming < playbackPosition))
            {
                note.HighlightOn();
                try
                {
                    syn.NoteOn(note.channel, note.notePosition, note.noteVelocity);
                }
                catch (FluidSynthInteropException e)
                {
                    Debug.Log(e);
                }
                offsetBuffer.Add(new NoteOffset(note.id, note.channel, note.notePosition, note.endTiming, note));
            }
        }
    }

    private void OnApplicationQuit()
    {
        Dispose();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    /// <summary>
    /// ¿Ωæ« √‚∑¬ ¿Âƒ°∏¶ ≤Ù∞Ì ≈∏¿Ã∏”∏¶ ∏ÿ√‰¥œ¥Ÿ.
    /// </summary>
    public void Dispose()
    {
        if (HasStart)
        {
            //for (int i = 0; i <= 8; i++) StopPlaying(i);
            audioDriver.Dispose();
            syn.Dispose();
            settings.Dispose();
            HasStart = false;
            IsReady = false;
        }
    }

    public void Play()
    {
        if (HasStart)
        {
            IsPlaying = true;
            HasCameraLocked = true;
        }
    }

    public void Pause()
    {
        if (HasStart)
        {
            IsPlaying = false;
            HasCameraLocked = false;
            foreach (NoteOffset offset in offsetBuffer)
            {
                offset.note.HighlightOff();
                try
                {
                    syn.NoteOff(offset.channel, offset.notePosition);
                }
                catch (FluidSynthInteropException e)
                {
                    Debug.Log(e);
                }
            }
            offsetBuffer.Clear();
        }
    }

    public void Stop()
    {
        if (HasStart)
        {
            IsPlaying = false;
            HasCameraLocked = false;
            playbackPosition = 0;
            playbackBar.transform.localPosition = new Vector3(0f, 0f, 0f);
            foreach (NoteOffset offset in offsetBuffer)
            {
                offset.note.HighlightOff();
                try
                {
                    syn.NoteOff(offset.channel, offset.notePosition);
                }
                catch (FluidSynthInteropException e)
                {
                    Debug.Log(e);
                }
            }
            offsetBuffer.Clear();
        }
    }

    public void ToggleCameraLock()
    {
        HasCameraLocked = !HasCameraLocked;
    }
}

public struct NoteOffset
{
    public int id;
    public int channel;
    public int notePosition;    // pitch
    public long endTiming;
    public Note note;

    public NoteOffset(int id, int channel, int notePosition, long endTiming, Note note)
    {
        this.id = id;
        this.channel = channel;
        this.notePosition = notePosition;
        this.endTiming = endTiming;
        this.note = note;
    }
}