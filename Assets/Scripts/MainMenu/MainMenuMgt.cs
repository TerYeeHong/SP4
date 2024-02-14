using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMgt : MonoBehaviour
{
    public GameObject bg;

    [SerializeField] private GameObject endTransition;

    // Start is called before the first frame update
    void Start()
    {
        endTransition.SetActive(true);
        bg.transform.LeanMoveLocal(new Vector2(48, -50), 1.5f).setEaseOutSine().setLoopPingPong();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
