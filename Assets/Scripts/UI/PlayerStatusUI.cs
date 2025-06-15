using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    [SerializeField] Slider hpSlider;
    [SerializeField] TMP_Text hpText;
    [SerializeField] Slider staminaSlider;

    private void LateUpdate()
    {
        HealthStatusUpdate();
    }

    private void HealthStatusUpdate()
    {
        if (hpSlider == null || PlayerManager.Instance.PlayerStat == null)
            return;

        float hpValue = (float)PlayerManager.Instance.PlayerStat.currentHealth / (float)PlayerManager.Instance.PlayerStat.maxHealth;

        hpSlider.value = hpValue;
        hpText.text = $"{PlayerManager.Instance.PlayerStat.currentHealth} / {PlayerManager.Instance.PlayerStat.maxHealth}";
    }
}
