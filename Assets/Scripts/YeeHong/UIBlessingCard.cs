using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBlessingCard : MonoBehaviour
{
    public GameObject ui_selection_complete;

    public static UIBlessingCard m_instance;
    BlessingCard[] blessing_cards = { null, null, null }; 

    //Prefab to instantiate
    public GameObject blessing_card_prefab;
    
    //Keep track of what is selected
    BlessingCard selected_blessing_card = null;

    //GaneEvent managing the current angel statue
    GameObject current_blessing_monument;


    Status[] statuses_on_hold; //Temporary to keep track of what status to display but dont assign yet

    BlessingCard CreateBlessingCard(Status status)
    {
        //Create the card


        GameObject temp = Instantiate(blessing_card_prefab, transform);
        BlessingCard blessing_card = temp.GetComponent<BlessingCard>();
        blessing_card.SetStatus(status);
        blessing_card.SetRarity(); //status must be set already
        blessing_card.SetDescription(); //status must be set already

        return blessing_card;
    }

    public void SetCards(Status[] statuses)
    {
        statuses_on_hold = statuses;
    }

    public void SetStatue(GameObject statue)
    {
        current_blessing_monument = statue;
    }

    public void AssignCardDetails()
    {
        for (int i = 0; i < statuses_on_hold.Length; ++i)
        {
            if (statuses_on_hold[i] != null)
                blessing_cards[i] = CreateBlessingCard(statuses_on_hold[i]);
        }
    }

    private void Awake()
    {
        m_instance = this;
        selected_blessing_card = null;
    }
    public void CardSelected(BlessingCard selected_blessing_card_)
    {
        //Deselect all first
        foreach (BlessingCard blessing_card in blessing_cards)
        {
            if (blessing_card != null)
                blessing_card.Deselect();
        }
        
        //Change selected
        selected_blessing_card = selected_blessing_card_;
    }


    //This is called by the button
    public void ChooseBlessing()
    {
        if (selected_blessing_card == null)
        {
            Debug.Log("No blessing card selected");
            return;
        }

        ui_selection_complete.SetActive(true);
        selected_blessing_card.AddBlessing();

        ////Remove 1 use from statue
        current_blessing_monument.GetComponent<BlessingMonument>().RemoveUse();
    }

    public void Reset()
    {
        //Delete child
        for (int i = gameObject.transform.childCount -1; i >= 0; --i)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }

        ui_selection_complete.SetActive(false);
    }
}
