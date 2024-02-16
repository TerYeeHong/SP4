using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Combat/Strike")]
public class AbilityStrike : Ability
{
    [Header("Strike Details")]
    [SerializeField] protected int base_damage_ability;
    [SerializeField] protected float power_percentage_ability;
    [SerializeField] protected ABILITY_STRIKE_ZONE_TYPE zone_type_ability = ABILITY_STRIKE_ZONE_TYPE.SPHERE;
    [SerializeField] protected Vector3 zone_size_ability;
    [SerializeField] protected float move_forward_speed = 1;
    [SerializeField] protected float knockback_force_ability;

    [Tooltip("Zone distance away only needed for TOWARDS_ENEMY type of zone casting")]
    [SerializeField] protected ABILITY_STRIKE_ZONE_CASTING zone_casting_ability = ABILITY_STRIKE_ZONE_CASTING.TOWARDS_ENEMY;
    [SerializeField] protected float zone_distance_away_ability;

    [SerializeField] protected Vector3 detection_zone_size_ability;

    [Tooltip("Strike total times during Active and their interval")]
    [SerializeField] protected int strike_times = 1;
    [SerializeField] protected float interval_to_next_strike;


    //Assigned when activated
    protected Transform nearest_target;
    protected Vector3 direction;
    protected Rigidbody rigidbody_parent;
    protected Unit unit_parent;
    protected LayerMask target_layermask;

    public enum ABILITY_STRIKE_ZONE_CASTING
    {
        TOWARDS_ENEMY = 0,
        ON_ENEMY,
    }
    public enum ABILITY_STRIKE_ZONE_TYPE
    {
        SPHERE = 0,
        BOX,
    }

    public override bool OnActivate(GameObject parent)
    {
        base.OnActivate(parent);

        //DAMAGE TARGETS IN DIRECTION
        switch (zone_casting_ability)
        {
            case ABILITY_STRIKE_ZONE_CASTING.TOWARDS_ENEMY:
                Vector3 centre_zone = parent.transform.position + direction * zone_distance_away_ability;

                //Collider[] colliders;
                if (HandleCastingZone(centre_zone, target_layermask, out Collider[] colliders))
                {
                    foreach (Collider collider in colliders)
                    {
                        //if (collider.TryGetComponent(out Unit unit_target))
                        //{


                        //    //Debug.Log(base_damage_ability + (int)(power_percentage_ability * unit_parent.Power));
                        //    unit_target.TakeDamage(base_damage_ability + (int)(power_percentage_ability * unit_parent.Power));
                        //}
                        if (collider.TryGetComponent(out EnemyController unit_target))
                        {


                            //Debug.Log(base_damage_ability + (int)(power_percentage_ability * unit_parent.Power));
                            unit_target.TakeDamage(base_damage_ability + (int)(power_percentage_ability * unit_parent.Power));
                        }
                        if (collider.TryGetComponent(out Rigidbody rb_target))
                        {
                            Vector3 knockback_dir = collider.transform.position - parent.transform.position;
                            knockback_dir.y = 0;
                            knockback_dir = knockback_dir.normalized;
                            rb_target.AddForce(knockback_force_ability * knockback_dir + Vector3.up * 5);
                        }
                    }
                }
                break;
            case ABILITY_STRIKE_ZONE_CASTING.ON_ENEMY:
                //todo
                break;
        }
        
        return true;
    }
    bool HandleCastingZone(Vector3 centre_zone, LayerMask target_layermask, out Collider[] colliders)
    {
        switch (zone_type_ability)
        {
            case ABILITY_STRIKE_ZONE_TYPE.SPHERE:
                colliders = Physics.OverlapSphere(centre_zone, zone_size_ability.x * 0.5f, target_layermask);
                return colliders.Length > 0;
               
            case ABILITY_STRIKE_ZONE_TYPE.BOX:
                colliders = Physics.OverlapBox(centre_zone, zone_size_ability * 0.5f, Quaternion.identity, target_layermask);
                return colliders.Length > 0;
        }
        colliders = null;
        return false;
    }
    public override void OnFirstDelay(GameObject parent)
    {
        base.OnFirstDelay(parent); //this is needed to play animation etc


        //Dont need check, if dont have these, this ability will not work anyway
        rigidbody_parent = parent.GetComponent<Rigidbody>();
        unit_parent = parent.GetComponent<Unit>();

        //Reset nearest target
        nearest_target = null;
        Collider[] all_targets = null;

        // 1. FIND NEAREST ENEMY
        //if player is the one casting this ability
        if (unit_parent.Team == UnitType.UNIT_TEAM.PLAYER)
            target_layermask = GameEvents.m_instance.EnemyLayermask;
        else
            target_layermask = GameEvents.m_instance.PlayerLayermask;

        //find enemies
        all_targets = Physics.OverlapBox(parent.transform.position, detection_zone_size_ability,
            Quaternion.identity, target_layermask);

        //Find the Closest, set as target
        if (all_targets != null
            && all_targets.Length > 0)
        {

            Transform closest = all_targets[0].transform; //default
            float closest_distance = Vector3.Distance(closest.position, parent.transform.position);

            int size = all_targets.Length;
            for (int i = 1; i < size; ++i) //skip the first one since its already assigned as default
            {
                float curr_distance = Vector3.Distance(all_targets[i].transform.position, parent.transform.position);

                //Compare
                if (curr_distance < closest_distance)
                {
                    closest_distance = curr_distance;
                    closest = all_targets[i].transform;
                }
            }

            //All comparisons done, closest is found
            nearest_target = closest;
        }

        //2 scenarios
        //1. swing to nearest if found
        if (nearest_target != null)
        {
            direction = nearest_target.position - parent.transform.position;
        }
        //2. just strike forward
        else
        {
            //direction = unit.GetFacingDirection();
            direction = parent.transform.forward;
        }

        //Normalize direction
        if (direction != Vector3.zero)
            direction = direction.normalized;
    }

    public override void OnActive(GameObject parent)
    {
        unit_parent.SetFacincDirection(direction);
        unit_parent.MovementLock = true; //lock input 

        //See can strike how many times, each time cast the zone and detect all enemies, damage them
        //TODO

    }

    public override void OnLastActive(GameObject parent)
    {

    }
    public override void OnActiveDelay(GameObject parent)
    {
        //moves forward
        rigidbody_parent.velocity = direction * move_forward_speed;
        unit_parent.SetFacincDirection(direction);
        unit_parent.MovementLock = true; //lock input 
    }
}
