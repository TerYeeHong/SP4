using Photon.Pun;
using System.Collections;
using UnityEngine;

public class SkyReviveMonument : MonoBehaviour
{
    public float holdDuration = 10.0f; // Time in seconds that the player needs to hold down the interaction button
    public float cooldownDuration = 120.0f; // Cooldown duration in seconds (2 minutes)
    private bool isPlayerNearby = false;
    private float holdTimer = 0f;
    private float lastReviveTime = -120.0f; // Initialize to allow immediate use at game start
    private float revivalRadius = 1.0f;

    void Update()
    {
        TryRevive();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player has entered the trigger zone
        if (other.CompareTag("Player")) // Make sure your player GameObject has the "Player" tag
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player has exited the trigger zone
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private void TryRevive()
    {
        if (isPlayerNearby && Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;

            // Show the slider and update progress
            UIManager.Instance.ShowActionProgress(true);
            UIManager.Instance.SetActionProgress(holdTimer / holdDuration);

            if (Time.time - lastReviveTime >= cooldownDuration)
            {
                if (holdTimer >= holdDuration)
                {
                    ReviveAllDeadPlayers();
                    holdTimer = 0f;
                    lastReviveTime = Time.time;
                    // Hide the slider when the action is complete
                    UIManager.Instance.ShowActionProgress(false);
                }
            }
        }
        else
        {
            if (holdTimer > 0)
            {
                UIManager.Instance.ShowActionProgress(false);
            }
            holdTimer = 0f;
        }
    }

    private void ReviveAllDeadPlayers()
    {
        foreach (SkyPlayerController player in SkyWinManager.instance.playerControllers)
        {
            if (player.playerHealth.isDead)
            {
                Vector2 randomPoint = Random.insideUnitCircle * revivalRadius;
                Vector3 revivePosition = transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);
                player.GetPhotonView.RPC("OnReviveRPC", RpcTarget.All);
                player.GetPhotonView.RPC("TeleportToPosition", RpcTarget.All, revivePosition);
            }
        }
    }
}
