using System.Collections;
using UnityEngine;
using TMPro;

public class AbilityXRay : MonoBehaviour
{
    public Camera cam;
    public KeyCode activationKey = KeyCode.X;
    public float cooldownDuration = 10f;
    public float abilityActive = 5f;
    public TMP_Text cooldownText;
    public SkinnedMeshRenderer enemyRenderer;
    private float dissolveSpeed = 2.5f;
    private int enemyLayerIndex;
    private bool isEnemyLayerActive = false;
    private bool isCooldownActive = false;
    public AudioClip sfx;
    private float dissolveAmt;

    public Material originalMat, xrayMat;

    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("Camera not assigned!");
            return;
        }

        enemyLayerIndex = LayerMask.NameToLayer("InvisEnemy");
        DeactivateEnemyLayer(); // Deactivate enemy layer by default

        dissolveAmt = 0.9f;
        SetDissolveAmt(dissolveAmt);

        enemyRenderer.sharedMaterial = originalMat;

        cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isCooldownActive && Input.GetKeyDown(activationKey) && !isEnemyLayerActive)
        {
            StartCoroutine(ActivateEnemyLayerForSeconds(abilityActive));
            StartCoroutine(CooldownCoroutine());
        }
    }

    IEnumerator ActivateEnemyLayerForSeconds(float seconds)
    {
        ActivateEnemyLayer();
        GameEvents.m_instance.playNewAudioClip.Invoke(sfx, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
        // Start dissolve effect
        StartCoroutine(DissolveInEffect());

        yield return new WaitForSeconds(seconds);

        StartCoroutine(DissolveOutEffect());
    }

    IEnumerator DissolveInEffect()
    {
        enemyRenderer.sharedMaterial = xrayMat;

        dissolveAmt = 0.9f;
        while (dissolveAmt > -1.1f)
        {
            dissolveAmt -= Time.deltaTime * dissolveSpeed;

            SetDissolveAmt(dissolveAmt);
            yield return null;
        }
    }

    IEnumerator DissolveOutEffect()
    {
        enemyRenderer.sharedMaterial = originalMat;

        dissolveAmt = -1.1f;
        while (dissolveAmt < 0.9f)
        {
            dissolveAmt += Time.deltaTime * dissolveSpeed;

            SetDissolveAmt(dissolveAmt);
            yield return null;
        }

        DeactivateEnemyLayer();
    }

    IEnumerator CooldownCoroutine()
    {
        isCooldownActive = true;
        cooldownText.gameObject.SetActive(true);

        float remainingTime = cooldownDuration;

        while (remainingTime > 0)
        {
            cooldownText.text = "Cooldown: " + remainingTime.ToString();

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        cooldownText.gameObject.SetActive(false);
        isCooldownActive = false;
    }

    void ActivateEnemyLayer()
    {
        cam.cullingMask |= 1 << enemyLayerIndex;
        isEnemyLayerActive = true;
    }

    void DeactivateEnemyLayer()
    {
        cam.cullingMask &= ~(1 << enemyLayerIndex);
        isEnemyLayerActive = false;
    }

    void SetDissolveAmt(float amt)
    {
        enemyRenderer.material.SetFloat("_Dissolve", amt);
    }
}
