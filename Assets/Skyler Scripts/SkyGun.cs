using UnityEngine;
using Photon.Pun;
using System.Collections;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.Pool;
public class SkyGun : MonoBehaviour
{
    public SkylerGunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;
    public SkyShootConfig ShootConfig;
    public SkylerTrailConfig TrailConfig;
    public bool IsAutomatic;

    public Transform ShootPoint;
    private float LastShootTime;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;

    public bool isAiming = false;
    public float normalFOV = 60f;
    public float adsFOV = 45f;
    private Camera playerCamera;
    public Vector3 adsPosition; 
    public Vector3 adsRotation; 
    private Vector3 originalPosition; 
    private Quaternion originalRotation;
    public float adsSpeed = 10f;
    private Coroutine aimingCoroutine = null;

    public int damage;
    public bool isHitScan = true;

    public GameObject projectilePrefab; 
    public float projectileSpeed = 1000f; // Speed of the projectile
    public float projectileLifetime = 5f; // How long the projectile exists before being automatically destroyed

    void Awake()
    {
        playerCamera = GetComponentInParent<Camera>();
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        LastShootTime = 0;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail, null, null, null, false, 10, 50);

        // Assuming ModelPrefab is already the gun model attached to this GameObject.
        // If ModelPrefab should be instantiated as a child object, uncomment the following lines:
        // Model = Instantiate(ModelPrefab, SpawnPoint, Quaternion.Euler(SpawnRotation), transform);
        // If there's a specific child transform for shooting, find it by name or tag.
        ShootSystem = GetComponentInChildren<ParticleSystem>();
    }

    public void Shoot(Camera playerCamera, PhotonView photonView)
    {
        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;
            ShootSystem.Play();

            Vector3 shootDirection = playerCamera.transform.forward;
            shootDirection += new Vector3(Random.Range(-ShootConfig.Spread.x, ShootConfig.Spread.x),
                                          Random.Range(-ShootConfig.Spread.y, ShootConfig.Spread.y),
                                          Random.Range(-ShootConfig.Spread.z, ShootConfig.Spread.z));

            shootDirection.Normalize();

            Debug.Log("Photon view shooting : " + photonView);
            Debug.Log(ShootPoint.position);

            if (Physics.Raycast(playerCamera.transform.position, shootDirection, out RaycastHit hit, float.MaxValue, ShootConfig.HitMask))
            {
                StartCoroutine(PlayTrail(ShootPoint.position, hit.point, hit));
                RaisePlayerShootEvent(ShootPoint.position, hit.point, photonView);

                Unit unit = hit.collider.GetComponent<Unit>();
                Unit playerUnit = photonView.GetComponent<Unit>();

                if (unit != null && playerUnit != null)
                    RaiseUnitHitEvent(unit, damage + playerUnit.Power);

                if (unit.TryGetComponent(out BlessingMonument blessingMonument))
                {
                    unit.TakeDamage(playerUnit.Power);
                }
            }
            else
            {
                print("MISS");
                StartCoroutine(PlayTrail(ShootPoint.position, playerCamera.transform.position + (shootDirection * TrailConfig.MissDistance), new RaycastHit()));
                RaisePlayerShootEvent(ShootPoint.position, playerCamera.transform.position + (shootDirection * TrailConfig.MissDistance), photonView);
            }
        }
    }
    // Adjusted ShootProjectile method to accept parameters
    public void ShootProjectile(Vector3 position, Quaternion rotation, Vector3 shootDirection, float speed, float lifetime)
    {
        print("projectile being shot");
        GameObject projectile = Instantiate(projectilePrefab, position, rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = shootDirection * speed; 
        }

        Destroy(projectile, lifetime);
    }

    public void ShootProjectileEvent(Camera playerCamera, PhotonView photonView)
    {
        print("ROcket Shooting event");
        if (Time.time > ShootConfig.FireRate + LastShootTime && !isHitScan)
        {
            print("Can shoot.");

            LastShootTime = Time.time;

            Vector3 shootDirection = playerCamera.transform.forward;
            object[] content = new object[] {
            photonView.ViewID,
            ShootPoint.position,
            Quaternion.identity.eulerAngles,
            shootDirection,
            projectileSpeed,
            projectileLifetime
        };

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(RaiseEvents.PROJECTILE_SHOOT, content, raiseEventOptions, sendOptions);
        }
    }


    // This would be in your projectile's script, called upon collision
    public void ApplyDamageEvent(int targetViewID, int damageAmount)
    {
        object[] content = new object[] { targetViewID, damageAmount };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(RaiseEvents.PROJECTILE_DAMAGE, content, raiseEventOptions, sendOptions);
    }


    void RaiseUnitHitEvent(Unit target, int damage)
    {
        if (target.GetComponent<PhotonView>() != null)
        {
            Debug.Log("Hit Unit Event raised");
            int targetViewID = target.GetComponent<PhotonView>().ViewID;
            object[] content = new object[] { targetViewID, damage };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(RaiseEvents.UNIT_DAMAGED, content, raiseEventOptions, sendOptions);
        }
    }

    void RaisePlayerShootEvent(Vector3 startPoint, Vector3 endPoint, PhotonView photonView)
    {
        Debug.Log("Event raised");
        int viewID = photonView.ViewID;
        object[] content = new object[] { viewID, startPoint, endPoint };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(RaiseEvents.PLAYER_SHOOT, content, raiseEventOptions, sendOptions);
    }

    public IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        Debug.Log("Playing Trail");
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null;

        instance.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(StartPoint, EndPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;
            yield return null;
        }

        instance.transform.position = EndPoint;

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        instance.SetActive(false); // Start inactive, will be activated when used.
        return trail;
    }


    public void PlayerAim(bool onAim, PhotonView pv)
    {
        if (onAim && aimingCoroutine == null)
        {
            isAiming = true;
            aimingCoroutine = StartCoroutine(StartAiming());
            RaisePlayerAimingEvent(true, pv);
        }
        else if (!onAim && aimingCoroutine != null)
        {
            isAiming = false;
            StopCoroutine(aimingCoroutine);
            aimingCoroutine = StartCoroutine(StopAiming());
            RaisePlayerAimingEvent(false, pv);
        }
    }

    public IEnumerator StartAiming()
    {
        while (isAiming)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, adsFOV, adsSpeed * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, adsPosition, adsSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(adsRotation), adsSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator StopAiming()
    {
        while (!isAiming)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFOV, adsSpeed * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, SpawnPoint, adsSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(SpawnRotation), adsSpeed * Time.deltaTime);
            aimingCoroutine = null;
            yield return null;
        }
    }

    void RaisePlayerAimingEvent(bool onAim, PhotonView photonView)
    {
        Debug.Log("Event raised");
        int viewID = photonView.ViewID;
        object[] content = new object[] { viewID, onAim };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(RaiseEvents.PLAYER_AIM, content, raiseEventOptions, sendOptions);
    }
}
