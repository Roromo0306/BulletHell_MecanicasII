using UnityEngine;
using UnityEngine.UI;


public class ChargeFillUI : MonoBehaviour
{
    public Image fillImage;
    public Transform targetCamera;

    private void Awake()
    {
        SetFill(0f);
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;

        if (targetCamera != null)
            transform.forward = targetCamera.forward;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        SetFill(0f);
        gameObject.SetActive(false);
    }

    public void SetFill(float amount)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = Mathf.Clamp01(amount);
    }
}