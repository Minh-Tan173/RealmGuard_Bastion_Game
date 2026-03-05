using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioClipRefsSO : ScriptableObject
{
    [Header("Main Menu SFX")]
    public AudioClip paperScrollDownSFX;
    public AudioClip paperScrollUpSFX;

    [Header("Button SFX")]
    public AudioClip buttonClickedSFX;
    public AudioClip buttonDataHubClickedSFX;
    public AudioClip buttonScrollClickedSFX;

    [Header("Ability SFX")]
    public AudioClip explosionBarrelSFX;
    public AudioClip thunderStrikeSFX;
    public AudioClip spikeAttackSFX;

    [Header("Tower Main SFX")]
    public AudioClip towerUpgradeProgressSFX;

    [Header("Archer Tower SFX")]
    public AudioClip archerTowerPlacedSFX;
    public AudioClip[] archerSpawnSFX;
    public AudioClip archerAttackSFX;
    public AudioClip archerTowerUpgradeSuccessSFX;

    [Header("Mage Tower SFX")]
    public AudioClip mageTowerPlacedSFX;
    public AudioClip[] mageSpawnSFX;
    public AudioClip mageAttackSFX;
    public AudioClip mageTowerUpgradeSuccessSFX;

    [Header("Catapult Tower SFX")]
    public AudioClip catapultTowerPlacedSFX;
    public AudioClip catapultAttackSFX;
    public AudioClip catapultTowerUpgradeSuccessSFX;

    [Header("Guardian Tower SFX")]
    public AudioClip guardianTowerPlacedSFX;
    public AudioClip[] guardianOnCommandMovedSFX;
    public AudioClip[] guardianTowerOnAssignPosVisualSFX;
    public AudioClip guardianDeployComplete;
    public AudioClip guardianTowerUpgradeSuccessSFX;
}
