using System.Collections;
using TMPro;
using UnityEngine;

public class FadeOutText : MonoBehaviour
{
    private TMP_Text textToFade;  
    private float fadeDuration = 2f;  
    [HideInInspector] public bool fadeOut = false;  
    private bool isFading = false;

    void Start()
    {
        if (textToFade == null)
        {
            textToFade = gameObject.GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        if (fadeOut && !isFading)
        {
            StartCoroutine(FadeOutRoutine());
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        isFading = true;  
        float elapsedTime = 0f; 

        Color originalColor = textToFade.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime; 
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textToFade.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;  
        }

        textToFade.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);  
        isFading = false;
        fadeOut = false;
    }
}
