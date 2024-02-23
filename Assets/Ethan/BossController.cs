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
    [SerializeField] private Camera[] cameras;
    [SerializeField] private Vector3[] camerasOriginalPos;
    [SerializeField] private Quaternion[] camerasOriginalRot;
    [SerializeField] public Canvas BossHPCanvas;
    [SerializeField] public Image BossHPBar;
    [SerializeField] public CapsuleCollider BossCollider;
    [SerializeField] public BoxCollider HitboxCollider;
    [SerializeField] private GameObject targetPlayer;

    public GameObject pillarPrefab; 
    public float spawnRadius = 15f; // pillar radius
    Vector3 StartBossCollider;
    //[SerializeField] private PhotonView photonView;
    //[SerializeField] private PhotonView photonView;
    //SerializeField] private Rigidbody rigidbody;
    [SerializeField] public float AttackCD;
    [SerializeField] public float Skill1CD = 25;
    [SerializeField] public float Skill2CD = 16;

    public float circleRadius = 5f; // Radius of the circular motion
    public float circleSpeed = 1f; // Speed of the circular motion

    private float angle = 0f; // Angle used to calculate the position
    float timer;
    public float m_Thrust = 20f;
    float distance;
    float nearestDistance = 1000;

    float cameraY = 6;
    bool storedCameraPos = false;
    bool dashAttack = false;   
    bool knockup = false;  
    bool swipeAttack = false;

    STATES CURRENT_STATE;
    public enum STATES
    {
        INTRO,
        IDLE,
        WALKING,
        RUNNING,
        DEAD,
        HIT,
        SKILL1,
        SKILL2,
        ATTACK
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an object tagged as "Player"
        if (other.CompareTag("Player"))
        {
            // Perform actions when the hitbox collider overlaps with the player
            if (dashAttack)
            {
                Debug.Log("BOSS DASH INTO PLAYER.");
                if (other.GetComponent<SkyPlayerHealth>().Health > 0)
                {
                    other.GetComponent<SkyPlayerHealth>().TakeDamage(20);
                    Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();
                    if (playerRigidbody != null)
                    {

                        float knockbackForce = 4f;
                        playerRigidbody.AddForce(Vector3.forward * knockbackForce, ForceMode.Impulse);
                    }
                }

            }
            if (swipeAttack)
            {
                Debug.Log("BOSS SWIPE THE PLAYER.");
                if (other.GetComponent<SkyPlayerHealth>().Health > 0)
                {
                    other.GetComponent<SkyPlayerHealth>().TakeDamage(35);
                }
            }
            if (knockup)
            {
                Debug.Log("BOSS KNOCKUP PLAYER");
                if (other.GetComponent<SkyPlayerHealth>().Health > 0)
                {
                    other.GetComponent<SkyPlayerHealth>().TakeDamage(12);
                    Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();
                    if (playerRigidbody != null)
                    {
                        
                        float knockupForce = 20f;
                        playerRigidbody.AddForce(Vector3.up * knockupForce, ForceMode.Impulse);
                    }
                }
            }

            // You can perform additional actions here, such as dealing damage to the player or triggering events.
        }
    }

    void SpawnPillar(float angle)
    {
        // spawn postion based on ground level
        Vector3 spawnPosition = enemyTransform.position + Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * spawnRadius);
        spawnPosition.y = -7.7f; // spawn underground

        // spawn the pillar at the ground level
       
        GameObject pillar = PhotonNetwork.InstantiateRoomObject("HEAL PILLAR", spawnPosition, Quaternion.identity);

        //pillar.transform.parent = enemyTransform;


        StartCoroutine(RisePillar(pillar));
    }

    IEnumerator RisePillar(GameObject pillar)
    {
        float elapsedTime = 0f;
        float duration = 4f; 
        Vector3 startPosition = pillar.transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * 8.2f; 

        while (elapsedTime < duration)
        {
            // lerp to new position
            Vector3 newPos = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            pillar.transform.position = newPos;

          
            elapsedTime += Time.deltaTime;
            yield return null; 
        }


        pillar.transform.position = targetPosition;
    }

    public void DashAttackSTART()
    {
        dashAttack = true;
    }

    public void DashAttackEND()
    {
        dashAttack = false;
    }

    public void SwipeAttackSTART()
    {
       swipeAttack = true;
    }

    public void SwipeAttackEND()
    {
        swipeAttack = false;
    } 
    public void SpawnPillarSTART()
    {
        SpawnPillar(0f);
        SpawnPillar(90f);
        SpawnPillar(180f);
        SpawnPillar(270f);
 
    }
    public void SpawnPillarEND()
    {
     
       // CURRENT_STATE = STATES.IDLE;
    }

    public void KnockupSTART()
    {
        knockup = true;
    }

    public void KnockupEND()
    {
        knockup = false;
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
        FindNearestPlayer();
      // CURRENT_STATE = STATES.INTRO;
       // ChangeAnimationState(STATES.INTRO);
    }

    public void Awake()
    {
        cameras = FindObjectsOfType<Camera>();

        // Store the original positions of the cameras
        camerasOriginalPos = new Vector3[cameras.Length];
        camerasOriginalRot = new Quaternion[cameras.Length];
        Debug.Log("Original position of the GameObject: " + camerasOriginalPos);
        for (int i = 0; i < cameras.Length; i++)
        {
            camerasOriginalPos[i] = cameras[i].gameObject.transform.position;
            camerasOriginalRot[i] = cameras[i].gameObject.transform.rotation;
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
        //if (photonView.IsMine)
        //{
        //    Debug.Log("photonView.ViewID" + photonView.ViewID);
        //    string sentData = "" + photonView.ViewID;
        //    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        //    PhotonNetwork.RaiseEvent(RaiseEvents.ENEMYDIEEVENT, sentData, raiseEventOptions, SendOptions.SendReliable);
        //}
        navMeshAgent.speed = 0;

        // GameEvents.m_instance.unitDied.Invoke(unit_type.name);
        //Destroy(gameObject, 1.0f);
    }




    private void Update()
    {
       // Debug.Log(health_unit + " / " + max_health_unit);
        BossHPBar.fillAmount = (float)health_unit / (float)max_health_unit;
        range_unit = Vector3.Distance(enemyTransform.position, movePositionTransform.position);
        //Debug.Log("dist "+ range_unit);
        // Debug.Log("current state: " + CURRENT_STATE);

        FindNearestPlayer();
        Debug.LogWarning("CURRENT_STATE " + CURRENT_STATE);
        // angle += circleSpeed * Time.deltaTime;

        if (CURRENT_STATE != STATES.DEAD && !targetPlayer.GetComponent<SkyPlayerHealth>().isDead)
        {

         

            if (CURRENT_STATE == STATES.INTRO)
            {
               

                timer += 1  * Time.deltaTime;

                angle += circleSpeed * Time.deltaTime;

                if (timer < 7)
                {
                    // Loop through all the cameras
                    for (int i = 0; i < cameras.Length; i++)
                    {
                        cameraY += 2 * Time.deltaTime;
                        // circular motiona round enemy
                        Vector3 offset = new Vector3(Mathf.Cos(angle) * circleRadius + 5, cameraY, Mathf.Sin(angle) * circleRadius+ 10);
                        Vector3 cameraCirclePos = enemyTransform.position + offset;

                        // Set the position of the camera
                        cameras[i].transform.position = cameraCirclePos;

                        // Make the camera look at the enemy
                        cameras[i].transform.LookAt(enemyTransform);
                    }

                }
                else
                {
                  
                    for (int i = 0; i < cameras.Length; i++)
                    {
                        timer = 0;
                        //Vector3 currentPosition = new Vector3(0, 0, 0);
                        cameras[i].transform.position = camerasOriginalPos[i];
                        cameras[i].transform.rotation = camerasOriginalRot[i];
                        //cameras[i].transform.rotation = currentPosition;
                        CURRENT_STATE = STATES.IDLE;
                        //ChangeAnimationState(STATES.IDLE);
                    }
                }

          
            }
            
            
          

            else if (range_unit < 9)
            {
                CURRENT_STATE = STATES.ATTACK;
                Vector3 lookAt = new Vector3(targetPlayer.transform.position.x, 0, targetPlayer.transform.position.z);
                gameObject.transform.LookAt(lookAt);
                ChangeAnimationState(STATES.ATTACK);
            }

            else if (CURRENT_STATE == STATES.SKILL1)
            {
                Debug.LogWarning("SKILL1 SKILL1SKILL1");
                Skill1CD = 25;
                //timer += 1 * Time.deltaTime;
                animator.SetTrigger("IsPillar");
                //speed_unit = 0;
                navMeshAgent.speed = 0;
                 CURRENT_STATE = STATES.IDLE;
                Vector3 lookAt = new Vector3(targetPlayer.transform.position.x, 0, targetPlayer.transform.position.z);
                gameObject.transform.LookAt(lookAt);
                // ChangeAnimationState(STATES.IDLE);



            }
            else if (CURRENT_STATE == STATES.SKILL2)
            {
             
                Skill2CD = 16;
                //timer += 1 * Time.deltaTime;
                animator.SetTrigger("IsKnockup");
                //speed_unit = 0;
                navMeshAgent.speed = 0;
                CURRENT_STATE = STATES.IDLE;
                Vector3 lookAt = new Vector3(targetPlayer.transform.position.x, 0, targetPlayer.transform.position.z);
                gameObject.transform.LookAt(lookAt);

                // ChangeAnimationState(STATES.IDLE);


            }

            else if (Skill1CD <= 0 /*&& CURRENT_STATE != STATES.SKILL1)*/)
            {
                CURRENT_STATE = STATES.SKILL1;
                ChangeAnimationState(STATES.SKILL1);
            }
            else if (Skill2CD <= 0 /*&& CURRENT_STATE != STATES.SKILL1)*/)
            {
                CURRENT_STATE = STATES.SKILL2;
                ChangeAnimationState(STATES.SKILL2);
            }

            else if (range_unit < 35)
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

          

            if (CURRENT_STATE == STATES.ATTACK)
            {

                //BossCollider.transform.forward = BossCollider.transform.forward + new Vector3(0, 0, 5);
                Vector3 hitBox = new Vector3(0, 1.7f, 5.15f);

                
                animator.SetTrigger("IsAttacking");


                Vector3 lookAt = new Vector3(targetPlayer.transform.position.x, 0, targetPlayer.transform.position.z);
                gameObject.transform.LookAt(lookAt);
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
                Skill1CD -= 1 * Time.deltaTime;
                Skill2CD -= 1 * Time.deltaTime;
                navMeshAgent.speed = speed_unit;
                //navMeshAgent.speed = unit_type.SpeedDefault;
                animator.SetBool("IsMoving", true);
                navMeshAgent.destination = movePositionTransform.position;
            }
       
        }
        else if (CURRENT_STATE == STATES.DEAD)
        {
            Debug.LogWarning("HDIAHDIA");
            animator.SetBool("IsDead", true);
           // enabled = false;
            // navMeshAgent.destination = movePositionTransform.position;
        }

        //Debug.LogWarning("CURRENT_STATE " + CURRENT_STATE);
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



    public bool HealTarget(int heal)
    {
        if (!enabled)
            return true;

        //Randomise it a bit
        if (heal > 0)
            heal += UnityEngine.Random.Range(-2, 2);

        health_unit += heal;


        if (heal > 0)
            GameEvents.m_instance.createTextPopup.Invoke(heal.ToString(), transform.position, Color.green);

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


    public override bool TakeDamage(int damage)
    {
        //Damage pop up
        if (damage > 0)
        {
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
