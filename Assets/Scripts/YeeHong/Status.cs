using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/DefaultStatus")]
public class Status : ScriptableObject
{
    //Details to set
    [Header("Status Details")]
    [SerializeField] protected Sprite icon;
    [SerializeField] protected string m_name_status;
    [SerializeField] protected bool m_have_duration = true;
    [SerializeField] protected float m_active_duration;

    [Header("Blessing details")]
    [SerializeField] protected STATUS_RARITY m_rarity;
    [SerializeField] protected string m_description;
    protected bool blessing_permanent = false; //set to true only when beat level

    public enum STATUS_RARITY
    {
        MINI,
        MODERATE,
        DIVINE,
    }

    //Private stuff
    private bool m_active = false;
    private float m_active_duration_timer = 0;

    public string Name_status { get { return m_name_status; } }
    public Sprite Icon_status { get { return icon; } }
    public float Duration_status { get { return m_active_duration; } set { m_active_duration = value; } }
    public bool Active_status { 
        get { return m_active; } 
        set { m_active = value; m_active_duration_timer = m_active_duration; } 
    }
    public STATUS_RARITY Rarity { get { return m_rarity; } }
    public string Description { get { return m_description; } }
    public bool Permanent { get { return blessing_permanent; } set { blessing_permanent = value; } }

    public virtual void OnEquip(Unit unit_parent)
    {
        m_have_duration = false;
    }

    public virtual void OnUnequip(Unit unit_parent)
    {
    }


    public void Update()
    {
        //If status is not active
        if (!(m_active && m_have_duration))
            return;

        //Countdown
        if (m_active_duration_timer > 0)
        {
            m_active_duration_timer -= Time.deltaTime;
        }
        //Set status to no longer active
        else
        {
            m_active = false;
        }
    }
}
