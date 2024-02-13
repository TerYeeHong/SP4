using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JLGame 
{
    public const string PLAYER_LIVES = "PlayerLives";
    public const string PLAYER_READY = "IsPlayerReady";
    public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";

    public const string PLAYER_TEAM = "PlayerTeam";
    public const string PLAYER_SCORE = "PlayerScore";

    public static Color GetColor(int colorChoice)
    {
        switch (colorChoice)
        {
            case 0: return Color.red;
            case 1: return Color.blue;
            case 2: return Color.green;
            case 3: return Color.yellow;
            case 4: return Color.white;
            case 5: return Color.red;
            case 6: return Color.red;
            case 7: return Color.red;
        }

        return Color.black;
    }
}
