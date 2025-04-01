using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ContinuousMovementUI : MonoBehaviour
{
    [SerializeField] private float rotationAmp = 5f;
    [SerializeField] private float rotationDuration = 1f;

    [SerializeField] private float scaleAmp = 0.1f;
    [SerializeField] private float scaleDuration = 1f;


    Quaternion startRot;
    Vector3 startScale;

    void Start()
    {
        startRot = transform.rotation;
        startScale = transform.localScale;

        StartCoroutine(Rotation());
        StartCoroutine(Scale());
    }

    IEnumerator Rotation()
    {
        while (true)
        {
            yield return LerpRotation(startRot.eulerAngles.z + rotationAmp, rotationDuration);
            yield return LerpRotation(startRot.eulerAngles.z - rotationAmp, rotationDuration);
        }
    }

    private IEnumerator LerpRotation(float targetAngle, float duration)
    {
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle);

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation; // Assurez-vous que la rotation finale est correcte
    }

    IEnumerator Scale()
    {
        while (true)
        {
            yield return LerpScale(startScale.x + scaleAmp, scaleDuration);
            yield return LerpScale(startScale.x - scaleAmp, scaleDuration);
        }
    }

    private IEnumerator LerpScale(float targetScale, float duration)
    {

        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, targetScale);

        while (elapsedTime < duration)
        {

            transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale; // Assurez-vous que l'échelle finale est correcte
    }




    public void SetVisualsActive(bool _bool)
    {
        foreach (var renderer in GetComponentsInChildren<Image>())
        {
            renderer.enabled = _bool;
        }
        foreach (var renderer in GetComponentsInChildren<TextMeshProUGUI>())
        {
            renderer.enabled = _bool;
        }
    }
}