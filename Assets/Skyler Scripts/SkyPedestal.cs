using UnityEngine;

public class SkyPedestal : MonoBehaviour
{
    public bool isOccupied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure your player GameObject has the "Player" tag
        {
            isOccupied = true;
            print("Is Occupied : " + isOccupied);
            SkyWinManager.instance.CheckPedestals();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOccupied = false;
            print("Is Occupied : " + isOccupied);
            SkyWinManager.instance.CheckPedestals();
        }
    }
}
