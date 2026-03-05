using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasPadLock
{
    public event EventHandler<UnlockPadLockEventArgs> UnlockPadLock;
    public class UnlockPadLockEventArgs : EventArgs {
        public Action callbackAction;
    }
}
