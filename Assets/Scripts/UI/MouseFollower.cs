using UnityEngine;
// 1. Thêm thư viện này
using UnityEngine.InputSystem;

public class MouseFollower : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 2. Cách lấy vị trí chuột mới
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // 3. Chuyển đổi sang World Point (giữ nguyên logic cũ)
        Vector3 mousePosWithDepth = new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePosWithDepth);

        transform.position = worldPos;
    }
}