using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float lifetime = 1f;
    public float upSpeed = 1.5f;
    public float fadeSpeed = 2.5f;

    private Color textColor;
    private float defaultFontSize; // Lưu size gốc để reset
    private float timer; // Biến đếm thời gian thay cho Destroy

    private void Awake()
    {
        // Lưu lại size gốc lúc khởi tạo
        if (textMesh != null) defaultFontSize = textMesh.fontSize;
    }

    public void Setup(float damageAmount, bool isCritical)
    {
        textMesh.text = damageAmount.ToString("0");
        timer = lifetime; // Reset thời gian sống

        // Reset lại alpha màu về 1 (vì lần trước nó bị mờ đi)
        textColor = textMesh.color;
        textColor.a = 1f;
        textMesh.color = textColor;

        if (isCritical)
        {
            textMesh.fontSize = 10; 
            textColor = Color.yellow;
            transform.localScale = Vector3.one * 1.5f;
        }
        else
        {
            // Phải reset về bình thường nếu không object cũ đang to sẽ vẫn to
            textMesh.fontSize = defaultFontSize;
            textColor = Color.white; 
            transform.localScale = Vector3.one;
        }

        textMesh.color = textColor;

        // Random vị trí offset nhẹ
        transform.localPosition += new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
    }

    void Update()
    {
        // Bay lên
        transform.position += Vector3.up * upSpeed * Time.deltaTime;

        // Mờ dần
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;

        // Kiểm tra thời gian để trả về pool
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            // Thay vì Destroy, gọi ReturnToPool
            DamagePopupPool.Instance.ReturnToPool(this);
        }
    }
}