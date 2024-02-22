using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SkyWinManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject winSceen;

    private void Update()
    {
        // Example key inputs for testing
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Manually activate lose screen for testing
            loseScreen.SetActive(true);
            winSceen.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Manually deactivate lose screen for testing
            loseScreen.SetActive(false);
            winSceen.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Manually activate lose screen for testing
            winSceen.SetActive(true);
            loseScreen.SetActive(false);
        }

        //if (CheckAllPlayersDead())
        //{
        //    loseScreen.SetActive(true);
        //}
    }

    private bool CheckAllPlayersDead()
    {
        foreach (GameObject player in JLGameManager.Instance.player_list)
        {
            if (!player.GetComponent<SkyPlayerHealth>().isDead)
                return false;
        }

        return true;
    }
}
