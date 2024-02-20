using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] float elapsedTime;

    public int min, sec;

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        min = Mathf.FloorToInt(elapsedTime / 60);
        sec = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", min, sec);
    }
}
