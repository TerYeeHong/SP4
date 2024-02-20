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
    SkyCameraSwitch playerCameraSwitch;
    SkyPlayerHealth playerHealth;
    private PhotonView photonView;
    public GameObject body;
    public Vector3 bodyPos = new Vector3(0, -0.9f, 0);

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerMovement = GetComponent<SkyPlayerMovement>();
        playerShooting = GetComponent<SkyPlayerShooting>();
        gunSelector = GetComponent<SkyPlayerGunSelector>();
        playerCameraSwitch = GetComponent<SkyCameraSwitch>();
        playerHealth = GetComponent<SkyPlayerHealth>();
    } 

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && !playerHealth.isDead)
        {
            playerMovement.HandleInput();
            playerMovement.SlideCheck();
            playerMovement.ResetJump();
            playerMovement.HandleJump();
            playerMovement.HandleDash();
            playerMovement.UpdateAnimationState();
            playerMovement.SpeedControl();
            playerMovement.HandleStaminaRegen();

            playerShooting.ShootGun(photonView, gunSelector.activeGun);

            gunSelector.ADSActiveGun();

            playerHealth.AttemptToRevive();
            playerHealth.InputHandler();
        }
        else if (photonView.IsMine && playerHealth.isDead)
        {
            playerCameraSwitch.CheckForCameraSwitchInput();
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine && !playerHealth.isDead)
        {
            playerMovement.HandleMovement();
            playerMovement.SlidingMovement();
        }
    }
}
