using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

public class RaiseEvents : MonoBehaviour, IOnEventCallback
{
    //public const byte BOMBTRIGGER = 1;
    public const byte PLAYERSHOOTTRIGGER = 2;
    public const byte UNITTAKEDAMAGE = 3;
    public const byte SPAWNASTEROID = 4;
    public const byte UPDATEASTEROIDTRANSFORM = 5;

    public const byte UPDATETEAMSCORE = 6;

    public const byte CHATNOTICE = 7;
    public const byte ENEMYDIEEVENT = 8;

    public const byte ENEMYSPAWNEVENT = 9;
    public const byte SETINACTIVEEVENT = 10; 
    public const byte SETACTIVEEVENT = 11;

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

    public delegate void OnEnemyDie(string data);
    public static event OnEnemyDie OnEnemyDieEvent;

    public delegate void OnEnemySpawn(string data);
    public static event OnEnemySpawn OnEnemySpawnEvent;

    public delegate void OnSetInactive(string data);
    public static event OnSetInactive SetInactiveEvent;

    public delegate void OnSetActive(string data);
    public static event OnSetActive SetActiveEvent;

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
        if (eventCode == ENEMYDIEEVENT)
        {
            OnEnemyDieEvent?.Invoke(photonEvent.CustomData.ToString());
        }
        if (eventCode == ENEMYSPAWNEVENT)
        {
            OnEnemySpawnEvent?.Invoke(photonEvent.CustomData.ToString());
        }  
        if (eventCode == SETINACTIVEEVENT)
        {
            SetInactiveEvent?.Invoke(photonEvent.CustomData.ToString());
        } 
        if (eventCode == SETACTIVEEVENT)
        {
            SetActiveEvent?.Invoke(photonEvent.CustomData.ToString());
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
