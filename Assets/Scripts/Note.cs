using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

public class Note : MonoBehaviour
{
    public enum Pitch { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B }

    public int id = -1;
    public int startTick;
    public int endTick;
    public long startTiming;
    public long endTiming;
    public int channel;
    public int notePosition;    // pitch
    public int noteVelocity;    // dynamics
    public Pitch notePitchClass;
    public int noteOctave;
    public int startSeqIndex;
    public int endSeqIndex;
    public int startNoteIndex;
    public int endNoteIndex;
    public int noteDurationUnits;

    public void Initialize(JObject note)
    {
        id = (int)note["ID"];
        startTick = (int)note["Start_tick"];
        endTick = (int)note["End_tick"];
        startTiming = (long)note["Start_timing"];
        endTiming = (long)note["End_timing"];
        channel = (int)note["Channel"];
        notePosition = (int)note["Note_position"];
        noteVelocity = (int)note["Note_velocity"];
        notePitchClass = StringToPitch((string)note["Note_pitch_class"]);
        noteOctave = (int)note["Note_octave"];
        startSeqIndex = (int)note["Start_seq_index"];
        startNoteIndex = (int)note["Start_note_index"];
        endSeqIndex = (int)note["End_seq_index"];
        endNoteIndex = (int)note["End_note_index"];
        noteDurationUnits = (int)note["Note_duration_units"];

        transform.localPosition = new Vector3((startTiming + endTiming) / 2f / PianoRoll.pr.XScale, -5.08f + 0.08f * notePosition, 0f);
        transform.localScale = new Vector3(1f, 0.4f, 1f);
        GetComponent<SpriteRenderer>().color = ColorPalette.ChangeAlpha(PianoRoll.pr.GetComponent<ColorPalette>().colorPalettes[PianoRoll.pr.colorPaletteIndex][notePitchClass],
            1f/*noteVelocity / 127f*/);
        GetComponent<SpriteRenderer>().size = new Vector3((endTiming - startTiming) / PianoRoll.pr.XScale, 0.2f, 1f);
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    public void UpdateNote()
    {
        if (id >= 0)
        {
            transform.localPosition = new Vector3((startTiming + endTiming) / 2f / PianoRoll.pr.XScale, -5.08f + 0.08f * notePosition, 0f);
            transform.localScale = new Vector3(1f, 0.4f, 1f);
            GetComponent<SpriteRenderer>().color = ColorPalette.ChangeAlpha(PianoRoll.pr.GetComponent<ColorPalette>().colorPalettes[PianoRoll.pr.colorPaletteIndex][notePitchClass],
                1f/*noteVelocity / 127f*/);
            GetComponent<SpriteRenderer>().size = new Vector3((endTiming - startTiming) / PianoRoll.pr.XScale, 0.2f, 1f);
        }
    }

    public void PlayNote()
    {

    }

    public static Pitch StringToPitch(string pitchClass)
    {
        return (Pitch)Enum.Parse(typeof(Pitch), SharpToFlat(pitchClass));
    }

    public static string SharpToFlat(string pitchClass)
    {
        switch (pitchClass)
        {
            case "A#": return "Bb";
            case "C#": return "Db";
            case "D#": return "Eb";
            case "F#": return "Gb";
            case "G#": return "Ab";
            default: return pitchClass;
        }
    }
}
