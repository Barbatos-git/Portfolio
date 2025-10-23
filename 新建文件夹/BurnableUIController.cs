using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BurnableUIController : MonoBehaviour
{
    [SerializeField] private RawImage burnImage;
    [SerializeField] private Material burnMaterial;
    [SerializeField] private float burnDuration = 1.5f;

    void Awake()
    {
        burnMaterial = new Material(burnMaterial);
        GetComponent<RawImage>().material = burnMaterial;
        burnMaterial.SetFloat("_MaskThreshold", 0f);
    }

    public void StartBurn(System.Action onFinished = null)
    {
        gameObject.SetActive(true);
        StartCoroutine(BurnRoutine(onFinished));
    }

    private System.Action onComplete;

    void Update()
    {

    }

    private IEnumerator BurnRoutine(System.Action onFinished)
    {
        float t = 0f;

        while (t < burnDuration)
        {
            float burnValue = Mathf.Lerp(0, 1, t / burnDuration);
            burnMaterial.SetFloat("_MaskThreshold", burnValue);
            t += Time.deltaTime;
            yield return null;
        }

        burnMaterial.SetFloat("_MaskThreshold", 1f);

        onFinished?.Invoke();

        gameObject.SetActive(false);
    }

    public void ResetBurn()
    {
        burnMaterial.SetFloat("_MaskThreshold", 0f);
        gameObject.SetActive(true);
    }
}
