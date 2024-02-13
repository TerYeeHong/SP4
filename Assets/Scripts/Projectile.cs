using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] Rigidbody rigidbody_projectile;
    [SerializeField] GameObject collision_impact_vfx_prefab;

    //LayerMask targetLayer;
    int damage = 1;
    UnitType.UNIT_TEAM shooter_team;


    public void SetTeam(UnitType.UNIT_TEAM shooter_team)
    {
        this.shooter_team = shooter_team;
    }
    //public void SetLayer(LayerMask targetLayer)
    //{
    //    this.targetLayer = targetLayer;
    //}
    public void SetDamage(int damage)
    {
        this.damage = damage;
    }
    public void SetVelocity(Vector3 velocity)
    {
        rigidbody_projectile.velocity = velocity;
    
    }

    private void Start()
    {
        Destroy(gameObject, 10.0f);    
    }


    private void OnTriggerEnter(Collider other)
    {
        //if collides with target layer
        if ((GameEvents.m_instance.AllLayerMask.value & (1 << other.gameObject.layer)) > 0){
            //try damage the collided
            if (other.TryGetComponent(out Unit unit)
                && unit.Team != shooter_team)
            {
                unit.TakeDamage(damage);
            }

            OnCollide();

            if (collision_impact_vfx_prefab != null)
            {
                Vector3 displace_normal = gameObject.transform.position - other.ClosestPoint(gameObject.transform.position);
                GameObject vfx_go = Instantiate(collision_impact_vfx_prefab, gameObject.transform.position + displace_normal, Quaternion.identity);
                vfx_go.SetActive(true);
                Destroy(vfx_go, 1.0f);

                
            }

            //Already collided, disable self
            //gameObject.SetActive(false);
            Destroy(gameObject, 0);
        }
    }

    void OnCollide()
    {

    }
}
