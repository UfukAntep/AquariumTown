using System.Collections.Generic;
using UnityEngine;

public enum Snd { Catch, Drop, Cash, Buy, Collect, Tick, Laugh, Alarm, Crash, Splash, Punch }

public static class Sfx
{
    static AudioSource src;
    static Dictionary<Snd, AudioClip> clips = new Dictionary<Snd, AudioClip>();
    public static bool Muted;

    public static void Clear() { src = null; clips.Clear(); Muted = false; }

    public static void Init(GameObject host)
    {
        src = host.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
    }

    public static void Play(Snd s, float vol = 0.6f)
    {
        if (src == null || Muted) return;
        AudioClip c;
        if (!clips.TryGetValue(s, out c) || c == null)
        {
            c = Make(s);
            clips[s] = c;
        }
        src.PlayOneShot(c, vol);
    }

    static AudioClip Make(Snd s)
    {
        switch (s)
        {
            case Snd.Catch: return Tone(new float[] { 660f, 880f }, 0.06f);
            case Snd.Drop: return Tone(new float[] { 440f, 330f }, 0.07f);
            case Snd.Cash: return Tone(new float[] { 880f, 1320f }, 0.06f);
            case Snd.Buy: return Tone(new float[] { 523f, 659f, 784f, 1046f }, 0.08f);
            case Snd.Tick: return Tone(new float[] { 1400f }, 0.03f);
            case Snd.Laugh: return Tone(new float[] { 700f, 550f, 700f, 550f, 700f, 450f }, 0.09f);
            case Snd.Alarm: return Tone(new float[] { 900f, 600f, 900f, 600f }, 0.11f);
            case Snd.Crash: return Tone(new float[] { 200f, 150f, 100f }, 0.12f);
            case Snd.Splash: return Tone(new float[] { 300f, 500f, 250f }, 0.08f);
            case Snd.Punch: return Tone(new float[] { 140f, 90f }, 0.05f); // "pat!" thud
            default: return Tone(new float[] { 784f, 988f, 1175f }, 0.06f);
        }
    }

    static AudioClip Tone(float[] freqs, float noteDur)
    {
        int sr = 22050;
        int noteSamples = (int)(sr * noteDur);
        int total = noteSamples * freqs.Length;
        float[] data = new float[total];
        for (int n = 0; n < freqs.Length; n++)
        {
            for (int i = 0; i < noteSamples; i++)
            {
                float t = (float)i / sr;
                float env = 1f - (float)i / noteSamples;
                data[n * noteSamples + i] = Mathf.Sin(2f * Mathf.PI * freqs[n] * t) * env * 0.5f;
            }
        }
        AudioClip clip = AudioClip.Create("sfx", total, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }
}
