using UnityEngine;
using UnityEngine.UI;

public class Boss3HealthUI : MonoBehaviour
{
    public Boss3Health bossHealth;
    public Slider slider;

    private void Start()
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
        }
    }

    private void Update()
    {
        if (bossHealth == null || slider == null) return;

        slider.value = bossHealth.CurrentHealthNormalized();
    }
}