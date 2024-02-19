using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SkyPlayerHealth : Unit
{
    public bool isDead;
    public float revivalRadius = 5f;

    public Color gizmoColor = Color.yellow;
    private PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public override void OnDeath()
    {
        isDead = true;
        rigidbody_unit.isKinematic = true;
        GameEvents.m_instance.unitDied.Invoke(unit_type.name);
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
            collide_with_attacks = false;
            photonView.RPC("OnDeathRPC", RpcTarget.All);
            return true;
        }
        return false;
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
        yield return new WaitForSeconds(10); 

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
        rigidbody_unit.isKinematic = false;
        collide_with_attacks = true;
        Health = MaxHealth;
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
