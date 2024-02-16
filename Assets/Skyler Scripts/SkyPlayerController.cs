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

    public PhotonView GetPhotonView { get { return photonView; } }


    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerMovement = GetComponent<SkyPlayerMovement>();
        playerShooting = GetComponent<SkyPlayerShooting>();
        gunSelector = GetComponent<SkyPlayerGunSelector>();

        //disable mesh rendering for self
        if (photonView.IsMine)
        {
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in renderers)
                renderer.enabled = false;
        }
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
