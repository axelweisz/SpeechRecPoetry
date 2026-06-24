using Newtonsoft.Json;
using UnityEngine;

public class TranscriptLogger : MonoBehaviour
{
    private void OnEnable()  => DeepgramClient.OnTranscriptReceived += HandleTranscript;
    private void OnDisable() => DeepgramClient.OnTranscriptReceived -= HandleTranscript;

    private void HandleTranscript(string json)
    {
        DeepgramResponse response;
        try { response = JsonConvert.DeserializeObject<DeepgramResponse>(json); }
        catch { Debug.LogWarning($"[Transcript] Could not parse JSON: {json}"); return; }

        if (response?.channel?.alternatives == null || response.channel.alternatives.Length == 0) return;

        string transcript = response.channel.alternatives[0].transcript;
        if (string.IsNullOrWhiteSpace(transcript)) return;

        if (response.is_final)
            Debug.Log($"[FINAL] {transcript}");
        else
            Debug.Log($"[INTERIM] {transcript}");
    }

    // --- JSON model ---

    private class DeepgramResponse
    {
        public Channel channel;
        public bool is_final;
    }

    private class Channel
    {
        public Alternative[] alternatives;
    }

    private class Alternative
    {
        public string transcript;
        public float confidence;
        public Word[] words;
    }

    private class Word
    {
        public string word;
        public float start;
        public float end;
        public float confidence;
    }
}
