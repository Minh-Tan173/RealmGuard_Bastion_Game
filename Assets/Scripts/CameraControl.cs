using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CameraControl : MonoBehaviour {

    public static CameraControl Instance { get; private set; }

    private CinemachineVirtualCamera virtualCamera;

    private Vector2 cameraPosition;

    private void Awake() {

        Instance = this;

        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        virtualCamera.transform.position = new Vector3(0f, 0f, -10f);

    }

    private void Start() {

        // When start game

        // Setup camera position

        int mapWidth = GridManager.Instance.GetMapWidth();
        int mapHeight = GridManager.Instance.GetMapHeight();

        cameraPosition = new Vector2(mapWidth / 2, mapHeight / 2);

        virtualCamera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, -10f);

    }

    public CinemachineVirtualCamera GetCinemachineVirtualCamera() {
        return this.virtualCamera;
    }

    public Vector2 GetCameraPosition() {
        return cameraPosition;
    }

}
