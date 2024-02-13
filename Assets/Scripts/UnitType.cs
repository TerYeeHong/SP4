using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CONTAIN DEFAULT VALUES TO ASSIGN TO A UNIT COMPONENT
[CreateAssetMenu]
public class UnitType : ScriptableObject
{
    //AS LONG AS 2 UNITS NOT FROM SAME TEAM, THEY CAN DAMAGE EACH OTHER
    public enum UNIT_TEAM
    {
        PLAYER = 0,
        ENEMY,

        TEAM_ONE,
        TEAM_TWO,
        TEAM_THREE,
        TEAM_FOUR,
    };

    [Header("Attributes for Character")]
    //[SerializeField] private int ID_unit;
    [SerializeField] protected UNIT_TEAM team_unit = UNIT_TEAM.ENEMY;
    [SerializeField] protected string name_unit;
    [SerializeField] protected int health_unit;
    [SerializeField] protected int power_unit;
    [SerializeField] protected float speed_unit;

    //[SerializeField] protected ItemDrop[] item_drops;

    //AS THIS SERVE AS THE BASIC UNIT DATA, THEY SHOULD NOT BE MANIPULATED IN ANY WAY.
    //MANIPULATE THE UNIT DATA INSIDE THE UNIT.CS COMPONENT, THIS IS A PLACEHOLDER FOR DEFAULT DATA
    public UNIT_TEAM TeamDefault { get { return team_unit; } }
    public string NameDefault { get { return name_unit; } }
    public int HealthDefault { get { return health_unit; } }
    public int PowerDefault { get { return power_unit; } }
    public float SpeedDefault { get { return speed_unit; } }
    //public ItemDrop[] ItemDrops { get { return item_drops; } }
}
