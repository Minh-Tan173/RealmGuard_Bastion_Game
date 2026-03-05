using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MusicManagerSO : ScriptableObject
{
    public AudioClip[] mainTheme;
    public AudioClip startGameBattleTheme;
    public AudioClip gameOverTheme;
    public AudioClip gameWinTheme;
}
