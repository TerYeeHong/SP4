// Made by: Matt Palero

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PFGlobalData
{
    public static int xp = 0;
    public static int epg = 0; //currency (El Primo Gems)
    public static int level = 1;

    public static void AddXP(int value)
    {
        //dont allow negative
        if (xp < 0)
            return;

        //add xp
        xp += value;

        //level up
        while (xp > 50)
        {
            xp -= 50;
            level++;
        }
    }

    public static void AddEPG(int value)
    {
        //dont allow negative
        if (value < 0)
            return;

        //add gold
        epg += value;
    }
}
