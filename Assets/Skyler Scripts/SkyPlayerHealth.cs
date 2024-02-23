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
    public bool isDead;
    public float revivalRadius = 5f;
    [SerializeField] public Canvas playerHPCanvas;
    [SerializeField] public Image playerHPBar;
    [SerializeField] public TextMeshProUGUI playerHPText;
    [SerializeField] private float reviveTime = 5f;

    public Color gizmoColor = Color.yellow;
    private PhotonView photonView;
    private SkyPlayerController player;

    void Awake()
    {
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

        // Check if the K key was pressed - Revive the player
        if (Input.GetKeyDown(KeyCode.K))
        {
            // Ensure only the local player can revive themselves for testing
            if (photonView.IsMine && isDead)
            {
                photonView.RPC("OnReviveRPC", RpcTarget.All);
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
        yield return new WaitForSeconds(reviveTime); 

        if (Vector3.Distance(startPosition, transform.position) < 1f) 
        {
            playerToRevive.photonView.RPC("OnReviveRPC", RpcTarget.All);
        }
        else
        {
            Debug.Log("Revival failed: Player moved.");
        }
    }


    public void OnRevive()
    {
        isDead = false;
        collide_with_attacks = true;
        Health = MaxHealth;
        if (photonView.IsMine)
        {
            SkyCameraManager.Instance.SwitchCamera(photonView.OwnerActorNr);
            player.SetChildrenMeshRenderersEnabled(false);
            GetComponent<SkyPlayerAnimation>().ChangeAnimationState(SkyPlayerAnimation.PLAYER_IDLE);
        }
        photonView.gameObject.transform.position = new Vector3(photonView.gameObject.transform.position.x, photonView.gameObject.transform.position.y + 1, photonView.gameObject.transform.position.z);
    }


    [PunRPC]
    public void OnDeathRPC()
    {
        OnDeath();
    }

    [PunRPC]
    public void OnReviveRPC()
    {
        OnRevive(); // Call the existing OnRevive logic
    }

}
