using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectVisual : MonoBehaviour
{
    [SerializeField] private ObjectSO objectSO;

    private SpriteRenderer spriteRenderer;
    private Sprite[] spriteArray;

    private void Awake() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteArray = objectSO.spriteArray;
    }

    private void Start() {

        // Update visual
        spriteRenderer.sprite = spriteArray[UnityEngine.Random.Range(0, spriteArray.Length)];
    }

}
