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

    private Transform ShootPoint;
    private GameObject Model;
    private float LastShootTime;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;

    void Awake()
    {
        LastShootTime = 0;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail, null, null, null, false, 10, 50);

        // Assuming ModelPrefab is already the gun model attached to this GameObject.
        // If ModelPrefab should be instantiated as a child object, uncomment the following lines:
        // Model = Instantiate(ModelPrefab, SpawnPoint, Quaternion.Euler(SpawnRotation), transform);

        ShootPoint = transform; // Assuming the shoot point is the transform this script is attached to.
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
            }
            else
            {
                StartCoroutine(PlayTrail(ShootPoint.position, playerCamera.transform.position + (shootDirection * TrailConfig.MissDistance), new RaycastHit()));
                RaisePlayerShootEvent(ShootPoint.position, playerCamera.transform.position + (shootDirection * TrailConfig.MissDistance), photonView);
            }
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
}
