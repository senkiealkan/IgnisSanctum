using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TreeDepthSorter : MonoBehaviour
{
    private Transform playerTarget;
    private SpriteRenderer spriteRenderer;

    // Tùy chỉnh offset nếu tâm (pivot) của cây không nằm ngay gốc
    // Giúp việc chuyển đổi mượt hơn
    [SerializeField] private float yOffset = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Tìm Player theo Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
    }

    void Update()
    {
        // Nếu chưa tìm thấy Player (do Player spawn trễ) thì tìm lại
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
            return;
        }

        // Tính toán vị trí tương đối
        // Thêm yOffset để tinh chỉnh điểm cắt (ví dụ ngay gốc rễ thay vì tâm ảnh)
        float dirY = playerTarget.position.y - (transform.position.y + yOffset);

        if (dirY < 0)
        {
            // Player đang ở DƯỚI cây (Player Y < Tree Y)
            // -> Cây phải nằm sau lưng Player
            if (spriteRenderer.sortingOrder != 1)
                spriteRenderer.sortingOrder = 1;
        }
        else
        {
            // Player đang ở TRÊN cây (Player Y > Tree Y)
            // -> Cây phải nằm đè lên Player
            if (spriteRenderer.sortingOrder != 3)
                spriteRenderer.sortingOrder = 3;
        }
    }
}