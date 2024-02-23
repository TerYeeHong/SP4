using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class MultiplayerMessages : MonoBehaviour
{
    //public TextMeshProUGUI text;

    public TMP_InputField message_input_field;
    public TextMeshProUGUI message_display;
    public bool typing = false;
    bool teamOnly = true;

    List<string> chat_display_messages = new();

    private void OnEnable()
    {
        RaiseEvents.OnChatNoticeEvent += ChatNotice;
    }
    private void OnDisable()
    {
        RaiseEvents.OnChatNoticeEvent -= ChatNotice;
    }
    void ChatNotice(string data)
    {
        //data : type of msg, every other var
        string[] dataSplit = data.Split("$@");
        int team;
        switch (int.Parse(dataSplit[0]))
        {
            //0:chat msg (sendername, team, msg) team=-1 means send to everyone
            case 0:
                //int client_team = (int)ship_player.Team;
              
                {
                    AddNewChatMessage($"{dataSplit[1]}: " + dataSplit[2]);
                }
                break;
            //system msg, Welcome msg (joiner, team)
            case 1:
               // team = int.Parse(dataSplit[2]);
                //if (team == (int)ship_player.Team)
                {
                    AddNewChatMessage($"{dataSplit[1]} Has Died!");
                    //Debug.Log("jigga");
                }
                break;
            case 2:
                // team = int.Parse(dataSplit[2]);
                //if (team == (int)ship_player.Team)
                {
                    AddNewChatMessage($"{dataSplit[1]} Has Died!");
                    //Debug.Log("jigga");
                }
                break;
            case 3:
                // team = int.Parse(dataSplit[2]);
                //if (team == (int)ship_player.Team)
                {
                    AddNewChatMessage($"{dataSplit[1]} Has Joined!");
                    //Debug.Log("jigga");
                }
                break;
            case 4:
                // team = int.Parse(dataSplit[2]);
                //if (team == (int)ship_player.Team)
                {
                    AddNewChatMessage($"{dataSplit[1]} Left!");
                    //Debug.Log("jigga");
                }
                break;
        }
    }
    

    public void ToggleToTeam(bool toggleEnable)
    {
        //teamOnly = toggleEnable;
        teamOnly = !teamOnly;
    }

    public void EnterMessage()
    {
        if (message_input_field.text == "")
            return;
        //if (message_display.enabled)
        //    return;
        //int team = (teamOnly) ? (int) ship_player.Team : -1;

        //data: chat msg (0, sendername, team, msg)
        string dataSent = $"0$@" +
            $"{PhotonNetwork.LocalPlayer.NickName}" +
        // $"$@{team}" +
            $"$@{message_input_field.text}";

        // You would have to set the Receivers to ALL in order to receive this event on the local client as well
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(RaiseEvents.CHATNOTICE, dataSent, raiseEventOptions, SendOptions.SendReliable);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            EnterMessage();
            message_input_field.text = "";
            typing = false;
        }


        if(Input.GetKeyDown(KeyCode.Tab))
        {
            message_display.enabled = !message_display.enabled;
        }

        if (Input.GetKeyDown(KeyCode.Slash))
        {
            typing = true;
            // Check if the input field is not focused before selecting it
            if (!message_input_field.isFocused)
            {
                message_input_field.Select();
                message_input_field.ActivateInputField();
                EnterMessage();
            }
        }

        if(!message_display.enabled)
        {
            typing = false;
        }

    }

    void AddNewChatMessage(string msg)
    {
        int max = 5;
        chat_display_messages.Add(msg);

        if (chat_display_messages.Count > max)
            chat_display_messages.RemoveAt(0);

        UpdateChatDisplay();
    }
    void UpdateChatDisplay()
    {
        int size = chat_display_messages.Count;
        string final_msg = "";
        for (int i = size -1; i >= 0; --i)
        {
            final_msg += $"{chat_display_messages[i]}\n";
        }

        message_display.text = final_msg;
    }
}
