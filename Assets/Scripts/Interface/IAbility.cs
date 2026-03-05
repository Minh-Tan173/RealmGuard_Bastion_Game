using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IAbility
{
    public enum AbilityType {
        None,
        ThunderLightning,
        ExplosiveBarrel,
        SpikeTrap
    }

    public enum AbilityLevel {
        Level1 = 1,
        Level2 = 2,
        Level3 = 3
    }

    public event EventHandler<OnAttackSFXEventArgs> OnAttackSFX;
    public class OnAttackSFXEventArgs : EventArgs {
        public AudioClip audioClip;
    }

    public void UpdateDataWithLevel();
}
