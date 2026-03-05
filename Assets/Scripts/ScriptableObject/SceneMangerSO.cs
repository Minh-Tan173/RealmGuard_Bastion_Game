using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SceneMangerSO : ScriptableObject
{
    [Header("HUD")]
    public Transform levelManagerUI;

    [Header("Scene UI")]
    public Transform gamePauseUI;
    public Transform gameOverUI;
    public Transform gameWinUI;

    [Header("Main Menu UI")]
    public Transform modeSelectUI;
    public Transform campaignMapUI;
    public Transform customMapUI;
    public Transform upgradeAbilityUI;
    public Transform dataTowerUI;
    public Transform creditsUI;
}
