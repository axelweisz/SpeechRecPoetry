using Newtonsoft.Json;
using UnityEngine;

public class WordSpawner : MonoBehaviour
{
    [SerializeField] private WordObject wordPrefab;
    [SerializeField] private float spawnDepth = 5f;      // distance from camera in world units
    [SerializeField] private float marginViewport = 0.1f; // keep words away from edges (0–0.5)

    private void OnEnable()  => DeepgramClient.OnTranscriptReceived += HandleTranscript;
    private void OnDisable() => DeepgramClient.OnTranscriptReceived -= HandleTranscript;

    private void HandleTranscript(string json)
    {
        DeepgramResponse response;
        try { response = JsonConvert.DeserializeObject<DeepgramResponse>(json); }
        catch { return; }

        if (response == null || !response.is_final) return;
        if (response.channel?.alternatives == null || response.channel.alternatives.Length == 0) return;

        Word[] words = response.channel.alternatives[0].words;
        if (words == null) return;

        foreach (var w in words)
            SpawnWord(w.word);
    }

    private void SpawnWord(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        float margin = marginViewport;
        float vx = Random.Range(margin, 1f - margin);
        float vy = Random.Range(margin, 1f - margin);

        Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(vx, vy, spawnDepth));

        WordObject instance = Instantiate(wordPrefab, worldPos, Quaternion.identity);
        instance.SetWord(text);
    }

    // --- JSON model (mirrors TranscriptLogger) ---

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
        public Word[] words;
    }

    private class Word
    {
        public string word;
        public float start;
        public float end;
    }
}
