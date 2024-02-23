using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HealPillerController : EnemyUnit
{

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform movePositionTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private BossController boss; 
    [SerializeField] private GameObject bossGameobject;  
    [SerializeField] private SphereCollider sphereCollider;
    //[SerializeField] private GameObject[] enemy;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private Rigidbody rigidbody;

    public float m_Thrust = 20f;
    float distance;
    float nearestDistance = 1000;
    bool healingBoss = false;
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
     
        FindNearestBoss();

    }

    private void OnTriggerStay(Collider other)
    {
        // Check if the collider belongs to an object tagged as "Player"
        if (other.CompareTag("Boss"))
        {
            // Perform actions when the hitbox collider overlaps with the player
            //if (healingBoss)
            {
                Debug.Log("HEAL BOSS.");
                if (other.GetComponent<BossController>().Health > 0 && other.GetComponent<BossController>().Health <= other.GetComponent<BossController>().MaxHealth)
                {
                    other.GetComponent<BossController>().HealTarget(1);
                }

            }

          
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
        healingBoss = true;
    }

    public void AttackBasicEND()
    {
        healingBoss = false;
    }

    public void FindNearestBoss()
    {

        //enemy = GameObject.FindGameObjectsWithTag("Enemy");
        //for (int i = 0; i < enemy.Length; i++)
        //{
        //    boss = enemy[i].GetComponent<BossController>();
        //}

        boss = FindObjectOfType<BossController>();
     

    }


   

    public override void OnDeath()
    {
        base.OnDeath();
        // animator.SetBool("IsDead", true);

        CURRENT_STATE = STATES.DEAD;
        //if (photonView.IsMine)
        //{
        //    Debug.Log("photonView.ViewID" + photonView.ViewID);
        //    string sentData = "" + photonView.ViewID;
        //    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        //    PhotonNetwork.RaiseEvent(RaiseEvents.ENEMYDIEEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
        //}

        if(PhotonNetwork.IsMasterClient)
        PhotonNetwork.Destroy(gameObject);

        // GameEvents.m_instance.unitDied.Invoke(unit_type.name);
        //Destroy(gameObject, 1.0f);
    }




    private void Update()
    {
       // Debug.Log("huh.");
        //OnTrigger();
        //SphereCollider other = sphereCollider;
        //OnTrigger(other);
        //if (other.CompareTag("Boss"))
        //{
        //    // Perform actions when the hitbox collider overlaps with the player
        //    //if (healingBoss)

        //        Debug.Log("HEAL BOSS.");
        //    if (other.GetComponent<BossController>().Health > 0 && other.GetComponent<BossController>().Health <= other.GetComponent<BossController>().MaxHealth)
        //    {
        //        other.GetComponent<BossController>().HealTarget(10);
        //    }



        //}
    }

    public override bool TakeDamage(int damage)
    {
    
        //Damage pop up
        if (damage > 0)
        {


            // rigidbody.AddForce(transform.forward * m_Thrust);
            //CURRENT_STATE = STATES.HIT;
            //animator.SetTrigger("IsHit");



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
