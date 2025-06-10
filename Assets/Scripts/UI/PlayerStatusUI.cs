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
        if (hpSlider == null || PlayerManager.instance.PlayerStat == null)
            return;

        float hpValue = (float)PlayerManager.instance.PlayerStat.currentHealth / (float)PlayerManager.instance.PlayerStat.maxHealth;

        hpSlider.value = hpValue;
        hpText.text = $"{PlayerManager.instance.PlayerStat.currentHealth} / {PlayerManager.instance.PlayerStat.maxHealth}";
    }
}
