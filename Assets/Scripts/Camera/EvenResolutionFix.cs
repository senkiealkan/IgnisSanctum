using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EvenResolutionFix : MonoBehaviour
{
    private Camera cam;
    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        int width = Screen.width;
        int height = Screen.height;

        if (width % 2 != 0 || height % 2 != 0)
        {
            // Làm tròn kích thước về số chẵn
            int newWidth = width - (width % 2);
            int newHeight = height - (height % 2);

            Screen.SetResolution(newWidth, newHeight, Screen.fullScreen);
        }
    }
}
