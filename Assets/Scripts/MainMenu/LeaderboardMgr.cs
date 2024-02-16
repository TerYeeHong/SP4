using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class LeaderboardMgr : MonoBehaviour
{
    private string userDisplayName;

    [Header("LB Prefabs")]
    [SerializeField] GameObject rowPrefab;
    [SerializeField] Transform rowParent;

    [Header("Canvas")]
    [SerializeField] GameObject leaderboardCanvas;
    [SerializeField] GameObject mainCanvas;

    private int score;

    private void Awake()
    {
        GetPlayerProfile();
    }

    void Start()
    {
        leaderboardCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    //void UpdateMsg(string msg)
    //{
    //    leaderboardBox.text = msg;
    //}

    void GetPlayerProfile()
    {
        GetPlayerProfileRequest request = new GetPlayerProfileRequest();
        PlayFabClientAPI.GetPlayerProfile(request, GetProfileSuccess, OnError);
    }

    void GetProfileSuccess(GetPlayerProfileResult r)
    {
        userDisplayName = r.PlayerProfile.DisplayName;
    }

    public void OnAddScore()
    {
        score += 10;
        SendLeaderboard();
        Invoke("OnGetGlobalTimerLeaderboard", 1.0f);
    }

    public void OnBack()
    {
        leaderboardCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    void OnError(PlayFabError e) //function to handle error
    {
        Debug.Log("Error: " + e.ErrorMessage);
    }

    public void OnGetGlobalTimerLeaderboard()
    {
        leaderboardCanvas.SetActive(true);
        mainCanvas.SetActive(false);

        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "Best Time", //playfab leaderboard stat name
            StartPosition = 0,
            MaxResultsCount = 100
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnGetGlobalTimerLeaderboard, OnError);
        leaderboardCanvas.SetActive(true);
    }

    void OnGetGlobalTimerLeaderboard(GetLeaderboardResult r)
    {
        int min, sec;
        // Destroy Exisiting prefabs
        foreach(Transform item in rowParent)
        {
            Destroy(item.gameObject);
        }

        foreach (var item in r.Leaderboard)
        {
            GameObject newGO = Instantiate(rowPrefab, rowParent);
            TMP_Text[] texts = newGO.GetComponentsInChildren<TMP_Text>();

            texts[0].text = (item.Position + 1).ToString();
            texts[1].text = item.DisplayName.ToString();

            min = Mathf.FloorToInt(item.StatValue / 60);
            sec = Mathf.FloorToInt(item.StatValue % 60);
            texts[2].text = string.Format("{0:00}:{1:00}", min, sec);
        }
    }

    public void SendLeaderboard()
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Best Time",
                    Value = score
                }
            }
        };
        Debug.Log("Submitting score: " + score);
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        Debug.Log("Successful leaderboard sent: " + r.ToString());
    }
}
