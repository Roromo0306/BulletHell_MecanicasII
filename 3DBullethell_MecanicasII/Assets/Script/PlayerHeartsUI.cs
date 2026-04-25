using UnityEngine;
using UnityEngine.UI;

public class PlayerHeartsUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Image[] hearts;

    public Sprite fullHeart;
    public Sprite emptyHeart;

    private void Start()
    {
        UpdateHearts();

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHearts);
        }
    }

    public void UpdateHearts()
    {
        if (playerHealth == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < playerHealth.currentHearts)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }

            hearts[i].enabled = i < playerHealth.maxHearts;
        }
    }
}