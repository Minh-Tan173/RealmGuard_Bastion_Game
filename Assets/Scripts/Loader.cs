using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public enum Scene {
        MainMenu,
        LoadingScene,
        SampleScene,
        ForestLevel,
        RiverLevel
    };

    public static Scene targetScene;
    public static LevelManagerSO levelDataLoaded;

    public static LevelData currentLevelData;

    public static void Load(Scene targetScene) {

        // Ensure Time.timeScale is reset when loading a new scene
        Time.timeScale = 1f;

        // Load LoadingScene first
        SceneManager.LoadScene(Scene.LoadingScene.ToString());

        // Assign targetScene for targetScene pramater
        Loader.targetScene = targetScene;

    }

    public static void Load(string targetScene) {

        // Ensure Time.timeScale is reset when loading a new scene
        Time.timeScale = 1f;

        // Load LoadingScene first
        SceneManager.LoadScene(Scene.LoadingScene.ToString());

        // Assign targetScene for targetScene pramater

        if (Enum.TryParse(targetScene, out Scene actualScene)) {
            // If targetScene has in enum Scene

            Loader.targetScene = actualScene;

        }
        else {

            Debug.LogError($"Your {targetScene} don't have in Scene enum");
        }

    }

    public static void Load(LevelData levelData) {

        // If using this method
        Loader.currentLevelData = null;// Clear old data first

        // Ensure Time.timeScale is reset when loading a new scene
        Time.timeScale = 1f;

        // Load LoadingScene first
        SceneManager.LoadScene(Scene.LoadingScene.ToString());

        // After Load targetscene Done
        Loader.currentLevelData = levelData;
        Loader.targetScene = levelData.sceneToLoad;
    }

    public static string GetTargetScene() {
        return Loader.targetScene.ToString();
    }

}
