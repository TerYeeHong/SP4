using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class SpawnMonument : MonoBehaviour
{
    private List<Grid> island_grids = new(); // Assuming this is populated with the islands where enemies can spawn
    private int spawnAmount;
    private bool isFinal;
    private bool isInteractable = true;
    private List<EnemyUnit> spawnedEnemies = new List<EnemyUnit>();

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
            for (int i = 0; i < spawnAmount; ++i)
            {
                int odds = Random.Range(0, 100); //0-99
                //Chance to spawn lizards
                if (odds < 0)
                {
                    // Spawn wolves equal to spawnAmount on a random island
                    StartCoroutine(StartSpawningLizard(spawnAmount, 1.0f));
                }
                else
                {
                    // Spawn wolves equal to spawnAmount on a random island
                    StartCoroutine(StartSpawningFish(spawnAmount, 1.0f));
                }
            }


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
    IEnumerator StartSpawningLizard(int amount, float delay)
    {
        WaitForSeconds delayTime = new WaitForSeconds(delay);

        for (int i = 0; i < amount; i++)
        {
            Vector3 position = ChooseRandomIslandPosition();
            GameObject fish = SpawnLizard(position);

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
        enemyUnit.SetDefaultStat();
        enemyUnit.isDead = false;
        enemy.transform.position = position;

        spawnedEnemies.Add(enemyUnit); // Add the enemy to the list

        return enemy;
    }

    GameObject SpawnWolf(Vector3 position)
    {
        GameObject enemy = MobManager.m_instance.FetchEnemy(EnemyUnitType.ENEMY_RACE.WOLF);
        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
        enemyUnit.photonView.RPC(nameof(enemyUnit.SetActive), RpcTarget.All, true);
        enemyUnit.SetDefaultStat();
        enemyUnit.isDead = false;
        enemy.transform.position = position;

        spawnedEnemies.Add(enemyUnit); // Add the enemy to the list

        return enemy;
    }

    GameObject SpawnLizard(Vector3 position)
    {
        GameObject enemy = MobManager.m_instance.FetchEnemy(EnemyUnitType.ENEMY_RACE.LIZARD);
        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
        enemyUnit.photonView.RPC(nameof(enemyUnit.SetActive), RpcTarget.All, true);
        enemyUnit.SetDefaultStat();
        enemyUnit.isDead = false;
        enemy.transform.position = position;

        spawnedEnemies.Add(enemyUnit); // Add the enemy to the list

        return enemy;
    }

    private void CheckForLevelCompletion()
    {
        // Remove any null references from the list in case enemies were destroyed
        spawnedEnemies.RemoveAll(enemy => enemy == null);

        // Check if all enemies in the list are dead
        if (spawnedEnemies.Count > 0 && spawnedEnemies.All(enemy => enemy.isDead))
        {
            gameObject.SetActive(false);
            // All enemies are dead, proceed to next level or trigger victory
            ProceedToNextLevel();
        }
    }

    private void Update()
    {
        CheckForLevelCompletion();
    }

    private void ProceedToNextLevel()
    {
        // Example: Load the next level scene
        // SceneManager.LoadScene("NextLevelSceneName");

        // Or simply inform the player and unlock the next level
        Debug.Log("All enemies defeated. Proceed to the next level.");
    }

    void MakeMonumentDisappear()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
}
