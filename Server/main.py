from fastapi import FastAPI, UploadFile, Request
from midi_extractor import *
from mido import MidiFile
import json

app = FastAPI()

@app.get("/")
async def root():
    return {"message": "Hello World"}

@app.post("/upload/")
async def upload_midi(request: Request, file: UploadFile):
    print(request)
    print(file.headers)
    print(file.content_type)
    if file.content_type not in ["audio/mid", "audio/midi", "audio/x-midi"]:
        return {"error": "The file is not a midi file."}
    midi = MidiFile(file=file.file)
    events, notes, ticks_per_beat = parse_events_helper(midi)

    return notes