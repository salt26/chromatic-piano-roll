from fastapi import FastAPI, UploadFile
from midi_extractor import *
from mido import MidiFile
import json

app = FastAPI()

@app.get("/")
async def root():
    return {"message": "Hello World"}

@app.post("/upload/")
async def upload_midi(file: UploadFile):
    if file.content_type not in ["audio/mid", "audio/midi", "audio/x-midi"]:
        return {"error": "The file is not a midi file."}
    midi = MidiFile(file=file.file)
    events, notes, ticks_per_beat = parse_events_helper(midi)

    return notes