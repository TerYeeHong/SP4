using Photon.Pun;
using UnityEngine;

public class SkyPlayerShooting : MonoBehaviour
{
    [SerializeField]
    private SkyPlayerGunSelector GunSelector;
    private Camera playerCam;

    private void Start()
    {
        playerCam = Camera.main;
    }

    private void Update()
    {
    }

    public void ShootGun(PhotonView photonView, SkyGun skyGun)
    {
        if (GunSelector.activeGun != null)
        {
            // Check if the gun is automatic
            if (GunSelector.activeGun.IsAutomatic)
            {
                // For automatic guns, shoot as long as the fire button is held down
                if (Input.GetButton("Fire1"))
                {
                    GunSelector.activeGun.Shoot(playerCam, photonView);
                }
            }
            else
            {
                // For semi-automatic guns, shoot only on button down (once per click)
                if (Input.GetButtonDown("Fire1"))
                {
                    GunSelector.activeGun.Shoot(playerCam, photonView);
                }
            }
        }
    }
}
