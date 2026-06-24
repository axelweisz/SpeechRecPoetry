using System;
using UnityEngine;

public class MicrophoneCapture : MonoBehaviour
{
    public static event Action<byte[]> OnAudioChunk;

    private const int SampleRate = 16000;
    private const int BufferLengthSeconds = 10;
    private const float ChunkIntervalSeconds = 0.1f;

    private AudioClip _clip;
    private int _lastSamplePos;
    private float _timer;

    private void Awake()
    {
        _clip = Microphone.Start(null, true, BufferLengthSeconds, SampleRate);
        while (Microphone.GetPosition(null) <= 0) { }  // wait for mic to warm up
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        if (_timer < ChunkIntervalSeconds) return;
        _timer = 0f;

        int currentPos = Microphone.GetPosition(null);
        if (currentPos == _lastSamplePos) return;

        int sampleCount = currentPos > _lastSamplePos
            ? currentPos - _lastSamplePos
            : (_clip.samples - _lastSamplePos) + currentPos;

        float[] floatSamples = new float[sampleCount];
        _clip.GetData(floatSamples, _lastSamplePos % _clip.samples);
        _lastSamplePos = currentPos;

        byte[] pcmBytes = ConvertToPcm16(floatSamples);
        OnAudioChunk?.Invoke(pcmBytes);
    }

    private void OnDestroy()
    {
        Microphone.End(null);
    }

    private static byte[] ConvertToPcm16(float[] floatSamples)
    {
        short[] samples16 = new short[floatSamples.Length];
        for (int i = 0; i < floatSamples.Length; i++)
            samples16[i] = (short)(Mathf.Clamp(floatSamples[i], -1f, 1f) * short.MaxValue);

        byte[] bytes = new byte[samples16.Length * 2];
        Buffer.BlockCopy(samples16, 0, bytes, 0, bytes.Length);
        return bytes;
    }
}
