// Made by: Matt Palero

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class MainMenuMgt : MonoBehaviour
{
    [Header("Scripts")]
    public PFDataMgr dataMgr;
    public SceneMgt sceneMgt;

    [Header("Transitions")]
    [SerializeField] private GameObject endTransition;
    [SerializeField] private GameObject startTransition;

    [SerializeField] private GameObject bg;

    // Start is called before the first frame update
    void Start()
    {
        startTransition.SetActive(false);
        endTransition.SetActive(true);

        bg.transform.LeanMoveLocal(new Vector2(48, -50), 1.5f).setEaseOutSine().setLoopPingPong();

        dataMgr.SetUserData();
        dataMgr.GetEPG();

        Debug.Log("EPG: " + PFGlobalData.epg);
    }

    public void OnLogOut()
    {
        startTransition.SetActive(true);
        PlayFabClientAPI.ForgetAllCredentials();

        Invoke("LogoutChangeScene", 1.7f);
    }

    private void LogoutChangeScene()
    {
        sceneMgt.ChangeScene("LogRegScene");
    }
}
