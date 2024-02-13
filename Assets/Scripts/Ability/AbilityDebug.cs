using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Ability Debug")]
public class AbilityDebug : Ability
{
    public override bool OnActivate(GameObject parent)
    {
        base.OnActivate(parent);
        //Debug.Log(name_ability + " Has been activated");
        return true;
    }

    public override void OnFirstDelay(GameObject parent)
    {
        base.OnFirstDelay(parent);
        //Debug.Log(name_ability + " On First Delay");
    }

    public override void OnActive(GameObject parent)
    {
        base.OnActive(parent);
        //Debug.Log(name_ability + " is Active");
    }

    public override void OnLastActive(GameObject parent)
    {
        base.OnLastActive(parent);
        //Debug.Log(name_ability + " On Last Active");
    }
}