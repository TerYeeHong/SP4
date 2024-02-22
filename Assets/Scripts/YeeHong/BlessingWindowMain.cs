using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlessingWindowMain : MonoBehaviour
{
    public static BlessingWindowMain m_instance = null;
    private void Awake()
    {
        if (m_instance == null)
            m_instance = this;
    }

    public GameObject window_main;

    public void BlessingOver()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        GameEvents.m_instance.onLockInput.Invoke(false);
    }
}
