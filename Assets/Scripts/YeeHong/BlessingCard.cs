using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlessingCard : MonoBehaviour
{
    Status status;

    [Header("Blessing Card Details")]
    public Color selected_color;
    public Color unselected_color;
    public Image[] rarity_image;
    public TextMeshProUGUI description_text;
    public Image icon_image;
    public TextMeshProUGUI name_tmp;

    Image image;
    public Color mini_blessing_color;
    public Color moderate_blessing_color;
    public Color divine_blessing_color;
    
    private void Awake()
    {
        image = GetComponent<Image>();
        Deselect();

    }

    public void Select()
    {
        UIBlessingCard.m_instance.CardSelected(this);
        image.color = selected_color;
    }

    public void Deselect()
    {
        image.color = unselected_color;
    }
    public void SetStatus(Status status_)
    {
        status = status_;

        SetRarity();
        SetDescription();
        SetIcon();
        SetName();
    }

    public void AddBlessing()
    {
        Unit player_unit = null;
        GameObject[] all_players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in all_players)
        {
            if (player.GetComponent<SkyPlayerController>().GetPhotonView.IsMine)
                player_unit = player.GetComponent<Unit>();
        }

        PFGlobalData.AddBlessing(status, player_unit);


        //status.OnEquip(GameObject.FindWithTag("Player").GetComponent<Unit>());
    }

    public void SetRarity()
    {
        //Set the card cosmetics design
        switch (status.Rarity)
        {
            case Status.STATUS_RARITY.MINI:
                foreach (Image image in rarity_image)
                {
                    image.color = mini_blessing_color;
                }
                break;
            case Status.STATUS_RARITY.MODERATE:
                foreach (Image image in rarity_image)
                {
                    image.color = moderate_blessing_color;
                }
                break;
            case Status.STATUS_RARITY.DIVINE:
                foreach (Image image in rarity_image)
                {
                    image.color = divine_blessing_color;
                }
                break;
        }
    }

    public void SetDescription()
    {
        description_text.text = status.Description;
    }
    public void SetIcon()
    {
        icon_image.sprite = status.Icon_status;
    }
    public void SetName()
    {
        name_tmp.text = status.Name_status;
    }
}
