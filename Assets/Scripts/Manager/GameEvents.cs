using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    [Header("Global References")]
    [SerializeField] LayerMask player_layermask;
    [SerializeField] LayerMask enemy_layermask;
    [SerializeField] LayerMask all_layermask;
    public LayerMask PlayerLayermask { get { return player_layermask; } }
    public LayerMask EnemyLayermask { get { return enemy_layermask; } }
    public LayerMask AllLayerMask { get { return all_layermask; } }


    //Input controller
    [NonSerialized] public UnityEvent<IKeyInput> useNewKeyInput;
    [NonSerialized] public UnityEvent<IMouseInput> useNewMouseInput;

    [NonSerialized] public UnityEvent<Vector3> headLookAtTarget;
    [NonSerialized] public UnityEvent<float, float> cameraThetaPhiAngle;

    [NonSerialized] public UnityEvent<InputController.REPLAY_STATUS, float> updateReplayStatus;

    [NonSerialized] public UnityEvent<bool> onLockInput;

    //Gameplay Events
    [NonSerialized] public UnityEvent<bool> updatePlayerOnWater;
    [NonSerialized] public UnityEvent<string, Vector3, Color> createTextPopup;

    [NonSerialized] public UnityEvent<string> unitDied;
    [NonSerialized] public UnityEvent onStatusChange;

    //Post process/Shadersd
    [NonSerialized] public UnityEvent<bool> UnderwaterPostProcessEnable;

    //UI 
    [NonSerialized] public UnityEvent<bool> updateCanStartSpawnEnable;

    //Audio
    [NonSerialized] public UnityEvent<AudioClip, AudioSfxManager.AUDIO_EFFECT> playNewAudioClip;
    [NonSerialized] public UnityEvent<AudioClip, AudioSfxManager.AUDIO_EFFECT, Vector3> playNewAudioClip3D;
    [NonSerialized] public UnityEvent<AudioClip, AudioSfxManager.AUDIO_EFFECT, float> playNewAudioClipDelayed;
    //[NonSerialized] public UnityEvent<AudioClip, AudioSfxManager.AUDIO_EFFECT> playNewAudioClipEffect;


    //Player skills
    [NonSerialized] public UnityEvent<PlayerController.STANCE> playerStance;
    [NonSerialized] public UnityEvent<Ability, PlayerController.PLAYER_ABILITY> useNewAbility;


    public static GameEvents m_instance = null;
    private void Awake()
    {
        if (m_instance == null)
            m_instance = this;

        useNewKeyInput = new UnityEvent<IKeyInput>();
        useNewMouseInput = new UnityEvent<IMouseInput>();

        headLookAtTarget = new UnityEvent<Vector3>();
        cameraThetaPhiAngle = new UnityEvent<float, float>();
        updateReplayStatus = new UnityEvent<InputController.REPLAY_STATUS, float>();

        updatePlayerOnWater = new UnityEvent<bool>();
        createTextPopup = new UnityEvent<string, Vector3, Color>();

        onStatusChange = new UnityEvent();
        unitDied = new UnityEvent<string>();

        onLockInput = new UnityEvent<bool>();

        UnderwaterPostProcessEnable = new UnityEvent<bool>();

        updateCanStartSpawnEnable = new UnityEvent<bool>();

        playNewAudioClip = new UnityEvent<AudioClip, AudioSfxManager.AUDIO_EFFECT>();
        playNewAudioClip3D = new UnityEvent<AudioClip, AudioSfxManager.AUDIO_EFFECT, Vector3>();
        playNewAudioClipDelayed = new UnityEvent<AudioClip, AudioSfxManager.AUDIO_EFFECT, float>();

        playerStance = new UnityEvent<PlayerController.STANCE>();
        useNewAbility = new UnityEvent<Ability, PlayerController.PLAYER_ABILITY>();
    }
}
