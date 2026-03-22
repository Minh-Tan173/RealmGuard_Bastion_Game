using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasFieldOfView
{
    public event EventHandler<OnFOVVisualEventArgs> OnFOVVisual;
    public class OnFOVVisualEventArgs : EventArgs {

        public SoldierSO.SoldierDirection soldierDirection;
        public float size;
    }

    public event EventHandler<OnUpdateFOVSizeEventArgs> UpdateFOVSize;
    public class OnUpdateFOVSizeEventArgs : EventArgs {

        public float size;
    }

    public event EventHandler ShowFOVVisual;
    public event EventHandler HideFOVVisual;
}
