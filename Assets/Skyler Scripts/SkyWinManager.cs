using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SkyWinManager : MonoBehaviour
{
    public static SkyWinManager instance { get; private set; }

    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject winSceen;

    [SerializeField] private Camera startCam;
    public List<SkyPlayerController> playerControllers = new List<SkyPlayerController>();

    private void Awake()
    {
        // Check if instance already exists and if it's not this one, destroy this instance (Singleton pattern)
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivateLoseScreen();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DeactivateScreens();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ActivateWinScreen();
        }
    }

    public void ActivateLoseScreen()
    {
        loseScreen.SetActive(true);
        winSceen.SetActive(false);
    }

    public void ActivateWinScreen()
    {
        winSceen.SetActive(true);
        loseScreen.SetActive(false);
    }

    public void DeactivateScreens()
    {
        loseScreen.SetActive(false);
        winSceen.SetActive(false);
    }

    public void CheckLose()
    {
        if (CheckAllPlayersDead())
        {
            ActivateLoseScreen();
        }
    }

    public void FloorClear()
    {
        SkyGlobalStuffs.floorsCleared += 1;
        CheckWin();

        if (!CheckWin())
        {
            RestartGame();
        }
        else
        {
            OnWin();
        }
    }

    public void OnWin()
    {
        ActivateWinScreen();
    }

    private void RestartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startCam.gameObject.SetActive(true);
            LevelGenerator.m_instance.RaiseEventGenerateLevel();
        }
    }

    private bool CheckWin()
    {
        if (SkyGlobalStuffs.floorsCleared > SkyGlobalStuffs.maxFloors)
        {
            return true;
        }

        return false;
    }

    private bool CheckAllPlayersDead()
    {
        foreach (SkyPlayerController player in playerControllers)
        {
            if (!player.playerHealth.isDead)
                return false;
        }

        return true;
    }
}
