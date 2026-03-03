using UnityEngine;
using TMPro; // Ensure you have TextMeshPro installed
using UnityEngine.UI;

public class BossDebugUI : MonoBehaviour {
    [Header("References")]
    public BossController boss;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI attackText;
    public Slider healthBar; // Optional visual slider

    void Update() {
        if (boss == null) return;

        float currentHP = boss.GetCurrentHealth();
        float maxHP = boss.maxHealth;
        healthText.text = $"HP: {currentHP} / {maxHP}";
        
        if (healthBar != null) {
            healthBar.value = currentHP / maxHP;
        }

        phaseText.text = $"Phase: {boss.GetCurrentPhaseIndex() + 1}";

        if (boss.GetCurrentAttack() != null) {
            attackText.text = $"Attack: {boss.GetCurrentAttack().attackName}";
        }
    }
}