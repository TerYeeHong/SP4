using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardMgr : MonoBehaviour
{
    //[SerializeField] private TMP_Text leaderboardBox;

    //private string userDisplayName;

    //public GameObject leaderboardCanvas;

    //private void Awake()
    //{
    //    GetPlayerProfile();
    //}

    //void UpdateMsg(string msg)
    //{
    //    leaderboardBox.text = msg;
    //}

    //void GetPlayerProfile()
    //{
    //    GetPlayerProfileRequest request = new GetPlayerProfileRequest();
    //    PlayFabClientAPI.GetPlayerProfile(request, GetProfileSuccess, OnError);
    //}

    //void GetProfileSuccess(GetPlayerProfileResult r)
    //{
    //    userDisplayName = r.PlayerProfile.DisplayName;
    //}

    //void OnError(PlayFabError e) //function to handle error
    //{
    //    UpdateMsg("Error!" + e.ErrorMessage());
    //}

    //public void OnGetGlobalTimerLeaderboard()
    //{
    //    var lbreq = new GetLeaderboardRequest
    //    {
    //        StatisticName = "Best Time", //playfab leaderboard stat name
    //        StartPosition = 0,
    //        MaxResultsCount = 100
    //    };
    //    PlayFabClientAPI.GetLeaderboard(lbreq, OnGlobalTimerLeaderboardGet, OnError);
    //    leaderboardCanvas.SetActive(true);
    //}

    //void OnGlobalTimerLeaderboardGet(GetLeaderboardResult r)
    //{
    //    string LeaderboardStr = "Global Leaderboard \n \n Position | Name | Score\n";
    //    foreach (var item in r.Leaderboard)
    //    {
    //        string onerow;
    //        if (item.DisplayName == userDisplayName) // highlight current user's name
    //        {
    //            onerow = ">> " + (item.Position + 1) + " | " + item.DisplayName + " | " + item.StatValue + " <<\n";

    //        }
    //        else
    //            onerow = (item.Position + 1) + " | " + item.DisplayName + " | " + item.StatValue + "\n";
    //        LeaderboardStr += onerow; // combine all display into one string
    //    }
    //    UpdateMsg(LeaderboardStr);
    //}

    ////public void SendLeaderboard()
    ////{
    ////    var req = new UpdatePlayerStatisticsRequest
    ////    {
    ////        Statistics = new List<StatisticUpdate>
    ////        {
    ////            new StatisticUpdate
    ////            {
    ////                StatisticName = "High Score",
    ////                Value = gameController.score
    ////            }
    ////        }
    ////    };
    ////    UpdateMsg("Submitting score: " + gameController.score);
    ////    PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);
    ////}

    //void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    //{
    //    UpdateMsg("Successful leaderboard sent: " + r.ToString());
    //}
}
