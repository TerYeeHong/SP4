using UnityEngine;
using Photon.Pun; // Make sure to include the Photon.Pun namespace

public class SkyCameraController : MonoBehaviour
{
    public Transform player; // Assign the player's transform here in the inspector
    public float mouseSensitivity = 100f;
    public float rotationLimit = 80f;

    private float xRotation = 0f;
    private PhotonView photonView; // To check if the parent player is ours
    private Camera cam; // Reference to the Camera component
    private AudioListener audioListener; // Reference to the AudioListener component, if present

    void Awake()
    {
        cam = GetComponent<Camera>();
        audioListener = GetComponent<AudioListener>();
        photonView = GetComponentInParent<PhotonView>(); // Adjusted to get the PhotonView from the parent

        // Disable the camera and audio listener for non-local players
        if (photonView != null && !photonView.IsMine)
        {
            if (cam != null) cam.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
        }

        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen for the local player
        }
    }

    void Update()
    {
        // Only handle mouse input if the camera belongs to the local player
        if (photonView != null && photonView.IsMine)
        {
            HandleMouseInput();
        }
    }

    void HandleMouseInput()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Adjust camera rotation based on vertical mouse movement
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -rotationLimit, rotationLimit);

        // Apply rotation to the camera (for looking up and down)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player object (or camera's parent) based on horizontal mouse movement
        if (photonView.IsMine)
        {
            player.Rotate(Vector3.up * mouseX);
        }
    }
}
