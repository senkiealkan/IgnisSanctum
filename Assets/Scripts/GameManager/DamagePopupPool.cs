using System.Collections.Generic;
using UnityEngine;

public class DamagePopupPool : MonoBehaviour
{
    public static DamagePopupPool Instance { get; private set; }

    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private int initialPoolSize = 20; 

    private Queue<DamagePopup> poolQueue = new Queue<DamagePopup>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Khởi tạo sẵn một lượng popup để dùng dần
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPopup();
        }
    }

    private DamagePopup CreateNewPopup()
    {
        GameObject obj = Instantiate(damagePopupPrefab, transform); // Gom gọn vào object cha cho gọn Hierarchy
        DamagePopup popup = obj.GetComponent<DamagePopup>();
        obj.SetActive(false); // Mặc định tắt đi
        poolQueue.Enqueue(popup);
        return popup;
    }

    public DamagePopup Get(Vector3 position, float damageAmount, bool isCritical)
    {
        // Nếu hàng chờ hết, tạo mới thêm
        if (poolQueue.Count == 0)
        {
            CreateNewPopup();
        }

        DamagePopup popup = poolQueue.Dequeue();

        // Đặt vị trí và bật lên
        popup.transform.position = position;
        popup.gameObject.SetActive(true);

        // Setup thông số
        popup.Setup(damageAmount, isCritical);

        return popup;
    }

    public void ReturnToPool(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
        poolQueue.Enqueue(popup);
    }
}