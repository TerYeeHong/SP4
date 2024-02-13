using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Combat/StrikeSlash")]
public class AbilityStrikeSlash : AbilityStrike
{
    [Header("Slash Details")]
    [SerializeField] GameObject slash_prefab;
    [SerializeField] protected float slash_speed;
    [SerializeField] protected float slash_lifetime;

    [SerializeField] protected int base_damage_slash;
    [SerializeField] protected float power_percentage_slash;
    [SerializeField] protected float knockback_force_slash;


    protected Vector3 direction_horizontal;

    public override bool OnActivate(GameObject parent)
    {
        base.OnActivate(parent);

        //ignore y direction
        direction_horizontal = direction;
        direction_horizontal.y = 0;

        //Create slash
        GameObject slash = Instantiate(slash_prefab, parent.transform.position + direction, Quaternion.LookRotation(direction_horizontal));
        Rigidbody slash_rb = slash.GetComponent<Rigidbody>();
        slash_rb.velocity = slash_speed * direction;

        StrikeSlash strikeSlash = slash.GetComponent<StrikeSlash>();

        strikeSlash.parent = unit_parent;
        strikeSlash.damage = base_damage_slash +(int)( unit_parent.Power * power_percentage_slash);
        strikeSlash.knockup_force = knockback_force_slash;

        Destroy(slash, slash_lifetime);
        
        return true;
    }
    public override void OnFirstDelay(GameObject parent)
    {
        base.OnFirstDelay(parent); //this is needed to play animation etc

       
    }

    public override void OnActive(GameObject parent)
    {
        base.OnActive(parent);

    }

    public override void OnLastActive(GameObject parent)
    {
        base.OnLastActive(parent);
    }
    public override void OnActiveDelay(GameObject parent)
    {
        base.OnActiveDelay(parent);
    }
}
