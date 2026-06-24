using UnityEngine;

[CreateAssetMenu(menuName = "VoiceText/DeepgramConfig", fileName = "DeepgramConfig")]
public class DeepgramConfig : ScriptableObject
{
    public string apiKey;
    public string language = "en";
    public string model = "nova-2";
    public int sampleRate = 16000;
}
