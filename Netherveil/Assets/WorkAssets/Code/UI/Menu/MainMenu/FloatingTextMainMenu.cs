using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class FloatingTextMainMenu : MonoBehaviour
{
    [SerializeField] private float fadeTime = 0.2f;
    private TMP_Text m_Text;
    private Coroutine routine;

    private void Start()
    {
        m_Text = GetComponent<TMP_Text>();
    }

    public void FadeFloatingTexts(bool toggle)
    {
        if (routine != null)
            StopCoroutine(routine);

        if (toggle)
            routine = StartCoroutine(FadeFloatingText(0.0f, 1.0f));
        else
            routine = StartCoroutine(FadeFloatingText(1.0f, 0.0f));
    }

    private IEnumerator FadeFloatingText(float from, float to)
    {
        float elapsed = 0;

        m_Text.alpha = from;
        while (elapsed < fadeTime)
        {
            yield return null;
            elapsed = Mathf.Min(elapsed + Time.deltaTime, fadeTime);
            m_Text.alpha = Mathf.Lerp(from, to, elapsed / fadeTime);
        }
        m_Text.alpha = to;
    }
}
