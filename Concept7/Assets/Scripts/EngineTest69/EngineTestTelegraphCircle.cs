using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTestTelegraphCircle : MonoBehaviour, IEngineTestTelegraph, IActorLifetimeHandler
{
    public GameObject Pulse;
    public GameObject Fill;
    public GameObject Border;

    Coroutine finishCoroutine;

    float PulseTime = 1.65f;
    float PulseFade = 0.5f;

    float FinishAlphaMult = 1f;
    float PulseAlpha = 0.5f;

    float LifetimeFinishDur = 0.5f;

    public float Size;

    void Start()
    {
        Initialize(transform.position, 45f, new Vector2(Size, Size));
    }

    public void Initialize(Vector2 position, float rotation, Vector2 size)
    {
        // starting local position starting point of rectangle at position
        Vector3 scale = new Vector3(size.x, size.y, transform.localScale.z);
        transform.localScale = scale;
        transform.localPosition = new Vector3(position.x, position.y, transform.localPosition.z);
        transform.localEulerAngles = new Vector3(0f, 0f, rotation);
        // start pulsing coroutine
        StartCoroutine(PulseCoroutine());
    }

    public void Finish(float dur)
    {
        finishCoroutine = StartCoroutine(FinishCoroutine(dur));
    }
    IEnumerator FinishCoroutine(float dur)
    {
        SpriteRenderer borderSr = Border.GetComponent<SpriteRenderer>();
        SpriteRenderer fillSr = Fill.GetComponent<SpriteRenderer>();
        float time = 0f;
        while (true)
        {
            if (time > dur)
            {
                Destroy(gameObject);
                yield break;
            }
            FinishAlphaMult = Mathf.Lerp(1f, 0f, time / dur);
            borderSr.color = new Color(borderSr.color.r, borderSr.color.b, borderSr.color.b, FinishAlphaMult);
            fillSr.color = new Color(fillSr.color.r, fillSr.color.b, fillSr.color.b, FinishAlphaMult);
            yield return null;
            time += Time.deltaTime;
        }
    }

    IEnumerator PulseCoroutine()
    {
        float time = 0f;
        SpriteRenderer sr = Pulse.GetComponent<SpriteRenderer>();
        while (true)
        {
            if (time > PulseTime)
            {
                time -= PulseTime;
            }
            float t = time / PulseTime;
            Pulse.transform.localScale = new Vector3(t, t, transform.localScale.z);
            float a = FinishAlphaMult * PulseAlpha;
            if (PulseFade > 0.02)
            {
                a *= Mathf.Min(Mathf.Lerp(0f, 1 / PulseFade, t), Mathf.Lerp(1 / PulseFade, 0f, t));
            }
            sr.color = new Color(sr.color.r, sr.color.b, sr.color.b, Mathf.Clamp01(a));
            yield return null;
            time += Time.deltaTime;
        }
    }

    public void HandleLifetime(float dur)
    {
        StartCoroutine(HandleLifetimeCoroutine(dur));
    }
    IEnumerator HandleLifetimeCoroutine(float dur)
    {
        yield return new WaitForSeconds(Mathf.Max(dur - LifetimeFinishDur, 0f));
        Finish(LifetimeFinishDur);
    }
}