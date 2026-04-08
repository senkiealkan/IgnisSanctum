using TMPro;
using UnityEngine;

public class GemStatDisplay : MonoBehaviour
{
    public TextMeshProUGUI statGemText;
    public TextMeshProUGUI fireGemText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        statGemText.text = MetaProgressionManager.Instance.GetDisplayStatGems().ToString();
        fireGemText.text = MetaProgressionManager.Instance.GetDisplayFireGems().ToString();
    }
}
