using UnityEngine;
using Photon.Pun;

public class SkyCameraSwitch : MonoBehaviour
{
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
    }

    public void CheckForCameraSwitchInput()
    {
        print("Attempting to switch");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                int targetPlayerID = PhotonNetwork.PlayerList[i].ActorNumber;
                SkyCameraManager.Instance.SwitchCamera(targetPlayerID);
                break;
            }
        }
    }
}
