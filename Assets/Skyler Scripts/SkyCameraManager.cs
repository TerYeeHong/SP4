using UnityEngine;
using System.Collections.Generic;

public class SkyCameraManager : MonoBehaviour
{
    public static SkyCameraManager Instance;

    private Dictionary<int, Camera> playerCameras = new Dictionary<int, Camera>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void LogRegisteredCameras()
    {
        foreach (var item in playerCameras)
        {
            int playerID = item.Key;
            Camera camera = item.Value;
            string cameraName = camera != null ? camera.name : "null";

            Debug.Log($"Player ID: {playerID}, Camera: {cameraName}");
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SkyCameraManager.Instance.LogRegisteredCameras();
        }
    }

    public void RegisterCamera(int playerID, Camera camera)
    {
        playerCameras[playerID] = camera;
    }

    public void EnableCamera(int playerID, bool enable)
    {
        if (playerCameras.TryGetValue(playerID, out Camera camera))
        {
            camera.enabled = enable;
            if (camera.TryGetComponent(out AudioListener audioListener))
            {
                audioListener.enabled = enable;
            }
        }
    }
    public void SwitchCamera(int targetPlayerID)
    {
        print("Switching to : " + targetPlayerID);
        foreach (var kvp in playerCameras)
        {
            bool enable = kvp.Key == targetPlayerID;
            kvp.Value.enabled = enable;
            if (kvp.Value.TryGetComponent(out AudioListener audioListener))
            {
                audioListener.enabled = enable;
            }
        }
    }
}
