using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    private NetworkPlayerHealth targetPlayer;

    public void Initialize(NetworkPlayerHealth player)
    {
        targetPlayer = player;
        
        // Listen for health changes synchronized over the network
        targetPlayer.CurrentHealth.OnValueChanged += OnHealthChanged;
        
        // Set the initial UI value on spawn
        UpdateImageFill(targetPlayer.CurrentHealth.Value);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        UpdateImageFill(newValue);
    }

    private void UpdateImageFill(int currentHealth)
    {
        if (healthBarFill != null)
        {
            // Image fill amount works on a 0.0f to 1.0f scale.
            // Dividing currentHealth by 100f normalizes it (e.g., 75 HP becomes 0.75 fill).
            float fillAmount = (float)currentHealth / 100f;
            healthBarFill.fillAmount = fillAmount;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to protect against memory leaks when changing scenes
        if (targetPlayer != null)
        {
            targetPlayer.CurrentHealth.OnValueChanged -= OnHealthChanged;
        }
    }
}