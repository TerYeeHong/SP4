using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SkyPlayerGunSelector : MonoBehaviour
{
    [SerializeField]
    private Transform gunParent;
    [SerializeField]
    private List<GameObject> gunPrefabs; // Store gun prefabs here

    [Space]
    [Header("Runtime Filled")]
    public GameObject activeGunGameObject; // Holds the active gun GameObject
    public SkyGun activeGun; // Holds the SkyGun component of the active gun

    private void Start()
    {
        // Initialize with the first gun as the default active gun
        SwitchGunByIndex(0);
    }

    public void Update()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            SwitchGun();
        }
    }

    public void SwitchGun()
    {
        // Example input handling for switching guns using number keys 1-9
        for (int i = 0; i < gunPrefabs.Count; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
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

        // Instantiate the new gun prefab
        GameObject gunPrefab = gunPrefabs[index];
        GameObject instantiatedGun = Instantiate(gunPrefab, gunParent.position, gunParent.rotation, gunParent);
        activeGunGameObject = instantiatedGun;
        activeGun = instantiatedGun.GetComponent<SkyGun>();
        instantiatedGun.transform.localPosition = activeGun.SpawnPoint;
        instantiatedGun.transform.localRotation = Quaternion.Euler(activeGun.SpawnRotation);

        RaiseGunSwitchEvent(index);

        // Log for debugging
        Debug.Log($"Switched to gun: {activeGun.Name}");
    }

    private void RaiseGunSwitchEvent(int gunIndex)
    {

        int viewID = GetComponent<PhotonView>().ViewID;
        object[] content = { viewID, gunIndex }; // You can include more information as needed
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(RaiseEvents.PLAYER_SWITCH_GUN, content, raiseEventOptions, sendOptions);
    }
}
