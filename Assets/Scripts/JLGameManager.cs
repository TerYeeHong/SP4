using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using ExitGames.Client.Photon;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using Cinemachine;

using UnityEngine.SceneManagement;

using TMPro;

public class JLGameManager : MonoBehaviourPunCallbacks
{
    public static JLGameManager Instance = null;
    public Text InfoText;
    public TextMeshProUGUI health_tmp;
    //public GameObject wayPoints;
    //public GameObject enemies;
    //public GameEnding endingScript;
    public UIManager uimanager;
    public CinemachineVirtualCamera virtualCam;


    [Header("UI")]
    public TextMeshProUGUI red_score_tmp;
    public TextMeshProUGUI blue_score_tmp;
    int red_score;
    int blue_score;
    public int score_max;

    [SerializeField] GameObject win_window;
    [SerializeField] TextMeshProUGUI win_text;

    public List<Player> playerList = new List<Player>();


    float start_time = 0.0f;
    public float GetStartTime()
    {
        return start_time - Time.time;
    }

    public void Awake()
    {
        Instance = this;
        red_score = 0;
        blue_score = 0;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        CountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;

        RaiseEvents.UpdateTeamScoreEvent += UpdateTeamScore;
    }
    public override void OnDisable()
    {
        base.OnDisable();

        CountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;

        RaiseEvents.UpdateTeamScoreEvent -= UpdateTeamScore;

    }

    // Start is called before the first frame update
    public void Start()
    {
        Hashtable props = new Hashtable
            {
                {JLGame.PLAYER_LOADED_LEVEL, true}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }


    #region PUN CALLBACKS

    public override void OnDisconnected(DisconnectCause cause)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("DemoAsteroids-LobbyScene");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckEndOfGame();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(JLGame.PLAYER_LIVES))
        {
            CheckEndOfGame();
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // if there was no countdown yet, the master client (this one) waits until everyone loaded the level and sets a timer start
        int startTimestamp;
        bool startTimeIsSet = CountdownTimer.TryGetStartTime(out startTimestamp);

        //cache all players into the list (shld update again when players join/leave)
        playerList.Clear();
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            playerList.Add(p);
        }

        if (changedProps.ContainsKey(JLGame.PLAYER_LOADED_LEVEL))
        {
            if (CheckAllPlayerLoadedLevel())
            {
                if (!startTimeIsSet)
                {
                    CountdownTimer.SetStartTime();
                }

                //StartGame();
            }
            else
            {
                // not all players loaded yet. wait:
                Debug.Log("setting text waiting for players! ", this.InfoText);
                InfoText.text = "Waiting for other players...";
            }
        }

    }

    #endregion

    private bool CheckAllPlayerLoadedLevel()
    {
        foreach (Player p in playerList)
        {
            object playerLoadedLevel;

            if (p.CustomProperties.TryGetValue(JLGame.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
            {
                if ((bool)playerLoadedLevel)
                {
                    continue;
                }
            }

            return false;
        }

        //PhotonNetwork.LocalPlayer.;
        return true;
    }
    private void StartGame()
    {
        Debug.Log("StartGame!");


        Vector3 position = new Vector3(Random.Range(-3, 3.0f), 0.0f, Random.Range(-3, 3.0f));
        Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

        //PhotonNetwork.InstantiateRoomObject is to create a object that requires sychronisation in all clients
        GameObject player = PhotonNetwork.Instantiate("Player", position, rotation, 0);      

        //if im the real dude :)
        if (player.GetComponent<PhotonView>().IsMine)
        {
            //virtualCam.Follow = player.transform;
        }
        else
        {
            player.GetComponent<Rigidbody>().isKinematic = true;
        }



        //Now as the master client, set the teams
        if (!PhotonNetwork.IsMasterClient)
            return;
    }

    public void AddPlayerScore(int playerActorId, int score)
    {
        object playerScore;
        if (PhotonNetwork.PlayerList[playerActorId].CustomProperties.TryGetValue(JLGame.PLAYER_SCORE, out playerScore))
        {
            Hashtable props = new Hashtable
            {
                {JLGame.PLAYER_SCORE, score + (int)playerScore}
            };
            PhotonNetwork.PlayerList[playerActorId].SetCustomProperties(props);
        }
    }
    public void SetPlayerScore(int playerActorId, int new_score)
    {
        Hashtable props = new Hashtable
        {
            {JLGame.PLAYER_SCORE, new_score }
        };
        PhotonNetwork.PlayerList[playerActorId].SetCustomProperties(props);
    }
    public void AddTeamScore(int team, int score)
    {
        ////SHOULD ALREADY DO THE MASTERCLIENT CHECK BEFORE THIS FUNCITON CALL
        //if (!PhotonNetwork.IsMasterClient)
        //    return;

        Debug.Log("Add score"+ score+"-"+team);


        int team_score = -1;
        switch (team)
        {
            case (int)UnitType.UNIT_TEAM.TEAM_ONE:
                red_score += score;
                team_score = red_score;
                break;
            case (int)UnitType.UNIT_TEAM.TEAM_TWO:
                blue_score += score;
                team_score = blue_score;
                break;
        }

        //data: team, new_score
        string dataSent = $"{team}/{team_score}";

        // Update other clients
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(RaiseEvents.UPDATETEAMSCORE, dataSent, raiseEventOptions, SendOptions.SendReliable);
    }
    public void UpdateTeamScore(string data)
    {
        string[] dataSplit = data.Split("/");
        int team = int.Parse(dataSplit[0]);
        int team_score = int.Parse(dataSplit[1]);

        switch ((UnitType.UNIT_TEAM)team)
        {
            case UnitType.UNIT_TEAM.TEAM_ONE:
                red_score = team_score;
                red_score_tmp.text = $"{team_score}/{score_max}";

                if (red_score >= score_max)
                {
                    red_score = score_max;
                    win_text.text = "RED TEAM WINS";
                    WinGame();
                }
                break;
            case UnitType.UNIT_TEAM.TEAM_TWO:
                blue_score = team_score;
                blue_score_tmp.text = $"{team_score}/{score_max}";

                if (blue_score >= score_max)
                {
                    blue_score = score_max;
                    win_text.text = "BLUE TEAM WINS";
                    WinGame();
                }
                break;
        }
    }
    void WinGame()
    {
        win_window.SetActive(true);
        Invoke(nameof(SceneBack), 5.0f);
    }
    void SceneBack()
    {
        SceneManager.LoadScene("Scenes/Menu");
    }

    private void SetPlayerTeams()
    {
        int team1_member_count = 0, team2_member_count = 0;

        foreach (Player p in playerList)
        {
            //Balance the teams, but prioritising team1
            UnitType.UNIT_TEAM team_type = (team1_member_count <= team2_member_count)? UnitType.UNIT_TEAM.TEAM_ONE: UnitType.UNIT_TEAM.TEAM_TWO;
            switch (team_type)
            {
                case UnitType.UNIT_TEAM.TEAM_ONE:
                    ++team1_member_count;
                    break;
                case UnitType.UNIT_TEAM.TEAM_TWO:
                    ++team2_member_count;
                    break;
            }

            Hashtable props = new Hashtable
            {
                {JLGame.PLAYER_TEAM, (int)team_type}
            };
            p.SetCustomProperties(props);
        }
    }

    private void CheckEndOfGame()
    {
    }

    private void OnCountdownTimerIsExpired()
    {
        Debug.Log("HAHA HGAME START LOAD");

        StartGame();
        start_time = Time.time;
    }
    // Update is called once per frame
    void Update()
    {
        //if (!started)
        //    return;

        //InfoText.text = ""+(Time.time - start_time);
    }
}
