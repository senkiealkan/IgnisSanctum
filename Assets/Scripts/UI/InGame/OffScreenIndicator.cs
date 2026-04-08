using UnityEngine;
using UnityEngine.UI;

public class OffScreenIndicator : MonoBehaviour
{
    public Image indicatorImage;
    public float margin = 50f;
    public Transform player;
    private Camera mainCamera;
    private RectTransform canvasRect;
    void Start()
    {
        // 1. Kiểm tra an toàn: Nếu quên gán ảnh thì tắt script luôn để tránh lỗi
        if (indicatorImage == null)
        {
            Debug.LogWarning("Chưa gán Indicator Image cho " + gameObject.name);
            enabled = false;
            return;
        }

        // 2. Tìm Player an toàn
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            // Chỉ gán nếu tìm thấy, tránh lỗi khi ở Menu (Player bị tắt)
            if (playerObj != null) player = playerObj.transform;
        }

        mainCamera = Camera.main;

        // 3. [FIX LỖI NULL REFERENCE] Lấy RectTransform an toàn
        // indicatorImage.canvas có thể null nếu object này đang bị disable hoặc chưa nằm trong Canvas active
        Canvas parentCanvas = indicatorImage.canvas;

        // Fallback: Nếu không lấy được qua property, thử tìm component ở cha
        if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }
        else
        {
            // Nếu vẫn không tìm thấy Canvas (ví dụ object trôi nổi), tắt script
            // Debug.Log("Không tìm thấy Canvas cha, tạm tắt Indicator.");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        // Kiểm tra hàng loạt điều kiện để đảm bảo không lỗi
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || indicatorImage == null || canvasRect == null)
        {
            if (indicatorImage != null) indicatorImage.enabled = false;
            return;
        }

        // Nếu Player chết hoặc bị tắt (về Menu), ẩn indicator đi
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            indicatorImage.enabled = false;
            return;
        }

        Transform targetEnemy = GetClosestEnemy();
        if (targetEnemy == null)
        {
            indicatorImage.enabled = false;
            return;
        }

        // --- BẮT ĐẦU LOGIC TÍNH TOÁN (Đã update fix tỷ lệ màn hình) ---

        // 1. Dùng kích thước Canvas thay vì Screen.width/height
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Tính toán vị trí trong World Space -> Screen Space
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetEnemy.position);

        // Kiểm tra xem quái có nằm trong màn hình không
        bool isInside = screenPos.z > 0 &&
                        screenPos.x > 0 && screenPos.x < Screen.width &&
                        screenPos.y > 0 && screenPos.y < Screen.height;

        if (!isInside)
        {
            indicatorImage.enabled = true;

            // 2. Logic xoay và đặt vị trí (đã update)
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;
            if (screenPos.z < 0) screenPos *= -1f;

            Vector3 direction = screenPos - screenCenter;
            float angle = Mathf.Atan2(direction.y, direction.x);
            indicatorImage.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

            Vector3 outPos = Vector3.zero;
            float halfWidth = canvasWidth / 2f - margin;   // Dùng kích thước Canvas
            float halfHeight = canvasHeight / 2f - margin; // Dùng kích thước Canvas

            // Tính giao điểm cạnh màn hình
            if (Mathf.Abs(direction.x) * halfHeight > Mathf.Abs(direction.y) * halfWidth)
            {
                outPos.x = direction.x > 0 ? halfWidth : -halfWidth;
                outPos.y = outPos.x * (direction.y / direction.x);
            }
            else
            {
                outPos.y = direction.y > 0 ? halfHeight : -halfHeight;
                outPos.x = outPos.y * (direction.x / direction.y);
            }

            indicatorImage.transform.localPosition = outPos;
        }
        else
        {
            indicatorImage.enabled = false;
        }
    }

    //void UpdateIndicatorPosition(Vector3 screenPos)
    //{
    //    Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;

    //    if (screenPos.z < 0)
    //    {
    //        screenPos *= -1f;
    //    }

    //    Vector3 direction = screenPos - screenCenter;
    //    float angle = Mathf.Atan2(direction.y, direction.x);
    //    indicatorImage.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

    //    float screenAspect = (float)Screen.width / (float)Screen.height;
    //    float m = Mathf.Tan(angle); 

    //    Vector3 outPos = Vector3.zero;

    //    if (Mathf.Abs(direction.x) * (Screen.height / 2f) > Mathf.Abs(direction.y) * (Screen.width / 2f))
    //    {
    //        outPos.x = direction.x > 0 ? (Screen.width / 2f - margin) : (-Screen.width / 2f + margin);
    //        outPos.y = outPos.x * (direction.y / direction.x);
    //    }
    //    else
    //    {
    //        outPos.y = direction.y > 0 ? (Screen.height / 2f - margin) : (-Screen.height / 2f + margin);
    //        outPos.x = outPos.y * (direction.x / direction.y);
    //    }

    //    indicatorImage.transform.localPosition = outPos;
    //}
    void UpdateIndicatorPosition(Vector3 screenPos)
    {
        // 1. Chuyển đổi Screen Space sang Canvas Space
        // Thay vì dùng Screen.width, ta dùng kích thước thực tế của Canvas
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;

        if (screenPos.z < 0) screenPos *= -1f;

        Vector3 direction = screenPos - screenCenter;
        float angle = Mathf.Atan2(direction.y, direction.x);
        indicatorImage.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

        // 2. Tính toán vị trí dựa trên Canvas Size
        Vector3 outPos = Vector3.zero;
        float halfWidth = canvasWidth / 2f - margin;
        float halfHeight = canvasHeight / 2f - margin;

        // Tính toán giao điểm với cạnh của Canvas
        if (Mathf.Abs(direction.x) * halfHeight > Mathf.Abs(direction.y) * halfWidth)
        {
            outPos.x = direction.x > 0 ? halfWidth : -halfWidth;
            outPos.y = outPos.x * (direction.y / direction.x);
        }
        else
        {
            outPos.y = direction.y > 0 ? halfHeight : -halfHeight;
            outPos.x = outPos.y * (direction.x / direction.y);
        }

        indicatorImage.transform.localPosition = outPos;
    }
    Transform GetClosestEnemy()
    {
        // Giữ nguyên code cũ
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float minDist = Mathf.Infinity;

        // Thêm kiểm tra Player null để tránh lỗi ở hàm Distance
        if (player == null) return null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(player.position, enemy.transform.position);
            if (dist < minDist)
            {
                closest = enemy.transform;
                minDist = dist;
            }
        }
        return closest;
    }
}