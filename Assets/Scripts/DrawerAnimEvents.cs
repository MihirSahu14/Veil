using UnityEngine;

public class DrawerAnimEvents : MonoBehaviour
{
    // Called by animation event on the Drawer_Open clip
    public void OnDrawerOpenAnimFinished()
    {
        if (DrawerUIManager.Instance != null)
        {
            DrawerUIManager.Instance.OnDrawerOpenAnimFinished();
        }
        else
        {
            Debug.LogWarning("[DrawerAnimEvents] No DrawerUIManager.Instance found.");
        }
    }
}
