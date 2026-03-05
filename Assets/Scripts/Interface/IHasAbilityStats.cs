using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasAbilityStats
{
    public event EventHandler<OnUpdateVisualEventArgs> OnUpdateDataVisual;
    public class OnUpdateVisualEventArgs : EventArgs {
        public IAbility.AbilityType abilityType;
    }
}
