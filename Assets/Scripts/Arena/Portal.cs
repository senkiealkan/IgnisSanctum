using UnityEngine;

public class Portal : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToNextArena();
            }
        }
    }
}