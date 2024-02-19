using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossController : EnemyUnit
{
   
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform movePositionTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject[] players;   
    [SerializeField] private GameObject[] cameras;
    [SerializeField] private GameObject[] camerasOriginalPos;
    [SerializeField] public Canvas BossHPCanvas;
    [SerializeField] public Image BossHPBar;
    [SerializeField] public CapsuleCollider BossCollider;
    [SerializeField] public BoxCollider HitboxCollider;
    Vector3 StartBossCollider;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private PhotonView photonView;
    //SerializeField] private Rigidbody rigidbody;
    [SerializeField] public float AttackCD;
    [SerializeField] public float Skill1CD;
    [SerializeField] public float Skill2CD;

    public float circleRadius = 5f; // Radius of the circular motion
    public float circleSpeed = 1f; // Speed of the circular motion

    private float angle = 0f; // Angle used to calculate the position
    float timer;
    public float m_Thrust = 20f;
    float distance;
    float nearestDistance = 1000;

    bool dashAttack = false;

    STATES CURRENT_STATE = STATES.IDLE;
    enum STATES
    {
        INTRO,
        IDLE,
        WALKING,
        RUNNING,
        DEAD,
        HIT,
        ATTACK
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an object tagged as "Player"
        if (other.CompareTag("Player"))
        {
            // Perform actions when the hitbox collider overlaps with the player
            
            Debug.Log("Hitbox overlaps with the player.");

            // You can perform additional actions here, such as dealing damage to the player or triggering events.
        }
    }

    public void DashAttackSTART()
    {
        dashAttack = true;
    }

    public void DashAttackEND()
    {
        dashAttack = false;
    }


    //public void HitCollideEvent(string moveName)
    //{

    //    Debug.Log("Hit event: " + moveName);


    //    switch (moveName)
    //    {
    //        case "Dash_Attack":
    //            Debug.Log("Hitbox DASH HIT PLAYER.");
    //            break;
    //        case "Swipe_Attack":
    //            Debug.Log("Hitbox SWIPE ATTACK HIT PLAYER.");
    //            break;
    //        default:
    //            // Handle other moves
    //            break;
    //    }
    //}

    public override void Init()
    {
        base.Init();
        StartBossCollider = BossCollider.center;
         //BossHPCanvas.enabled = false;
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
        Debug.Log(health_unit + " / " + max_health_unit);
        BossHPBar.fillAmount = (float)health_unit / (float)max_health_unit;
        range_unit = Vector3.Distance(enemyTransform.position, movePositionTransform.position);
        //Debug.Log("dist "+ range_unit);
        Debug.Log("current state: " + CURRENT_STATE);

        angle += circleSpeed * Time.deltaTime;

        if (CURRENT_STATE != STATES.DEAD)
        {
            if (CURRENT_STATE == STATES.INTRO)
            {
                cameras = GameObject.FindGameObjectsWithTag("MainCamera");
                for (int i = 0; i < cameras.Length; i++)
                {

                 
                    camerasOriginalPos = GameObject.FindGameObjectsWithTag("MainCamera");
                    for (int j = 0; j < camerasOriginalPos.Length; j++)
                    {
                        timer++;
                        //camerasOriginalPos[j].transform.position = cameras[i].transform.position;


                        Vector3 offset = new Vector3(Mathf.Cos(angle) * circleRadius, 8, Mathf.Sin(angle) * circleRadius);
                        Vector3 cameraCirclePos = enemyTransform.position + offset;

                        animator.SetTrigger("IsIntro");


                        cameras[i].transform.position = cameraCirclePos;

                        cameras[i].transform.LookAt(enemyTransform);

                        if (timer > 20)
                        {
                            CURRENT_STATE = STATES.IDLE;
                            cameras[i].transform.position = camerasOriginalPos[j].transform.position;
                            timer = 0;
                            break;

                        }
                    }
                }


            
            }

            if( range_unit < 6)
            {
                CURRENT_STATE = STATES.ATTACK;
            }
          
            else if (range_unit < 35)
            {

                CURRENT_STATE = STATES.WALKING;
            }
            else if (movePositionTransform.position == navMeshAgent.destination)
            {

                CURRENT_STATE = STATES.IDLE;
                //animator.SetBool("IsHit", false);
            }


            if (CURRENT_STATE == STATES.ATTACK)
            {

                //BossCollider.transform.forward = BossCollider.transform.forward + new Vector3(0, 0, 5);
                Vector3 hitBox = new Vector3(0, 1.7f, 5.15f);

                
                animator.SetTrigger("IsAttacking");

                if(animator.GetCurrentAnimatorStateInfo(0).IsName("attack1"))
                {
                    //BossCollider.center = hitBox;
                    BossCollider.center = Vector3.Lerp(BossCollider.center, hitBox, 1.5f * Time.deltaTime);
                    //if(HitboxCollider)
                }
                //animator.SetBool("IsHit", true);
                //CURRENT_STATE = STATES.IDLE;
            }
            else
            {

                BossCollider.center = StartBossCollider;
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
       
        }
        else if (CURRENT_STATE == STATES.DEAD)
        {
            Debug.LogWarning("HDIAHDIA");
            animator.SetBool("IsDead", true);
            enabled = false;
            // navMeshAgent.destination = movePositionTransform.position;
        }

        Debug.LogWarning("CURRENT_STATE " + CURRENT_STATE);
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
