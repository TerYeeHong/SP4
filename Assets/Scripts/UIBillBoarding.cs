using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillBoarding : MonoBehaviour
{
    private Camera cam;

    // Start is called before the first frame update
    void Awake()
    {
        cam = Camera.main;


    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = cam.transform.forward;
    }
}
