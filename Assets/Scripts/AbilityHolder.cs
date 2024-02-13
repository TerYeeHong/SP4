using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityHolder : MonoBehaviour
{
    [SerializeField] List<AbilityPlayer> ability_list;

    [SerializeField] Transform shoot_origin_transform;
    public Transform ShootOrigin { get { return shoot_origin_transform; } }

    //TEMP
    //TODO: MAKE IT USUABLE FOR OTHER ENTITIES AS WELL
    //CURRENT SET UP USES A UNITY EVENT CALL THAT IS MADE FOR THE PLAYER

    public List<AbilityPlayer> AbilityList { get { return ability_list; } }

    private void Start()
    {
        int size = ability_list.Count;
        for (int i = 0; i < size; ++i)
        {
            //Make a copy of the scriptable object, we dont want the original asset file to be editted
            Ability ability_clone = Instantiate(ability_list[i].ability);
            ability_clone.Init();
            //GameEvents.m_instance.useNewAbility.Invoke(ability_clone, ability_list[i].ability_type);
        }
    }
}

[System.Serializable]
public class AbilityPlayer
{
    public Ability ability;
    public PlayerController.PLAYER_ABILITY ability_type;
}
