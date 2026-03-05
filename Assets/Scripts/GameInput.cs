using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnLeftClicked;
    public event EventHandler OnRightClicked;

    public event EventHandler OnPauseGameAction;

    [SerializeField] private Camera mainCamera;

    private PlayerInputActions playerInputActions;

    private void Awake() {

        Instance = this;

        playerInputActions = new PlayerInputActions();

        playerInputActions.Enable();
        playerInputActions.Mouse.LeftClicked.performed += LeftClicked_performed;
        playerInputActions.Mouse.RightClicked.performed += RightClicked_performed;

        playerInputActions.Player.PausedGame.performed += PausedGame_performed;
        
    }

    private void OnDestroy() {
        playerInputActions.Mouse.LeftClicked.performed -= LeftClicked_performed;
        playerInputActions.Mouse.RightClicked.performed -= RightClicked_performed;

        playerInputActions.Player.PausedGame.performed -= PausedGame_performed;
    }


    private void PausedGame_performed(InputAction.CallbackContext obj) {
        OnPauseGameAction?.Invoke(this, EventArgs.Empty);
    }


    private void RightClicked_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        if (IsPointerOverSceneUI()) {
            // If mouse is on UI - Dont call event

            return;
        }

        // Only happen in game, not in UI
        OnRightClicked?.Invoke(this, EventArgs.Empty);
    }

    private void LeftClicked_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        if (IsPointerOverSceneUI()) {
            // If mouse is on UI - Dont call event

            return;
        }

        // Only happen in game, not in UI
        OnLeftClicked?.Invoke(this, EventArgs.Empty);
    }

    private bool IsPointerOverSceneUI() {

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = new Vector2(GetMouseCamPos().x, GetMouseCamPos().y);

        List<RaycastResult> uiDetectedList = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, uiDetectedList);

        if (uiDetectedList.Count > 0) {

            foreach(RaycastResult ui in uiDetectedList) {

                Canvas canvas = ui.gameObject.GetComponentInParent<Canvas>();

                if (canvas == null) {
                    // This UI is PhysicRaycaster (is not GraphicRaycaster)
                    continue;
                }

                if (canvas.renderMode != RenderMode.WorldSpace) {
                    // If main canvas of this UI is not WorldSpace mode
                    return true;
                }

                if (canvas.renderMode == RenderMode.WorldSpace) {
                    // If main canvas of this UI is WorldSpace mode

                    if (ui.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null) {
                        // If this ui is a Button

                        return true;
                    }
                    else {
                        // If this ui is not a Button
                    }
                }


            }
        }

        // If not any UI in uiDetectedList is Wor
        return false;
    }

    public Vector3 GetMouseCamPos() {

        Vector3 mousePositionOnScreen = playerInputActions.Mouse.MousePosition.ReadValue<Vector2>();

        return mousePositionOnScreen;
    }

    public Vector3 GetMouseWorldPos() {

        Vector3 mousePositionOnScreen = playerInputActions.Mouse.MousePosition.ReadValue<Vector2>();

        mousePositionOnScreen.z = -Camera.main.transform.position.z;

        Vector3 mousePositionOnWorld = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);

        return mousePositionOnWorld;
    }
}
