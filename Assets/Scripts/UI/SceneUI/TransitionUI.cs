using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionUI : MonoBehaviour
{
    public static TransitionUI Instance { get; private set; }

    [SerializeField] private Transform parentObject;
    [SerializeField] private Transform fadeTransitionPrefab;

    private ISceneControl sceneControl;

    private float fadeTimer;

    private void Awake() {

        sceneControl = parentObject.GetComponent<ISceneControl>();

        if (sceneControl == null) {
            Debug.LogError("This game object dont inherit ISceneControl script");
        }

        sceneControl.StartScene += SceneControl_StartScene;
        sceneControl.EndScene += SceneControl_EndScene;

    }

    private void OnDestroy() {

        sceneControl.StartScene -= SceneControl_StartScene;
        sceneControl.EndScene -= SceneControl_EndScene;
    }

    private void SceneControl_EndScene(object sender, System.EventArgs e) {

        FadeTransition fadeTransition = FadeTransition.SpawnFadeTransition(fadeTransitionPrefab, this.transform); 

        StartCoroutine(fadeTransition.FadeOut());
    }

    private void SceneControl_StartScene(object sender, System.EventArgs e) {

        FadeTransition fadeTransition = FadeTransition.SpawnFadeTransition(fadeTransitionPrefab, this.transform);

        this.fadeTimer = fadeTransition.GetFadeTimer();

        StartCoroutine(fadeTransition.FadeIn());
    }

    public float GetFadeTimer() {
        return this.fadeTimer;
    }
}
