using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnMonument : MonoBehaviour
{
    public List<Grid> island_grids; // Assuming this is populated with the islands where enemies can spawn
    public int spawnAmount;
    public bool isFinal;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && PhotonNetwork.IsMasterClient)
        {
            StartSpawning();
            MakeMonumentDisappear();
        }
    }

    private void StartSpawning()
    {
        if (isFinal)
        {
            // Spawn a boss
            Vector3 bossPosition = ChooseRandomIslandPosition();
            SpawnWolf(bossPosition);
        }
        else
        {
            // Spawn wolves equal to spawnAmount on a random island
            StartCoroutine(StartSpawningWolves(spawnAmount, 1.0f));
        }
    }

    IEnumerator StartSpawningWolves(int amount, float delay)
    {
        WaitForSeconds delayTime = new WaitForSeconds(delay);

        for (int i = 0; i < amount; i++)
        {
            Vector3 wolfPosition = ChooseRandomIslandPosition();
            SpawnWolf(wolfPosition);
            yield return delayTime;
        }
    }

    Vector3 ChooseRandomIslandPosition()
    {
        // Assuming Grid class has a method or properties to get a position
        Grid randomGrid = island_grids[Random.Range(0, island_grids.Count)];
        // Adjust Y coordinate as needed, and ensure it's correct for your game's coordinate system
        return new Vector3(randomGrid.x, 0, randomGrid.y);
    }

    void SpawnFish(Vector3 position)
    {
        GameObject enemy = MobManager.m_instance.FetchEnemy(EnemyUnitType.ENEMY_RACE.FISH);
        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();

        enemyUnit.photonView.RPC(nameof(enemyUnit.SetActive), RpcTarget.All, true);
        enemy.transform.position = position;
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
