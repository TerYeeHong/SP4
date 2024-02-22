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
    [SerializeField] TMP_InputField if_emailReg;
    [SerializeField] TMP_InputField if_Login;
    [SerializeField] TMP_InputField if_passwordReg;
    [SerializeField] TMP_InputField if_passwordLogin;

    [Header("Canvas")]
    [SerializeField] GameObject loginCanvas;
    [SerializeField] GameObject regCanvas;

    public SceneMgt sceneMgt;

    private int buttonNum;

    void Start()
    {
        startTransition.SetActive(false);
        endTransition.SetActive(true);

        regCanvas.SetActive(false);
        loginCanvas.SetActive(true);
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
            loginCanvas.SetActive(true);
            regCanvas.SetActive(false);
        }
        else if (buttonNum == 2) // login email -> reg
        {
            loginCanvas.SetActive(false);
            regCanvas.SetActive(true);
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

    public void OnButtonLogin()
    {
        string input = if_Login.text;
        string password = if_passwordLogin.text;

        // Create a login request
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = input,
            Password = password,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };

        // Attempt to log in with the provided email
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSucc, error =>
        {
            // If login with email fails, assume input is a username and attempt login with username
            var loginWithUsernameRequest = new LoginWithPlayFabRequest
            {
                Username = input,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };

            PlayFabClientAPI.LoginWithPlayFab(loginWithUsernameRequest, OnLoginSucc, OnError);
        });
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