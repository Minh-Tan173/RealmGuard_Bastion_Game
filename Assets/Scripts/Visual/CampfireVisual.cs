using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireVisual : MonoBehaviour
{
    public enum CamfireType {
        Type1,
        Type2
    }

    [Header("Campfire 1")]
    [SerializeField] private Sprite[] campFire1SpriteArray;
    
    [Header("Campfire 2")]
    [SerializeField] private Sprite[] campFire2SpriteArray;

    [Header("Campfire Setup animation")]
    [SerializeField] private Transform campFireVisual;
    [SerializeField] private float frameRate;

    private SpriteRenderer spriteRenderer;

    private CamfireType camfireType;
    private int numberFrameCampfire1;
    private int numberFrameCampfire2;
    private float frameTimer;
    private int currentFrame;

    private void Awake() {

        spriteRenderer = campFireVisual.GetComponent<SpriteRenderer>();

        numberFrameCampfire1 = campFire1SpriteArray.Length;
        numberFrameCampfire2 = campFire2SpriteArray.Length;

        frameTimer = frameRate;
    }

    private void Start() {

        RandomCamfireType();
    }

    private void Update() {

        UpdateVisualCampfire();

    }

    private void UpdateVisualCampfire() {

        frameTimer -= Time.deltaTime;

        if (frameTimer <= 0f) {

            frameTimer += frameRate;

            switch (camfireType) {
                case CamfireType.Type1:

                    currentFrame = (currentFrame + 1) % numberFrameCampfire1;
                    spriteRenderer.sprite = campFire1SpriteArray[currentFrame];

                    break;

                case CamfireType.Type2:

                    currentFrame = (currentFrame + 1) % numberFrameCampfire2;
                    spriteRenderer.sprite = campFire2SpriteArray[currentFrame];

                    break;
            }

        }

    }

    private void RandomCamfireType() {

        int randomValue = Mathf.FloorToInt(UnityEngine.Random.value * 1.99f);

        if (randomValue == 0) {
            this.camfireType = CamfireType.Type1;

            spriteRenderer.sprite = campFire1SpriteArray[UnityEngine.Random.Range(0, numberFrameCampfire1)];
        }

        if (randomValue == 1) {
            this.camfireType = CamfireType.Type2;

            spriteRenderer.sprite = campFire2SpriteArray[UnityEngine.Random.Range(0, numberFrameCampfire2)];
        }

    }
}
