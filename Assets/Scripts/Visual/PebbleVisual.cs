using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PebbleVisual : MonoBehaviour
{
    [SerializeField] private Pebble pebble;
    [SerializeField] private Transform boomEffect;
    [SerializeField] private float baseBoomSize;

    private const string TRIGGER_EXPLOSION = "TriggerExplosion";

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {

        pebble.OnExplosionAnim += Pebble_OnExplosionAnim;
        pebble.OnResetAnim += Pebble_OnResetAnim;

    }

    private void OnDestroy() {

        pebble.OnExplosionAnim -= Pebble_OnExplosionAnim;
        pebble.OnResetAnim -= Pebble_OnResetAnim;
    }

    private void Pebble_OnResetAnim(object sender, System.EventArgs e) {

        // 1.Reset boom effect size
        boomEffect.localScale = new Vector3(baseBoomSize, baseBoomSize, baseBoomSize);

        // 2.Reset animator
        animator.Rebind();
        animator.Update(0f);
    }

    private void Pebble_OnExplosionAnim(object sender, System.EventArgs e) {

        // 1.Update size base on catapult tower level
        float newSize = baseBoomSize * pebble.GetSplashSize();
        //boomEffect.localScale = new Vector3(newSize, newSize, newSize);

        animator.SetTrigger(TRIGGER_EXPLOSION);
    }

    public float GetBaseBoomSize(){
        return this.baseBoomSize;
    }
}
