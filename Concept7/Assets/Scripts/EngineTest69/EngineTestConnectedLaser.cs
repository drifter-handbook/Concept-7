using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTestConnectedLaser : MonoBehaviour, IActorLifetimeHandler
{
    // connects to closest laser with Index-1
    public int Index;
    EngineTestConnectedLaser connected;

    public float FadeTime = 0.5f;
    float FinishAlphaMult = 1f;
    float LaserAlpha = 1f;

    LineRenderer lr;

    Coroutine lifetimeCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        Initialize();
        HandleLifetime(1f);
    }

    public void Initialize()
    {
        foreach (EngineTestConnectedLaser laser in FindObjectsOfType<EngineTestConnectedLaser>())
        {
            if (laser == this || laser.Index != Index - 1)
            {
                continue;
            }
            if (connected == null || (laser.transform.position - transform.position).magnitude < (connected.transform.position - transform.position).magnitude)
            {
                connected = laser;
            }
        }
    }

    public void HandleLifetime(float dur)
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }
        lifetimeCoroutine = StartCoroutine(FadeEdgeCoroutine(dur));
    }

    IEnumerator FadeEdgeCoroutine(float dur)
    {
        while (connected == null)
        {
            yield return null;
        }
        float time = 0f;
        while (time < dur)
        {
            float t = time / dur;
            float a = FinishAlphaMult * LaserAlpha;
            if (FadeTime > 0.02)
            {
                a *= Mathf.Min(Mathf.Lerp(0f, 1 / FadeTime, t), Mathf.Lerp(1 / FadeTime, 0f, t));
            }
            Color c = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            lr.startColor = c;
            lr.endColor = c;
            lr.widthMultiplier = t;
            lr.positionCount = 2;
            if (connected != null)
            {
                lr.SetPosition(0, connected.transform.position);
            }
            lr.SetPosition(1, transform.position);
            yield return null;
            time += Time.deltaTime;
        }
    }
}
