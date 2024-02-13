using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBgmManager : MonoBehaviour
{
    public static AudioBgmManager m_instance;

    [Header("Default BGM type")]
    [SerializeField] protected AUDIO_BGM_TYPE default_bgm = AUDIO_BGM_TYPE.MENU;

    //References
    [Header("2 Audio Sources only for Mixing Fade Effects")]
    [SerializeField] protected AudioSource[] audio_source;

    public AudioClip[] ambient_audio;
    public AudioClip[] action_audio;
    public AudioClip[] menu_audio;

    //Vars
    protected int audio_source_current = 0;
    protected const float fade_duration = 1f;
    protected AUDIO_BGM_TYPE audio_type_current = AUDIO_BGM_TYPE.NUM_BGM_TYPES;


    //CHOICE of BGM
    [SerializeField] public bool randomise_music_ambient;
    [SerializeField] public bool randomise_music_action;
    [SerializeField] public bool randomise_music_menu;
    public int selected_ambient_index = -1;
    public int selected_action_index = -1;
    public int selected_menu_index = -1;


    public enum AUDIO_BGM_TYPE {
        AMBIENT = 0,
        ACTION,
        MENU,
        NUM_BGM_TYPES,
    }

    //private void OnEnable()
    //{
    //    GameEvents.m_instance.newAudioEnvironment.AddListener(PlayRandom);
    //    GameEvents.m_instance.playerEnterRoom.AddListener(OnRoomEnter);
    //    GameEvents.m_instance.playerExitRoom.AddListener(OnRoomExit);
    //}
    //private void OnDisable()
    //{
    //    GameEvents.m_instance.newAudioEnvironment.RemoveListener(PlayRandom);
    //    GameEvents.m_instance.playerEnterRoom.RemoveListener(OnRoomEnter);
    //    GameEvents.m_instance.playerExitRoom.RemoveListener(OnRoomExit);
    //}

    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
            DontDestroyOnLoad(gameObject);

            //Disable both first so the OnAwake can be called when one turns active
            foreach (AudioSource audio in audio_source)
                audio.enabled = false;

            PlayRandom(default_bgm);
        }
    }


    public void PlayAmbient() {
        //Debug.Log("Ambient sound will now play");

        if (randomise_music_ambient)
            PlayRandom(AUDIO_BGM_TYPE.AMBIENT);
        else
            PlaySelected(AUDIO_BGM_TYPE.AMBIENT);
    }
    public void PlayAction() {
        if (randomise_music_action)
            PlayRandom(AUDIO_BGM_TYPE.ACTION);
        else
            PlaySelected(AUDIO_BGM_TYPE.ACTION);
    }
    public void PlayMenu() {
        if (randomise_music_menu)
            PlayRandom(AUDIO_BGM_TYPE.MENU);
        else
            PlaySelected(AUDIO_BGM_TYPE.MENU);
    }
    public void PlayRandom(AUDIO_BGM_TYPE audio_type)
    {
        //Set new current, dont override the same category of music
        //if (audio_type == audio_type_current)
        //    return;
        audio_type_current = audio_type;

        //Fade out the old audio source 
        StartCoroutine(FadeOut(audio_source[audio_source_current], fade_duration));

        //Switch source
        audio_source_current++;
        if (audio_source_current >= audio_source.Length)
            audio_source_current = 0;

        //Play the new audio in the other source first to fade in
        audio_source[audio_source_current].clip = 
            GetAudioType(audio_type)[Random.Range(0, GetAudioType(audio_type).Length - 1)];
        StartCoroutine(FadeIn(audio_source[audio_source_current], fade_duration));
    }
    public void PlaySelected(AUDIO_BGM_TYPE audio_type)
    {
        //Find the selected index
        int index = audio_type switch
        {
            AUDIO_BGM_TYPE.AMBIENT => selected_ambient_index,
            AUDIO_BGM_TYPE.ACTION => selected_action_index,
            AUDIO_BGM_TYPE.MENU => selected_menu_index,
            _ => -1,
        };
        //NOT SELECTED OR NOT FOUND
        if (index == -1)
        {
            Debug.LogWarning("Music not selected");
            return;
        }

        //Set new current, dont override the same category of music
        audio_type_current = audio_type;

        //Fade out the old audio source 
        StartCoroutine(FadeOut(audio_source[audio_source_current], fade_duration));

        //Switch source
        audio_source_current++;
        if (audio_source_current >= audio_source.Length)
            audio_source_current = 0;

        //Play the new audio in the other source first to fade in
        audio_source[audio_source_current].clip =
            GetAudioType(audio_type)[index];
        StartCoroutine(FadeIn(audio_source[audio_source_current], fade_duration));
    }

    protected AudioClip[] GetAudioType(AUDIO_BGM_TYPE audio_type)
    {
        return audio_type switch
        {
            AUDIO_BGM_TYPE.AMBIENT => ambient_audio,
            AUDIO_BGM_TYPE.ACTION => action_audio,
            AUDIO_BGM_TYPE.MENU => menu_audio,
            _ => null,
        };
    }

    IEnumerator FadeOut (AudioSource audio_source, float duration)
    {
        for (int i = 0; i < 10; ++i)
        {
            audio_source.volume -= 0.1f;
            yield return new WaitForSeconds(0.1f * duration);
        }
        audio_source.enabled = false;
    }

    IEnumerator FadeIn(AudioSource audio_source, float duration)
    {
        audio_source.enabled = true;
        audio_source.volume = 0;
        for (int i = 0; i < 10; ++i)
        {
            audio_source.volume += 0.1f;
            yield return new WaitForSeconds(0.1f * duration);
        }
    }

}
