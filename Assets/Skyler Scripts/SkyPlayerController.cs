using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SkyPlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    SkyPlayerMovement playerMovement;
    SkyPlayerShooting playerShooting;
    SkyPlayerGunSelector gunSelector;
    private PhotonView photonView;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerMovement = GetComponent<SkyPlayerMovement>();
        playerShooting = GetComponent<SkyPlayerShooting>();
        gunSelector = GetComponent<SkyPlayerGunSelector>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            playerMovement.ResetJump();
            playerMovement.HandleJump();
            playerMovement.HandleDash();
            playerMovement.UpdateAnimationState();
            playerShooting.ShootGun(photonView, gunSelector.activeGun);
            gunSelector.ADSActiveGun();
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
