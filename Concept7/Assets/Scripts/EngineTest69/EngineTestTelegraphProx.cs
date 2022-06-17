using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTestTelegraphProx : MonoBehaviour, IEngineTestTelegraph, IActorLifetimeHandler
{
    public GameObject Pulse;
    public GameObject Fill;
    public GameObject Border;

    Coroutine finishCoroutine;

    float PulseTime = 1.65f;
    float PulseFade = 0.5f;

    float FinishAlphaMult = 1f;
    float PulseAlpha = 0.8f;

    float LifetimeFinishDur = 0.5f;

    public float Size;

    float RotationSpeed = 180f;

    public Color PulseStartColor;
    public Color PulseEndColor;

    void Start()
    {
        Initialize(transform.localPosition, 45f, new Vector2(Size, Size));
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

    void Update()
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + Time.deltaTime * RotationSpeed);
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
        SpriteRenderer srPulse = Pulse.GetComponent<SpriteRenderer>();
        SpriteRenderer srFill = Fill.GetComponent<SpriteRenderer>();
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
            srPulse.color = LerpHSV(PulseStartColor, PulseEndColor, t);
            srPulse.color = new Color(srPulse.color.r, srPulse.color.g, srPulse.color.b, Mathf.Clamp01(a));
            srFill.color = new Color(srFill.color.r, srFill.color.g, srFill.color.b, Mathf.Clamp01(a));
            yield return null;
            time += Time.deltaTime;
        }
    }

    Color LerpHSV(Color start, Color end, float t)
    {
        float h, s, v;
        Color.RGBToHSV(start, out h, out s, out v);
        Vector3 vstart = new Vector3(h, s, v);
        Color.RGBToHSV(end, out h, out s, out v);
        Vector3 vend = new Vector3(h, s, v);
        // convert vend to closest angle
        float hdiff = vend.x - vstart.x;
        if (Mathf.Abs(vend.x - vstart.x + 1) < Mathf.Abs(hdiff))
        {
            hdiff = vend.x - vstart.x + 1;
        }
        if (Mathf.Abs(vend.x - vstart.x - 1) < Mathf.Abs(hdiff))
        {
            hdiff = vend.x - vstart.x - 1;
        }
        vend.x = vstart.x + hdiff;
        Vector3 outhsv = Vector3.Lerp(vstart, vend, t);
        outhsv.x = (outhsv.x + 1f) % 1f;
        Color c = Color.HSVToRGB(outhsv.x, outhsv.y, outhsv.z);
        c.a = Mathf.Lerp(start.a, end.a, t);
        return c;
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