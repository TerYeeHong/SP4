using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Dash")]
public class AbilityDash : Ability
{
    [Header("Dasb Details")]
    [SerializeField] protected float dash_speed_ability;

    Vector3 dash_direction;
    Rigidbody rigidbody_player;

    public override bool OnActivate(GameObject parent)
    {
        base.OnActivate(parent);

        //Get direction for
        dash_direction = parent.transform.forward;

        //Get rb
        rigidbody_player = parent.GetComponent<Rigidbody>();
        return true;
    }
    public override void OnFirstDelay(GameObject parent)
    {
        base.OnFirstDelay(parent); //this is needed to play animation etc
    }

    public override void OnActive(GameObject parent)
    {
        //Override velocity 
        rigidbody_player.velocity = dash_direction * dash_speed_ability;

    }

    public override void OnLastActive(GameObject parent)
    {
        rigidbody_player.velocity = Vector3.zero;
    }
    public override void OnActiveDelay(GameObject parent)
    {

    }
}


//SPEEDY LINES PP
////float2 toPolar(float2 cartesian)
////{
////    float distance = length(cartesian);
////    float angle = atan2(cartesian.y, cartesian.x);
////    return float2(angle / UNITY_TWO_PI, distance);
////}
//////Pass in screen pos to Polar and use the new UV as uv for noise;