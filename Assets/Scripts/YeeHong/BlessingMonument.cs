using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlessingMonument : MonoBehaviour
{
    public List<BlessingCardOdds> blessing_cards;
    protected List<BlessingCardOdds> blessing_cards_copy;
    //public GameObject button_angel_statue;

    public int choices = 1;
    public int uses = 1;

    bool assigned = false;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Player"))
    //    {
    //        if (!assigned)
    //        {
    //            //make a copy
    //            blessing_cards_copy = new List<BlessingCardOdds>();
    //            foreach (var item in blessing_cards)
    //            {
    //                blessing_cards_copy.Add(new BlessingCardOdds
    //                {
    //                    status = item.status,
    //                    odds = item.odds
    //                });
    //            }

    //            //Assign using the copy
    //            AssignBlessingCards();
    //            assigned = true;
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Player"))
    //    {
    //        assigned = false;
    //    }
    //}

    public void Activate()
    {
        if (!assigned)
        {
            //make a copy
            blessing_cards_copy = new List<BlessingCardOdds>();
            foreach (var item in blessing_cards)
            {
                blessing_cards_copy.Add(new BlessingCardOdds
                {
                    status = item.status,
                    odds = item.odds
                });
            }

            //Assign using the copy
            AssignBlessingCards();
            assigned = true;

            Activated();
        }
    }



    private void Awake()
    {
        //button_angel_statue = GameObject.Find("Canvas").transform.Find("ButtonUI").Find("ButtonAngelStatue").gameObject;
        //blessing_cards = new List<BlessingCardOdds>();
    }

    public void Activated()
    {
        gameObject.SetActive(false);
    }

    BlessingCardOdds ChooseRandomBlessingCard()
    {
        //Get total odds first
        int total_odds = 0;
        foreach (BlessingCardOdds blessing_card in blessing_cards_copy)
        {
            total_odds += blessing_card.odds;
        }

        foreach (BlessingCardOdds blessing_card in blessing_cards_copy)
        {
            //If odds is this
            int rand = Random.Range(1, total_odds + 1);
            if (rand <= blessing_card.odds)
            {
                return blessing_card;
            }

            total_odds -= blessing_card.odds;
        }

        return null;
    }

    void AssignBlessingCards()
    {
        //max 3 statuses
        Status[] statuses_temp = { null, null, null };

        //Choice
        if (choices > 3)
            choices = 3;

        //Choose a random blessing
        for (int i = 0; i < choices; ++i)
        {
            BlessingCardOdds randomised_blessing = ChooseRandomBlessingCard();
            //if (randomised_blessing == null)
            //{
            //    Debug.LogError("Randomised shouldnt be null");
            //    return;
            //}
            //Debug.LogWarning(randomised_blessing.status.name);


            statuses_temp[i] = randomised_blessing.status;
            //Pop the blessing
            blessing_cards_copy.Remove(randomised_blessing);
        }

        //Set the cards onto the UI
        BlessingWindowMain.m_instance.window_main.SetActive(true);
        UIBlessingCard.m_instance.SetCards(statuses_temp);
        UIBlessingCard.m_instance.SetStatue(gameObject);
        UIBlessingCard.m_instance.AssignCardDetails();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameEvents.m_instance.onLockInput.Invoke(true);

        


    }

    public void RemoveUse()
    {
        uses--;

        //reset
        //button_angel_statue.SetActive(false);
        assigned = false;
        GetComponent<Collider>().enabled = false;

        if (uses > 0)
        {
            GetComponent<Collider>().enabled = true;
        }
    }
}

[System.Serializable]
public class BlessingCardOdds
{
    public Status status;
    public int odds;
}