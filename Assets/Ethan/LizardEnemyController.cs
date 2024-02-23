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
    [SerializeField] private GameObject targetPlayer;
    EnemyController playerHit;
    AbilityXRay abilityXRay;


    [SerializeField] public AudioClip attack;
    [SerializeField] public AudioClip invisible;
    [SerializeField] public AudioClip visible;
    [SerializeField] public AudioClip step;


    public Vector3 bodyPos = new Vector3(0, -0.9f, 0);

    //public NavMeshSurface navMeshSurface;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private Rigidbody rigidbody;

    public float m_Thrust = 20f;
    float distance;
    float nearestDistance = 100;
    bool attackBasic = false;
    public int invisTimer = 0;
    [SerializeField] public float enhanceAttackTimer = 1.5f;

    public bool isInvisible = false;

    [Header("Materials")]
    public SkinnedMeshRenderer enemyRenderer;
    public Material originalMat, xrayMat;

    STATES CURRENT_STATE = STATES.IDLE;
    public enum STATES
    {
        IDLE,
        WALKING,
        GOINVIS,
        DEAD,
        ATTACK,
        HIT

    }

    public void Step()
    {
        GameEvents.m_instance.playNewAudioClip.Invoke(step, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
    }

    

    public void AttackBasicSTART()
    {
        attackBasic = true;
        GameEvents.m_instance.playNewAudioClip.Invoke(attack, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
    }

    public void AttackBasicEND()
    {
        attackBasic = false;
    }

    public void InvisibleEND()
    {
        isInvisible = true;

        GameEvents.m_instance.playNewAudioClip.Invoke(invisible, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
        CURRENT_STATE = STATES.WALKING;
        ChangeAnimationState(STATES.WALKING);
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



    public override void Init()
    {

        base.Init();
        FindNearestPlayer();


    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            
            if (attackBasic)
            {
                Debug.Log("LIZARD ATTACK PLAYER.");

                
                //other.GetComponent<EnemyController>().Health = playerHit.Health;
                if (other.GetComponent<SkyPlayerHealth>().Health > 0)
                {
                    if (enhanceAttackTimer > 0)
                    {
                        other.GetComponent<SkyPlayerHealth>().TakeDamage(30);
                    }
                    else
                    {
                        other.GetComponent<SkyPlayerHealth>().TakeDamage(8);
                    }
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

    public override void OnDeath()
    {
        base.OnDeath();
        // animator.SetBool("IsDead", true);

        CURRENT_STATE = STATES.DEAD;
        ChangeAnimationState(STATES.DEAD);
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
        abilityXRay = FindAnyObjectByType<AbilityXRay>();


        //Debug.Log("ability active: "+ abilityXRay.xrayActive);
        Debug.Log("current state: " + CURRENT_STATE);
        Debug.Log("invis timer: " + invisTimer);


        if (CURRENT_STATE != STATES.DEAD && !targetPlayer.GetComponent<SkyPlayerHealth>().isDead)
        {
         
            if (range_unit < 2.5)
            {

                CURRENT_STATE = STATES.ATTACK;
                
                ChangeAnimationState(STATES.ATTACK);
            }

            else if (range_unit < 20 && CURRENT_STATE != STATES.GOINVIS)
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
                //animator.SetBool("IsHit", true);
                //CURRENT_STATE = STATES.IDLE;
            }

            else if (CURRENT_STATE == STATES.ATTACK)
            {
                animator.SetTrigger("IsAttacking");
                Vector3 lookAt = new Vector3(targetPlayer.transform.position.x, 0, targetPlayer.transform.position.z);
                gameObject.transform.LookAt(lookAt);
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
                    ChangeAnimationState(STATES.GOINVIS);

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
                enhanceAttackTimer = 1.5f;
               // SetDissolveAmt(1.1f);
            }
            else if (!isInvisible)
            {
                enhanceAttackTimer -= 1 * Time.deltaTime;
                enemyRenderer.sharedMaterial = originalMat;
                //SetDissolveAmt(-1.1f);
            }

        }
        else if (CURRENT_STATE == STATES.DEAD)
        {
            speed_unit = 0;
            navMeshAgent.speed = speed_unit;
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

