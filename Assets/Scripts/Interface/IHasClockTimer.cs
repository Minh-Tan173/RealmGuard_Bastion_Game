using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasClockTimer
{
    public event EventHandler<OnChangeProgressEventArgs> OnChangeProgress;
    public class OnChangeProgressEventArgs : EventArgs {
        public float progressNormalized;
    }
}
