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

    [SerializeField] public Camera startCam;
    public List<SkyPlayerController> playerControllers = new List<SkyPlayerController>();
    [SerializeField] private List<SkyPedestal> pedestals;
    [SerializeField] private GameObject pedestalPrefab; // Assign in the inspector
    [SerializeField] private int numberOfPedestalsToSpawn;

    [SerializeField] private AudioClip winSFX;
    [SerializeField] private AudioClip loseSFX;

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
    public void CheckPedestals()
    {
        foreach (SkyPedestal pedestal in pedestals)
        {
            if (!pedestal.isOccupied)
                return; 
        }

        // If all pedestals are occupied
        ProceedToNextFloor();
    }

    private void ProceedToNextFloor()
    {
        // Logic to proceed to the next floor
        Debug.Log("Proceeding to the next floor...");
        FloorClear();
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    ActivateLoseScreen();
        //}
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    ActivateWinScreen();
        //}
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DeactivateScreens();
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
            GameEvents.m_instance.playNewAudioClip.Invoke(loseSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
            ActivateLoseScreen();
        }
    }
    public void SpawnPedestalsOnIsland(Island island)
    {
        numberOfPedestalsToSpawn = PhotonNetwork.PlayerList.Length;
        for (int i = 0; i < numberOfPedestalsToSpawn; i++)
        {
            // Assuming island.island_grid is accessible and contains grid points
            if (island.island_grid.Count > 0)
            {
                int randomIndex = Random.Range(0, island.island_grid.Count);
                Grid selectedGrid = island.island_grid[randomIndex];

                // Calculate the spawn position based on the selected grid
                Vector3 spawnPosition = new Vector3(selectedGrid.x, 1, selectedGrid.y); // Adjust Y position as needed

                // Instantiate the pedestal prefab at the spawn position
                var newPedestal = Instantiate(pedestalPrefab, spawnPosition, pedestalPrefab.transform.rotation);

                // Assuming SkyPedestal is a component of the pedestal prefab
                SkyPedestal pedestalComponent = newPedestal.GetComponent<SkyPedestal>();
                if (pedestalComponent != null)
                {
                    pedestals.Add(pedestalComponent);
                }
            }
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
        GameEvents.m_instance.playNewAudioClip.Invoke(winSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
        ActivateWinScreen();
    }

    private void RestartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SkyGlobalStuffs.currentIsland = 0;
            startCam.gameObject.SetActive(true);
            LevelGenerator.m_instance.RaiseEventGenerateLevel();
        }
    }

    public void TPToSpawn()
    {
        List<Grid> spawnPos = LevelGenerator.m_instance.SpawnSpoints;
        int random_index = 0;
        Vector3 position = new Vector3(spawnPos[random_index].x, 3.0f, spawnPos[random_index].y);
        Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

        foreach (SkyPlayerController player in playerControllers)
        {
            player.transform.position = position;
            player.transform.rotation = rotation;
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
