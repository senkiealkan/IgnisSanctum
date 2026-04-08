using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint; // Điểm xuất phát của đạn (tạo 1 Empty Object con của Quái)

    // Hàm này sẽ được gọi từ Animation Event
    public void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // Khởi tạo viên đạn
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