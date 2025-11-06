// Assets/Scripts/PlayerInventory.cs
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool HasKey => GameManager.Instance && GameManager.Instance.HasKey;

    public void GiveKey()
    {
        if (GameManager.Instance) GameManager.Instance.GiveKey();
    }

    public void ConsumeKey()
    {
        if (GameManager.Instance) GameManager.Instance.ConsumeKey();
    }
}
