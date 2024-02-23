using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySFX : MonoBehaviour
{
    public AudioClip clickSFX;

    public void OnClick()
    {
        GameEvents.m_instance.playNewAudioClip.Invoke(clickSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
    }
}
