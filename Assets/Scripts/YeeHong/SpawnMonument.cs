using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnMonument : MonoBehaviour
{
    private List<Grid> island_grids = new(); // Assuming this is populated with the islands where enemies can spawn
    private int spawnAmount;
    private bool isFinal;
    private bool isInteractable = true;

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && PhotonNetwork.IsMasterClient && isInteractable)
        {
            isInteractable = false;
            StartSpawning();
        }
    }

    public void InitMonument(int islandNum)
    {
        island_grids.Clear();
        print("Init monument at  : " + islandNum);   
        island_grids = LevelGenerator.m_instance.islands_list[islandNum].island_grid;
        spawnAmount = islandNum + Random.Range(3, 10 + islandNum);
        isFinal = LevelGenerator.m_instance.islands_list[islandNum].Equals(LevelGenerator.m_instance.islands_list[LevelGenerator.m_instance.islands_list.Count - 1]);
    }

    private void StartSpawning()
    {
        if (isFinal)
        {
            // Spawn a boss
            Vector3 bossPosition = ChooseRandomIslandPosition();
            SpawnWolf(bossPosition);
            Invoke(nameof(MakeMonumentDisappear), 1.0f);
        }
        else
        {
            // Spawn wolves equal to spawnAmount on a random island
            StartCoroutine(StartSpawningFish(spawnAmount, 1.0f));
        }
    }

    IEnumerator StartSpawningFish(int amount, float delay)
    {
        WaitForSeconds delayTime = new WaitForSeconds(delay);

        for (int i = 0; i < amount; i++)
        {
            Vector3 position = ChooseRandomIslandPosition();
            GameObject fish = SpawnFish(position);
            yield return delayTime;
        }

        // After spawning all fish, make the monument disappear
        MakeMonumentDisappear();
    }


    Vector3 ChooseRandomIslandPosition()
    {
        // Assuming Grid class has a method or properties to get a position
        Grid randomGrid = island_grids[Random.Range(0, island_grids.Count)];
        // Adjust Y coordinate as needed, and ensure it's correct for your game's coordinate system
        return new Vector3(randomGrid.x, 3, randomGrid.y);
    }

    GameObject SpawnFish(Vector3 position)
    {
        GameObject enemy = MobManager.m_instance.FetchEnemy(EnemyUnitType.ENEMY_RACE.FISH);
        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();

        enemyUnit.photonView.RPC(nameof(enemyUnit.SetActive), RpcTarget.All, true);
        enemy.transform.position = position;
        return enemy;
    }

    void SpawnWolf(Vector3 position)
    {
        GameObject enemy = MobManager.m_instance.FetchEnemy(EnemyUnitType.ENEMY_RACE.WOLF);
        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();

        enemyUnit.photonView.RPC(nameof(enemyUnit.SetActive), RpcTarget.All, true);
        enemy.transform.position = position;
    }

    void MakeMonumentDisappear()
    {
        // Option 1: Deactivate the GameObject (can be reactivated later)
        gameObject.SetActive(false);

        // Option 2: Destroy the GameObject (permanent)
        // Destroy(gameObject);
    }
}
