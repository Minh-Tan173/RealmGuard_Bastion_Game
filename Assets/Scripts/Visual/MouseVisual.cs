using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseVisual : MonoBehaviour
{
    public enum CursorType {
        Arrow,
        Aim
    }

    public static MouseVisual Instance { get; private set; }

    [Header("Cursor Texture")]
    [SerializeField] private Texture2D mouseTexture;
    [SerializeField] private List<CursorAnimation> cursorAnimationList;

    private CursorType cursorType;
    private CursorAnimation currentCursor;

    private int currentFrame;
    private float frameTimer;
    private int frameCount;

    private void Awake() {

        Instance = this;
        
    }

    private void Start() {

        SetActiveCursorType(CursorType.Arrow);

    }

    private void Update() {

        frameTimer -= Time.deltaTime;

        if (frameTimer <= 0f) {

            frameTimer += currentCursor.frameRate;
            currentFrame = (currentFrame + 1) % frameCount;

            Cursor.SetCursor(currentCursor.textureArray[currentFrame], currentCursor.offset, CursorMode.Auto);
        }   

    }

    private void SetActiveCursorAnimaton(CursorAnimation cursorAnimation) {

        this.currentCursor = cursorAnimation;
        currentFrame = 0;
        frameTimer = cursorAnimation.frameRate;
        frameCount = cursorAnimation.textureArray.Length;
        
    }

    private CursorAnimation GetActiveCursorAnimaton(CursorType cursorType) {

        foreach (CursorAnimation cursor in cursorAnimationList) {

            if (cursor.cursorType == cursorType) {
                return cursor;
            }
       
        }

        // Couldn't find this cursorType
        return null;
    }

    public void SetActiveCursorType(CursorType cursorType) {

        SetActiveCursorAnimaton(GetActiveCursorAnimaton(cursorType));

    }

    [System.Serializable]
    public class CursorAnimation 
    {
        public Texture2D[] textureArray;
        public float frameRate;
        public Vector2 offset;
        public CursorType cursorType;
    }
}

