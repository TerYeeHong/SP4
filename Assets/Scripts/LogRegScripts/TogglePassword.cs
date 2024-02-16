// Made by: Matt Palero
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TogglePassword : MonoBehaviour
{
    [SerializeField] private TMP_InputField if_passwordEmail, if_passwordUser, if_passwordReg;

    public void togglePassEmail()
    {
        bool isPasswordEmailHide = if_passwordEmail.contentType == TMP_InputField.ContentType.Password;

        if_passwordEmail.contentType = isPasswordEmailHide ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

        if_passwordEmail.ForceLabelUpdate();
    }

    public void togglePassUser()
    {
        bool isPasswordUserHide = if_passwordUser.contentType == TMP_InputField.ContentType.Password;

        if_passwordUser.contentType = isPasswordUserHide ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

        if_passwordUser.ForceLabelUpdate();
    }

    public void togglePassReg()
    {
        bool isPasswordRegHide = if_passwordReg.contentType == TMP_InputField.ContentType.Password;

        if_passwordReg.contentType = isPasswordRegHide ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

        if_passwordReg.ForceLabelUpdate();
    }
}
