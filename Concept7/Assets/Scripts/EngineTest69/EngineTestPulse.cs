using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTestPulse : MonoBehaviour, IEngineTestTelegraph, IActorLifetimeHandler
{
    public GameObject Pulse;

    Coroutine finishCoroutine;

    float PulseFade = 0.5f;
    float PulseAlpha = 1f;

    public float Size;

    public Color PulseStartColor;
    public Color PulseEndColor;

    void Start()
    {
        Initialize(transform.localPosition, 0f, new Vector2(Size, Size));
    }

    public void Initialize(Vector2 position, float rotation, Vector2 size)
    {
        // starting local position starting point of rectangle at position
        transform.localScale = new Vector3(size.x * transform.localScale.x, size.y * transform.localScale.y, transform.localScale.z);
        transform.localPosition = new Vector3(position.x, position.y, transform.localPosition.z);
        transform.localEulerAngles = new Vector3(0f, 0f, rotation);
        HandleLifetime(0.75f);
    }

    public void Finish(float dur)
    {
        finishCoroutine = StartCoroutine(FinishCoroutine(dur));
    }
    IEnumerator FinishCoroutine(float dur)
    {
        float time = 0f;
        SpriteRenderer srPulse = Pulse.GetComponent<SpriteRenderer>();
        while (time < dur)
        {
            float t = time / dur;
            Pulse.transform.localScale = new Vector3(t, t, transform.localScale.z);
            float a = PulseAlpha;
            if (PulseFade > 0.02)
            {
                a *= Mathf.Min(Mathf.Lerp(0f, 1 / PulseFade, t), Mathf.Lerp(1 / PulseFade, 0f, t));
            }
            srPulse.color = LerpHSV(PulseStartColor, PulseEndColor, t);
            srPulse.color = new Color(srPulse.color.r, srPulse.color.g, srPulse.color.b, Mathf.Clamp01(a));
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
        outhsv.x = outhsv.x % 1f;
        Color c = Color.HSVToRGB(outhsv.x, outhsv.y, outhsv.z);
        c.a = Mathf.Lerp(start.a, end.a, t);
        return c;
    }

    public void HandleLifetime(float dur)
    {
        Finish(dur);
    }
}