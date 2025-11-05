// EnemyStunnable.cs
using System.Collections;
using UnityEngine;

public class EnemyStunnable : MonoBehaviour, IStunnable
{
    [Tooltip("Drag the movement/AI MonoBehaviour you want disabled during stun.")]
    public MonoBehaviour aiScript;
    public float defaultSeconds = 1.25f;

    Coroutine stunCR;

    public void Stun(float seconds)
    {
        if (stunCR != null) StopCoroutine(stunCR);
        stunCR = StartCoroutine(StunRoutine(seconds > 0 ? seconds : defaultSeconds));
    }

    IEnumerator StunRoutine(float s)
    {
        if (aiScript) aiScript.enabled = false;
        yield return new WaitForSeconds(s);
        if (aiScript) aiScript.enabled = true;
        stunCR = null;
    }
}
