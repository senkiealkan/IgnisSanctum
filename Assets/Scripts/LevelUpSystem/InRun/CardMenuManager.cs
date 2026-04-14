using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMenuManager : MonoBehaviour
{
    public List<UpgradeCardConfig> allUpgradeCards;

    public GameObject cardUIPrefab;

    // Horizontal Layout Group
    public Transform cardContainer;

    public XPManager xpManager;
    public PlayerStats playerStats;
    public CanvasGroup menuCanvasGroup;
    public float safetyDelay = 0.5f;
    private List<GameObject> activeCards = new List<GameObject>();

    public void DisplayCards()
    {
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false; 
            menuCanvasGroup.alpha = 0f; 
        }
        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        // 1. Chọn 3 Card ngẫu nhiên 
        List<UpgradeCardConfig> availableCards = new List<UpgradeCardConfig>(allUpgradeCards);

        for (int i = 0; i < 3 && availableCards.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableCards.Count);
            UpgradeCardConfig selectedConfig = availableCards[randomIndex];

            // 2. Sinh ra Card UI
            GameObject newCard = Instantiate(cardUIPrefab, cardContainer);
            newCard.transform.localScale = Vector3.one;
            activeCards.Add(newCard);

            // 3. Cấu hình Card UI 
            CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Setup(selectedConfig, this); 
            }

            availableCards.RemoveAt(randomIndex);
        }
        StartCoroutine(EnableInteractionCoroutine());
    }
    private IEnumerator EnableInteractionCoroutine()
    {
        // Dùng Realtime vì TimeScale đang bằng 0
        float timer = 0f;
        while (timer < safetyDelay)
        {
            timer += Time.unscaledDeltaTime;

            //  Hiệu ứng Fade In 
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / safetyDelay);
            }

            yield return null;
        }

        // Hết giờ -> Cho phép bấm
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }
    }
    // --- Hàm xử lý khi Card được người chơi chọn ---
    public void ApplyUpgrade(UpgradeCardConfig config)
    {
        // Áp dụng logic nâng cấp vào PlayerStats
        playerStats.ApplyUpgrade(config);

        // Gọi lại XPManager để tiếp tục game
        xpManager.CardSelected();
    }
}