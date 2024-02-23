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
    [SerializeField] private GameObject targetPlayer;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private Rigidbody rigidbody;

    [SerializeField] public AudioClip attack;
    [SerializeField] public AudioClip step1;
    [SerializeField] public AudioClip step2;

    public float m_Thrust = 20f;
    float distance;
    float nearestDistance = 1000;
    bool attackBasic = false;
    bool moving = false;
    STATES CURRENT_STATE = STATES.IDLE;
    public enum STATES
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

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an object tagged as "Player"
        if (other.CompareTag("Player"))
        {
            // Perform actions when the hitbox collider overlaps with the player
            if (attackBasic)
            {
                Debug.Log("FISH ATTACK PLAYER.");
                if (other.GetComponent<SkyPlayerHealth>().Health > 0)
                {
                    other.GetComponent<SkyPlayerHealth>().TakeDamage(10);
                }

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

    public void Step1()
    {
        //GameEvents.m_instance.playNewAudioClip3D.Invoke(step1, AudioSfxManager.AUDIO_EFFECT.DEFAULT, gameObject.transform.position);
        GameEvents.m_instance.playNewAudioClip.Invoke(step1, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
    }

    public void Step2()
    {

        //GameEvents.m_instance.playNewAudioClip3D.Invoke(step2, AudioSfxManager.AUDIO_EFFECT.DEFAULT, gameObject.transform.position);
        GameEvents.m_instance.playNewAudioClip.Invoke(step2, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
    }


    public void AttackBasicSTART()
    {
         attackBasic = true;
        //GameEvents.m_instance.playNewAudioClip3D.Invoke(attack, AudioSfxManager.AUDIO_EFFECT.DEFAULT, gameObject.transform.position);
        GameEvents.m_instance.playNewAudioClip.Invoke(attack, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
    }

    public void AttackBasicEND()
    {
        attackBasic = false;
    }

    public void FindNearestPlayer()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        //Assume first player is nearest
        distance = Vector3.Distance(enemyTransform.position, players[0].transform.position);
        nearestDistance = distance;
        targetPlayer = players[0];

        //Check all other players
        for (int i = 1; i < players.Length; i++)
        {
            distance = Vector3.Distance(enemyTransform.position, players[i].transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                targetPlayer = players[i];
            }
        }

        movePositionTransform = targetPlayer.transform;
    }


    public bool HealTarget(int heal)
    {
        if (!enabled)
            return true;

        //Randomise it a bit
        if (heal > 0)
            heal += UnityEngine.Random.Range(-2, 2);

        health_unit += heal;


        if (heal > 0)
            GameEvents.m_instance.createTextPopup.Invoke(heal.ToString(), transform.position, Color.white);

        if (team_unit == UnitType.UNIT_TEAM.PLAYER)
        {

        }

        if (health_unit <= 0)
        {
            health_unit = 0;
            collide_with_attacks = false;
            photonView.RPC("OnDeathRPC", RpcTarget.All);
            return true;
        }
        return false;
    }

    public override void OnDeath()
    {
        base.OnDeath();
        // animator.SetBool("IsDead", true);
       
        CURRENT_STATE = STATES.DEAD;
        ChangeAnimationState(STATES.WALKING);
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

        FindNearestPlayer();

            //Debug.Log("dist "+ range_unit);
            //Debug.Log("current state: " + CURRENT_STATE);

        if (CURRENT_STATE != STATES.DEAD && !targetPlayer.GetComponent<SkyPlayerHealth>().isDead)
        {
         
                if (range_unit < 4)
                {

                    
                    CURRENT_STATE = STATES.ATTACK;
                    Vector3 lookAt = new Vector3(targetPlayer.transform.position.x, targetPlayer.transform.position.y, targetPlayer.transform.position.z);
                    gameObject.transform.LookAt(lookAt);
                    ChangeAnimationState(STATES.ATTACK);
                }
                else if (range_unit < 20)
                {

                    CURRENT_STATE = STATES.WALKING;
                    ChangeAnimationState(STATES.WALKING);
                }
                else if (movePositionTransform.position == navMeshAgent.destination)
                {

                    CURRENT_STATE = STATES.IDLE;
                     ChangeAnimationState(STATES.IDLE);
                    //animator.SetBool("IsHit", false);
                }

                if (CURRENT_STATE == STATES.HIT)
                {
                    animator.SetTrigger("IsHit");
                  ChangeAnimationState(STATES.HIT);
                //animator.SetBool("IsHit", true);
                //CURRENT_STATE = STATES.IDLE;
                }
                else if (CURRENT_STATE == STATES.ATTACK)
                {
                    animator.SetTrigger("IsAttacking");
                    Vector3 lookAt = new Vector3(targetPlayer.transform.position.x, 0, targetPlayer.transform.position.z);
                    gameObject.transform.LookAt(lookAt);
                    navMeshAgent.speed = 0;

                }
                else if (CURRENT_STATE == STATES.IDLE)
                {
                    animator.SetBool("IsMoving", false);
                }
                else if (CURRENT_STATE == STATES.WALKING)
                {

                    animator.SetBool("IsMoving", true);
                    //moving = true;
                    //speed_unit = 2;
                    //navMeshAgent.speed = speed_unit;
                    //navMeshAgent.destination = movePositionTransform.position;


                    speed_unit = enemy_type.SpeedDefault;
                    FindNearestPlayer();
                    StartChase();
                    //Debug.LogWarning("CURRENT_STATE " + CURRENT_STATE);
                }



            }
            else if (CURRENT_STATE == STATES.DEAD)
            {
                Debug.LogWarning("HDIAHDIA");
                animator.SetBool("IsDead", true);
                enabled = false;
                // navMeshAgent.destination = movePositionTransform.position;
            }


            if (animator.GetCurrentAnimatorStateInfo(0).IsName("attack1"))
            {
               
                navMeshAgent.speed = 0;
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

    public void ChangeAnimationState(STATES newState, float transitionDuration = 0.1f)
    {
        if (CURRENT_STATE == newState) return;

        // gameObject.transform.localPosition = gameObject.bodyPos;

        animator.CrossFade(newState.ToString(), transitionDuration);
        CURRENT_STATE = newState;

        // Network synchronization code remains the same
        if (photonView.IsMine)
        {
            object[] content = new object[] { GetComponent<PhotonView>().ViewID, newState };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(RaiseEvents.PLAYER_ANIMATION_CHANGE, content, raiseEventOptions, SendOptions.SendReliable);
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
