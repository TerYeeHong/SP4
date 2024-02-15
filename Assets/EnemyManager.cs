using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    List<GameObject> Enemies = new List<GameObject>();
    [SerializeField] public PhotonView photonView;

    [System.Serializable]
    public class Pool
    {
        public string tag;
      //  public GameObject prefab;
        public int size;
    }

    Queue<GameObject> objectPool = new Queue<GameObject>();
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    
    private void OnEnable()
    {
        RaiseEvents.OnEnemyDieEvent += DisableEnemies;
        RaiseEvents.OnEnemySpawnEvent += InstantiateEnemy;
        RaiseEvents.SetActiveEvent += ActivateFish;
    }

    private void OnDisable()
    {

        RaiseEvents.OnEnemyDieEvent -= DisableEnemies;
        RaiseEvents.OnEnemySpawnEvent -= InstantiateEnemy;
        RaiseEvents.SetActiveEvent -= ActivateFish;
    }


    // Start is called before the first frame update
    void Start()
    {
        //InstantiateEnemy("Fish");
        //Enemies[1].SetActive(false);
        //FetchEnemies();
        //DisableEnemies(string data)
        //InstantiateEnemy("Fish");
        string sentData = "" + tag;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(RaiseEvents.ENEMYSPAWNEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        //FetchEnemies();

        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    //FetchEnemies();
        //    Vector3 spawnPos = new Vector3(0, 0, 0);

        //    //SpawnFromPool("Fish", spawnPos, Quaternion.identity);

        //    //photonView.RPC("SpawnFromPool", RpcTarget.MasterClient, "Fish", spawnPos , Quaternion.identity);
        //    photonView.RPC("InstantiateEnemy", RpcTarget.All, "Fish");
        //}
        if (Input.GetKeyDown(KeyCode.N))
        {

            Vector3 spawnPos = new Vector3(0, 0, 0);
            //SpawnFromPool("Fish", spawnPos, Quaternion.identity);
            // InstantiateEnemy("Fish");
            //string sentData = "" + tag;
            //RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            //PhotonNetwork.RaiseEvent(RaiseEvents.ENEMYSPAWNEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
            
            photonView.RPC("SpawnFromPool", RpcTarget.All, "Fish", spawnPos, Quaternion.identity);
        }
    }

    public void FetchEnemies()
    {
        Vector3[] positionArray = new[] { new Vector3(-5.3f, 0f, -3.1f),
                                            new Vector3(1.5f, 0f, 4f),
                                            new Vector3(3.2f, 0f, 6.5f),
                                            new Vector3(7.4f, 0f, -3f)};

        for (int i = 0; i < Enemies.Count; i++)
        {
            int photonViewNo;
            photonViewNo = Enemies[i].GetComponent<PhotonView>().ViewID;

            //if inactive
            if (Enemies[i].activeSelf == false)
            {
                //Debug.Log("Enemies" + photonViewNo);
                //set active
                Enemies[i].SetActive(true);

                for (int p = 0; p < positionArray.Length; ++p)
                {
                    Enemies[i].transform.position = positionArray[p];
                }
            }
            else
            {
                // Create a new enemy
                GameObject obj = PhotonNetwork.InstantiateRoomObject("CatFishA", positionArray[i], Quaternion.identity);
                Enemies.Add(obj);
            }
        }

        ////loop through the array position
        //for (int i = 0; i < positionArray.Length; ++i)
        //{
        //    // Create a ghost prefab
        //    GameObject obj = PhotonNetwork.InstantiateRoomObject("CatFishA", positionArray[i], Quaternion.identity);
        //    Enemies.Add(obj);
        //}


        //movePositionTransform = player.transform;
        //for (int i = 0; i < Enemies[i].Length; i++)
        //{

        //}


    }

    [PunRPC]
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if(!poolDictionary.ContainsKey(tag))
        {
            Debug.Log("Pool with tag" + tag + " does not exist");
            return null;
        }

        Debug.Log(objectPool.Count);
        //if (poolDictionary[tag].Dequeue() == null)
        //{
        //    //Debug.Log(poolDictionary[tag].Dequeue());
        //    SpawnEnemy();
        //}
        if (objectPool.Count == 0)
        {
            InstantiateEnemy("Fish");
            return null;
        }
        else
        {


            GameObject objectToSpawn = poolDictionary[tag].Dequeue();
            int fishViewID = objectToSpawn.GetComponent<PhotonView>().ViewID;

            string sentData = "" + fishViewID;

            //Debug.Log("setactive");

            Debug.Log("fishViewID" + fishViewID);

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(RaiseEvents.SETACTIVEEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);

            //photonView.RPC("ActivateFish", RpcTarget.All, fishViewID);
            //objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;


            return objectToSpawn;
        }

    }

    
    [PunRPC]
    public void InstantiateEnemy(string tag)
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach ( Pool pool in pools)
        {
           // Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {

                //if (tag == "Fish")
                {

                    //obj.SetActive(false);

                    Vector3 spawnPos = new Vector3(0, 0, 0);
                    GameObject fish = PhotonNetwork.InstantiateRoomObject("CatFishA", spawnPos, Quaternion.identity);
                    Enemies.Add(fish);
                    int fishViewID = fish.GetComponent<PhotonView>().ViewID;


                    string sentData = "" + fishViewID;
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(RaiseEvents.SETINACTIVEEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
                   // photonView = GetComponent<PhotonView>();

                    //fish.SetActive(false);
                    objectPool.Enqueue(fish);

                }

            
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    //public void SpawnEnemy(string data)
    //{
    //    Vector3 spawnPos = new Vector3(0, 0, 0);
    //    GameObject obj = PhotonNetwork.InstantiateRoomObject("CatFishA", spawnPos, Quaternion.identity);
    //    obj.SetActive(false);
    //    objectPool.Enqueue(obj);
    //}

    public void DestroyEnemy()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                Vector3 spawnPos = new Vector3(0, 0, 0);
                GameObject obj = PhotonNetwork.InstantiateRoomObject("CatFishA", spawnPos, Quaternion.identity);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    
    public void DisableEnemies(string data)
    {
        Debug.Log("Disable Enemies");
        for (int i = 0; i < Enemies.Count; i++)
        {
            int photonViewNo;
            photonViewNo = Enemies[i].GetComponent<PhotonView>().ViewID;

            foreach (Pool pool in pools)
            {
                if (data == "" + photonViewNo)
                {
                    Debug.Log("Enemies" + photonViewNo);

                    Enemies[i].SetActive(false);
                    //poolDictionary.Add(Enemies[i].tag, objectPool);
                    objectPool.Enqueue(Enemies[i]);

                }
            }
        }


    }

    [PunRPC]
    public void ActivateFish(string data)
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            int photonViewNo;
            photonViewNo = Enemies[i].GetComponent<PhotonView>().ViewID;
            EnemyController EnemyController = Enemies[i].GetComponent<EnemyController>();

            if (data == "" + photonViewNo)
            {
                Debug.Log("activate" + photonViewNo);

                Enemies[i].SetActive(true);
                EnemyController.enabled = true;

            }
        }

    }
}
