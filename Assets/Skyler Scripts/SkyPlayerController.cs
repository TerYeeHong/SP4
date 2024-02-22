using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SkyPlayerController : MonoBehaviour
{

    private void OnEnable()
    {
        GameEvents.m_instance.onLockInput.AddListener(OnLockInput);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.onLockInput.RemoveListener(OnLockInput);
    }
    void OnLockInput(bool enable)
    {
        input_locked = enable;
    }
    bool input_locked = false;


    // Start is called before the first frame update
    SkyPlayerMovement playerMovement;
    SkyPlayerShooting playerShooting;
    SkyPlayerGunSelector gunSelector;
    SkyCameraSwitch playerCameraSwitch;
    public SkyPlayerHealth playerHealth;
    private PhotonView photonView;
    public GameObject body;
    public Vector3 bodyPos = new Vector3(0, -0.9f, 0);

    public PhotonView GetPhotonView { get { return photonView; } }


    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerMovement = GetComponent<SkyPlayerMovement>();
        playerShooting = GetComponent<SkyPlayerShooting>();
        gunSelector = GetComponent<SkyPlayerGunSelector>();
        playerCameraSwitch = GetComponent<SkyCameraSwitch>();
        playerHealth = GetComponent<SkyPlayerHealth>();

        if (photonView.IsMine)
        {
            SetChildrenMeshRenderersEnabled(false);
        }

        SkyWinManager.instance.playerControllers.Add(this);
    } 

    // Update is called once per frame
    void Update()
    {
        if (input_locked)
            return;

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
    public void SetChildrenMeshRenderersEnabled(bool isEnabled)
    {
        SkinnedMeshRenderer[] childMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in childMeshRenderers)
        {
            renderer.enabled = isEnabled;
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
