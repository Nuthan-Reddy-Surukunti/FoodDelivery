import os
import io
import wave
import uuid
import emoji
import sherpa_onnx
import numpy as np
import subprocess
from fastapi import FastAPI, UploadFile, File, Response, Form
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI(title="QuickBite Audio Engine")

# Allow requests from the React frontend
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Load Sherpa-ONNX ASR (Listening) - Replaces Whisper
print("Loading Sherpa-ONNX ASR model (Zipformer Small)...")
recognizer = sherpa_onnx.OfflineRecognizer.from_transducer(
    encoder="sherpa-onnx-zipformer-small-en-2023-06-26/encoder-epoch-99-avg-1.int8.onnx",
    decoder="sherpa-onnx-zipformer-small-en-2023-06-26/decoder-epoch-99-avg-1.int8.onnx",
    joiner="sherpa-onnx-zipformer-small-en-2023-06-26/joiner-epoch-99-avg-1.int8.onnx",
    tokens="sherpa-onnx-zipformer-small-en-2023-06-26/tokens.txt",
    num_threads=1,
)
print("Sherpa-ONNX ASR loaded!")

# Load Sherpa-ONNX TTS (Speaking)
print("Loading Sherpa-ONNX TTS model (Kristin Medium)...")
tts_config = sherpa_onnx.OfflineTtsConfig(
    model=sherpa_onnx.OfflineTtsModelConfig(
        vits=sherpa_onnx.OfflineTtsVitsModelConfig(
            model="vits-piper-en_US-kristin-medium/en_US-kristin-medium.onnx",
            tokens="vits-piper-en_US-kristin-medium/tokens.txt",
            data_dir="vits-piper-en_US-kristin-medium/espeak-ng-data",
        ),
        num_threads=1,
    ),
)
tts = sherpa_onnx.OfflineTts(tts_config)
print("Sherpa-ONNX TTS loaded!")

@app.on_event("startup")
def startup_event():
    print("Local Audio Engine is ready. Unified Sherpa-ONNX for both Listening and Speaking.")

@app.post("/transcribe")
async def transcribe_audio(file: UploadFile = File(...)):
    """Converts uploaded audio file to text using Sherpa-ONNX ASR."""
    audio_bytes = await file.read()
    temp_filename = f"temp_{file.filename}"
    temp_wav = f"temp_{uuid.uuid4().hex}.wav"
    
    with open(temp_filename, "wb") as f:
        f.write(audio_bytes)
        
    try:
        # Convert to wav using macOS afconvert (Built-in tool)
        # -f WAVE: format is WAVE
        # -d LEI16@16000: Linear PCM, Little-Endian, Signed 16-bit, 16kHz (Standard for ASR)
        subprocess.run(["afconvert", "-f", "WAVE", "-d", "LEI16@16000", temp_filename, temp_wav], check=True)
        
        # Read wav using wave + numpy
        with wave.open(temp_wav, "rb") as f:
            num_frames = f.getnframes()
            data = f.readframes(num_frames)
            samples = np.frombuffer(data, dtype=np.int16).astype(np.float32) / 32768.0
            
        # Transcribe
        stream = recognizer.create_stream()
        stream.accept_waveform(16000, samples)
        recognizer.decode_stream(stream)
        transcript = stream.result.text
        
        print(f"Transcription result: {transcript.strip()}")
        return {"text": transcript.strip()}
    except Exception as e:
        print(f"Transcription error: {e}")
        return {"text": "", "error": str(e)}
    finally:
        # Cleanup temp files
        if os.path.exists(temp_filename): os.remove(temp_filename)
        if os.path.exists(temp_wav): os.remove(temp_wav)

@app.post("/speak")
async def speak_text(text: str = Form(...)):
    """Converts text to speech using Sherpa-ONNX and returns a WAV audio stream."""
    print(f"Generating audio for text: {text}")
    
    # Remove markdown and emojis
    clean_text = text.replace('*', '').replace('#', '')
    clean_text = emoji.replace_emoji(clean_text, replace='')
    
    # Generate audio samples
    audio = tts.generate(clean_text)
    
    # Save samples to a temporary WAV file
    temp_output = f"output_{uuid.uuid4().hex}.wav"
    with wave.open(temp_output, "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(audio.sample_rate)
        # Convert float32 [-1, 1] to int16 for WAV format
        samples = (np.array(audio.samples) * 32767).astype(np.int16)
        f.writeframes(samples.tobytes())
        
    # Read the generated wav
    with open(temp_output, "rb") as f:
        audio_data = f.read()
        
    os.remove(temp_output)
    
    return Response(content=audio_data, media_type="audio/wav")

if __name__ == "__main__":
    import uvicorn
    # Run on port 8000
    uvicorn.run(app, host="0.0.0.0", port=8000)
