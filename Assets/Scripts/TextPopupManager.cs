using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextPopupManager : MonoBehaviour
{
    [SerializeField] GameObject text_popup_prefab;


    private void OnEnable()
    {
        GameEvents.m_instance.createTextPopup.AddListener(OnCreateTextPopup);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.createTextPopup.RemoveListener(OnCreateTextPopup);
    }

    void OnCreateTextPopup(string text, Vector3 position, Color color)
    {
        var popup = Instantiate(text_popup_prefab, position, Quaternion.identity);
        var temp = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        temp.text = text;
        temp.faceColor = color;

        //destroy it
        Destroy(popup, 1f);
    }
}
