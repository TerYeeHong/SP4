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
    [SerializeField] private Rigidbody rigidbody;

    public float m_Thrust = 20f;


    STATES CURRENT_STATE = STATES.IDLE;
    enum STATES
    {
        IDLE,
        WALKING,
        RUNNING,
        HIT
        
    }

    public override void Init()
    {
        base.Init();
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
        Debug.Log("current state: " + CURRENT_STATE);

        if (range_unit < 4)
        {
           
            CURRENT_STATE = STATES.WALKING;
        }
        else if (movePositionTransform.position == navMeshAgent.destination)
        {
           
            CURRENT_STATE = STATES.IDLE;
            //animator.SetBool("IsHit", false);
        }


        if (CURRENT_STATE == STATES.HIT)
        {
            animator.SetTrigger("IsHit");
            //animator.SetBool("IsHit", true);
            //CURRENT_STATE = STATES.IDLE;
        }
        else if (CURRENT_STATE == STATES.IDLE)
        {
            animator.SetBool("IsMoving", false);
        }
        else if (CURRENT_STATE == STATES.WALKING)
        {
            
            animator.SetBool("IsMoving", true);
            navMeshAgent.destination = movePositionTransform.position;
        }
     
        //animator.SetTrigger("IsHit");
    }

    public override bool TakeDamage(int damage)
    {
        //health_unit -= damage;
        Debug.Log("GETTING MY ASS HIT");
        //Damage pop up
        if (damage > 0)
        {
            Debug.Log("OUCHH");

            rigidbody.AddForce(transform.forward * m_Thrust);
            animator.SetTrigger("IsHit");
            CURRENT_STATE = STATES.HIT;
        }


       return base.TakeDamage(damage);
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
