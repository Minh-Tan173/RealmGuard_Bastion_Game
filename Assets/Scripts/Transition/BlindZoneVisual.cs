using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindZoneVisual : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private CatapultTower catapultTower;

    [Header("Data")]
    [SerializeField] private float fadeInTimerMax;
    [SerializeField] private float fadeOutTimerMax;
    [SerializeField] private float maxAlphaValue;


    private Coroutine currentCoroutine;
    private bool isFirstStart = true;

    private void Awake() {

    }

    private void Start() {

        catapultTower.OnAttackZone += Catapult_OnAttackZone;
        catapultTower.UnAttackZone += catapult_UnAttackZone;
        catapultTower.UpdateAttackZone += Catapult_UpgradeAttackZone;

        HideBlindZone();
    }

    private void OnDestroy() {
        catapultTower.OnAttackZone -= Catapult_OnAttackZone;
        catapultTower.UnAttackZone -= catapult_UnAttackZone;
        catapultTower.UpdateAttackZone -= Catapult_UpgradeAttackZone;
    }

    private void Catapult_UpgradeAttackZone(object sender, ITowerObject.UpgradeAttackZoneEventArgs e) {
        // Setup circle size

        float attackZone = catapultTower.GetCatapultTowerSO().blindSpotRange;
        this.transform.localScale = new Vector3(attackZone * 2f, attackZone * 2f, 1f);
    }

    private void catapult_UnAttackZone(object sender, System.EventArgs e) {

        HideBlindZone();
    }

    private void Catapult_OnAttackZone(object sender, System.EventArgs e) {

        ShowBlindZone();

    }

    private void SetColorImage(float alphaValueNormalized, SpriteRenderer sprite) {

        Color tempColor = sprite.color;
        tempColor.a = Mathf.Clamp01(alphaValueNormalized);
        sprite.color = tempColor;

    }

    private IEnumerator FadeIn() {
        // Alpha tăng dần

        float timer = 0f;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        // Start
        SetColorImage(0f, sprite);

        yield return null;

        // FadeIn progress
        while (timer <= fadeInTimerMax) {

            float alphaValue = (timer / fadeInTimerMax) * maxAlphaValue;

            SetColorImage(alphaValue, sprite);

            timer += Time.deltaTime;

            yield return null;
        }

        // End
        yield return null;

        SetColorImage(maxAlphaValue, sprite);

    }

    private IEnumerator FadeOut() {
        // Alpha giảm dần

        float timer = fadeOutTimerMax;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        // Start
        SetColorImage(maxAlphaValue, sprite);

        yield return null;

        // FadeIn progress
        while (timer >= 0f) {

            float alphaValue = (timer / fadeOutTimerMax) * maxAlphaValue;

            SetColorImage(alphaValue, sprite);

            timer -= Time.deltaTime;

            yield return null;
        }

        // End
        yield return null;

        SetColorImage(0f, sprite);

        this.gameObject.SetActive(false);
    }

    private void ShowBlindZone() {

        this.gameObject.SetActive(true);

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        currentCoroutine = StartCoroutine(FadeIn());
    }

    private void HideBlindZone() {

        if (isFirstStart) {

            this.gameObject.SetActive(false);
            isFirstStart = false;

        }
        else {

            if (currentCoroutine != null) {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            currentCoroutine = StartCoroutine(FadeOut());
        }

    }
}
