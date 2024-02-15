using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using System;

public class RaiseEvents : MonoBehaviour, IOnEventCallback
{
    //public const byte BOMBTRIGGER = 1;
    public const byte PLAYERSHOOTTRIGGER = 2;
    public const byte UNITTAKEDAMAGE = 3;
    public const byte SPAWNASTEROID = 4;
    public const byte UPDATEASTEROIDTRANSFORM = 5;

    public const byte UPDATETEAMSCORE = 6;

    public const byte CHATNOTICE = 7;
    public const byte PLAYER_ANIMATION_CHANGE = 8;

    public const byte PLAYER_SHOOT = 9;


    //public delegate void OnExplode(string position);
    //public static event OnExplode ExplodeEvent;

    public delegate void OnPlayerShoot(string data);
    public static event OnPlayerShoot PlayerShootEvent;

    public delegate void OnUnitTakeDamage(string data);
    public static event OnUnitTakeDamage UnitTakeDamageEvent;

    public delegate void OnSpawnAsteroid(string data);
    public static event OnSpawnAsteroid SpawnAsteroidEvent;

    public delegate void OnUpdateAsteroidTransform(string data);
    public static event OnUpdateAsteroidTransform UpdateAsteroidTransformEvent;

    public delegate void OnUpdateTeamScore(string data);
    public static event OnUpdateTeamScore UpdateTeamScoreEvent;

    public delegate void OnChatNotice(string data);
    public static event OnChatNotice OnChatNoticeEvent;

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        //if (eventCode == BOMBTRIGGER)
        //{
        //    ExplodeEvent?.Invoke(photonEvent.CustomData.ToString());
        //}
        if (eventCode == PLAYERSHOOTTRIGGER)
        {
            PlayerShootEvent?.Invoke(photonEvent.CustomData.ToString());
        }
        if (eventCode == UNITTAKEDAMAGE)
        {
            UnitTakeDamageEvent?.Invoke(photonEvent.CustomData.ToString());
        }
        if (eventCode == SPAWNASTEROID)
        {
            SpawnAsteroidEvent?.Invoke(photonEvent.CustomData.ToString());
        }
        //if (eventCode == UPDATEASTEROIDTRANSFORM)
        //{
        //    UpdateAsteroidTransformEvent?.Invoke(photonEvent.CustomData.ToString());
        //}
        if (eventCode == UPDATETEAMSCORE)
        {
            UpdateTeamScoreEvent?.Invoke(photonEvent.CustomData.ToString());
        }
        if (eventCode == CHATNOTICE)
        {
            OnChatNoticeEvent?.Invoke(photonEvent.CustomData.ToString());
        }
        if (eventCode == PLAYER_ANIMATION_CHANGE)
        {
            print("event raised");
            object[] data = (object[])photonEvent.CustomData;
            int viewID = (int)data[0];
            string newState = (string)data[1];

            PhotonView pv = PhotonView.Find(viewID);
            if (pv != null)
            {
                SkyPlayerAnimation playerAnimation = pv.GetComponent<SkyPlayerAnimation>();
                if (playerAnimation != null)
                {
                    print("Found");
                    playerAnimation.ChangeAnimationState(newState);
                }
            }
        }

        if (eventCode == PLAYER_SHOOT)
        {
            object[] data = (object[])photonEvent.CustomData;
            int viewID = (int)data[0];
            Vector3 startPoint = (Vector3)data[1];
            Vector3 endPoint = (Vector3)data[2];

            PhotonView photonView = PhotonView.Find(viewID);

            // Assuming you have a method to find the PhotonView by ActorNumber
            if (photonView != null)
            {
                print("View found");
                // Assuming the shooter's SkyPlayerGunSelector component can give us the active gun
                SkyPlayerGunSelector gunSelector = photonView.GetComponent<SkyPlayerGunSelector>();
                if (gunSelector != null && gunSelector.ActiveGun != null)
                {
                    print("Gun Active");
                    // Now, use the active gun to play the trail
                    gunSelector.ActiveGun.ActiveMonoBehaviour.StartCoroutine(gunSelector.ActiveGun.PlayTrail(startPoint, endPoint, new RaycastHit()));
                }
            }
        }

    }
}

//ON BOMB SCRIPT

//OnEnable { RaiseEvents.ExplodeEvent += Explode }
//OnDisable { RaiseEvents.ExplodeEvent -= Explode }

//Explode(string data){ if (data == ""+transform.position) { //do stuff } } 

//void Update()
//{
//string dataSent = ""+transform.position;

//// You would have to set the Receivers to ALL in order to receive this event on the local client as well
//RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All };
//PhotonNetwork.RaiseEvent(RaiseEvents.BOMBTRIGGER, dataSent, raiseEventOptions, SendOptions.SendReliable);
//pos = transform.position;
//}




//On CALLING SCRIPT (Player)

//If Input.GetButtonDown() && photonView.IsMine
//  photonView.RPC("PlantBomb", RpcTarget.AllViaServer, rigidbody.position);

//[PunRPC]
//public void PlantBomb(Vector3 position){
//  GameObject bomb = instantiate(bombPrefab, position, Quarternion.identity) as GameObject;
//  bonb.GetComponent<BombScript>().isMine = photonView.IsMine;
//}




//ON THE OBJECTS TO BE HIT
//OnHit(int targetID){
//if (GetComponent<PhotonView>().ViewID == targetID){
//  if (PhotonNetwork.IsMasterClient) {
//      PhotonNetwork.Destroy(gameObject);
//  }
//}
