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
    [SerializeField] private GameObject[] players;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private Rigidbody rigidbody;

    public float m_Thrust = 20f;
    float distance;
    float nearestDistance = 1000;
    bool attackBasic = false;
    bool moving = false;
    STATES CURRENT_STATE = STATES.IDLE;
    enum STATES
    {
        IDLE,
        WALKING,
        RUNNING,
        DEAD,
        ATTACK,
        HIT
        
    }

    public override void Init()
    {
        base.Init();
        navMeshAgent.speed = speed_unit;
        FindNearestPlayer();
        
        //foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        //{
        //    if (playerObj.name == "Player")
        //    {
                
        //        movePositionTransform = playerObj.transform;
        //    }
        //}

        // player = GameObject.Find("Player");
        //movePositionTransform = player.transform;

    }

    public void FindNearestPlayer()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            distance = Vector3.Distance(enemyTransform.position, players[i].transform.position);
            if (distance < nearestDistance)
            {
                movePositionTransform = players[i].transform;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an object tagged as "Player"
        if (other.CompareTag("Player"))
        {
            // Perform actions when the hitbox collider overlaps with the player
            if (attackBasic)
            {
                Debug.Log("FISH ATTACK PLAYER.");
            }
          

            // You can perform additional actions here, such as dealing damage to the player or triggering events.
        }
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

    public void AttackBasicSTART()
    {
         attackBasic = true;
    }

    public void AttackBasicEND()
    {
        attackBasic = false;
    }

    public override void OnDeath()
    {
        base.OnDeath();
        // animator.SetBool("IsDead", true);
       
        CURRENT_STATE = STATES.DEAD;
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
        //Debug.Log("current state: " + CURRENT_STATE);

        if (CURRENT_STATE != STATES.DEAD)
        {

            if (range_unit < 3)
            {

                CURRENT_STATE = STATES.ATTACK;
            }
            else if (range_unit < 20)
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
            else if (CURRENT_STATE == STATES.ATTACK)
            {
                animator.SetTrigger("IsAttacking");
             

            }
            else if (CURRENT_STATE == STATES.IDLE)
            {
                animator.SetBool("IsMoving", false);
            }
            else if (CURRENT_STATE == STATES.WALKING)
            {

                animator.SetBool("IsMoving", true);
                FindNearestPlayer();
                StartChase();
            }

       

        }
        else if (CURRENT_STATE == STATES.DEAD)
        {
            Debug.LogWarning("HDIAHDIA");
            animator.SetBool("IsDead", true);
            enabled = false;
            // navMeshAgent.destination = movePositionTransform.position;
        }

        Debug.LogWarning("CURRENT_STATE " + CURRENT_STATE);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("attack1"))
        {
            speed_unit = 0;
            navMeshAgent.speed = speed_unit;
        }

        //animator.SetTrigger("IsHit");
    }

    public virtual void StartChase()
    {
        //GetComponent<NavMeshAgent>().enabled = true;
        //navMeshAgent.enabled = true;
        GetComponent<NavMeshAgent>().enabled = true;


        //moving = true;
        speed_unit = enemy_type.SpeedDefault;
        navMeshAgent.speed = speed_unit;
        navMeshAgent.destination = movePositionTransform.position;
    }

    public override bool TakeDamage(int damage)
    {
        //Damage pop up
        if (damage > 0)
        {
            // rigidbody.AddForce(transform.forward * m_Thrust);
            CURRENT_STATE = STATES.HIT;
            animator.SetTrigger("IsHit");


            FindNearestPlayer();
            StartChase();
        }

        //if (health_unit <= 0)
        //{
        //    OnDeath();
        //}

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
