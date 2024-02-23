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
                // Generate a random point within a donut shape around the monument
                float minRadius = 1f; // Minimum distance from the monument center to start spawning
                float maxRadius = revivalRadius; // Maximum distance from the monument center to spawn
                float angle = Random.Range(0, 2 * Mathf.PI); // Random angle

                // Ensure the spawn point is at least minRadius away from the monument
                float radius = Random.Range(minRadius, maxRadius);
                Vector2 randomPoint = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

                // Convert the 2D point to a 3D position using the monument's position as a base
                Vector3 revivePosition = transform.position + new Vector3(randomPoint.x, 2, randomPoint.y);

                // Revive the player at the calculated position
                player.GetPhotonView.RPC("OnReviveRPC", RpcTarget.All);
                player.GetPhotonView.RPC("TeleportToPosition", RpcTarget.All, revivePosition);
            }
        }
    }

}
