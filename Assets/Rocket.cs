using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class Rocket : MonoBehaviourPun
{
    public float explosionRadius = 5f;
    public int explosionDamage = 100;
    public GameObject explosionEffectPrefab;
    public LayerMask enemyLayer;

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        if (explosionEffectPrefab)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Find all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
        foreach (Collider hit in colliders)
        {
            print("Enemy in range");
            PhotonView targetPhotonView = hit.GetComponent<PhotonView>();
            if (targetPhotonView != null)
            {
                print("IM TRYNA DAMAGE!");
                // Send damage event to all objects with PhotonView within the explosion radius
                ApplyDamageEvent(targetPhotonView.ViewID, explosionDamage);
            }
        }

        Destroy(gameObject);
    }

    void ApplyDamageEvent(int targetViewID, int damageAmount)
    {
        object[] content = new object[] { targetViewID, damageAmount };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        // Use the same event code for damage that you've used in other parts of your game
        PhotonNetwork.RaiseEvent(RaiseEvents.UNIT_DAMAGED, content, raiseEventOptions, sendOptions);
    }
}
