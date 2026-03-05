using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl_GENSCENE : MonoBehaviour
{
    public static CameraControl_GENSCENE Instance { get; private set; }

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

        int mapWidth = GridManager_GENSCENE.Instance.GetMapWidth();
        int mapHeight = GridManager_GENSCENE.Instance.GetMapHeight();

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
