# Unity Continuous Speech Recognition — Technical Spec
## Project: VoiceText Prototype / Phase 1

---

## Goal

Build a minimal Unity scene that:
1. Captures microphone input continuously
2. Streams audio chunks to the Deepgram API via WebSocket
3. Receives real-time transcription and prints recognized words to the Unity console

This is a "Hello World" for the speech recognition layer of a larger kinetic typography / poetry app. No UI, no visuals — just a working mic → text pipeline.

---

## Success Criteria

- App starts and immediately begins listening (no button press required)
- Spoken words appear in the Unity console within ~1 second of being spoken
- Recognition continues indefinitely without manual restart
- Partial/interim results are acceptable; final results should be clearly labeled in the log

---

## Tech Stack

| Layer | Choice | Reason |
|---|---|---|
| Engine | Unity 6 (6000.x) | Target platform |
| Speech API | Deepgram Streaming WebSocket | Real-time word-level transcription, generous free tier |
| WebSocket library | NativeWebSocket (by Endel) | Lightweight, Unity-native, widely used |
| Audio source | Unity `Microphone` API | Built-in, no extra dependency |
| Audio format | Linear PCM, 16-bit, 16kHz, mono | Deepgram default; directly producible from Unity AudioClip |

---

## Deepgram API Details

**Endpoint:**
```
wss://api.deepgram.com/v1/listen?encoding=linear16&sample_rate=16000&channels=1&model=nova-2&interim_results=true&language=en
```

**Authentication:** HTTP header on WebSocket handshake:
```
Authorization: Token YOUR_DEEPGRAM_API_KEY
```

**Audio input:** Raw binary PCM frames sent as binary WebSocket messages. No container format (no WAV header). Each chunk should be ~100ms of audio = 3200 bytes at 16kHz 16-bit mono.

**Response shape (JSON):**
```json
{
  "type": "Results",
  "channel": {
    "alternatives": [
      {
        "transcript": "hello world",
        "confidence": 0.99,
        "words": [
          { "word": "hello", "start": 0.24, "end": 0.56, "confidence": 0.99 },
          { "word": "world", "start": 0.64, "end": 0.96, "confidence": 0.97 }
        ]
      }
    ]
  },
  "is_final": true
}
```

Key fields:
- `channel.alternatives[0].transcript` — the recognized text
- `is_final` — `true` when the chunk is committed, `false` for interim/live results
- `words[].word` + `words[].start` / `words[].end` — per-word timestamps (useful later for visual sync)

---

## Project Folder Structure

```
Assets/
├── Scripts/
│   └── SpeechRecognition/
│       ├── MicrophoneCapture.cs       # Handles Unity Microphone API, emits audio chunks
│       ├── DeepgramClient.cs          # WebSocket connection, sends audio, receives JSON
│       └── TranscriptLogger.cs        # Parses JSON response, logs to console
├── Scenes/
│   └── SpeechTest.unity               # Minimal test scene (empty — just a GameObject with the scripts)
├── Prefabs/
│   └── SpeechManager.prefab           # GameObject with all three scripts attached
└── Resources/
    └── Config/
        └── DeepgramConfig.asset       # ScriptableObject holding API key + settings (NOT committed to git)
```

**.gitignore additions:**
```
Assets/Resources/Config/DeepgramConfig.asset
Assets/Resources/Config/DeepgramConfig.asset.meta
```

---

## Script Responsibilities

### `MicrophoneCapture.cs`
- Starts `Microphone.Start()` on `Awake` using the default device
- Samples at **16000 Hz**, single channel
- Uses a circular `AudioClip` buffer (e.g. 10 seconds)
- Every ~100ms, reads the new samples since last read, converts float samples to **16-bit PCM bytes**, and fires an `OnAudioChunk` event (`byte[]`)
- Stops microphone on `OnDestroy`

**Float → Int16 conversion:**
```csharp
short[] samples16 = new short[floatSamples.Length];
for (int i = 0; i < floatSamples.Length; i++)
    samples16[i] = (short)(Mathf.Clamp(floatSamples[i], -1f, 1f) * short.MaxValue);
Buffer.BlockCopy(samples16, 0, byteBuffer, 0, samples16.Length * 2);
```

---

### `DeepgramClient.cs`
- On `Start`, opens a WebSocket connection to the Deepgram endpoint (with API key header)
- Subscribes to `MicrophoneCapture.OnAudioChunk`
- On each chunk event, sends the `byte[]` as a **binary** WebSocket message
- On text message received, fires `OnTranscriptReceived` event (`string` raw JSON)
- Handles reconnection on disconnect
- Sends a close message (`{"type":"CloseStream"}`) on `OnDestroy`
- Calls `websocket.DispatchMessageQueue()` in `Update` (required by NativeWebSocket)

---

### `TranscriptLogger.cs`
- Subscribes to `DeepgramClient.OnTranscriptReceived`
- Parses JSON using Unity's `JsonUtility` or `Newtonsoft.Json`
- Logs interim results as: `[INTERIM] hello wor...`
- Logs final results as: `[FINAL] hello world`
- (Optional) Logs word-level timestamps for future sync use

---

### `DeepgramConfig.asset` (ScriptableObject)
```csharp
[CreateAssetMenu]
public class DeepgramConfig : ScriptableObject {
    public string apiKey;
    public string language = "en";
    public string model = "nova-2";
    public int sampleRate = 16000;
}
```

---

## NativeWebSocket Installation

Install via Unity Package Manager using the Git URL:
```
https://github.com/endel/NativeWebSocket.git#upm
```

Package Manager → Add package from Git URL → paste above.

---

## Scene Setup

1. Create an empty scene: `SpeechTest.unity`
2. Create an empty GameObject named `SpeechManager`
3. Attach `MicrophoneCapture`, `DeepgramClient`, `TranscriptLogger` to it
4. Assign `DeepgramConfig` asset to `DeepgramClient`
5. Hit Play — console should begin printing recognized speech

---

## Notes for Claude Code

- Do not hard-code the API key anywhere. Always read from `DeepgramConfig` ScriptableObject
- `Microphone` API requires microphone permission on macOS: ensure `Info.plist` has `NSMicrophoneUsageDescription` (Unity handles this for standalone builds but worth verifying)
- NativeWebSocket's `DispatchMessageQueue()` **must** be called in `Update()` or messages won't fire on the main thread
- Use `System.Buffer.BlockCopy` for float→byte conversion, not a loop, for performance
- JSON parsing: prefer `Newtonsoft.Json` (available via Package Manager as `com.unity.nuget.newtonsoft-json`) over `JsonUtility` since the Deepgram response has nested arrays that `JsonUtility` handles poorly
- Target platform for this phase: **macOS standalone** (Editor testing is fine)

---

## Out of Scope for Phase 1

- Any UI or visual output
- Word-level animation or timing
- Video export
- Mobile builds
- Error/retry UI
- Speaker diarization
