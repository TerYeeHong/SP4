using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LizardEnemyController : EnemyUnit
{

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform movePositionTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject[] players;
    AbilityXRay abilityXRay;


    //public NavMeshSurface navMeshSurface;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private Rigidbody rigidbody;

    public float m_Thrust = 20f;
    float distance;
    float nearestDistance = 100;
    bool attackBasic = false;
    public int invisTimer = 0;

    public bool isInvisible = false;

    [Header("Materials")]
    public SkinnedMeshRenderer enemyRenderer;
    public Material originalMat, xrayMat;

    STATES CURRENT_STATE = STATES.IDLE;
    enum STATES
    {
        IDLE,
        WALKING,
        GOINVIS,
        DEAD,
        ATTACK,
        HIT

    }

    public void AttackBasicSTART()
    {
        attackBasic = true;
    }

    public void AttackBasicEND()
    {
        attackBasic = false;
    }

    public void InvisibleEND()
    {
        isInvisible = true;
        CURRENT_STATE = STATES.WALKING;
    }



    public override void Init()
    {

        base.Init();
        navMeshAgent.speed = speed_unit;
        players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
        {


            distance = Vector3.Distance(enemyTransform.position, players[i].transform.position);
            if (distance < nearestDistance)
            {
                movePositionTransform = players[i].transform;

              
            }

          //  range_unit = Vector3.Distance(enemyTransform.position, movePositionTransform.position);
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
                Debug.Log("LIZARD ATTACK PLAYER.");
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

        abilityXRay = FindAnyObjectByType<AbilityXRay>();


        //Debug.Log("ability active: "+ abilityXRay.xrayActive);
        Debug.Log("current state: " + CURRENT_STATE);
        Debug.Log("invis timer: " + invisTimer);


        if (CURRENT_STATE != STATES.DEAD)
        {

            if (range_unit < 3)
            {

                CURRENT_STATE = STATES.ATTACK;
              
            }

            else if (range_unit < 20 && CURRENT_STATE != STATES.GOINVIS)
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
                isInvisible = false;

            }
            else if (CURRENT_STATE == STATES.IDLE)
            {
                animator.SetBool("IsMoving", false);
            }

            else if (CURRENT_STATE == STATES.GOINVIS && isInvisible == false)
            {

                //GRADUALLY SET THE DISSOLVE
               // SetDissolveAmt(1.1f);
                invisTimer = 0;
                animator.SetTrigger("IsInvis");
                speed_unit = 0;
                navMeshAgent.speed = speed_unit;



                //set to walk in animation evennt InvisibleEND()
            }
            else if (CURRENT_STATE == STATES.WALKING)
            {
                invisTimer++;
                if( invisTimer >= 10 && isInvisible == false)
                {
                    //invisTimer = 0;
                    CURRENT_STATE = STATES.GOINVIS;
                   
                }
                animator.SetBool("IsMoving", true);
                //moving = true;
                speed_unit = 2;
                navMeshAgent.speed = speed_unit;
                navMeshAgent.destination = movePositionTransform.position;
            }
            //if (abilityXRay.isXRayActive == true)
            //{

            //}
            
            if (isInvisible && abilityXRay.isXRayActive == false)
            {
                enemyRenderer.sharedMaterial = xrayMat;
               // SetDissolveAmt(1.1f);
            }
            else if (!isInvisible)
            {
                enemyRenderer.sharedMaterial = originalMat;
                //SetDissolveAmt(-1.1f);
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

    public override bool TakeDamage(int damage)
    {
        //health_unit -= damage;
        Debug.Log("GETTING MY ASS HIT");

        Debug.Log("damage" + damage);
        //Damage pop up
        if (damage > 0)
        {
            Debug.Log("OUCHH");

            // rigidbody.AddForce(transform.forward * m_Thrust);
            CURRENT_STATE = STATES.HIT;
            animator.SetTrigger("IsHit");



        }

        //if (health_unit <= 0)
        //{
        //    OnDeath();
        //}

        return base.TakeDamage(damage);
    }


    void SetDissolveAmt(float amt)
    {
        enemyRenderer.material.SetFloat("_Dissolve", amt);
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

