using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : EnemyUnit
{
   
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform movePositionTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject player;
    [SerializeField] private PhotonView photonView;


    STATES CURRENT_STATE = STATES.IDLE;
    enum STATES
    {
        IDLE,
        WALKING,
        RUNNING
        
    }
    private void Awake()
    {
        player = GameObject.Find("Player");
        movePositionTransform = player.transform;
        photonView = GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        RaiseEvents.SetInactiveEvent += Inactive;
        RaiseEvents.SetActiveEvent += Active;
    }

    private void OnDisable()
    {
        RaiseEvents.SetInactiveEvent -= Inactive;
        RaiseEvents.SetActiveEvent -= Active;
    }


    public override void OnDeath()
    {
       ///Destroy(rigidbody_unit);
        //if (gameObject.TryGetComponent(out Collider collider))
        //{
        //    collider.enabled = false;
        //}
        //if (gameObject.TryGetComponent(out Dissolve dissolve))
        //{
        //    dissolve.OnDeath(1.0f);
        //}
      

        if (photonView.IsMine)
        {
            Debug.Log("photonView.ViewID" + photonView.ViewID);
            string sentData = "" + photonView.ViewID;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent(RaiseEvents.ENEMYDIEEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
        }
        // GameEvents.m_instance.unitDied.Invoke(unit_type.name);
        //Destroy(gameObject, 1.0f);
    }


    private void Update()
    {
        range_unit = Vector3.Distance(enemyTransform.position, movePositionTransform.position);
        //Debug.Log("dist "+ range_unit);
        if (range_unit < 4)
        {
            
            animator.SetBool("IsMoving", true);
            navMeshAgent.destination = movePositionTransform.position;
            CURRENT_STATE = STATES.WALKING;
        }
        else if (CURRENT_STATE == STATES.WALKING)
        {
            
            animator.SetBool("IsMoving", true);
            navMeshAgent.destination = movePositionTransform.position;
        }
    }


    public void Inactive(string data)
    {
        Debug.Log("INACTIE CALLLED");
        if ("" + data == "" + photonView.ViewID)
        {
            Debug.Log("setINactuve");
            gameObject.SetActive(false);
        }
    }

    public void Active(string data)
    {
        Debug.Log("ACTIVEEEEEE CALLLED");
        if ("" + data == "" + photonView.ViewID)
        {
            Debug.Log("setactuve");
            gameObject.SetActive(true);
        }
    }

}
