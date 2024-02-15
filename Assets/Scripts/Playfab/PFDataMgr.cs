using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PFDataMgr : MonoBehaviour
{
    public void SetUserData()
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                //saves player's data after every game
                {"XP", PFGlobalData.xp.ToString()},
                {"Level",PFGlobalData.level.ToString()}
            }
        },
        result => Debug.Log("Successfully update user data"),
        error =>
        {
            Debug.Log("Got error setting user data");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            // PlayFabId = myPlayFabId,
            // Keys = null
        }, result =>
        {
            Debug.Log("Got user data: ");
            if (result.Data == null || !result.Data.ContainsKey("XP"))
                Debug.Log("No XP");
            else
            {
                //gets data to print out the saved data during the game scene
                int.TryParse(result.Data["XP"].Value, out PFGlobalData.xp); //convert string to int
                int.TryParse(result.Data["Level"].Value, out PFGlobalData.level); //convert string to int
            }
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data: ");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void GetEPG() // get currency
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r => {
            int EPG = r.VirtualCurrency["EP"];
            PFGlobalData.epg = EPG;
        }, OnError);
    }

    void OnError(PlayFabError e) //function to handle error
    {
        Debug.Log(e.GenerateErrorReport());
    }
}
