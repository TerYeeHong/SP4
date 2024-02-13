using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeSlash : MonoBehaviour
{
    public Unit parent;
    public int damage;
    public float knockup_force;

    List<GameObject> targets_hit = new();

    private void FixedUpdate()
    {
        RaycastHit hit;
        Vector3 distance = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        if (Physics.Raycast(distance, transform.TransformDirection(-Vector3.up), out hit, GameEvents.m_instance.AllLayerMask)) {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!targets_hit.Contains(other.gameObject) && other.TryGetComponent(out Unit unit))
        {
            if (unit.Team != parent.Team)
            {
                //Damage them
                unit.TakeDamage(damage);
                targets_hit.Add(other.gameObject);

                //knock back
                if (other.TryGetComponent(out Rigidbody rb))
                {
                    Vector3 dir = other.transform.position - transform.position;
                    dir.y = 0;
                    dir = dir.normalized;
                    //knock up is way stronger than sideways
                    rb.AddForce(new Vector3(dir.x * knockup_force, 3 * knockup_force, dir.z * knockup_force));
                }
            }


            
        }
    }
}
