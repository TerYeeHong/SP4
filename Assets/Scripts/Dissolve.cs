using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [SerializeField] Unit unit;
    [SerializeField] MeshRenderer renderer_go;

    Material mat;

    private void Start()
    {
        mat = renderer_go.material;
    }
    public void OnDeath(float duration)
    {
        StartCoroutine(DissolveEffect(duration));
    }

    IEnumerator DissolveEffect(float duration)
    {
        while (duration > 0)
        {
            
            mat.SetFloat("_Dissolve_Value", duration);
            yield return null;
            duration -= Time.deltaTime;
        }
    }
}
