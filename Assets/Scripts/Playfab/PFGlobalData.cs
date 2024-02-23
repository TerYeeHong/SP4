// Made by: Matt Palero

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PFGlobalData
{
    public static int xp = 0;
    public static int epg = 0; //currency (El Primo Gems)
    public static int level = 1;

    public static List<Status> blessings = new();


    public static void AddBlessing(Status status, Unit player)
    {
        blessings.Add(status);
        //status.OnEquip(player);

        GameEvents.m_instance.onStatusChange.Invoke();

    }
    public static bool ContainBlessingName(string name)
    {
        foreach (Status status in blessings)
        {
            if (status.Name_status == name)
                return true;
        }
        return false;
    }
    public static int GetBlessingCount(string name)
    {
        int amt = 0;
        foreach (Status status in blessings)
        {
            if (status.Name_status == name)
                ++amt;
        }
        return amt;
    }


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
