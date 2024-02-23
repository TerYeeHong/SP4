using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Material mat_invisible;
    [SerializeField] Material mat_original;
    [SerializeField] Renderer renderer_self;

    [Header("Custom dissolve settings")]
    [SerializeField] float dissolveSpeed;

    float dissolveAmt;

    //public void SetInvisible(bool invi)
    //{
    //    if (invi)
    //    {
    //        renderer_self.material = mat_invisible;
    //    }
    //    else
    //    {
    //        renderer_self.material = mat_original;
    //    }
    //}

    private void Start()
    {
        renderer_self.material = mat_invisible;
    }

    public void SetDissolveAmt(float amt)
    {
        renderer_self.material.SetFloat("_DissolveAmt", amt);

        //if (amt <= -1.1f)
        //    SetVisible();
        //else
        //    renderer_self.material.SetFloat("_DissolveAmt", amt);
    }
    public void SetVisible()
    {
        renderer_self.material = mat_original;
    }


    public void StartDissolveIn()
    {
        renderer_self.material = mat_invisible;
        StartCoroutine(nameof(DissolveInEffect));
    }
    public void StartDissolveOut()
    {
        renderer_self.material = mat_invisible;
        StartCoroutine(nameof(DissolveOutEffect));
    }

    IEnumerator DissolveInEffect()
    {
        dissolveAmt = 1.2f;
        while (dissolveAmt > -1.1f)
        {
            dissolveAmt -= Time.deltaTime * dissolveSpeed;
            SetDissolveAmt(dissolveAmt);
            yield return null;
        }
    }
    IEnumerator DissolveOutEffect()
    {
        dissolveAmt = 0.0f;
        while (dissolveAmt < 1.2f)
        {
            dissolveAmt += Time.deltaTime * dissolveSpeed;
            SetDissolveAmt(dissolveAmt);
            yield return null;
        }
    }


}
