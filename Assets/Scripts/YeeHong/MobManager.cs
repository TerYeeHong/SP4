using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class MobManager : MonoBehaviour
{
    public static MobManager m_instance = null;
    private void Awake()
    {
        if (m_instance == null) 
            m_instance = this;
    }

    List<GameObject> enemy_list = new List<GameObject>(); //only the master client will spawn, only the master client should access this
    List<int> view_id_list = new List<int>();


    public GameObject FetchEnemy(EnemyUnitType.ENEMY_RACE enemy_race)
    {
        if (!PhotonNetwork.IsMasterClient)
            return null;

        foreach (GameObject enemy in enemy_list)
        {
            //Only use inactive enemies
            if (enemy.activeSelf)
                continue;

            //Make sure same race
            EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
            if (enemyUnit.EnemyRace == enemy_race)
            {
                enemy.GetComponent<NavMeshAgent>().enabled = false;
                return enemy;
            }
        }
        //cant find, so instantiate new one
        for (int i = 0; i < 10; ++i)
        {
            string prefab_name = GetPrefabName(enemy_race);
            if (prefab_name != "")
            {
                GameObject enemy = PhotonNetwork.InstantiateRoomObject(prefab_name, Vector3.zero, Quaternion.identity);
                EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
                enemyUnit.photonView.RPC(nameof(enemyUnit.SetActive), RpcTarget.All, false); //set to inactive at the start
                enemy_list.Add(enemy);
            }
            else
                return null;
        }
        return FetchEnemy(enemy_race);
    }
    //public GameObject FetchEnemy(EnemyUnitType.ENEMY_RACE enemy_race, Vector3 position)
    //{
    //    if (!PhotonNetwork.IsMasterClient)
    //        return null;

    //    foreach (GameObject enemy in enemy_list)
    //    {
    //        //Only use inactive enemies
    //        if (enemy.activeSelf)
    //            continue;

    //        //Make sure same race
    //        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
    //        if (enemyUnit.EnemyRace == enemy_race)
    //        {
    //            return enemy;
    //        }
    //    }
    //    //cant find, so instantiate new one
    //    for (int i = 0; i < 10; ++i)
    //    {
    //        string prefab_name = GetPrefabName(enemy_race);
    //        if (prefab_name != "")
    //        {
    //            GameObject enemy = PhotonNetwork.InstantiateRoomObject(prefab_name, position, Quaternion.identity);
    //            EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
    //            enemyUnit.photonView.RPC(nameof(enemyUnit.SetActive), RpcTarget.All, false); //set to inactive at the start
    //            enemy_list.Add(enemy);
    //        }
    //        else
    //            return null;
    //    }
    //    return FetchEnemy(enemy_race);
    //}
    //public int FetchEnemy(EnemyUnitType.ENEMY_RACE enemy_race)
    //{
    //    if (!PhotonNetwork.IsMasterClient)
    //        return -1;

    //    foreach (GameObject enemy in enemy_list)
    //    {
    //        //Only use inactive enemies
    //        if (enemy.activeSelf)
    //            continue;

    //        //Make sure same race
    //        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
    //        if (enemyUnit.EnemyRace == enemy_race)
    //        {
    //            return enemyUnit.photonView.ViewID;
    //            //return enemy;
    //        }
    //    }
    //    //cant find, so instantiate new one
    //    for (int i = 0; i < 10; ++i)
    //    {
    //        string prefab_name = GetPrefabName(enemy_race);
    //        if (prefab_name != "")
    //        {
    //            GameObject enemy = PhotonNetwork.InstantiateRoomObject(prefab_name, Vector3.zero, Quaternion.identity);
    //            EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
    //            enemyUnit.photonView.RPC(nameof(enemyUnit.SetActive), RpcTarget.All, false); //set to inactive at the start
    //            enemy_list.Add(enemy);
    //        }
    //        else
    //            return -1;
    //    }
    //    return FetchEnemy(enemy_race);
    //}

    public void SpawnEnemy(EnemyUnitType.ENEMY_RACE enemy_race)
    {
        GameObject enemy = PhotonNetwork.InstantiateRoomObject(GetPrefabName(enemy_race), Vector3.zero, Quaternion.identity);
    }


    string GetPrefabName(EnemyUnitType.ENEMY_RACE enemy_race)
    {
        switch (enemy_race)
        {
            case EnemyUnitType.ENEMY_RACE.FISH:
                return "CatfishA";
            case EnemyUnitType.ENEMY_RACE.WOLF:
                return "Wolfboss_A";
        }

        return "";
    }

}

