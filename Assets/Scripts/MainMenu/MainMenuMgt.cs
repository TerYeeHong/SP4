// Made by: Matt Palero

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;

public class MainMenuMgt : MonoBehaviour
{
    [Header("Scripts")]
    public PFDataMgr dataMgr;
    public SceneMgt sceneMgt;

    [Header("Transitions")]
    [SerializeField] private GameObject endTransition;
    [SerializeField] private GameObject startTransition;

    [SerializeField] private GameObject bg;

    private string scene;
    private string playerName;
    public AudioClip clickSFX;

    // Start is called before the first frame update
    void Start()
    {
        GetPlayFabDisplayName();

        scene = "";
        startTransition.SetActive(false);
        endTransition.SetActive(true);

        bg.transform.LeanMoveLocal(new Vector2(48, -50), 1.5f).setEaseOutSine().setLoopPingPong();

        dataMgr.GetUserData();
        dataMgr.GetEPG();

        //Debug.Log(playerName + " XP: " + PFGlobalData.xp);
        //Debug.Log(playerName + " Level: " + PFGlobalData.level);
    }

    private void SceneChange()
    {
        switch (scene)
        {
            case "LogReg":
                sceneMgt.ChangeScene("LogRegScene");
                break;

            case "Play":
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
                sceneMgt.ChangeScene("DemoAsteroids-LobbyScene");
                break;

        }
    }
    public void OnLogOut()
    {
        GameEvents.m_instance.playNewAudioClip.Invoke(clickSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);

        startTransition.SetActive(true);
        PlayFabClientAPI.ForgetAllCredentials();

        scene = "LogReg";
        Invoke("SceneChange", 1.7f);
    }

    public void OnPlay()
    {
        GameEvents.m_instance.playNewAudioClip.Invoke(clickSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);

        startTransition.SetActive(true);

        scene = "Play";
        Invoke("SceneChange", 1.7f);
    }

    public void GetPlayFabDisplayName()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            var request = new GetAccountInfoRequest();
            PlayFabClientAPI.GetAccountInfo(request, OnSuccess, OnError);
        }
        else
        {
            Debug.LogError("Not logged in to PlayFab. Cannot retrieve display name.");
        }
    }

    // Callback for successful GetAccountInfo request
    private void OnSuccess(GetAccountInfoResult result)
    {
        var displayName = result.AccountInfo?.TitleInfo?.DisplayName;
        if (!string.IsNullOrEmpty(displayName))
        {
            playerName = displayName;
            Debug.Log("PlayFab display name: " + displayName);
            // You can use the displayName here in your game logic
        }
        else
        {
            Debug.LogWarning("PlayFab display name is null or empty.");
        }
    }

    // Callback for failed GetAccountInfo request
    private void OnError(PlayFabError error)
    {
        Debug.LogError("Failed to retrieve PlayFab display name: " + error.ErrorMessage);
    }
}
