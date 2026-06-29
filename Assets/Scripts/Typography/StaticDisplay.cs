using System.Collections;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class StaticDisplay : MonoBehaviour
{
    [SerializeField] private float spawnDepth   = 5f;
    [SerializeField] private float fontSize      = 6f;
    [SerializeField] private Color interimColor  = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color finalColor    = Color.white;
    [SerializeField] private float holdDuration  = 1.2f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    private TextMeshPro _tmp;
    private Coroutine _clearRoutine;

    private void Awake()
    {
        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, spawnDepth));
        var go = new GameObject("StaticText");
        go.transform.position = center;
        go.transform.SetParent(transform);

        _tmp = go.AddComponent<TextMeshPro>();
        _tmp.fontSize = fontSize;
        _tmp.alignment = TextAlignmentOptions.Center;
        _tmp.color = new Color(0, 0, 0, 0);
    }

    private void OnEnable()  => DeepgramClient.OnTranscriptReceived += HandleTranscript;
    private void OnDisable() => DeepgramClient.OnTranscriptReceived -= HandleTranscript;

    private void HandleTranscript(string json)
    {
        DeepgramResponse response;
        try { response = JsonConvert.DeserializeObject<DeepgramResponse>(json); }
        catch { return; }

        if (response?.channel?.alternatives == null || response.channel.alternatives.Length == 0) return;

        string transcript = response.channel.alternatives[0].transcript;
        if (string.IsNullOrWhiteSpace(transcript)) return;

        if (_clearRoutine != null)
        {
            StopCoroutine(_clearRoutine);
            _clearRoutine = null;
        }

        _tmp.text = transcript;

        if (response.is_final)
        {
            _tmp.color = finalColor;
            _clearRoutine = StartCoroutine(ClearAfterHold());
        }
        else
        {
            _tmp.color = interimColor;
        }
    }

    private IEnumerator ClearAfterHold()
    {
        yield return new WaitForSeconds(holdDuration);

        float t = 0f;
        Color start = _tmp.color;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(start.a, 0f, t / fadeOutDuration);
            _tmp.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }

        _tmp.text = "";
        _tmp.color = new Color(0, 0, 0, 0);
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
    }
}
