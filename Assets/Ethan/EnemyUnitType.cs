using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CONTAIN DEFAULT VALUES TO ASSIGN TO A UNIT COMPONENT
[CreateAssetMenu]
public class EnemyUnitType : ScriptableObject

{
    //AS LONG AS 2 UNITS NOT FROM SAME TEAM, THEY CAN DAMAGE EACH OTHER
    public enum ENEMY_RACE
    {
        FISH = 0,
        WOLF,

        NUM_TOTAL,
    };

    [Header("Attributes for Character")]
    [SerializeField] protected ENEMY_RACE enemy_race_unit;
    [SerializeField] protected float range_unit;
    [SerializeField] protected int rarity_unit;

    public float RangeDefault { get { return range_unit; } }
    public int RarityDefault { get { return rarity_unit; } }
    public ENEMY_RACE EnemyRace { get { return enemy_race_unit; } }
    //public ItemDrop[] ItemDrops { get { return item_drops; } }
}
