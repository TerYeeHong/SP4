using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class SkyPlayerHealth : Unit
{
    [Header("References for Status Checks")]
    [SerializeField] Status health_divine;
    [SerializeField] Status health_moderate;
    [SerializeField] Status health_mini;

    protected int health_divine_count;
    protected int health_moderate_count;
    protected int health_mini_count;


    private void OnEnable()
    {
        GameEvents.m_instance.onStatusChange.AddListener(StatusCheckAll);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.onStatusChange.RemoveListener(StatusCheckAll);
    }

    public void StatusCheckAll()
    {
        health_divine_count = PFGlobalData.GetBlessingCount(health_divine.Name_status);
        health_moderate_count = PFGlobalData.GetBlessingCount(health_moderate.Name_status);
        health_mini_count = PFGlobalData.GetBlessingCount(health_mini.Name_status);

        max_health_unit = unit_type.HealthDefault + health_divine_count * 100 + health_moderate_count * 40 + health_mini_count * 20;
        health_unit = max_health_unit;
    }



    public bool isDead;
    public float revivalRadius = 5f;
    [SerializeField] public Canvas playerHPCanvas;
    [SerializeField] public Image playerHPBar;
    [SerializeField] public TextMeshProUGUI playerHPText;
    [SerializeField] private float reviveTime = 5f;

    public Color gizmoColor = Color.yellow;
    private PhotonView photonView;
    private SkyPlayerController player;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private AudioClip hitSFX;

    void Awake()
    {
        SetDefaultStat();
        StatusCheckAll();


        photonView = GetComponent<PhotonView>();
        player = GetComponent<SkyPlayerController>();
        if (photonView.IsMine)
        {
            string dataSent = $"3$@" +
               $"{PhotonNetwork.LocalPlayer.NickName}" +
               // $"$@{team}" +
               $"$@{"blud is join"}";

            // You would have to set the Receivers to ALL in order to receive this event on the local client as well
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(RaiseEvents.CHATNOTICE, dataSent, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    public override void OnDeath()
    {
        isDead = true;
        if (photonView.IsMine)
            player.SetChildrenMeshRenderersEnabled(true);
        GetComponent<SkyPlayerAnimation>().ChangeAnimationState("Falling Back Death");
        GameEvents.m_instance.playNewAudioClip.Invoke(deathSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
        SkyWinManager.instance.CheckLose();
        //GameEvents.m_instance.unitDied.Invoke(unit_type.name);

        string dataSent = $"1$@" +
               $"{PhotonNetwork.LocalPlayer.NickName}" +
               // $"$@{team}" +
               $"$@{"blud is dead"}";

        // You would have to set the Receivers to ALL in order to receive this event on the local client as well
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(RaiseEvents.CHATNOTICE, dataSent, raiseEventOptions, SendOptions.SendReliable);
        // GameEvents.m_instance.unitDied.Invoke(unit_type.name);
    }

    public override bool TakeDamage(int damage)
    {
        if (!enabled)
            return true;

        //Randomise it a bit
        if (damage > 0)
            damage += Random.Range(-2, 2);

        health_unit -= damage;
        GameEvents.m_instance.playNewAudioClip.Invoke(hitSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);


        if (damage > 0)
            GameEvents.m_instance.createTextPopup.Invoke(damage.ToString(), transform.position, Color.white);

        if (team_unit == UnitType.UNIT_TEAM.PLAYER)
        {
        }

        if (health_unit <= 0)
        {
            health_unit = 0;
            collide_with_attacks = false;
            photonView.RPC("OnDeathRPC", RpcTarget.All);
            HandleHealthBar();
            return true;
        }
        return false;
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
            //data: chat msg (0, sendername, team, msg)
           
            return true;
        }
        return false;
    }


    public void HandleHealthBar()
    {
        playerHPBar.fillAmount = (float)health_unit / (float)max_health_unit;
        playerHPText.text = "HP " + health_unit;

    }

    public void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            // Ensure only the local player can kill themselves for testing
            if (photonView.IsMine && !isDead)
            {
                photonView.RPC("OnDeathRPC", RpcTarget.All);
            }
        }
    }

    public void AttemptToRevive()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, revivalRadius); 
            foreach (var hitCollider in hitColliders)
            {
                SkyPlayerHealth playerHealth = hitCollider.GetComponent<SkyPlayerHealth>();
                if (playerHealth != null && playerHealth.isDead)
                {
                    StartCoroutine(AttemptRevive(playerHealth));
                    break; 
                }
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        // Draw a wireframe sphere to indicate the revival area
        Gizmos.DrawWireSphere(transform.position, revivalRadius);
    }


    IEnumerator AttemptRevive(SkyPlayerHealth playerToRevive)
    {
        Vector3 startPosition = transform.position;
        UIManager.Instance.ShowActionProgress(true); // Show the progress bar at the start

        float elapsedTime = 0f; // Track the elapsed time
        while (elapsedTime < reviveTime)
        {
            elapsedTime += Time.deltaTime; // Increment the elapsed time

            // Calculate the current progress as a percentage
            float progress = Mathf.Clamp01(elapsedTime / reviveTime);
            UIManager.Instance.SetActionProgress(progress); // Update the progress bar

            // Optional: Check if the player has moved too far from the initial position
            if (Vector3.Distance(startPosition, transform.position) >= 1f)
            {
                Debug.Log("Revival failed: Player moved.");
                UIManager.Instance.ShowActionProgress(false); // Hide the progress bar if the player moves
                yield break; // Exit the coroutine early if the condition fails
            }

            yield return null; // Wait until the next frame to continue
        }

        // If the loop completes without interruptions, revive the player
        playerToRevive.photonView.RPC("OnReviveRPC", RpcTarget.All);
        UIManager.Instance.ShowActionProgress(false); // Hide the progress bar on completion
    }

    public void CheckForOffMap()
    {
        print("Checking for off map");
        if (gameObject.transform.position.y <= -10.0f)
        {
            photonView.RPC("OnVoidedRPC", RpcTarget.All);
            photonView.RPC("OnDeathRPC", RpcTarget.All);
        }
    }

    public void OnRevive()
    {
        isDead = false;
        UIManager.Instance.SetActiveAndChangeName(false, "");
        collide_with_attacks = true;
        Health = MaxHealth;
        player.SetChildrenMeshRenderersEnabled(true);
        if (photonView.IsMine)
        {
            SkyCameraManager.Instance.SwitchCamera(photonView.OwnerActorNr);
            player.SetChildrenMeshRenderersEnabled(false);
            GetComponent<SkyPlayerAnimation>().ChangeAnimationState(SkyPlayerAnimation.PLAYER_IDLE);
        }
        photonView.gameObject.transform.position = new Vector3(photonView.gameObject.transform.position.x, photonView.gameObject.transform.position.y + 1, photonView.gameObject.transform.position.z);
        gameObject.GetComponent<Rigidbody>().useGravity = true;

    }

    [PunRPC]
    public void OnVoidedRPC()
    {
        OnVoided();
    }

    public void OnVoided()
    {
        player.SetChildrenMeshRenderersEnabled(false);
        gameObject.GetComponent<Rigidbody>().useGravity = false;
    }


    [PunRPC]
    public void OnDeathRPC()
    {
        OnDeath();
    }

    [PunRPC]
    public void TeleportToPosition(Vector3 pos)
    {
        TPToPos(pos);
    }

    private void TPToPos(Vector3 pos)
    {
        transform.position = pos; 
    }

    [PunRPC]
    public void OnReviveRPC()
    {
        OnRevive(); // Call the existing OnRevive logic
    }

}
