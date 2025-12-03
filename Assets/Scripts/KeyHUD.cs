using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeyHUD : MonoBehaviour
{
    [Header("UI")]
    public Image keyIcon;

    [Header("Pop Animation")]
    public float popScale = 1.15f;
    public float popTime  = 0.15f;

    Vector3 _baseScale;
    bool _visible;
    bool _subscribed;

    void Awake()
    {
        if (!keyIcon) keyIcon = GetComponent<Image>();
        _baseScale = keyIcon ? keyIcon.rectTransform.localScale : Vector3.one;
        SetVisible(false, instant: true);
    }

    void OnEnable()
    {
        // Do everything after one frame so GameManager has time to Awake.
        StartCoroutine(DeferredInit());
    }

    void OnDisable()
    {
        if (_subscribed && GameManager.Instance != null)
        {
            GameManager.Instance.KeyChanged -= OnKeyChanged;
        }
        _subscribed = false;
    }

    IEnumerator DeferredInit()
    {
        // wait one frame
        yield return null;

        if (GameManager.Instance != null)
        {
            // subscribe *now*
            if (!_subscribed)
            {
                GameManager.Instance.KeyChanged += OnKeyChanged;
                _subscribed = true;
            }

            Debug.Log($"[KeyHUD] Refreshing from GM.HasKey={GameManager.Instance.HasKey}");
            SetVisible(GameManager.Instance.HasKey, instant: true);
        }
        else
        {
            Debug.LogWarning("[KeyHUD] No GameManager.Instance in DeferredInit.");
        }
    }

    void OnKeyChanged(bool hasKey)
    {
        Debug.Log($"[KeyHUD] KeyChanged => {hasKey}");
        SetVisible(hasKey, instant: false);
    }

    public void SetVisible(bool show, bool instant)
    {
        _visible = show;
        if (!keyIcon) return;

        keyIcon.enabled = show;
        if (show && !instant) StartCoroutine(PopCo());
        else keyIcon.rectTransform.localScale = _baseScale;
    }

    IEnumerator PopCo()
    {
        var rt = keyIcon.rectTransform;
        float t = 0f;
        Vector3 start = _baseScale;
        Vector3 mid   = _baseScale * popScale;

        while (t < popTime)
        {
            float k = t / popTime;
            rt.localScale = Vector3.Lerp(start, mid, k);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        rt.localScale = _baseScale;
    }
}
