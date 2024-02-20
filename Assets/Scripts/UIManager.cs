using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject crosshair_main;
    [SerializeField] TextMeshProUGUI replay_status_tmp;
    [SerializeField] GameObject main_start_spawn_object;

    private void OnEnable()
    {
        GameEvents.m_instance.playerStance.AddListener(OnPlayerStance);
        GameEvents.m_instance.updateReplayStatus.AddListener(OnUpdateReplayStatus);
        GameEvents.m_instance.updateCanStartSpawnEnable.AddListener(OnUpdateCanStartSpawnEnable);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.playerStance.RemoveListener(OnPlayerStance);
        GameEvents.m_instance.updateReplayStatus.RemoveListener(OnUpdateReplayStatus);
        GameEvents.m_instance.updateCanStartSpawnEnable.RemoveListener(OnUpdateCanStartSpawnEnable);


    }
    void OnUpdateCanStartSpawnEnable(bool enable)
    {
        main_start_spawn_object.SetActive(enable);
    }
    void OnUpdateReplayStatus(InputController.REPLAY_STATUS status, float time)
    {
        switch (status)
        {
            case InputController.REPLAY_STATUS.NONE:
                replay_status_tmp.text = "";
                break;
            case InputController.REPLAY_STATUS.WRITING_INPUT:
                replay_status_tmp.text = "RECORDING" + "\n" + Mathf.Round(time * 10.0f) * 0.1f; ;
                break;
            case InputController.REPLAY_STATUS.READING_INPUT:
                replay_status_tmp.text = "REPLAYING" + "\n" + Mathf.Round(time * 10.0f) * 0.1f; ;
                break;
        }

    }

    void OnPlayerStance(PlayerController.STANCE stance)
    {
        switch (stance)
        {
            case PlayerController.STANCE.MELEE_STANCE:
                crosshair_main.SetActive(false);
                break;
            case PlayerController.STANCE.RANGE_STANCE:
                crosshair_main.SetActive(true);
                break;
        }
    }
}
;