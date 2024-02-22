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


    public float m_Thrust = 20f;
    float distance;
    float nearestDistance = 100;
    bool attackBasic = false;
    public int invisTimer = 0;

    public bool isInvisible = false;

    [Header("Materials")]
    public SkinnedMeshRenderer enemyRenderer;
    public Material originalMat, xrayMat;

    private float dissolveSpeed = 2.5f;
    public float dissolveAmt;
    private bool isDissolveInEffectPlaying = false;

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
        StartCoroutine(DissolveOutEffect());
        CURRENT_STATE = STATES.WALKING;
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
                targetPlayer = players[i];
            }
        }
    }


    public override void Init()
    {
        dissolveAmt = 1.2f;

        base.Init();
        FindNearestPlayer();

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
        if (other.CompareTag("Player"))
        {
            if (attackBasic)
            {
                Debug.Log("LIZARD ATTACK PLAYER.");

                //other.GetComponent<EnemyController>().Health = playerHit.Health;
                if (other.GetComponent<SkyPlayerHealth>().Health > 0)
                {
                    other.GetComponent<SkyPlayerHealth>().TakeDamage(15);
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
        CURRENT_STATE = STATES.DEAD;
        if (photonView.IsMine)
        {
            string sentData = "" + photonView.ViewID;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent(RaiseEvents.ENEMYDIEEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    private void Update()
    {
        Debug.Log("isDissolve: " + isDissolveInEffectPlaying);
        Debug.Log("state: " + CURRENT_STATE);
        range_unit = Vector3.Distance(enemyTransform.position, movePositionTransform.position);
        FindNearestPlayer();
        abilityXRay = FindAnyObjectByType<AbilityXRay>();
        if (CURRENT_STATE != STATES.ATTACK)
            isDissolveInEffectPlaying = false;

        if (CURRENT_STATE != STATES.DEAD && !targetPlayer.GetComponent<SkyPlayerHealth>().isDead)
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
            }

            if (CURRENT_STATE == STATES.HIT)
            {
                animator.SetTrigger("IsHit");
            }
            else if (CURRENT_STATE == STATES.ATTACK)
            {
                if (!isDissolveInEffectPlaying)
                {
                    StartCoroutine(PlayDissolveInEffect());
                }
                animator.SetTrigger("IsAttacking");
                isInvisible = false;
            }
            else if (CURRENT_STATE == STATES.IDLE)
            {
                animator.SetBool("IsMoving", false);
            }
            else if (CURRENT_STATE == STATES.GOINVIS && isInvisible == false)
            {
                invisTimer = 0;
                animator.SetTrigger("IsInvis");
                speed_unit = 0;
                navMeshAgent.speed = speed_unit;
            }
            else if (CURRENT_STATE == STATES.WALKING)
            {
                invisTimer++;
                if (invisTimer >= 10 && isInvisible == false)
                {
                    CURRENT_STATE = STATES.GOINVIS;
                }
                animator.SetBool("IsMoving", true);
                speed_unit = 2;
                navMeshAgent.speed = speed_unit;
                navMeshAgent.destination = movePositionTransform.position;
            }

            if (isInvisible && abilityXRay.isXRayActive == false)
            {
                enemyRenderer.sharedMaterial = xrayMat;
            }
            else if (!isInvisible)
            {
                enemyRenderer.sharedMaterial = originalMat;
            }
        }
        else if (CURRENT_STATE == STATES.DEAD)
        {
            animator.SetBool("IsDead", true);
            enabled = false;
        }
    }

    public override bool TakeDamage(int damage)
    {
        Debug.Log("GETTING MY ASS HIT");
        Debug.Log("damage" + damage);
        if (damage > 0)
        {
            Debug.Log("OUCHH");
            CURRENT_STATE = STATES.HIT;
            animator.SetTrigger("IsHit");
        }
        return base.TakeDamage(damage);
    }

    void SetDissolveAmt(float amt)
    {
        enemyRenderer.material.SetFloat("_DissolveAmt", amt);
    }

    IEnumerator DissolveInEffect()
    {
        dissolveAmt = 1.2f;
        while (dissolveAmt > -1.1f)
        {
            dissolveAmt -= Time.deltaTime * dissolveSpeed;
            SetDissolveAmt(dissolveAmt);
            yield return null;
        }
    }

    IEnumerator DissolveOutEffect()
    {
        dissolveAmt = -1.1f;
        while (dissolveAmt < 1.2f)
        {
            dissolveAmt += Time.deltaTime * dissolveSpeed;
            SetDissolveAmt(dissolveAmt);
            yield return null;
        }
        isInvisible = true;
    }

    IEnumerator PlayDissolveInEffect()
    {
        isDissolveInEffectPlaying = true;
        StartCoroutine(DissolveInEffect());
        yield return new WaitForSeconds(dissolveSpeed);
    }

    public void Inactive(string data)
    {
        if ("" + data == "" + photonView.ViewID)
        {
            gameObject.SetActive(false);
        }
    }

    public void Active(string data)
    {
        if ("" + data == "" + photonView.ViewID)
        {
            gameObject.SetActive(true);
        }
    }
}
