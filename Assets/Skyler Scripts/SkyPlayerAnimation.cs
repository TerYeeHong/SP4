using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SkyPlayerAnimation : MonoBehaviour
{
    public Animator animator;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public static string PLAYER_IDLE = "Idle";
    public static string PLAYER_WALK = "Running";
    public static string PLAYER_SPEEDBURST = "SpeedBurst";
    public static string PLAYER_RUN = "Sprinting";
    public static string PLAYER_JUMP = "JumpInitial";
    public static string PLAYER_JUMPING = "Jumping";
    public static string PLAYER_FALL = "Falling";
    public static string PLAYER_LANDING = "Landing";

    public static string PLAYER_STAND_TO_CROUCH = "StandToCrouch";
    public static string PLAYER_CROUCH_IDLE = "CrouchIdle";
    public static string PLAYER_CROUCH_WALK = "CrouchWalk";
    public static string PLAYER_CROUCH_TO_STAND = "CrouchToStand";

    public static string PLAYER_ATTACK1 = "Great Sword Slash";
    public static string PLAYER_ATTACK2 = "Great Sword Slash (1)";
    public static string PLAYER_ATTACK3 = "Great Sword Jump Attack";


    private string currentState;

    public bool IsAnimationPlaying(string animationName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }

    public float GetCurrentAnimationLength()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);
        currentState = newState;

        // Check if we are connected and this GameObject is controlled by the local player
        if (photonView.IsMine)
        {
            print("Animation my photon view");
            object[] content = new object[] { GetComponent<PhotonView>().ViewID, newState };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(RaiseEvents.PLAYER_ANIMATION_CHANGE, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    private void OnAnimatorMove()
    {

    }
    
}
