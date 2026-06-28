using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class WordObject : MonoBehaviour
{
    [Header("Entrance")]
    [SerializeField] private float fadeInDuration  = 0.2f;
    [SerializeField] private float scaleFromSize   = 0.4f;  // start scale multiplier

    [Header("Life")]
    [SerializeField] private float holdDuration    = 2.5f;
    [SerializeField] private float driftSpeed      = 0.08f; // units/sec upward drift

    [Header("Exit")]
    [SerializeField] private float fadeOutDuration = 1.2f;

    [Header("Appearance")]
    [SerializeField] private float fontSize = 4f;
    [SerializeField] private Color wordColor = Color.white;

    private TextMeshPro _tmp;
    private Vector3 _targetScale;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
        _tmp.fontSize = fontSize;
        _tmp.color = new Color(wordColor.r, wordColor.g, wordColor.b, 0f);
        _tmp.alignment = TextAlignmentOptions.Center;
        _targetScale = transform.localScale;
    }

    public void SetWord(string text)
    {
        _tmp.text = text;
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        // --- Entrance: scale up + fade in ---
        float t = 0f;
        Vector3 startScale = _targetScale * scaleFromSize;
        transform.localScale = startScale;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeInDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f); // ease-out cubic

            transform.localScale = Vector3.LerpUnclamped(startScale, _targetScale, eased);
            SetAlpha(eased);
            yield return null;
        }

        SetAlpha(1f);
        transform.localScale = _targetScale;

        // --- Hold + drift ---
        t = 0f;
        while (t < holdDuration)
        {
            t += Time.deltaTime;
            transform.position += Vector3.up * (driftSpeed * Time.deltaTime);
            yield return null;
        }

        // --- Exit: fade out while drifting ---
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeOutDuration);
            SetAlpha(1f - p);
            transform.position += Vector3.up * (driftSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void SetAlpha(float a)
    {
        Color c = _tmp.color;
        c.a = a;
        _tmp.color = c;
    }
}
