using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint; 

    public void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        StraightProjectile projectileScript = projectileObj.GetComponent<StraightProjectile>();

        if (projectileScript != null)
        {
            float directionX = transform.localScale.x > 0 ? 1f : -1f;
            Vector2 shootDirection = new Vector2(directionX, 0);

            projectileScript.Initialize(shootDirection);
        }
    }
}