using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SkyPlayerGunSelector : MonoBehaviour
{
    [Header("References for Status Checks")]
    [SerializeField] Status weapon_master;
    int weapon_master_count;

    [SerializeField]
    private Transform gunParent;
    [SerializeField]
    private List<GameObject> gunPrefabs; // Store gun prefabs here
    [SerializeField]
    private List<GameObject> gunPrefabsList; // Store gun prefabs here
    public GameObject sniperScope;

    [Space]
    [Header("Runtime Filled")]
    public GameObject activeGunGameObject; // Holds the active gun GameObject
    public SkyGun activeGun; // Holds the SkyGun component of the active gun
    private void Start()
    {
        // Initialize with the first gun as the default active gun
        SwitchGunByIndex(0);

        StatusCheckAll();
    }

    private void OnEnable()
    {
        GameEvents.m_instance.onStatusChange.AddListener(StatusCheckAll);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.onStatusChange.RemoveListener(StatusCheckAll);
    }

    public void Update()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            SwitchGun();
        }
    }

    public void StatusCheckAll()
    {
        weapon_master_count = PFGlobalData.GetBlessingCount(weapon_master.Name_status) + 1;


        //gunPrefabs.Clear();
        //for (int i = 0; i < weapon_master_count; ++i)
        //{
        //    if (i >= gunPrefabsList.Count)
        //        break;
        //    gunPrefabs.Add(gunPrefabsList[i]);
        //}

    }

    public void SwitchGun()
    {
        // Example input handling for switching guns using number keys 1-9
        for (int i = 0; i < gunPrefabs.Count; i++)
        {
            if (i >= weapon_master_count)
                break;

            if (Input.GetKeyDown((i + 1).ToString()))
            {
                print("SWITHINC GUN");
                SwitchGunByIndex(i);
                break; // Prevent multiple switches in one frame
            }
        }
    }

    public void SwitchGunByIndex(int index)
    {
        if (index < 0 || index >= gunPrefabs.Count)
        {
            Debug.LogError($"Gun index out of range: {index}");
            return;
        }

        // Destroy the current gun GameObject, if any
        if (activeGunGameObject != null)
        {
            Destroy(activeGunGameObject);
        }

        SwitchToNewGun(index);

        RaiseGunSwitchEvent(index);

        // Log for debugging
        Debug.Log($"Switched to gun: {activeGun.Name}");
    }

    public void SwitchToNewGun(int index)
    {

        if (activeGunGameObject != null)
        {
            Destroy(activeGunGameObject);
        }


        GameObject gunPrefab = gunPrefabs[index];
        GameObject instantiatedGun = Instantiate(gunPrefab, gunParent.position, gunParent.rotation, gunParent);
        activeGunGameObject = instantiatedGun;
        activeGun = instantiatedGun.GetComponent<SkyGun>();
        instantiatedGun.transform.localPosition = activeGun.SpawnPoint;
        instantiatedGun.transform.localRotation = Quaternion.Euler(activeGun.SpawnRotation);
    }

    private void RaiseGunSwitchEvent(int gunIndex)
    {

        int viewID = GetComponent<PhotonView>().ViewID;
        object[] content = { viewID, gunIndex }; // You can include more information as needed
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(RaiseEvents.PLAYER_SWITCH_GUN, content, raiseEventOptions, sendOptions);
    }

    public void ADSActiveGun()
    {
        if (Input.GetButton("Fire2"))
        {
            print("BEN");
            activeGun.PlayerAim(true, GetComponent<PhotonView>());
            if (activeGun.Type == SkylerGunType.Sniper)
            {
                sniperScope.SetActive(true);
                activeGun.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        if (Input.GetButtonUp("Fire2"))
        {
            print("BNE");
            activeGun.PlayerAim(false, GetComponent<PhotonView>());
            if (activeGun.Type == SkylerGunType.Sniper)
            {
                sniperScope.SetActive(false);
                activeGun.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }
}
