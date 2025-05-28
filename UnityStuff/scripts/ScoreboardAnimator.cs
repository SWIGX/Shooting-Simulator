using UnityEngine;
using TMPro;

public class ScoreboardAnimator : MonoBehaviour
{
    public float pulseScale = 1.2f;
    public float pulseDuration = 0.2f;
    public Color glowColor = Color.yellow;
    public float glowDuration = 0.3f;

    private Vector3 originalScale;
    private TextMeshProUGUI[] textFields;

    void Start()
    {
        originalScale = transform.localScale;
        textFields = GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void PlayPulse()
    {
        StopAllCoroutines();
        StartCoroutine(PulseRoutine());
    }

    System.Collections.IEnumerator PulseRoutine()
    {
        float timer = 0f;
        while (timer < pulseDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(1f, pulseScale, timer / pulseDuration);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        timer = 0f;
        while (timer < pulseDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(pulseScale, 1f, timer / pulseDuration);
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }

    public void PlayGlow()
    {
        StopCoroutine("GlowRoutine");
        StartCoroutine(GlowRoutine());
    }

    System.Collections.IEnumerator GlowRoutine()
    {
        foreach (var txt in textFields)
            txt.outlineColor = glowColor;

        yield return new WaitForSeconds(glowDuration);

        foreach (var txt in textFields)
            txt.outlineColor = Color.black; 
    }
}
