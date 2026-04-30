# QuickBite Local Audio Engine

This is a lightweight Python server that provides local Speech-to-Text (Whisper) and Text-to-Speech (Piper) endpoints for the QuickBite AI Assistant.

It runs natively on your Mac to utilize your CPU/Metal hardware efficiently, using only ~250MB of RAM.

## Setup Instructions

1. Open a new terminal and navigate to this directory:
   ```bash
   cd "Documents/Projects worked on/Capgemini Projects/FoodDelivery-Final/LocalAudioEngine"
   ```

2. Create a virtual environment (recommended):
   ```bash
   python3 -m venv venv
   source venv/bin/activate
   ```

3. Install the dependencies:
   ```bash
   pip install -r requirements.txt
   ```

4. Run the server:
   ```bash
   python audio_server.py
   ```

*Note: The first time you run it, it will download the Whisper Base model (~150MB) and the Piper voice model (~50MB). Subsequent runs will start instantly.*

## Endpoints
- **POST /transcribe:** Accepts an audio file upload and returns text.
- **POST /speak:** Accepts text (form-data) and returns a `.wav` audio stream.
