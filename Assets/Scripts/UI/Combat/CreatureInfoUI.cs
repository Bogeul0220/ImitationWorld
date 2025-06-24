using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreatureInfoUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Creature creature;

    public void SetCreature(Creature creature)
    {
        this.creature = creature;
        nameText.text = creature.CreatureName;
        creature.Unitstat.OnDamaged += UpdateHealthSlider;
    }

    public void UpdateHealthSlider()
    {
        healthSlider.value = (float)creature.Unitstat.currentHealth / creature.Unitstat.maxHealth;
    }
}
