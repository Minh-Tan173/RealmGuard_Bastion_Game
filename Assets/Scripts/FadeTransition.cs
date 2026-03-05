using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTimer;

    private void SetAlpha(float alpha) {
        Color tempColor = fadeImage.color;
        tempColor.a = alpha;
        fadeImage.color = tempColor;
    }

    public IEnumerator FadeOut() {

        // 1.
        SetAlpha(0f);

        float timer = 0f;

        // 2. 
        while (timer <= fadeTimer) {

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / fadeTimer));
            float alpha = Mathf.Lerp(0f, 1f, t);

            SetAlpha(alpha);

            timer += Time.deltaTime;

            yield return null;
        }

        // 3.
        SetAlpha(1f);
    }

    public IEnumerator FadeIn() {

        // 1.
        SetAlpha(1f);

        float timer = 0f;

        // 2. 
        while (timer <= fadeTimer) {

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / fadeTimer));
            float alpha = Mathf.Lerp(1f, 0f, t);

            SetAlpha(alpha);

            timer += Time.deltaTime;

            yield return null;
        }

        // 3.
        SetAlpha(0f);

        yield return null;

        Destroy(this.gameObject);
    }

    public float GetFadeTimer() {
        return this.fadeTimer;
    }

    public static FadeTransition SpawnFadeTransition(Transform prefab, Transform parent) {
        Transform fadeTransitionTransform = Instantiate(prefab, parent);

        return fadeTransitionTransform.GetComponent<FadeTransition>();
    }
    
}
