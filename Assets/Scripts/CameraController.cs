using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, IMouseInput
{
    [Header("References")]
    public Transform target_transform;
    public Camera camera_self;
    public Vector3 camera_offset = new(0, 1, 0);
    public Vector3 camera_offset_ads_mode = new(0, 1, 0);

    [Header("Attributes")]
    public float camera_min_dist = 1;
    public float camera_max_dist = 10;
    public float sensitivity = 0.1f;

    [Header("ADS References")]
    public Transform target_ads_transform;
    public Transform player_mesh_transform;
    public float ads_distance;
    public float field_of_view_default;
    public float field_of_view_ads;
    public float field_of_view_zoom_speed = 100.0f;

    Transform camera_self_transform;
    Vector3 target_position_previous;

    float phi_angle = 0.0f; //TO pan camera around player
    float theta_angle = 0.0f; //TO pan camera around player
    float distance;

    bool isCamTransitioning = false;

    [SerializeField] CAMERA_MODE camera_mode_current = CAMERA_MODE.DEFAULT;


    Queue<MouseAxisCommand> mouseAxisCommandQueue = new();
    Queue<MouseButtonCommand> mouseButtonCommandQueue = new();
    //Queue<AbilityCommand> abilityCommandQueue = new();

    IEnumerator MoveCameraSmooth(Vector3 start, Vector3 end, float duration)
    {
        isCamTransitioning = true;

        float timer = duration;
        while (timer > 0)
        {
            yield return new WaitForEndOfFrame();
            timer -= Time.deltaTime;

            float t = (duration - timer)/duration;
            camera_self_transform.position = new Vector3(
                Mathf.Lerp(start.x, end.x, t), 
                Mathf.Lerp(start.y, end.y, t), 
                Mathf.Lerp(start.z, end.z, t));

            //UpdateCameraOrbit();
            HandleRotation();
        }
        UpdateCameraOrbit();

        isCamTransitioning = false;
        yield return null;
    }

    private void OnEnable()
    {
        GameEvents.m_instance.playerStance.AddListener(OnPlayerStance);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.playerStance.RemoveListener(OnPlayerStance);
    }


    void OnPlayerStance(PlayerController.STANCE stance)
    {
        switch (stance)
        {
            case PlayerController.STANCE.MELEE_STANCE:
                camera_mode_current = CAMERA_MODE.DEFAULT;

                //smoothly transition cam
                StartCoroutine(MoveCameraSmooth(camera_self_transform.position,
                    target_transform.position + camera_offset -
                camera_self_transform.forward * distance, 
                    0.5f));
                break;
            case PlayerController.STANCE.RANGE_STANCE:
                camera_mode_current = CAMERA_MODE.ADS;

                //smoothly transition cam
                StartCoroutine(MoveCameraSmooth(camera_self_transform.position,
                    target_ads_transform.position + camera_offset -
                camera_self_transform.forward * ads_distance,
                    0.5f));
                break;
        }
    }

    public enum CAMERA_MODE { 
        DEFAULT = 0,
        ADS,
    }

    //bool is_dirty = false;

    private void Awake()
    {
        camera_self_transform = camera_self.transform;

        //Set Camera to first face where player is facing
        camera_self_transform.rotation = Quaternion.Euler(0, target_transform.rotation.y, 0);

        //Set old target position
        target_position_previous = target_transform.position;

        distance = (camera_min_dist + camera_max_dist) * 0.5f;

        UpdateCameraOrbit();

        //Set input controller to update here
        GameEvents.m_instance.useNewMouseInput.Invoke(this);

        //Debug.LogWarning(camera_self_transform.position);

    }

    //Update called by Input Controller
    public void UpdateMouseInput()
    {
        //Read commmand
        MouseAxisCommand mouse_axis_command = null;
        MouseButtonCommand mouse_button_command = null;
        if (mouseAxisCommandQueue.Count > 0)
            mouse_axis_command = mouseAxisCommandQueue.Dequeue();
        if (mouseButtonCommandQueue.Count > 0)
            mouse_button_command = mouseButtonCommandQueue.Dequeue();

        //Still dequeue but dont update cam
        if (isCamTransitioning)
            return;

        //Zoom
        if (mouse_axis_command != null
            && mouse_axis_command.MouseDelta.y != 0
            && camera_mode_current != CAMERA_MODE.ADS //ADS DISTANCE WILL BE HARD SET
            )
        {
            distance += -mouse_axis_command.MouseDelta.y;

            //adjust distance
            if (distance > camera_max_dist)
                distance = camera_max_dist;
            else if (distance < camera_min_dist)
                distance = camera_min_dist;

            UpdateCameraOrbit();
        }

        switch (camera_mode_current) {
            case CAMERA_MODE.DEFAULT:
                //Drag to spin around target
                if (mouse_axis_command != null
                    && mouse_button_command != null && mouse_button_command.MouseButtonHold)
                {
                    phi_angle += -mouse_axis_command.MouseX * sensitivity;
                    theta_angle += mouse_axis_command.MouseY * sensitivity;

                    theta_angle = Mathf.Clamp(theta_angle, -89.0f, 89.0f);

                    UpdateCameraOrbit();
                }
                break;
            case CAMERA_MODE.ADS:
                //Drag to spin around target
                bool update = false;
                if (mouse_axis_command != null)
                {
                    phi_angle += mouse_axis_command.MouseX * sensitivity;
                    theta_angle += -mouse_axis_command.MouseY * sensitivity;

                    theta_angle = Mathf.Clamp(theta_angle, -89.0f, 89.0f);

                    update = true;
                }
                if (mouse_button_command != null)
                {
                    if (mouse_button_command.MouseButtonHold)
                    {
                        //Zoom
                        if (camera_self.fieldOfView > field_of_view_ads)
                            camera_self.fieldOfView -= Time.deltaTime * field_of_view_zoom_speed;
                        else
                            camera_self.fieldOfView = field_of_view_ads;
                    }
                    if (mouse_button_command.MouseButtonUp)
                    {
                        //Default
                        camera_self.fieldOfView = field_of_view_default;
                    }

                    update = true;
                }
                if (update)
                    UpdateCameraOrbit();
                break;
        }
    }

    private void LateUpdate()
    {
        //if (isCamTransitioning)
        //    return;

        //Accounting for if the object moves
        switch (camera_mode_current)
        {
            case CAMERA_MODE.DEFAULT:
                //new position
                camera_self_transform.position += target_transform.position - target_position_previous;
                target_position_previous = target_transform.position;
                break;
            case CAMERA_MODE.ADS:
                //new position
                camera_self_transform.position += target_ads_transform.position - target_position_previous;
                target_position_previous = target_ads_transform.position;
                break;
        }

    }

    void UpdateCameraOrbit()
    {
        // Set the camera's rotation based on input
        camera_self_transform.rotation = Quaternion.Euler(theta_angle, phi_angle,
        0);

        switch (camera_mode_current) {
            case CAMERA_MODE.DEFAULT:
                // Set the position of the camera behind the player
                camera_self_transform.position = target_transform.position + camera_offset -
                camera_self_transform.forward * distance;
                //// Set camera to look at character's position with slight height offset
                //camera_self_transform.LookAt(target_transform.position + camera_offset);
                break;
            case CAMERA_MODE.ADS:
                // Set the position of the camera behind the player
                camera_self_transform.position
                    = target_ads_transform.position -
                camera_self_transform.forward * ads_distance; //distance set
                //// Set camera to look at character's position with slight height offset

                //Vector3 lookTarget = target_ads_transform.position + camera_self_transform.forward;
                ////lookTarget = target_ads_transform.position + camera_self_transform.forward * 10.0f;

                //GameEvents.m_instance.headLookAtTarget.Invoke(lookTarget); //this will rotate head with respect to y

                //lookTarget = target_ads_transform.position + camera_self_transform.forward * 10.0f;
                //camera_self_transform.LookAt(lookTarget);
                    
                ////Rotate Player body with respect to x,z
                //lookTarget.y = 0;
                //player_mesh_transform.LookAt(lookTarget);
    
                ////GameEvents.m_instance.rotateRigTargetsAround.Invoke(target_transform.position, camera_self_transform.right, phi_angle);
                //GameEvents.m_instance.cameraThetaPhiAngle.Invoke(theta_angle, phi_angle);
                break;
        }

        HandleRotation();
    }
    void HandleRotation()
    {
        switch (camera_mode_current)
        {
            case CAMERA_MODE.DEFAULT:
                // Set camera to look at character's position with slight height offset
                camera_self_transform.LookAt(target_transform.position + camera_offset);
                break;
            case CAMERA_MODE.ADS:
                // Set camera to look at character's position with slight height offset
                Vector3 lookTarget = target_ads_transform.position + camera_self_transform.forward;

                GameEvents.m_instance.headLookAtTarget.Invoke(lookTarget); //this will rotate head with respect to y

                lookTarget = target_ads_transform.position + camera_self_transform.forward * 10.0f;
                camera_self_transform.LookAt(lookTarget);

                //Rotate Player body with respect to x,z
                lookTarget.y = 0;
                player_mesh_transform.LookAt(lookTarget);

                //GameEvents.m_instance.rotateRigTargetsAround.Invoke(target_transform.position, camera_self_transform.right, phi_angle);
                GameEvents.m_instance.cameraThetaPhiAngle.Invoke(theta_angle, phi_angle);
                break;
        }
    }

    public void ReadMouseButtonCommand(MouseButtonCommand mouseButtonCommand)
    {
        mouseButtonCommandQueue.Enqueue(mouseButtonCommand);
    }
    public void ReadMouseAxisCommand(MouseAxisCommand mouseAxisCommand)
    {
        mouseAxisCommandQueue.Enqueue(mouseAxisCommand);
    }
   
}
