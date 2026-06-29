using System.Collections;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class SimpleReplaceDisplay : MonoBehaviour
{
    [SerializeField] private float spawnDepth = 5f;
    [SerializeField] private float fontSize   = 8f;
    [SerializeField] private Color textColor  = Color.white;

    private TextMeshPro _tmp;
    private Coroutine   _sequenceRoutine;

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
    private void OnDisable()
    {
        DeepgramClient.OnTranscriptReceived -= HandleTranscript;
        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
    }

    private void HandleTranscript(string json)
    {
        DeepgramResponse response;
        try { response = JsonConvert.DeserializeObject<DeepgramResponse>(json); }
        catch { return; }

        if (response == null || !response.is_final) return;
        if (response.channel?.alternatives == null || response.channel.alternatives.Length == 0) return;

        Word[] words = response.channel.alternatives[0].words;
        if (words == null || words.Length == 0) return;

        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = StartCoroutine(ShowWordsSequenced(words));
    }

    private IEnumerator ShowWordsSequenced(Word[] words)
    {
        float baseTime = words[0].start;

        for (int i = 0; i < words.Length; i++)
        {
            float delay = words[i].start - baseTime;
            if (i > 0) yield return new WaitForSeconds(delay - (words[i - 1].start - baseTime));

            _tmp.text = words[i].word;
        }
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
        public Word[] words;
    }

    private class Word
    {
        public string word;
        public float  start;
        public float  end;
    }
}
