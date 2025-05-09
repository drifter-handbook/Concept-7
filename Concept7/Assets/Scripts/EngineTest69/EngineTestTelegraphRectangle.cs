﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTestTelegraphRectangle : MonoBehaviour, IEngineTestTelegraph, IActorLifetimeHandler
{
    public GameObject Pulse;
    public GameObject Fill;
    public GameObject Border;

    Coroutine finishCoroutine;

    float PulseTime = 1.35f;
    float PulseWidth = 0.25f;
    float PulseFade = 0.5f;

    float FinishAlphaMult = 1f;
    float PulseAlpha = 0.5f;

    float LifetimeFinishDur = 0.5f;

    public Vector2 Size;

    float borderAlpha;
    float fillAlpha;

    void Awake()
    {
        borderAlpha = Border.GetComponent<SpriteRenderer>().color.a;
        fillAlpha = Fill.GetComponent<SpriteRenderer>().color.a;
    }

    void Start()
    {
        Initialize(transform.localPosition, 0f, Size);
    }

    public void Initialize(Vector2 position, float rotation, Vector2 size)
    {
        // starting local position starting point of rectangle at position
        transform.localPosition = new Vector3(position.x, position.y, transform.localPosition.z);
        transform.localEulerAngles = new Vector3(0f, 0f, rotation);
        Vector3 scale = new Vector3(size.x, size.y, transform.localScale.z);
        Fill.transform.localScale = scale;
        SetRectPos(Fill, rotation, size);
        Border.GetComponent<SpriteRenderer>().size *= new Vector2(Mathf.Abs(size.x), 1f);
        Border.transform.localScale = new Vector3(Border.transform.localScale.x, Mathf.Abs(size.y), Border.transform.localScale.y);
        SetRectPos(Border, rotation, size);
        // start pulsing coroutine
        if (Pulse.activeSelf)
        {
            StartCoroutine(PulseCoroutine());
        }
    }

    public void Finish(float dur)
    {
        finishCoroutine = StartCoroutine(FinishCoroutine(dur));
    }
    IEnumerator FadeInCoroutine(float dur)
    {
        SpriteRenderer borderSr = Border.GetComponent<SpriteRenderer>();
        SpriteRenderer fillSr = Fill.GetComponent<SpriteRenderer>();
        float time = 0f;
        while (time < dur)
        {
            FinishAlphaMult = Mathf.Lerp(0f, 1f, time / dur);
            borderSr.color = new Color(borderSr.color.r, borderSr.color.b, borderSr.color.b, FinishAlphaMult * borderAlpha);
            fillSr.color = new Color(fillSr.color.r, fillSr.color.g, fillSr.color.b, FinishAlphaMult * fillAlpha);
            yield return null;
            time += Time.deltaTime;
        }
    }
    IEnumerator FinishCoroutine(float dur)
    {
        SpriteRenderer borderSr = Border.GetComponent<SpriteRenderer>();
        SpriteRenderer fillSr = Fill.GetComponent<SpriteRenderer>();
        float time = 0f;
        while (time < dur)
        {
            FinishAlphaMult = Mathf.Lerp(1f, 0f, time / dur);
            borderSr.color = new Color(borderSr.color.r, borderSr.color.b, borderSr.color.b, FinishAlphaMult * borderAlpha);
            fillSr.color = new Color(fillSr.color.r, fillSr.color.g, fillSr.color.b, FinishAlphaMult * fillAlpha);
            yield return null;
            time += Time.deltaTime;
        }
        Destroy(gameObject);
        yield break;
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
            float x = Mathf.Lerp(0, 1f - PulseWidth, t);
            Pulse.transform.localPosition = new Vector3(x, Pulse.transform.localPosition.y, Pulse.transform.localPosition.z);
            float a = FinishAlphaMult * PulseAlpha;
            if (PulseFade > 0.02)
            {
                a *= Mathf.Min(Mathf.Lerp(0f, 1/PulseFade, t), Mathf.Lerp(1/PulseFade, 0f, t));
            }
            sr.color = new Color(sr.color.r, sr.color.b, sr.color.b, Mathf.Clamp01(a));
            yield return null;
            time += Time.deltaTime;
        }
    }

    // set rectangle position so that the left edge is at position
    public void SetRectPos(GameObject go, float rotation, Vector2 size)
    {
        go.transform.localPosition = new Vector3(size.x * 0.5f, go.transform.localPosition.y, go.transform.localPosition.z);
    }

    public void HandleLifetime(float dur)
    {
        StartCoroutine(HandleLifetimeCoroutine(dur));
    }
    IEnumerator HandleLifetimeCoroutine(float dur)
    {
        yield return FadeInCoroutine(LifetimeFinishDur);
        yield return new WaitForSeconds(Mathf.Max(dur - LifetimeFinishDur * 2, 0f));
        yield return FinishCoroutine(LifetimeFinishDur);
    }
}