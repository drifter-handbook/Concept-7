using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTestConnectedLaser : MonoBehaviour, IActorLifetimeHandler
{
    // connects to closest laser with Index-1
    public int Index;
    EngineTestConnectedLaser connected;

    float FadeTime = 0.5f;
    float FinishAlphaMult = 1f;
    float LaserAlpha = 1f;

    LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
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
        StartCoroutine(GrowNodeCoroutine());
        Debug.Log($"pre handle lifetime {gameObject}");
        HandleLifetime(2f);
    }

    public void HandleLifetime(float dur)
    {
        Debug.Log($"handle lifetime {gameObject}");
        StartCoroutine(FadeEdgeCoroutine(dur));
    }

    IEnumerator GrowNodeCoroutine()
    {
        float time = 0f;
        while (time < FadeTime)
        {
            float t = time / FadeTime;
            Vector2 scale = Vector2.one * t;
            transform.localScale = new Vector3(scale.x, scale.y, transform.localScale.z);
            yield return null;
            time += Time.deltaTime;
        }
    }

    IEnumerator FadeEdgeCoroutine(float dur)
    {
        Debug.Log($"Fade edge coroutine {gameObject}");
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
            lr.SetPosition(0, connected.transform.position);
            lr.SetPosition(1, transform.position);
            yield return null;
            time += Time.deltaTime;
        }
    }
}
