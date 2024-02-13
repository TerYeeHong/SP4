using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Animations.Rigging;
public class PlayerRigging : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] float switch_duration = 0.2f;

    [Header("Looking")]
    [SerializeField] Rig head_look_rig;
    [SerializeField] Transform head_look_at_target_transform;

    [Header("Gun Aiming")]
    [SerializeField] Rig body_bend_rig;
    [SerializeField] Transform body_bend_target_transform;
    [SerializeField] Transform head_turn_target_transform;

    [SerializeField] Transform parent_rig_targets;

    [SerializeField] Rig body_turn_rig;
    [SerializeField] Rig head_turn_rig;
    [SerializeField] Rig main_arm_rig;
    [SerializeField] Rig supporting_arm_rig;

    [Header("Rig Targets")]
    [SerializeField] List<Transform> targets_to_move_transform;
    [SerializeField] List<Transform> targets_to_move_crouch_position;
    [SerializeField] AnimationCurve target_move_speed_curve;

    List<Vector3> targets_to_move_position_origin = new();

    bool crouch = false;

    private void Start()
    {
        //int size = targets_to_move_transform.Count;
        //for (int i = 0; i < size; ++i)
        //{
        //    //save pos
        //    targets_to_move_position_origin.Add(targets_to_move_transform[i].position);
        //}
    }
    public void MoveTargetToCrouch(bool direction = true)
    {
        if (direction)
            targets_to_move_position_origin = new List<Vector3>();

        int size = targets_to_move_transform.Count;
        for (int i = 0; i < size; ++i)
        {
            if (direction)
            {
                //save pos
                targets_to_move_position_origin.Add(targets_to_move_transform[i].position);

                //targets_to_move_transform[i].position = targets_to_move_crouch_position[i].position;
                StartCoroutine(MoveTarget(targets_to_move_transform[i], targets_to_move_crouch_position[i].position));
            }
            else
            {
                //targets_to_move_transform[i].position = targets_to_move_position_origin[i];
                StartCoroutine(MoveTarget(targets_to_move_transform[i], targets_to_move_position_origin[i]));
            }
        }
    }
    
    IEnumerator MoveTarget(Transform start, Vector3 end, float duration = 0.7f)
    {
        float timer = duration;
        //int size = targets_to_move_transform.Count;
        Vector3 start_pos = start.position;

        while (timer > 0)
        {
            yield return new WaitForEndOfFrame();
            timer -= Time.deltaTime;

            float t = (duration - timer) / duration;
            float t_curve = target_move_speed_curve.Evaluate(t);

            start.position = new Vector3(
                Mathf.Lerp(start_pos.x, end.x, t_curve),
                Mathf.Lerp(start_pos.y, end.y, t_curve),
                Mathf.Lerp(start_pos.z, end.z, t_curve));

            
        }

        yield return null;
    }


    private void OnEnable()
    {
        GameEvents.m_instance.headLookAtTarget.AddListener(OnHeadLookAtTarget);
        GameEvents.m_instance.cameraThetaPhiAngle.AddListener(OnCameraThetaPhiAngle);
        GameEvents.m_instance.playerStance.AddListener(OnPlayerStance);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.headLookAtTarget.RemoveListener(OnHeadLookAtTarget);
        GameEvents.m_instance.cameraThetaPhiAngle.RemoveListener(OnCameraThetaPhiAngle);
        GameEvents.m_instance.playerStance.RemoveListener(OnPlayerStance);
    }

    void OnPlayerStance(PlayerController.STANCE stance)
    {
        switch (stance)
        {
            case PlayerController.STANCE.MELEE_STANCE:
                SwitchStance(STANCES.DEFAULT);
                break;
            case PlayerController.STANCE.RANGE_STANCE:
                SwitchStance(STANCES.GUN);
                break;
        }
    }
    void OnCameraThetaPhiAngle(float theta, float phi)
    {
        int size = parent_rig_targets.childCount;
        for (int i = 0; i < size; ++i)
        {
            parent_rig_targets.GetChild(i).rotation = Quaternion.Euler(theta + 90, phi, 0); //to get the gun to go in direction
        }
    }
    void OnHeadLookAtTarget(Vector3 target)
    {
        //body_bend_target_transform.position = target;
        //body_bend_rig.weight = 1.0f;
        head_turn_target_transform.position = target;

        //head_look_at_target_transform.position = target;
        //head_look_rig.weight = 1;
        //body_turn_rig.weight = 0.5f;
        //head_turn_rig.weight = 0.5f;
        ////main_arm_rig.weight = 0.5f;
        ////supporting_arm_rig.weight = 0.5f;
    }


    public enum STANCES { 
        DEFAULT = 0,
        GUN,
    }
    public void SwitchStance(STANCES new_stance)
    {
        switch (new_stance)
        {
            case STANCES.DEFAULT:
                StartCoroutine(WeightAdjustRoutine(body_turn_rig, body_turn_rig.weight, 0, switch_duration));
                StartCoroutine(WeightAdjustRoutine(head_turn_rig, head_turn_rig.weight, 0, switch_duration));
                StartCoroutine(WeightAdjustRoutine(main_arm_rig, main_arm_rig.weight, 0, switch_duration));
                StartCoroutine(WeightAdjustRoutine(supporting_arm_rig, supporting_arm_rig.weight, 0, switch_duration));
                break;
            case STANCES.GUN:
                StartCoroutine(WeightAdjustRoutine(body_turn_rig, body_turn_rig.weight, 1, switch_duration));
                StartCoroutine(WeightAdjustRoutine(head_turn_rig, head_turn_rig.weight, 1, switch_duration));
                StartCoroutine(WeightAdjustRoutine(main_arm_rig, main_arm_rig.weight, 1, switch_duration));
                StartCoroutine(WeightAdjustRoutine(supporting_arm_rig, supporting_arm_rig.weight, 1, switch_duration));
                break;
        }
    }


    IEnumerator WeightAdjustRoutine(Rig rig, float start_value, float end_value, float duration = 0.5f)
    {
        //Set origin
        rig.weight = start_value;
        float difference_value = end_value - start_value;

        float timer = duration;
        float inverse_duration = 1 / duration;
        while (timer > 0)
        {
            rig.weight += difference_value * Time.deltaTime * inverse_duration;

            //Time pass
            yield return new WaitForSeconds(Time.deltaTime);
            timer -= Time.deltaTime;
        }
    }
}
