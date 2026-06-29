using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class SimpleReplaceDisplay : MonoBehaviour
{
    [SerializeField] private float spawnDepth = 5f;
    [SerializeField] private float fontSize   = 8f;
    [SerializeField] private Color textColor  = Color.white;

    private TextMeshPro _tmp;

    private void Awake()
    {
        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, spawnDepth));
        var go = new GameObject("SimpleText");
        go.transform.position = center;
        go.transform.SetParent(transform);

        _tmp = go.AddComponent<TextMeshPro>();
        _tmp.fontSize  = fontSize;
        _tmp.color     = textColor;
        _tmp.alignment = TextAlignmentOptions.Center;
        _tmp.text      = "";
    }

    private void OnEnable()  => DeepgramClient.OnTranscriptReceived += HandleTranscript;
    private void OnDisable() => DeepgramClient.OnTranscriptReceived -= HandleTranscript;

    private void HandleTranscript(string json)
    {
        DeepgramResponse response;
        try { response = JsonConvert.DeserializeObject<DeepgramResponse>(json); }
        catch { return; }

        if (response == null || !response.is_final) return;
        if (response.channel?.alternatives == null || response.channel.alternatives.Length == 0) return;

        string transcript = response.channel.alternatives[0].transcript;
        if (!string.IsNullOrWhiteSpace(transcript))
            _tmp.text = transcript;
    }

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
    }
}
