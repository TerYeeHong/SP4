using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SkyPlayerGunSelector : MonoBehaviour
{
    [SerializeField]
    private SkylerGunType Gun;
    [SerializeField]
    private Transform GunParent;
    [SerializeField]
    private List<SkyGun> Guns;

    [Space]
    [Header("Runtime Filled")]
    public SkyGun ActiveGun;

    private void Start()
    {
        SkyGun gun = Guns.Find(gun => gun.Type == Gun);

        if (gun != null)
        {
            Debug.LogError($"No Gun found for GunType : {gun} ");
            return;
        }

        ActiveGun = gun;
        gun.Spawn(GunParent, this);
    }

}
