using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVVisual : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private Transform parent;

    [Header("FOV Visual")]
    [SerializeField] private Transform upFOV;
    [SerializeField] private Transform rightFOV;
    [SerializeField] private Transform downFOV;
    [SerializeField] private Transform leftFOV;

    private IHasFieldOfView hasFieldOfView;

    private bool isActiveUpView = false;
    private bool isActiveRightView = false;
    private bool isActiveDownView = false;
    private bool isActiveLeftView = false;

    private void Awake() {

        hasFieldOfView = parent.GetComponent<IHasFieldOfView>();

        if (hasFieldOfView == null) {

            Debug.LogError("Parent dont inherit IHasFieldOfView interface");
        }

        hasFieldOfView.OnFOVVisual += HasFieldOfView_OnFOVVisual;
        hasFieldOfView.UpdateFOVSize += HasFieldOfView_UpdateFOVSize;
        hasFieldOfView.ShowFOVVisual += HasFieldOfView_ShowFOVVisual;
        hasFieldOfView.HideFOVVisual += HasFieldOfView_HideFOVVisual;
    }

    private void Start() {

        upFOV.gameObject.SetActive(false);
        rightFOV.gameObject.SetActive(false);
        downFOV.gameObject.SetActive(false);
        leftFOV.gameObject.SetActive(false);
    }

    private void OnDestroy() {

        hasFieldOfView.OnFOVVisual -= HasFieldOfView_OnFOVVisual;
        hasFieldOfView.UpdateFOVSize -= HasFieldOfView_UpdateFOVSize;
        hasFieldOfView.ShowFOVVisual -= HasFieldOfView_ShowFOVVisual;
        hasFieldOfView.HideFOVVisual -= HasFieldOfView_HideFOVVisual;
    }


    private void HasFieldOfView_HideFOVVisual(object sender, System.EventArgs e) {

        SetActiveFOV(false);
    }

    private void HasFieldOfView_ShowFOVVisual(object sender, System.EventArgs e) {

        SetActiveFOV(true);
    }

    private void HasFieldOfView_UpdateFOVSize(object sender, IHasFieldOfView.OnUpdateFOVSizeEventArgs e) {
        
        float diameter = e.size * 2f;

        if (upFOV.gameObject.activeSelf) {

            upFOV.localScale = new Vector3(diameter, diameter, diameter);
        }

        if (rightFOV.gameObject.activeSelf) {

            rightFOV.localScale = new Vector3(diameter, diameter, diameter);
        }

        if (downFOV.gameObject.activeSelf) {

            downFOV.localScale = new Vector3(diameter, diameter, diameter);
        }

        if (leftFOV.gameObject.activeSelf) {

            leftFOV.localScale = new Vector3(diameter, diameter, diameter);
        }

    }

    private void HasFieldOfView_OnFOVVisual(object sender, IHasFieldOfView.OnFOVVisualEventArgs e) {

        switch (e.soldierDirection) {

            case SoldierSO.SoldierDirection.Up:

                isActiveUpView = true;
                ShowFOVAnimation(upFOV, e.size);

                break;

            case SoldierSO.SoldierDirection.Right:

                isActiveRightView = true;
                ShowFOVAnimation(rightFOV, e.size);
                
                break;

            case SoldierSO.SoldierDirection.Down:

                isActiveDownView = true;
                ShowFOVAnimation(downFOV, e.size);
                
                break;

            case SoldierSO.SoldierDirection.Left:

                isActiveLeftView = true;
                ShowFOVAnimation(leftFOV, e.size);
                
                break;
        }
    }

    private void ShowFOVAnimation(Transform fov, float radius) {

        float duration = 0.5f;
        fov.localScale = Vector3.zero;

        fov.gameObject.SetActive(true);

        fov.DOScale(new Vector3(radius * 2f, radius * 2f, radius * 2f), duration).SetEase(Ease.Linear);
    }

    private void SetActiveFOV(bool isActivated) {

        if (isActiveUpView) {

            upFOV.gameObject.SetActive(isActivated);
        }
        
        if (isActiveRightView) {

            rightFOV.gameObject.SetActive(isActivated);
        }
        
        if (isActiveDownView) {

            downFOV.gameObject.SetActive(isActivated);
        }
        
        if (isActiveLeftView) {

            leftFOV.gameObject.SetActive(isActivated);
        }
    }
}
