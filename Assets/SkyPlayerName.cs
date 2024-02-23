using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Drawing;
using UnityEngine.UIElements;

public class SkyPlayerName : MonoBehaviour
{
    [SerializeField] private GameObject nameTextPrefab;
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (!photonView.IsMine)
        {
            // Instantiate the name text prefab as a child of this GameObject
            // Adjust the position as needed, for example, above the player
            Vector3 namePosition = transform.position + new Vector3(0, 1, 0); // Offset to position the name above the player

            var popup = Instantiate(nameTextPrefab, namePosition, Quaternion.identity, transform);

            // Since the prefab is now a child, its position will be relative to the parent. Set localPosition if you need to adjust its position relative to its parent
            popup.transform.localPosition = new Vector3(0, 1, 0); // Adjust this as needed

            // Access the TextMeshPro component and set the player's name and color
            var temp = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            temp.text = photonView.Owner.NickName;
            temp.faceColor = new UnityEngine.Color(0, 1, 0, 1);

        }
    }
    void Update()
    {
        //transform.forward = Camera.main.transform.forward;
    }
}
