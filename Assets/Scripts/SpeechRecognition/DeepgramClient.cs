using System;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class DeepgramClient : MonoBehaviour
{
    [SerializeField] private DeepgramConfig config;

    public static event Action<string> OnTranscriptReceived;

    private WebSocket _ws;

    private void Start()
    {
        MicrophoneCapture.OnAudioChunk += SendAudioChunk;
        ConnectAsync();
    }

    private async void ConnectAsync()
    {
        string url = $"wss://api.deepgram.com/v1/listen" +
                     $"?encoding=linear16" +
                     $"&sample_rate={config.sampleRate}" +
                     $"&channels=1" +
                     $"&model={config.model}" +
                     $"&interim_results=true" +
                     $"&language={config.language}";

        _ws = new WebSocket(url, new System.Collections.Generic.Dictionary<string, string>
        {
            { "Authorization", $"Token {config.apiKey}" }
        });

        _ws.OnOpen    += () => Debug.Log("[Deepgram] Connected");
        _ws.OnError   += e  => Debug.LogError($"[Deepgram] Error: {e}");
        _ws.OnClose   += e  => Debug.Log($"[Deepgram] Closed: {e}");
        _ws.OnMessage += bytes => OnTranscriptReceived?.Invoke(System.Text.Encoding.UTF8.GetString(bytes));

        await _ws.Connect();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _ws?.DispatchMessageQueue();
#endif
    }

    private async void SendAudioChunk(byte[] chunk)
    {
        if (_ws != null && _ws.State == WebSocketState.Open)
            await _ws.Send(chunk);
    }

    private async void OnDestroy()
    {
        MicrophoneCapture.OnAudioChunk -= SendAudioChunk;
        if (_ws != null && _ws.State == WebSocketState.Open)
        {
            await _ws.SendText("{\"type\":\"CloseStream\"}");
            await _ws.Close();
        }
    }
}
