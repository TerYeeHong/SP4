using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class EnemyUnit : Unit
{
    //Unit type
    //Vars
    [Header("REFERENCES")]
    public LayerMask terrain_layer;
    public LayerMask player_layer;
    public LayerMask enemy_layer;
    public GameObject floating_item_prefab;

    public PhotonView photonView;

    //[Header("References")]
    //[SerializeField] protected Transform transform_head;

    //[Header("EnemeyUnit")]
    [SerializeField] protected EnemyUnitType enemy_type;

    

    ////For Testing purposes, you can test this off and set your own unit data
    //[SerializeField] protected bool inherit_from_unitType = true;


    //[Header("Unit Data (Inheriting from EnemyUnitType)")]
    ////[SerializeField] protected bool sprite_faceleft_at_start = true;

    [SerializeField] protected EnemyUnitType.ENEMY_RACE enemy_race_unit;
    [SerializeField] protected float range_unit;
    [SerializeField] protected int rarity_unit;

    //[Header("Experimental")]
    //[SerializeField] protected bool godmode = false;

    //protected bool collide_with_attacks = true;
    //public bool CollideWithAttack { get { return collide_with_attacks; } }
    //protected bool perma_jump = false;
    //public bool PermanentJump { get { return perma_jump; } set { perma_jump = value; } }



    //protected bool can_drop_item = true;
    //public bool UnitDropsItem { get { return can_drop_item; } set { can_drop_item = value; } }

    //protected Animator animator;
    //protected SpriteRenderer sprite_renderer;

    //public UnitType.UNIT_TEAM Team { get { return team_unit; } set { team_unit = value; } }
    //public int MaxHealth { get { return max_health_unit; } set { max_health_unit = value; } }
    //public int Power { get { return power_unit; } set { power_unit = value; } }
    //public float Speed { get { return speed_unit; } set { speed_unit = value; } }
    //public int Health { get { return health_unit; } set { health_unit = value; } }
    
    public EnemyUnitType.ENEMY_RACE EnemyRace { get { return enemy_race_unit; } set { enemy_race_unit = value; } }
    public float Range { get { return range_unit; } set { range_unit = value; } }
    public int Rarity { get { return rarity_unit; } set { rarity_unit = value; } }



    ///// <summary>
    ///// Direction
    ///// </summary>
    //protected Vector3 facing_direction = Vector3.forward; //which direction unit is facing
    //protected Vector3 pointing_direction = Vector3.forward; //which direction to shoot at

    ////Physics stuff
    //protected Rigidbody rigidbody_unit;
    //protected float max_speed = 15f; //Max speed a unit can go for collisions to work properly

    public override void Init()
    {
        
        SetDefaultStat();

        photonView = GetComponent<PhotonView>();
        rigidbody_unit = GetComponent<Rigidbody>();
    }
    public override void SetDefaultStat()
    {
        //base.SetDefaultStat();
        //Get default data from unit_type
        if (inherit_from_unitType
            && enemy_type
            != null)
        {
            team_unit = enemy_type.TeamDefault;
            name_unit = enemy_type.NameDefault;
            max_health_unit = enemy_type.HealthDefault;
            power_unit = enemy_type.PowerDefault;
            speed_unit = enemy_type.SpeedDefault;
            health_unit = max_health_unit;

            enemy_race_unit = enemy_type.EnemyRace;
            range_unit = enemy_type.RangeDefault;
            rarity_unit = enemy_type.RarityDefault;

        }

    }


    ////Accessors and mutators
    //public void SetFacingDirection(Vector2 new_dir)
    //{
    //    facing_direction = new_dir;
    //    UpdateDirection();
    //}
    //public Vector3 GetFacingDirection()
    //{
    //    return facing_direction;
    //}
    //public Vector3 GetPointingDirection()
    //{
    //    return pointing_direction;
    //}
    //public void SetFacincDirection(Vector3 dir)
    //{
    //    facing_direction = dir;
    //}
    //public void SetPointingDirection(Vector3 dir)
    //{
    //    pointing_direction = dir;
    //}

    //public Transform GetHeadTransform()
    //{
    //    return transform_head;
    //}
    //public Vector3 GetDir()
    //{
    //    return transform_head.forward;
    //}

    public override void OnDeath()
    {
        //base.OnDeath();
        //Debug.Log("NIGAAAAAAAAAAAAAA");
        // GameEvents.m_instance.unitDied.Invoke(unit_type.name);
        //Destroy(gameObject, 1.0f);

        //Destroy(rigidbody_unit);
        //if (gameObject.TryGetComponent(out Collider collider))
        //{
        //    collider.enabled = false;
        //}
        //if (gameObject.TryGetComponent(out Dissolve dissolve))
        //{
        //    dissolve.OnDeath(1.0f);
        //}
        photonView.RPC(nameof(SetActive), RpcTarget.All, false);


    }

    //protected void UpdateDirection()
    //{
    //    if (facing_direction.x > 0)
    //    {
    //        sprite_renderer.flipX = sprite_faceleft_at_start;
    //        ;
    //        facing_direction = Vector2.right;
    //    }
    //    else if (facing_direction.x < 0)
    //    {
    //        sprite_renderer.flipX = !sprite_faceleft_at_start;
    //        facing_direction = Vector2.left;
    //    }
    //}

    ////Check whether is touching ground
    //protected bool OnGround(Collision2D collision)
    //{
    //    //Loop thru all contact points in case player is touching a wall and ground at same time 
    //    for (int i = 0; i < collision.contactCount; ++i)
    //    {
    //        if ((terrain_layer.value & (1 << collision.gameObject.layer)) > 0 //touching ground layer
    //            && collision.GetContact(i).normal.normalized.y > 0.5) //Ground is below
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public override bool TakeDamage(int damage)
    {
        if (!enabled)
            return true;

        //Trigger fighting audio
        //GameEvents.m_instance.newAudioEnvironment.Invoke(AudioBgmManager.AUDIO_BGM_TYPE.ACTION);

        if (godmode)
            return false;

        //Randomise it a bit
        if (damage > 0)
            damage += Random.Range(-2, 2);

        ////See if immune, Ignore taking damage at all
        //if (godmode
        //    || (TryGetComponent(out StatusHolder statusholder)
        //    && statusholder.IsActiveStatus("Immunity")
        //    && damage > 0)) //Not healing
        //{
        //    return false;
        //}

        //Debug.Log("Damage taken by " + gameObject);
        health_unit -= damage;

        //Damage pop up
        if (damage > 0)
            GameEvents.m_instance.createTextPopup.Invoke(damage.ToString(), transform.position, Color.white);

        ////Healing cap
        //if (health_unit > max_health_unit)
        //{
        //    health_unit = max_health_unit;
        //}
        //else if (damage < 0)
        //    GameEvents.m_instance.unitTakeDamageEvent.Invoke(transform, Mathf.Abs(damage).ToString(), Color.green);

        //Trigger event that this player unit has changed health
        if (team_unit == UnitType.UNIT_TEAM.PLAYER)
        {
            //GameEvents.m_instance.playerHealthChangeEvent.Invoke(gameObject);
        }

        if (health_unit <= 0)
        {
            //enabled = false;
            //Debug.Log("Unit Dead");
            collide_with_attacks = false;
            OnDeath();

            //Broadcast that this unit with the name has died
            //GameEvents.m_instance.unitDied.Invoke(name_unit);

            return true;
        }


        return false;

    }

    //public virtual void OnDeath()
    //{
    //    //DropLoot();
    //    //gameObject.SetActive(false);
    //    //rigidbody_unit.isKinematic = true;
    //    Destroy(rigidbody_unit);
    //    if (gameObject.TryGetComponent(out Collider collider))
    //    {
    //        collider.enabled = false;
    //    }
    //    if (gameObject.TryGetComponent(out Dissolve dissolve))
    //    {
    //        dissolve.OnDeath(1.0f);
    //    }

    //    GameEvents.m_instance.unitDied.Invoke(unit_type.name);
    //    Destroy(gameObject, 1.0f);
    //}

    public Rigidbody GetRigidbody()
    {
        return rigidbody_unit;
    }
    public float GetSpeed()
    {
        return speed_unit;
    }

    [PunRPC]
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    [PunRPC]
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
