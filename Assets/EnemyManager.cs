using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager m_instance = null;
    private void Awake()
    {
        if (m_instance = null) m_instance = this;
    }


    List<GameObject> Enemies = new List<GameObject>();
    List<int> ViewID = new List<int>();
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
       // FetchEnemy("Wolfboss_A");
        //DisableEnemies(string data)
        //InstantiateEnemy("Fish");

        if(photonView.IsMine)
        Invoke(nameof(SpawnEvent), 15);
        photonView = GetComponent<PhotonView>();
    }
    void SpawnEvent()
    {
        foreach (Pool pool in pools)
        {
            string tag = pool.tag;
            // Now you can use the 'tag' variable as needed
            Debug.Log("Pool tag: " + tag);

            string sentData = "" + tag;

            //Debug.Log("SPWN sentData: " + pools.);
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent(RaiseEvents.ENEMYSPAWNEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
        }
        //string sentData2 = "Wolf";
        //RaiseEventOptions raiseEventOptions2 = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        //PhotonNetwork.RaiseEvent(RaiseEvents.ENEMYSPAWNEVENT, sentData2, raiseEventOptions2, SendOptions.SendReliable);
    }

    // Update is called once per frame
    void Update()
    {
        //FetchEnemies();

        if (Input.GetKeyDown(KeyCode.P))
        {
            //FetchEnemies();
            Vector3 spawnPos = new Vector3(0, 0, 0);
            GameObject obj = PhotonNetwork.InstantiateRoomObject("Lizard_skin", spawnPos, Quaternion.identity);
            int fishViewID = obj.GetComponent<PhotonView>().ViewID;

            string sentData = "" + fishViewID;

            //Debug.Log("setactive");

            Debug.Log("fishViewID" + fishViewID);
            //SpawnFromPool("Fish", spawnPos, Quaternion.identity);

            //photonView.RPC("SpawnFromPool", RpcTarget.MasterClient, "Fish", spawnPos , Quaternion.identity);
            // photonView.RPC("InstantiateEnemy", RpcTarget.All, "Fish");
           // photonView.RPC("SetGameobjectInactive", RpcTarget.All, sentData);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            //FetchEnemies();
            Vector3 spawnPos = new Vector3(0, 0, 0);
            GameObject obj = PhotonNetwork.InstantiateRoomObject("Wolfboss_A", spawnPos, Quaternion.identity);
            int fishViewID = obj.GetComponent<PhotonView>().ViewID;

            string sentData = "" + fishViewID;

            Debug.Log("fishViewID" + fishViewID);
 
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            //FetchEnemies();
            Vector3 spawnPos = new Vector3(0, 0, 0);
            GameObject obj = PhotonNetwork.InstantiateRoomObject("CatfishA", spawnPos, Quaternion.identity);
            int fishViewID = obj.GetComponent<PhotonView>().ViewID;

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


    //GameObject* SceneMovement::FetchGO(GameObject::GAMEOBJECT_TYPE type)
    //{
    //    for (std::vector<GameObject*>::iterator it = m_goList.begin(); it != m_goList.end(); ++it)
    //    {
    //        //std::cout << "List of all Game Objects:" << std::endl;

    //        /*for (std::vector<GameObject*>::iterator it = m_goList.begin(); it != m_goList.end(); ++it)
    //        {
    //            GameObject* go = *it;
    //            std::cout << "Type: " << go->type << ", Active: " << (go->active ? "true" : "false") << std::endl;
    //        }*/

    //        GameObject* go = (GameObject*)*it;
    //        if (!go->active && go->type == type)
    //        {
    //            go->active = true;
    //            return go;
    //        }
    //    }
    //    for (unsigned i = 0; i < 5; ++i)
    //    {
    //        GameObject* go = new GameObject(type);
    //        m_goList.push_back(go);
    //        //week 5
    //        //assign the appropriate statemachine to the object
    //        go->sm = m_stateMachines[type];
    //    }
    //    return FetchGO(type);
    //}


    public int FetchEnemy(string tag)
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            for (int v = 0; v < ViewID.Count; v++)
            {
                //if (tag == Enemies[i].tag)
                //{
                    ViewID[v] = Enemies[i].GetComponent<PhotonView>().ViewID;
                    //PhotonNetwork.GetPhotonView(ViewID[v])
                    return ViewID[v];
                //}
            }
        }
        for (int i = 0; i < 5; ++i)
        {
            Vector3 spawnPos = new Vector3(0, 0, 0);
            GameObject obj = PhotonNetwork.InstantiateRoomObject(tag, spawnPos, Quaternion.identity);

            Enemies.Add(obj);
            if (obj.TryGetComponent(out EnemyUnit enemyUnit))
            {
               // enemyUnit.RPC
            }
            

        }
        return FetchEnemy(tag);
    }
    //public GameObject FetchEnemy(string tag)
    //{
    //    for (int i = 0; i < Enemies.Count; i++)
    //    {
    //        for (int v = 0; v < ViewID.Count; v++)
    //        {
    //            if (tag == Enemies[i].tag)
    //            {
    //                ViewID[v] = Enemies[i].GetComponent<PhotonView>().ViewID;
    //                //PhotonNetwork.GetPhotonView(ViewID[v])
    //                return ViewID[v];
    //            }
    //        }
    //    }
    //    for (int i = 0; i < 5; ++i)
    //    {
    //        Vector3 spawnPos = new Vector3(0, 0, 0);
    //        GameObject obj = PhotonNetwork.InstantiateRoomObject(tag, spawnPos, Quaternion.identity);

    //        Enemies.Add(obj);
    //        if (obj.TryGetComponent(out EnemyUnit enemyUnit))
    //        {
    //            // enemyUnit.RPC
    //        }


    //    }
    //    return FetchEnemy(tag);
    //}


    [PunRPC]
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
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

        //GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        //int fishViewID = objectToSpawn.GetComponent<PhotonView>().ViewID;

        //string sentData = "" + fishViewID;

        ////Debug.Log("setactive");

        //Debug.Log("fishViewID" + fishViewID);

        //RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        //PhotonNetwork.RaiseEvent(RaiseEvents.SETACTIVEEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);

        ////photonView.RPC("ActivateFish", RpcTarget.All, fishViewID);
        ////objectToSpawn.SetActive(true);
        //objectToSpawn.transform.position = position;
        //objectToSpawn.transform.rotation = rotation;


        //return objectToSpawn;

        //SPOMETHING TO DO WITH THIS
        //if instantiate is outisde the loop, it can run

        if (objectPool.Count == 0)
        {
            Debug.LogWarning("count 0");

            InstantiateEnemy("Fish");
            Vector3 spawnPos = new Vector3(0, 0, 0);

            //GameObject fish = PhotonNetwork.InstantiateRoomObject("CatFishA", spawnPos, Quaternion.identity);
            GameObject fish = PhotonNetwork.InstantiateRoomObject(tag, spawnPos, Quaternion.identity);
            //Vector3 spawnPos = new Vector3(0, 0, 0);

            return null;
        }
        else
        {
            Debug.LogWarning("have count");

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
    public void SetGameobjectInactive(string data)
    {
        

    }
    
    [PunRPC]
    public void InstantiateEnemy(string tag)
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        Debug.LogWarning("trying to instantitate");

        foreach ( Pool pool in pools)
        {
            // Queue<GameObject> objectPool = new Queue<GameObject>();
            Debug.LogWarning("each pool");

            for (int i = 0; i < pool.size; i++)
            {
                Debug.LogWarning("each spawnnnn");

                //if (tag == "Fish")
                {

                    //obj.SetActive(false);

                    Vector3 spawnPos = new Vector3(0, 0, 0);
                    GameObject fish = PhotonNetwork.InstantiateRoomObject(tag, spawnPos, Quaternion.identity);
                    Enemies.Add(fish);
                    int fishViewID = fish.GetComponent<PhotonView>().ViewID;


                    //string sentData = "" + fishViewID;
                   // RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                   // PhotonNetwork.RaiseEvent(RaiseEvents.SETINACTIVEEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
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

    //public void DestroyEnemy()
    //{
    //    poolDictionary = new Dictionary<string, Queue<GameObject>>();

    //    foreach (Pool pool in pools)
    //    {
    //        //Queue<GameObject> objectPool = new Queue<GameObject>();

    //        for (int i = 0; i < pool.size; i++)
    //        {
    //            Vector3 spawnPos = new Vector3(0, 0, 0);
    //            GameObject obj = PhotonNetwork.InstantiateRoomObject("CatFishA", spawnPos, Quaternion.identity);
    //            obj.SetActive(false);
    //            objectPool.Enqueue(obj);
    //        }

    //        poolDictionary.Add(pool.tag, objectPool);
    //    }
    //}

    
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

                    //Enemies[i].SetActive(false);
                    //poolDictionary.Add(Enemies[i].tag, objectPool);
                    objectPool.Enqueue(Enemies[i]);

                }
            }
        }


    }

   // [PunRPC]
    public void ActivateFish(string data)
    {
        //    Debug.Log("data" + data);
        //    int num = int.Parse(data);
        //    Enemies[num+ 1].SetActive(true);

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
