using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class CustomAnimator : MonoBehaviour
{
    [SerializeField] Animator main_animator;
    [SerializeField] List<VisualEffect> visual_effects_list;

    [SerializeField] string expression_default_trigger;

    public void SetTrigger(string anim_name)
    {
        main_animator.SetTrigger(anim_name);
    }
    public void SetTriggerNewDefaultExpression(string anim_name)
    {
        expression_default_trigger = anim_name;
        main_animator.SetTrigger(anim_name);
    }
    public void SetExpression(string anim_name, float duration)
    {
        main_animator.SetTrigger(anim_name);
        StartCoroutine(ReturnToDefaultExpression(duration));
    }

    public void SetInteger(string anim_name, int value)
    {
        main_animator.SetInteger(anim_name, value);
    }

    public void AnimationEventTrigger(string anim_name)
    {
        //Play all visual effects
        foreach (VisualEffect vfx in visual_effects_list)
        {
            if (vfx.anim_name_trigger == anim_name)
            {
                StartCoroutine(PlayEffect(vfx));
                //StartCoroutine(vfx.PlayEffect());
            }
        }
    }
    IEnumerator PlayEffect(VisualEffect vfx_to_play)
    {
        //Instantiate
        Transform sample_transform = vfx_to_play.visual_effect_go.transform;
        GameObject vfx_temp = Instantiate(vfx_to_play.visual_effect_go,
            sample_transform.position,
            Quaternion.Euler(sample_transform.rotation.eulerAngles));
        vfx_temp.SetActive(false);

        //Display and destroy
        yield return new WaitForSeconds(vfx_to_play.delay);
        vfx_temp.SetActive(true);
        yield return new WaitForSeconds(vfx_to_play.duration);
        Destroy(vfx_temp);
    }
    IEnumerator ReturnToDefaultExpression(float duration)
    {
        yield return new WaitForSeconds(duration);
        main_animator.SetTrigger(expression_default_trigger);
    }
    IEnumerator BlinkLoop(float interval_min, float interval_max)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(interval_min, interval_max));
            if (expression_default_trigger != "eJoy")
                SetExpression("eBlinking", 0.3f);
        }
    }
    private void Update()
    {
        //TEMP

        if (Input.GetKeyDown(KeyCode.V))
        {
            SetTriggerNewDefaultExpression("eJoy");
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            SetTriggerNewDefaultExpression("eAngry");
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            SetExpression("eBlinking", 0.3f);
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            SetTriggerNewDefaultExpression("eDefault");
        }
    }
    private void Start()
    {
        SetTriggerNewDefaultExpression("eDefault");
        StartCoroutine(BlinkLoop(5, 10.0f));
    }
}

[System.Serializable]
public class VisualEffect
{
    public string anim_name_trigger;
    public GameObject visual_effect_go;

    public float delay = 0f;
    public float duration = 0.5f;

    //public IEnumerator PlayEffect()
    //{
    //    yield return new WaitForSeconds(delay);
    //    visual_effect_go.SetActive(true);
    //    yield return new WaitForSeconds(duration);
    //    visual_effect_go.SetActive(false);
    //}
}
