using UnityEngine;

public class DisplayModeManager : MonoBehaviour
{
    [SerializeField] private SimpleReplaceDisplay simpleDisplay;
    [SerializeField] private WordSpawner          wordSpawner;
    [SerializeField] private StaticDisplay        streamingDisplay;
    [SerializeField] private DeepgramClient       deepgramClient;

    private void Start()
    {
        SetMode(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetMode(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetMode(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetMode(3);

        if (Input.GetKeyDown(KeyCode.E)) deepgramClient.SetLanguage("en");
        if (Input.GetKeyDown(KeyCode.F)) deepgramClient.SetLanguage("fr");
        if (Input.GetKeyDown(KeyCode.P)) deepgramClient.SetLanguage("pt");
    }

    private void SetMode(int mode)
    {
        simpleDisplay.enabled    = (mode == 1);
        wordSpawner.enabled      = (mode == 2);
        streamingDisplay.enabled = (mode == 3);
        Debug.Log($"[DisplayMode] Mode {mode}");
    }
}
