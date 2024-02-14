using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyPlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    SkyPlayerMovement playerMovement;
    private PhotonView photonView;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerMovement = GetComponent<SkyPlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            playerMovement.ResetJump();
            playerMovement.HandleJump();
            playerMovement.UpdateAnimationState();
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            playerMovement.HandleMovement();
        }
    }
}
