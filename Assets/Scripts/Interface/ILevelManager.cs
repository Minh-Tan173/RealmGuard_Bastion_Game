using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelManager
{
    public enum GameMode {
        Campain,
        Custom
    }

    public enum BiomeType {
        Default = 0,
        Forest = 1,
        River = 2,
        Graveyard = 3,
        Swamp = 4
    }

    public enum GameLevel {
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5
    }

    public enum HeartChangedState {
        Increase,
        Decrase
    };

    public enum CoinChangedState {
        Increase,
        Decrase
    };

}
