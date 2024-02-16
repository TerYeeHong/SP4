using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class SpawnMonument : MonoBehaviour
{
    bool canStart = false;


    private void Update()
    {
        if (!canStart)
            return;

        if (Input.GetButtonDown("Jump"))
        {
            //SPAWN ENEMIES
            Debug.LogWarning("STA");

            StartCoroutine(StartSpawningEnemies(10, 3.0f));
            //Disable self
            canStart = false;
            //gameObject.SetActive(false);
            GetComponent<Renderer>().enabled = false;
            GameEvents.m_instance.updateCanStartSpawnEnable.Invoke(false);
        }
    }


    IEnumerator StartSpawningEnemies(int amount, float delay_interval = 3.0f)
    {
        Debug.LogWarning("ETHAN");

        WaitForSeconds delay = new WaitForSeconds(delay_interval);
        
        for (int i = 0; i < amount; ++i)
        {
            Vector3 position = transform.position 
                + new Vector3(Random.Range(-transform.localScale.x, transform.localScale.x), 
                3, Random.Range(-transform.localScale.z, transform.localScale.z));

            SpawnEnemy(position);

            yield return delay;
        }
    }

    void SpawnEnemy(Vector3 position)
    {
        
        EnemyManager.m_instance.SpawnFromPool("Fish", position, Quaternion.identity);

    }



    private void OnTriggerEnter(Collider other)
    {
        //Only updates if this is the master client
        if (!PhotonNetwork.IsMasterClient)
            return;

        //Check the player that is in trigger is also master client
        if (other.TryGetComponent(out SkyPlayerController skyPlayerController))
        {
            if (skyPlayerController.GetPhotonView.Owner.IsMasterClient)
            {
                GameEvents.m_instance.updateCanStartSpawnEnable.Invoke(true);
                canStart = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //Only updates if this is the master client
        if (!PhotonNetwork.IsMasterClient)
            return;

        //Check the player that is in trigger is also master client
        if (other.TryGetComponent(out SkyPlayerController skyPlayerController))
        {
            if (skyPlayerController.GetPhotonView.Owner.IsMasterClient)
            {
                GameEvents.m_instance.updateCanStartSpawnEnable.Invoke(false);
                canStart = false;

            }
        }
    }
}
