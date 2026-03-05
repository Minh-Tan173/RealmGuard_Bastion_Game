using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleAnimatorController : MonoBehaviour
{
    [SerializeField] private List<Sprite> idleSpriteList;
    [SerializeField] private float frameRate;

    private SpriteRenderer spriteRenderer;
    private int totalSprite;
    private int currentFrame;
    private float frameTimer;

    private void Awake() {

        spriteRenderer = GetComponent<SpriteRenderer>();

        totalSprite = idleSpriteList.Count;

        frameTimer = frameRate;
    }

    private void Update() {

        frameTimer -= Time.deltaTime;   

        if (frameTimer <= 0f) {

            frameTimer += frameRate;

            currentFrame = (currentFrame + 1) % totalSprite;

            spriteRenderer.sprite = idleSpriteList[currentFrame];
        }


    }
}
