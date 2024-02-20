using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Ability/Combat/Beam")]
public class AbilityBeam : Ability
{
    [Header("Beam Details")]
    [SerializeField] protected int base_damage_ability;
    [SerializeField] protected float power_percentage_ability;
    [SerializeField] protected GameObject projectile_prefab;

    //[SerializeField] protected LayerMask 
    float bullet_speed = 20.0f;

    public override bool OnActivate(GameObject parent)
    {
        base.OnActivate(parent);

        Unit parent_unit = parent.GetComponent<Unit>();

        //Show UI
        Camera cam = Camera.main;
        RaycastHit rayhit;

        //LayerMask enemyLayerMask = (parent.GetComponent<Unit>().Team == UnitType.UNIT_TEAM.PLAYER) ?
        //    GameEvents.m_instance.EnemyLayermask : GameEvents.m_instance.PlayerLayermask;

        Ray clickray = cam.ScreenPointToRay(Input.mousePosition);
        //Debug.Log(Input.mousePosition);
        if (Physics.Raycast(clickray,
            out rayhit,
            1000,
            GameEvents.m_instance.AllLayerMask))
        {
            Debug.Log("RayHit");
            //If found smth, shoot projectile towards it
            Transform fire_point = parent.GetComponent<AbilityHolder>().ShootOrigin;
            Vector3 direction = (rayhit.point
                - fire_point.position).normalized;

            GameObject projectile_go = Instantiate(projectile_prefab, fire_point.position, Quaternion.identity);
            Projectile projectile = projectile_go.GetComponent<Projectile>();
            projectile.SetDamage(base_damage_ability + (int)(parent_unit.Power * power_percentage_ability));
            projectile.SetTeam(parent_unit.Team);
            projectile.SetVelocity(direction * bullet_speed);

        }
        //found nothing
        else
        {
            //If found smth, just shoot forward
            Transform fire_point = parent.GetComponent<AbilityHolder>().ShootOrigin;
            Vector3 direction = fire_point.forward;

            GameObject projectile_go = Instantiate(projectile_prefab, fire_point.position, Quaternion.identity);
            Projectile projectile = projectile_go.GetComponent<Projectile>();
            projectile.SetDamage(base_damage_ability + (int)(parent_unit.Power * power_percentage_ability));
            projectile.SetTeam(parent_unit.Team);
            projectile.SetVelocity(direction * bullet_speed);
        }

        return true;
    }

    public override void OnFirstDelay(GameObject parent)
    {
        base.OnFirstDelay(parent);
    }

    public override void OnActive(GameObject parent)
    {
        //See can strike how many times, each time cast the zone and detect all enemies, damage them
        //TODO
    }

    public override void OnLastActive(GameObject parent)
    {

    }
}
