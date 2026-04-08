using UnityEngine;

public class ChasedProjectileSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    // Hàm này sẽ được gọi từ Animation Event
    public void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // 1. Tìm đối tượng Player trong Scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Vector2 shootDirection;

        if (player != null)
        {
            // 2. Tính toán hướng: (Vị trí đích - Vị trí đầu).normalized
            shootDirection = (player.transform.position - firePoint.position).normalized;
        }
        else
        {
            // Nếu không tìm thấy Player, mặc định bắn theo hướng mặt của quái
            float directionX = transform.localScale.x > 0 ? 1f : -1f;
            shootDirection = new Vector2(directionX, 0);
        }

        // 3. Khởi tạo viên đạn
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        StraightProjectile projectileScript = projectileObj.GetComponent<StraightProjectile>();

        if (projectileScript != null)
        {
            // 4. Truyền hướng đã tính toán vào viên đạn
            projectileScript.Initialize(shootDirection);
        }
    }
}
