# VoiceText — Project Design Document
**Version 1.0 — June 2026**
**Author: Axel**
**Status: Phase 1 in development**

---

## 1. Vision

VoiceText is an app that lets a person read a text aloud — a message, a poem, a thought — and captures both their voice and the words they speak. The result is a short video where the written text appears on screen in sync with the voice that reads it, creating an experience where the text seems to read itself.

The effect is intimate and poetic. Hearing a voice while seeing the words simultaneously collapses the distance between the written and the spoken. It replaces the phantom inner voice we hear when we read silently with the actual voice of the person who wrote the message. Text becomes time-based. Language becomes a living thing.

The app is designed first as a creative tool for writers and poets, and may evolve into a broader communication tool for general audiences.

---

## 2. Core Experience

**The Sender:**
Opens the app, speaks a text they have prepared (or improvises). The app listens continuously, recognizes the words in real time, and displays them on screen with kinetic typography — the words appear, transform, move, pulse in aesthetically interesting ways. At the end, the app exports a short video combining the recorded audio with the animated text.

**The Receiver:**
Watches the video. They see the words appearing on screen while hearing the sender's voice reading them aloud at the same time. They are not reading and listening separately — they are doing both at once, in the same moment the sender intended.

**The poetic effect:**
- Text and voice arrive together, like a single signal
- The written word is no longer static — it exists in time, like music
- The sender's voice replaces the silent inner voice the reader would otherwise supply
- It creates a kind of presence: the sender is there, in the room, speaking

---

## 3. Project Phases

### Phase 1 — Speech Recognition Prototype (CURRENT)
**Goal:** Prove the mic → API → text pipeline works reliably in Unity.
**Output:** Words spoken into the mic appear in the Unity console in real time.
**Platform:** Unity Editor / macOS standalone
**No visuals, no UI, no video export — just the pipeline.**

### Phase 2 — Kinetic Typography
**Goal:** Replace the console with a real-time visual display of recognized words.
**Output:** Spoken words appear on screen with motion, typography, and shader effects.
**Experiments:** Font choices, color, word entrance animations, particle effects, audio reactivity, physics-based text behavior, post-processing.
**Platform:** Unity standalone (Mac/Windows)

### Phase 3 — Record & Export
**Goal:** Capture the visual output as a video file.
**Output:** A short shareable video (MP4) with audio + animated text.
**Tools:** Unity Recorder package.
**Platform:** Unity standalone (Mac/Windows)

### Phase 4 — Art Project Deployment
**Goal:** Package the app for distribution to a closed group of writers and poets.
**Output:** A polished standalone app (Mac/Windows builds).
**Use case:** An art project — a compilation of voice messages, poems, personal texts created by invited participants.

### Phase 5 — Public App (Future)
**Goal:** Rebuild as a native mobile app for general audiences.
**Platform:** iOS and/or Android (native or Unity mobile build)
**Out of scope for now.**

---

## 4. Technical Architecture

### Pipeline Overview
```
Microphone (Unity) 
    → Audio chunks (PCM 16-bit, 16kHz, mono)
        → Deepgram WebSocket API
            → JSON transcript with word timestamps
                → [Phase 1] Console log
                → [Phase 2] Kinetic typography renderer
                → [Phase 3] Unity Recorder captures to MP4
```

### Key Technical Decisions

**Speech Recognition: Deepgram Streaming API**
- WebSocket-based, real-time streaming (not batch)
- Returns word-level timestamps — essential for later visual sync
- Free tier is generous enough for prototyping
- `nova-2` model, English language (multilingual later)
- Interim results enabled (live feedback while speaking)

**Engine: Unity 6 (6000.x)**
- Cross-platform (Mac, Windows, potential mobile later)
- Rich visual capabilities: shaders, particles, physics, post-processing, audio reactivity
- Unity Recorder for video export
- Familiar environment for the developer

**WebSocket Library: NativeWebSocket (by Endel)**
- Lightweight Unity-compatible WebSocket implementation
- Installed via Package Manager (Git URL)
- Requires `DispatchMessageQueue()` call in `Update()`

**JSON Parsing: Newtonsoft.Json**
- Available via Unity Package Manager (`com.unity.nuget.newtonsoft-json`)
- Handles nested arrays better than Unity's built-in `JsonUtility`

**API Key Management: ScriptableObject**
- `DeepgramConfig.asset` holds API key and settings
- Never committed to git (in `.gitignore`)
- Easy to swap API keys or switch models without touching code

---

## 5. Unity Project Structure

```
Assets/
├── Scripts/
│   ├── SpeechRecognition/
│   │   ├── MicrophoneCapture.cs       # Mic input, audio chunking, PCM conversion
│   │   ├── DeepgramClient.cs          # WebSocket connection, send audio, receive JSON
│   │   └── TranscriptLogger.cs        # Parse JSON, output to console (Phase 1)
│   └── Typography/                    # Phase 2 — kinetic text rendering (future)
├── Scenes/
│   ├── SpeechTest.unity               # Phase 1 test scene
│   └── TypographyTest.unity           # Phase 2 visual experiments (future)
├── Prefabs/
│   └── SpeechManager.prefab           # GameObject with all speech scripts attached
├── Materials/                         # Shaders and materials for text (Phase 2)
├── Fonts/                             # Custom fonts for typography experiments
└── Resources/
    └── Config/
        └── DeepgramConfig.asset       # API key — NOT in git
```

**.gitignore additions:**
```
Assets/Resources/Config/DeepgramConfig.asset
Assets/Resources/Config/DeepgramConfig.asset.meta
```

---

## 6. Scripts — Responsibilities

### `MicrophoneCapture.cs`
- Starts `Microphone.Start()` on `Awake` (default device, 16000Hz, 10s circular buffer)
- Polls every ~100ms in `Update()` for new samples since last read
- Converts float samples to 16-bit PCM bytes (`Buffer.BlockCopy`)
- Fires `OnAudioChunk` event (`byte[]`) for each chunk
- Stops microphone on `OnDestroy`

### `DeepgramClient.cs`
- Reads config from `DeepgramConfig` ScriptableObject
- Opens WebSocket to Deepgram on `Start` with auth header
- Subscribes to `MicrophoneCapture.OnAudioChunk`, sends binary frames
- Fires `OnTranscriptReceived` event (`string` raw JSON) on incoming messages
- Calls `websocket.DispatchMessageQueue()` in `Update()`
- Sends `{"type":"CloseStream"}` and closes on `OnDestroy`

### `TranscriptLogger.cs` (Phase 1 only)
- Subscribes to `DeepgramClient.OnTranscriptReceived`
- Parses JSON, extracts transcript and `is_final` flag
- Logs: `[INTERIM] hello wor...` and `[FINAL] hello world`
- Optionally logs per-word timestamps

### `DeepgramConfig.cs` (ScriptableObject)
```csharp
[CreateAssetMenu(menuName = "VoiceText/DeepgramConfig")]
public class DeepgramConfig : ScriptableObject {
    public string apiKey;
    public string language = "en";
    public string model = "nova-2";
    public int sampleRate = 16000;
}
```

---

## 7. Deepgram API Reference

**WebSocket Endpoint:**
```
wss://api.deepgram.com/v1/listen?encoding=linear16&sample_rate=16000&channels=1&model=nova-2&interim_results=true&language=en
```

**Auth Header:**
```
Authorization: Token YOUR_API_KEY
```

**Audio Format:**
- Raw PCM binary (no WAV header)
- 16-bit signed integer, little-endian
- 16000 Hz sample rate
- 1 channel (mono)
- ~100ms chunks = ~3200 bytes

**Response JSON (key fields):**
```json
{
  "channel": {
    "alternatives": [{
      "transcript": "hello world",
      "words": [
        { "word": "hello", "start": 0.24, "end": 0.56 },
        { "word": "world", "start": 0.64, "end": 0.96 }
      ]
    }]
  },
  "is_final": true
}
```

---

## 8. Package Dependencies

| Package | Source | Purpose |
|---|---|---|
| NativeWebSocket | Git: `https://github.com/endel/NativeWebSocket.git#upm` | WebSocket streaming |
| Newtonsoft.Json | Package Manager: `com.unity.nuget.newtonsoft-json` | JSON parsing |
| Unity Recorder | Package Manager (Unity registry) | Video export (Phase 3) |

---

## 9. Visual Direction (Phase 2 — for reference)

The visual aesthetic is intentionally open for experimentation. Some directions to explore:

- **Kinetic entrance:** words fade in, scale up, drop in with physics
- **Audio reactivity:** font size or opacity pulses with voice amplitude
- **Layering:** words accumulate on screen, building a visual texture of language
- **Typographic contrast:** mixing weights, sizes, colors across the same text
- **Shader effects:** dissolve, glow, distortion, chromatic aberration on text
- **Particles:** words emit particles, disintegrate, reform
- **Minimalism:** clean white text on black, the voice does all the work

The video output format is short (typically under 2 minutes). Aspect ratio TBD — could be vertical (9:16) for mobile sharing or landscape (16:9).

---

## 10. Out of Scope (for now)

- UI / HUD of any kind in Phase 1
- Error/retry UI
- Multiple languages (English only for now)
- Speaker diarization
- Editing or correcting the transcript
- Timing adjustments post-recording
- Web / WebGL build
- Mobile build
- Public distribution / app store

---

## 11. Reference Projects & Inspirations

- **P5.js voice sketch (Axel, ~2021):** Web Speech API → chunked text → printed to canvas. The spiritual predecessor to this project.
- **Unity Sentis speech recognition demo:** Voice recognition in Unity with manual start/stop — the limitation this project is designed to overcome.
- **Kinetic typography:** The broad tradition of text in motion in film titles, music videos, and motion graphics.

---

## 12. Notes for Claude Code

- Always read from `DeepgramConfig` ScriptableObject — never hardcode API key
- `DispatchMessageQueue()` is mandatory in `Update()` for NativeWebSocket
- Use `Newtonsoft.Json` not `JsonUtility` for response parsing
- macOS mic permission: verify `NSMicrophoneUsageDescription` in Player Settings
- Phase 1 success = words in console. Do not add visual output until Phase 1 is confirmed working.
- Keep scripts decoupled via C# events — `MicrophoneCapture` should have no knowledge of Deepgram
- Target: Unity 6 (6000.x), macOS standalone / Editor testing
