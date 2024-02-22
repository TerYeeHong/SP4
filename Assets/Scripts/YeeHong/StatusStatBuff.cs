using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/StatBuffStatus")]
public class StatusStatBuff : Status
{
    [Header("Stat Buffs")]
    [SerializeField] protected int health_buff = 0;
    [SerializeField] protected int power_buff = 0;
    [SerializeField] protected int speed_buff = 0;

    public override void OnEquip(Unit unit_parent)
    {
        unit_parent.MaxHealth += health_buff;
        unit_parent.TakeDamage(-health_buff);
        unit_parent.Power += power_buff;
        unit_parent.Speed += speed_buff;

        m_have_duration = false;
    }

    public override void OnUnequip(Unit unit_parent)
    {
        unit_parent.MaxHealth -= health_buff;
        unit_parent.TakeDamage(health_buff);
        unit_parent.Power -= power_buff;
        unit_parent.Speed -= speed_buff;
    }
}
