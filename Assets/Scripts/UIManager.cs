using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    [SerializeField] GameObject crosshair_main;
    [SerializeField] TextMeshProUGUI replay_status_tmp;
    [SerializeField] GameObject main_start_spawn_object;
    [SerializeField] private Slider actionProgressSlider;
    [SerializeField] private TMP_Text spectatingText;

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

    public void SetActiveAndChangeName(bool active, string name)
    {
        print("setting active");
        spectatingText.gameObject.SetActive(active);

        if (active)
        {
            spectatingText.text = "Spectating : " + name;
        }
    }


    public void ShowActionProgress(bool show)
    {
        if (actionProgressSlider != null)
        {
            actionProgressSlider.gameObject.SetActive(show);
            if (!show)
            {
                SetActionProgress(0);
            }
        }
    }

    public void SetActionProgress(float progress)
    {
        if (actionProgressSlider != null && actionProgressSlider.gameObject.activeSelf)
        {
            actionProgressSlider.value = progress;
        }
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