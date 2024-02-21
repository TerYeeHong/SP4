using System.Collections;
using UnityEngine;
using TMPro;

public class AbilityXRay : MonoBehaviour
{
    public Camera cam;
    public KeyCode activationKey = KeyCode.X;
    public float cooldownDuration = 10f;
    public float abilityActive = 5f;
    //public TMP_Text cooldownText;
    public SkinnedMeshRenderer enemyRenderer;
    private float dissolveSpeed = 2.5f;
    private bool isEnemyLayerActive = false;
    private bool isCooldownActive = false;
    public AudioClip sfx;
    public float dissolveAmt;

    public bool isXRayActive;
    private float xRayActivationTime;

    public Material originalMat, xrayMat;
    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("Camera not assigned!");
            return;
        }
        
        dissolveAmt = 1.2f;
        isXRayActive = false;

    //cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isCooldownActive && Input.GetKeyDown(activationKey) && !isXRayActive)
        {
            isXRayActive = true;
            xRayActivationTime = Time.time;

            StartCoroutine(ActivateEnemyLayerForSeconds(abilityActive));
            StartCoroutine(CooldownCoroutine());
        }

        // Check if X-ray ability is active and elapsed time is less than abilityActive duration
        if (isXRayActive && (Time.time - xRayActivationTime) < abilityActive)
        {
            SetDissolveAmt(-1.1f); // Set dissolve amount to -1.1f (visible)
        }
    }

    IEnumerator ActivateEnemyLayerForSeconds(float seconds)
    {
        GameEvents.m_instance.playNewAudioClip.Invoke(sfx, AudioSfxManager.AUDIO_EFFECT.DEFAULT);

        // Start dissolve effect
        StartCoroutine(DissolveInEffect());

        yield return new WaitForSeconds(seconds);


        StartCoroutine(DissolveOutEffect());
    }

    IEnumerator DissolveInEffect()
    {
        while (dissolveAmt > -1.1f)
        {
            dissolveAmt -= Time.deltaTime * dissolveSpeed;

            SetDissolveAmt(dissolveAmt);
            yield return null;
        }
    }

    IEnumerator DissolveOutEffect()
    {
        while (dissolveAmt < 1.2f)
        {
            dissolveAmt += Time.deltaTime * dissolveSpeed;

            SetDissolveAmt(dissolveAmt);
            yield return null;
        }

        isXRayActive = false;
    }

    IEnumerator CooldownCoroutine()
    {
        isCooldownActive = true;
        //cooldownText.gameObject.SetActive(true);

        float remainingTime = cooldownDuration;

        while (remainingTime > 0)
        {
            //cooldownText.text = "Cooldown: " + remainingTime.ToString();

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        isCooldownActive = false;
    }

    void SetDissolveAmt(float amt)
    {
        enemyRenderer.material.SetFloat("_Dissolve", amt);
    }
}