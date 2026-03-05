using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseObject : MonoBehaviour
{
    [SerializeField] private MouseVisual.CursorType cursorType;

    private void OnMouseEnter() {
        
        if (BuildingManager.Instance.IsPlacingAbility()) {

            MouseVisual.Instance.SetActiveCursorType(cursorType);

        }

    }

    private void OnMouseExit() {

        MouseVisual.Instance.SetActiveCursorType(MouseVisual.CursorType.Arrow);

    }
}
