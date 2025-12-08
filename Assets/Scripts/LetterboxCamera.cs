using UnityEngine;

/// <summary>
/// Maintains a fixed aspect ratio by letterboxing/pillarboxing the camera view.
/// Attach to the main camera. Set targetWidth/targetHeight to your reference (e.g., 701x588).
/// Works in WebGL/fullscreen by adding black bars instead of stretching.
/// </summary>
[RequireComponent(typeof(Camera))]
public class LetterboxCamera : MonoBehaviour
{
    public float targetWidth = 701f;
    public float targetHeight = 588f;

    Camera cam;
    Rect defaultRect;

    void Awake()
    {
        cam = GetComponent<Camera>();
        defaultRect = cam.rect;
    }

    void LateUpdate()
    {
        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = new Rect();

        if (scaleHeight < 1f)
        {
            // Add letterbox (bars top/bottom)
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) * 0.5f;
        }
        else
        {
            // Add pillarbox (bars left/right)
            float scaleWidth = 1f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) * 0.5f;
            rect.y = 0f;
        }

        cam.rect = rect;
    }

    void OnDisable()
    {
        if (cam != null)
            cam.rect = defaultRect;
    }
}
