using System.Collections.Generic;
using UnityEngine;

public enum Snd
{
    Catch, Drop, Cash, Buy, Collect, Tick, Laugh, Alarm, Crash, Splash, Punch,
    Spend, LevelUp, Thief, Quake, Throw, GlassBreak, Repair, TrashPickup,
    TrashDump, ShopToggle, Shark, MoneyPickup
}

// Lightweight procedural audio: the game remains self-contained and every
// important action has a distinct cue without requiring external audio files.
public static class Sfx
{
    static AudioSource src, music;
    static Dictionary<Snd, AudioClip> clips = new Dictionary<Snd, AudioClip>();
    static AudioClip calmMusic, dangerMusic;
    static bool danger;
    static float dangerUntil = -1f;
    static float lastSpend = -10f;
    public static bool Muted;

    public static void Clear()
    {
        src = null; music = null; clips.Clear();
        calmMusic = null; dangerMusic = null;
        danger = false; dangerUntil = -1f; Muted = false;
        GameAssets.Clear();
    }

    public static void Init(GameObject host, bool menuScene = false)
    {
        src = host.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        music = host.AddComponent<AudioSource>();
        music.spatialBlend = 0f;
        music.loop = true;
        music.volume = 0.16f;
        RuntimeAssetCatalog assets = GameAssets.Catalog;
        calmMusic = assets != null && menuScene && assets.menuMusic != null ? assets.menuMusic :
            assets != null && assets.calmMusic != null ? assets.calmMusic : CalmLoop();
        dangerMusic = assets != null && assets.dangerMusic != null ? assets.dangerMusic : DangerLoop();
        music.clip = calmMusic;
        music.Play();
        host.AddComponent<SfxDriver>();
    }

    public static void Play(Snd s, float vol = 0.6f)
    {
        if (src == null || Muted) return;
        if (s == Snd.Spend)
        {
            if (Time.unscaledTime - lastSpend < 0.11f) return;
            lastSpend = Time.unscaledTime;
        }
        AudioClip c;
        if (!clips.TryGetValue(s, out c) || c == null)
        {
            c = GameAssets.Sound(s);
            if (c == null) c = Make(s);
            clips[s] = c;
        }
        src.PlayOneShot(c, vol);
    }

    public static void BeginDanger()
    {
        danger = true;
        dangerUntil = -1f;
        RefreshMusic();
    }

    public static void DangerFor(float seconds)
    {
        danger = true;
        dangerUntil = Time.unscaledTime + seconds;
        RefreshMusic();
    }

    public static void EndDanger()
    {
        danger = false;
        dangerUntil = -1f;
        RefreshMusic();
    }

    internal static void Tick()
    {
        if (danger && dangerUntil > 0f && Time.unscaledTime >= dangerUntil) EndDanger();
    }

    static void RefreshMusic()
    {
        if (music == null) return;
        AudioClip wanted = danger ? dangerMusic : calmMusic;
        if (music.clip == wanted && music.isPlaying) return;
        music.Stop();
        music.clip = wanted;
        music.volume = danger ? 0.24f : 0.16f;
        music.Play();
    }

    static AudioClip Make(Snd s)
    {
        switch (s)
        {
            case Snd.Catch: return Tone(new float[] { 660f, 880f, 1175f }, 0.07f);
            case Snd.Drop: return Tone(new float[] { 440f, 330f }, 0.07f);
            case Snd.Cash: return Tone(new float[] { 880f, 1320f }, 0.06f);
            case Snd.Buy: return Tone(new float[] { 523f, 659f, 784f }, 0.08f);
            case Snd.Collect: return Tone(new float[] { 784f, 988f, 1175f }, 0.06f);
            case Snd.Tick: return Tone(new float[] { 1400f }, 0.03f);
            case Snd.Laugh: return Tone(new float[] { 700f, 550f, 700f, 550f, 700f, 450f }, 0.09f);
            case Snd.Alarm: return Tone(new float[] { 900f, 600f, 900f, 600f }, 0.11f);
            case Snd.Crash: return Noise(0.35f, 0.7f, 120f);
            case Snd.Splash: return Noise(0.28f, 0.35f, 620f);
            case Snd.Punch: return Tone(new float[] { 140f, 90f }, 0.05f);
            case Snd.Spend: return Tone(new float[] { 420f, 350f }, 0.035f);
            case Snd.LevelUp: return Tone(new float[] { 523f, 659f, 784f, 1046f, 1318f }, 0.11f);
            case Snd.Thief: return Tone(new float[] { 260f, 220f, 260f, 180f }, 0.1f);
            case Snd.Quake: return Noise(1.2f, 0.75f, 75f);
            case Snd.Throw: return Tone(new float[] { 520f, 390f, 260f }, 0.065f);
            case Snd.GlassBreak: return Noise(0.6f, 0.8f, 1800f);
            case Snd.Repair: return Tone(new float[] { 392f, 523f, 659f, 784f }, 0.08f);
            case Snd.TrashPickup: return Tone(new float[] { 300f, 480f }, 0.045f);
            case Snd.TrashDump: return Tone(new float[] { 300f, 220f, 520f }, 0.055f);
            case Snd.ShopToggle: return Tone(new float[] { 330f, 494f, 659f }, 0.075f);
            case Snd.Shark: return Tone(new float[] { 110f, 92f, 73f }, 0.16f);
            case Snd.MoneyPickup: return Tone(new float[] { 988f, 1318f, 1568f }, 0.07f);
            default: return Tone(new float[] { 784f }, 0.06f);
        }
    }

    static AudioClip Tone(float[] freqs, float noteDur)
    {
        const int sr = 22050;
        int noteSamples = (int)(sr * noteDur);
        float[] data = new float[noteSamples * freqs.Length];
        for (int n = 0; n < freqs.Length; n++)
            for (int i = 0; i < noteSamples; i++)
            {
                float t = (float)i / sr;
                float env = 1f - (float)i / noteSamples;
                data[n * noteSamples + i] = (Mathf.Sin(2f * Mathf.PI * freqs[n] * t) +
                    Mathf.Sin(4f * Mathf.PI * freqs[n] * t) * 0.18f) * env * 0.38f;
            }
        return Clip("sfx", data, sr);
    }

    static AudioClip Noise(float seconds, float amount, float tone)
    {
        const int sr = 22050;
        int count = (int)(sr * seconds);
        float[] data = new float[count];
        float smooth = 0f;
        for (int i = 0; i < count; i++)
        {
            float env = Mathf.Pow(1f - (float)i / count, 1.4f);
            smooth = Mathf.Lerp(smooth, Random.Range(-1f, 1f), Mathf.Clamp01(tone / sr * 8f));
            data[i] = smooth * env * amount;
        }
        return Clip("noise", data, sr);
    }

    static AudioClip CalmLoop()
    {
        const int sr = 22050;
        const float seconds = 24f;
        float[] data = new float[(int)(sr * seconds)];
        float[] roots = { 220f, 196f, 174.61f, 196f };
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            int chord = Mathf.FloorToInt(t / 6f) % roots.Length;
            float local = t % 6f;
            float fade = Mathf.SmoothStep(0f, 1f, Mathf.Min(local, 6f - local) / 0.8f);
            float r = roots[chord];
            float pad = Mathf.Sin(2f * Mathf.PI * r * t) * 0.12f +
                Mathf.Sin(2f * Mathf.PI * r * 1.5f * t) * 0.07f +
                Mathf.Sin(2f * Mathf.PI * r * 2f * t) * 0.04f;
            float sparkle = Mathf.Sin(2f * Mathf.PI * r * 4f * t) * (0.015f + 0.01f * Mathf.Sin(t * 0.7f));
            data[i] = (pad + sparkle) * fade;
        }
        return Clip("Calm Aquarium", data, sr);
    }

    static AudioClip DangerLoop()
    {
        const int sr = 22050;
        const float seconds = 8f;
        float[] data = new float[(int)(sr * seconds)];
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / sr;
            float pulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * 2f * t)), 5f);
            data[i] = Mathf.Sin(2f * Mathf.PI * 73f * t) * 0.18f +
                Mathf.Sin(2f * Mathf.PI * 110f * t) * pulse * 0.13f;
        }
        return Clip("Danger", data, sr);
    }

    static AudioClip Clip(string name, float[] data, int sampleRate)
    {
        AudioClip clip = AudioClip.Create(name, data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

public class SfxDriver : MonoBehaviour
{
    void Update() { Sfx.Tick(); }
}
