using UnityEngine;

public class BossHealthBarManager : MonoBehaviour
{
    public EnemyHealth enemyHealth;
    public HealthBar healthBar;
    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        healthBar = GetComponent<HealthBar>();
        healthBar.SetMaxHealth(enemyHealth.maxHealth);
    }

    void Update()
    {
        healthBar.SetHealth(enemyHealth.currentHealth);
    }
}
