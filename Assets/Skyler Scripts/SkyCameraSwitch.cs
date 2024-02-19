using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SkyCameraSwitcher : MonoBehaviour
{
    // Assuming each player has their own set of cameras, 
    // this could be a list of lists or a dictionary if you prefer
    private Dictionary<int, List<Camera>> playerCameras = new Dictionary<int, List<Camera>>();
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        AddCameraToPlayer(photonView.ViewID, GetComponentInChildren<Camera>());
    }
    [PunRPC]
    public void AddCameraToPlayerRPC(int playerID, Camera camera)
    {
        if (!playerCameras.ContainsKey(playerID))
        {
            playerCameras[playerID] = new List<Camera>();
        }

        playerCameras[playerID].Add(camera);
    }

    public void AddCameraToPlayer(int playerID, Camera camera)
    {
        // Call this method locally
        AddCameraToPlayerRPC(playerID, camera);

        // And then call it on all other clients
        photonView.RPC("AddCameraToPlayerRPC", RpcTarget.Others, playerID, camera.GetComponent<PhotonView>().ViewID);
    }

    void Update()
    {
        // Ensure we're only executing this code for the local player
        if (photonView != null && photonView.IsMine)
        {
            CheckForCameraSwitchInput();
        }
    }

    private void CheckForCameraSwitchInput()
    {
        // Loop through number keys to check for input
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                SwitchToCamera(i - 1); // Adjust for zero-based indexing
                break; // Exit loop once a key is detected to avoid multiple switches
            }
        }
    }

    private void SwitchToCamera(int cameraIndex)
    {
        if (playerCameras.ContainsKey(photonView.OwnerActorNr)) // Check if there are cameras for this player
        {
            var cameras = playerCameras[photonView.OwnerActorNr];
            if (cameraIndex < cameras.Count) // Check if the index is within range
            {
                for (int i = 0; i < cameras.Count; i++)
                {
                    cameras[i].enabled = (i == cameraIndex); // Enable the selected camera, disable others
                    if(cameras[i].TryGetComponent(out AudioListener audioListener))
                    {
                        audioListener.enabled = (i == cameraIndex); // Enable the selected camera's AudioListener, disable others
                    }
                }
            }
        }
    }
}