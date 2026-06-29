using Newtonsoft.Json;
using UnityEngine;

public class WordSpawner : MonoBehaviour
{
    [SerializeField] private WordObject wordPrefab;
    [SerializeField] private float spawnDepth       = 5f;
    [SerializeField] private float marginViewport   = 0.1f;
    [SerializeField] private float minSpawnDistance = 1.5f; // world units, min gap between consecutive words
    [SerializeField] private int   maxRetries       = 10;

    private Vector3 _lastSpawnPos = Vector3.positiveInfinity;

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

        Vector3 worldPos = PickPosition();
        _lastSpawnPos = worldPos;

        WordObject instance = Instantiate(wordPrefab, worldPos, Quaternion.identity);
        instance.SetWord(text);
    }

    private Vector3 PickPosition()
    {
        float margin = marginViewport;
        Vector3 candidate = Vector3.zero;

        for (int i = 0; i < maxRetries; i++)
        {
            float vx = Random.Range(margin, 1f - margin);
            float vy = Random.Range(margin, 1f - margin);
            candidate = Camera.main.ViewportToWorldPoint(new Vector3(vx, vy, spawnDepth));

            if (float.IsPositiveInfinity(_lastSpawnPos.x) ||
                Vector3.Distance(candidate, _lastSpawnPos) >= minSpawnDistance)
                return candidate;
        }

        return candidate; // best effort after max retries
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
