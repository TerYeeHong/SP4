using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyPlayerCanvas : MonoBehaviour
{
    [SerializeField] private Canvas playerCanvas;
    private PhotonView photonView;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            playerCanvas.enabled = true;
        }
        else
        {
            playerCanvas.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
