// Made by: Matt Palero

using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PFUserMgt : MonoBehaviour
{
    [SerializeField] TMP_Text msgBox;

    [Header("Transitions")]
    [SerializeField] private GameObject startTransition;
    [SerializeField] private GameObject endTransition;

    [Header("Input Fields")]
    [SerializeField] TMP_InputField if_usernameReg;
    [SerializeField] TMP_InputField if_usernameLogin;
    [SerializeField] TMP_InputField if_emailReset;
    [SerializeField] TMP_InputField if_emailReg;
    [SerializeField] TMP_InputField if_emailLogin;
    [SerializeField] TMP_InputField if_passwordReg;
    [SerializeField] TMP_InputField if_passwordLoginEmail;
    [SerializeField] TMP_InputField if_passwordLoginUser;

    [Header("Canvas")]
    [SerializeField] GameObject loginEmailCanvas;
    [SerializeField] GameObject loginUserCanvas;
    [SerializeField] GameObject regCanvas;
    [SerializeField] GameObject resetPassCanvas;

    public SceneMgt sceneMgt;

    private int buttonNum;

    void Start()
    {
        startTransition.SetActive(false);
        endTransition.SetActive(true);

        regCanvas.SetActive(false);
        resetPassCanvas.SetActive(false);
        loginEmailCanvas.SetActive(true);
        loginUserCanvas.SetActive(false);
    }

    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        msgBox.text = msg;
    }

    public void ButtonPressed(int butttonNo)
    {
        buttonNum = butttonNo;
    }

    public void ShowCanvas()
    {
        if (buttonNum == 1) //reg -> login email
        {
            loginEmailCanvas.SetActive(true);
            regCanvas.SetActive(false);
        }
        else if (buttonNum == 2) // login email -> reg
        {
            loginEmailCanvas.SetActive(false);
            regCanvas.SetActive(true);
        }
        else if (buttonNum == 3) // login email -> reset
        {
            resetPassCanvas.SetActive(true);
            loginEmailCanvas.SetActive(false);
        }
        else if (buttonNum == 4) // reset -> login email
        {
            resetPassCanvas.SetActive(false);
            loginEmailCanvas.SetActive(true);
        }
        else if (buttonNum == 5) // login email -> login username
        {
            loginEmailCanvas.SetActive(false);
            loginUserCanvas.SetActive(true);
        }
        else if (buttonNum == 6) // login username -> login email
        {
            loginEmailCanvas.SetActive(true);
            loginUserCanvas.SetActive(false);
        }
    }

    public void OnButtonRegUser() //request register
    {
        var regReq = new RegisterPlayFabUserRequest //create req object
        {
            Email = if_emailReg.text,
            Password = if_passwordReg.text,
            Username = if_usernameReg.text
        };
        //execute request by calling playfab api
        PlayFabClientAPI.RegisterPlayFabUser(regReq, OnRegSucc, OnError);
    }

    void OnRegSucc(RegisterPlayFabUserResult r) //function to handle register success
    {
        msgBox.color = Color.white;
        UpdateMsg("Successfully Registered!");

        // to create a player display name
        var req = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = if_usernameReg.text,
        };
        //update to profile
        PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnError);
    }

    public void OnButtonResetPassword() //resest password request
    {
        var resetReq = new SendAccountRecoveryEmailRequest //create req object
        {
            Email = if_emailReset.text,
            TitleId = "303F8"
        };
        //execute request by calling playfab api
        PlayFabClientAPI.SendAccountRecoveryEmail(resetReq, OnResetSucc, OnError);
    }

    void OnResetSucc(SendAccountRecoveryEmailResult r)
    {
        msgBox.color = Color.white;
        UpdateMsg("Reset Successful! Check your Email to reset password");
    }

    public void OnGuestLogin()// guest login
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = "guest@guest.com",
            Password = "guest123",

            //to get player profile, to get display name
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSucc, OnError);
    }

    public void OnButtonLoginEmail()// login using email + password
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = if_emailLogin.text,
            Password = if_passwordLoginEmail.text,
            //to get player profile, to get display name
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSucc, OnError);
    }

    public void OnButtonLoginUserName()// login using username + password
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = if_usernameLogin.text,
            Password = if_passwordLoginUser.text,
            //to get player profile, to get display name
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSucc, OnError);
    }

    void OnLoginSucc(LoginResult r)
    {
        msgBox.color = Color.white;
        UpdateMsg("Login Success!");

        startTransition.SetActive(true);
        Invoke("ChangeScene", 1.7f);
    }

    private void ChangeScene()
    {
        sceneMgt.ChangeScene("MainMenu");
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        msgBox.color = Color.white;
        UpdateMsg("Please Login");
    }

    void OnError(PlayFabError e) //function to handle error
    {
        string errorMessage = "Error: " + e.ErrorMessage;

        // Check if additional error details are available
        if (e.ErrorDetails != null && e.ErrorDetails.ContainsKey("details"))
        {
            errorMessage += "\nDetails: " + e.ErrorDetails["details"];
        }

        // Display the error message
        msgBox.color = Color.red;
        UpdateMsg(errorMessage);
    }
}