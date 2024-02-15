using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class SkyGun : ScriptableObject
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

    public MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private float LastShootTime;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);
        ShootPoint = Model.GetComponentInChildren<Transform>();

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
    }
    public void Despawn()
    {
        // Check if the model exists before trying to despawn it
        if (Model != null)
        {
            // Optionally, deactivate the model instead of destroying it if you plan to reuse it later
            GameObject.Destroy(Model);
            Model = null; // Nullify the reference to indicate the gun is no longer spawned

            // If there are any additional cleanup actions, like stopping particle systems, do them here
            if (ShootSystem != null)
            {
                ShootSystem.Stop(); // Stop the particle system if it's playing
                ShootSystem.Clear(); // Clear any existing particles to reset its state
            }

            // Reset any other state variables if necessary
            LastShootTime = 0;
        }
    }

    public void Shoot(Camera playerCamera, PhotonView photonView, SkyGun gun)
    {
        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;
            ShootSystem.Play();

            // Use the camera's forward direction as the base shoot direction
            Vector3 shootDirection = playerCamera.transform.forward;

            // Apply recoil/spread by adding randomness to the shoot direction
            shootDirection += new Vector3(Random.Range(-ShootConfig.Spread.x, ShootConfig.Spread.x),
                                          Random.Range(-ShootConfig.Spread.y, ShootConfig.Spread.y),
                                          Random.Range(-ShootConfig.Spread.z, ShootConfig.Spread.z));

            shootDirection.Normalize();

            UnityEngine.Debug.Log("PHoton view shooting : " + photonView);

            UnityEngine.Debug.Log(gun.ShootPoint.position);

            // Perform the raycast from the camera position towards the shoot direction
            if (Physics.Raycast(playerCamera.transform.position, shootDirection, out RaycastHit hit, float.MaxValue, ShootConfig.HitMask))
            {

                ActiveMonoBehaviour.StartCoroutine(PlayTrail(gun.ShootPoint.position, hit.point, hit));
                RaisePlayerShootEvent(gun.ShootPoint.position, hit.point, photonView);
            }
            else
            {
                // If the raycast doesn't hit anything, extend the trail to the miss distance
                ActiveMonoBehaviour.StartCoroutine(PlayTrail(gun.ShootPoint.position, playerCamera.transform.position + (shootDirection * TrailConfig.MissDistance), new RaycastHit()));
                RaisePlayerShootEvent(gun.ShootPoint.position, playerCamera.transform.position + (shootDirection * TrailConfig.MissDistance), photonView);
            }
        }
    }
    void RaisePlayerShootEvent(Vector3 startPoint, Vector3 endPoint, PhotonView photonView)
    {
        UnityEngine.Debug.Log("Event raised");
        // Extract the ViewID from the photonView
        int viewID = photonView.ViewID;

        // Package the viewID with the start and end points of the bullet trail
        object[] content = new object[] { viewID, startPoint, endPoint };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(RaiseEvents.PLAYER_SHOOT, content, raiseEventOptions, sendOptions);
    }



    public IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        UnityEngine.Debug.Log("Playing Trail");
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null;

        instance.emitting = true;
        
        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while(remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(StartPoint, EndPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = EndPoint;

        if (Hit.collider != null)
        {
            
        }

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

        return trail;
    }
}
