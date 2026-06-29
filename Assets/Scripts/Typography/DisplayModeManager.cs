using UnityEngine;

public class DisplayModeManager : MonoBehaviour
{
    [SerializeField] private StaticDisplay staticDisplay;
    [SerializeField] private WordSpawner   wordSpawner;

    private void Start()
    {
        SetMode(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetMode(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetMode(2);
    }

    private void SetMode(int mode)
    {
        staticDisplay.enabled = (mode == 1);
        wordSpawner.enabled   = (mode == 2);
        Debug.Log($"[DisplayMode] Switched to mode {mode} ({(mode == 1 ? "Static" : "Kinetic")})");
    }
}
