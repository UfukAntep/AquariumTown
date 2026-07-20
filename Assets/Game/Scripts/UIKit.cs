using UnityEngine;
using UnityEngine.UI;

// Cartoon-style UI toolkit: procedural rounded sprites, stars, circles,
// drop shadows and outlined text. Used by every menu/HUD in the game.
public static class UIKit
{
    // palette
    public static readonly Color Blue = new Color(0.22f, 0.55f, 0.9f);
    public static readonly Color BlueDark = new Color(0.13f, 0.35f, 0.62f);
    public static readonly Color Cream = new Color(1f, 0.97f, 0.9f);
    public static readonly Color CreamDark = new Color(0.95f, 0.88f, 0.75f);
    public static readonly Color Orange = new Color(1f, 0.62f, 0.15f);
    public static readonly Color Green = new Color(0.35f, 0.78f, 0.38f);
    public static readonly Color Red = new Color(0.93f, 0.35f, 0.3f);
    public static readonly Color Purple = new Color(0.62f, 0.45f, 0.9f);
    public static readonly Color Yellow = new Color(1f, 0.82f, 0.2f);
    public static readonly Color TextDark = new Color(0.32f, 0.2f, 0.1f);
    public static readonly Color PanelShade = new Color(0f, 0f, 0f, 0.25f);

    static Sprite rounded, circle, star, poop, trophy, baton;

    public static void Clear() { rounded = null; circle = null; star = null; poop = null; trophy = null; baton = null; }

    public static Sprite Rounded()
    {
        if (rounded != null) return rounded;
        int n = 64, r = 20;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float a = 1f;
                int cx = x < r ? r : (x >= n - r ? n - r - 1 : -1);
                int cy = y < r ? r : (y >= n - r ? n - r - 1 : -1);
                if (cx >= 0 && cy >= 0)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    a = Mathf.Clamp01(r - d + 0.5f);
                }
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        rounded = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r + 2, r + 2, r + 2, r + 2));
        return rounded;
    }

    public static Sprite Circle()
    {
        if (circle != null) return circle;
        int n = 64;
        float c = n * 0.5f - 0.5f, rad = n * 0.5f - 1f;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(rad - d + 0.5f)));
            }
        tex.Apply();
        circle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        return circle;
    }

    // Unambiguous wooden baton icon (the imported "Bat" pictogram can read
    // as the animal at small sizes).
    public static Sprite Baton()
    {
        if (baton != null) return baton;
        const int n = 128;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        Vector2 a = new Vector2(32f, 24f), b = new Vector2(92f, 104f);
        Vector2 ab = b - a;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                Vector2 p = new Vector2(x, y);
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                float distance = Vector2.Distance(p, a + ab * t);
                float radius = Mathf.Lerp(9f, 16f, t);
                float alpha = Mathf.Clamp01(radius - distance + 0.8f);
                Color wood = Color.Lerp(new Color(0.38f, 0.18f, 0.06f), new Color(0.72f, 0.42f, 0.14f), t);
                tex.SetPixel(x, y, new Color(wood.r, wood.g, wood.b, alpha));
            }
        tex.Apply();
        baton = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f);
        return baton;
    }

    public static Sprite Star()
    {
        if (star != null) return star;
        int n = 128;
        float c = n * 0.5f;
        float outer = n * 0.48f, inner = n * 0.22f;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = x - c, dy = y - c;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx) + Mathf.PI * 0.5f;
                // 5-point star: radius oscillates with angle
                float t = Mathf.Repeat(ang / (Mathf.PI * 2f) * 5f, 1f);
                float tri = Mathf.Abs(t - 0.5f) * 2f; // 1 at points, 0 between
                float rad = Mathf.Lerp(inner, outer, Mathf.Pow(tri, 1.5f));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(rad - d + 0.5f)));
            }
        tex.Apply();
        star = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        return star;
    }

    public static Sprite Poop()
    {
        if (poop != null) return poop;
        int n = 96;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        Color brown = new Color(0.38f, 0.19f, 0.08f, 1f);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                Vector2 p = new Vector2(x, y);
                bool filled = (p - new Vector2(48f, 25f)).sqrMagnitude < 31f * 31f ||
                    (p - new Vector2(48f, 48f)).sqrMagnitude < 23f * 23f ||
                    (p - new Vector2(51f, 67f)).sqrMagnitude < 14f * 14f ||
                    (p - new Vector2(57f, 79f)).sqrMagnitude < 8f * 8f;
                Color pixel = new Color(brown.r, brown.g, brown.b, filled ? 1f : 0f);
                bool leftEye = (p - new Vector2(40f, 51f)).sqrMagnitude < 5f * 5f;
                bool rightEye = (p - new Vector2(57f, 51f)).sqrMagnitude < 5f * 5f;
                bool leftPupil = (p - new Vector2(41f, 50f)).sqrMagnitude < 2f * 2f;
                bool rightPupil = (p - new Vector2(58f, 50f)).sqrMagnitude < 2f * 2f;
                float smileRadius = Vector2.Distance(p, new Vector2(49f, 42f));
                bool smile = p.y < 42f && smileRadius > 8f && smileRadius < 11f && Mathf.Abs(p.x - 49f) < 10f;
                if (filled && (leftEye || rightEye)) pixel = Color.white;
                if (filled && (leftPupil || rightPupil || smile)) pixel = new Color(0.16f, 0.08f, 0.035f, 1f);
                tex.SetPixel(x, y, pixel);
            }
        tex.Apply();
        poop = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        return poop;
    }

    public static Sprite Trophy()
    {
        if (trophy != null) return trophy;
        int n = 96;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float half = y >= 43 && y <= 78 ? Mathf.Lerp(19f, 34f, (y - 43f) / 35f) : 0f;
                bool bowl = half > 0f && Mathf.Abs(x - 48f) <= half;
                bool stem = y >= 23 && y < 45 && Mathf.Abs(x - 48f) <= 7f;
                bool basePart = y >= 13 && y < 25 && Mathf.Abs(x - 48f) <= 25f;
                Vector2 lp = new Vector2((x - 18f) / 17f, (y - 59f) / 20f);
                Vector2 rp = new Vector2((x - 78f) / 17f, (y - 59f) / 20f);
                bool handles = lp.sqrMagnitude <= 1f || rp.sqrMagnitude <= 1f;
                bool innerHandles = new Vector2((x - 18f) / 9f, (y - 59f) / 12f).sqrMagnitude < 1f ||
                    new Vector2((x - 78f) / 9f, (y - 59f) / 12f).sqrMagnitude < 1f;
                float a = (bowl || stem || basePart || (handles && !innerHandles)) ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        trophy = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        return trophy;
    }

    // ---------- builders ----------
    public static GameObject Panel(Transform parent, Vector2 anchor, Vector2 pivot, Vector2 offset, Vector2 size, Color c, bool round = true, bool shadow = false)
    {
        GameObject go = new GameObject("Panel");
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        if (round)
        {
            img.sprite = Rounded();
            img.type = Image.Type.Sliced;
        }
        img.color = c;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = pivot;
        rt.anchoredPosition = offset; rt.sizeDelta = size;
        if (shadow)
        {
            Shadow sh = go.AddComponent<Shadow>();
            sh.effectColor = PanelShade;
            sh.effectDistance = new Vector2(0f, -5f);
        }
        return go;
    }

    public static GameObject Icon(Transform parent, Sprite sprite, Vector2 anchor, Vector2 offset, Vector2 size, Color c)
    {
        GameObject go = new GameObject("Icon");
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = c;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset; rt.sizeDelta = size;
        return go;
    }

    public static Text Label(Transform parent, string s, int size, Color c, TextAnchor anchor, bool outline = false)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        go.AddComponent<AutoLocalizeText>();
        t.font = B.UIFont; t.fontSize = size; t.fontStyle = FontStyle.Bold;
        t.color = c; t.text = s; t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; // keep text inside its card
        t.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        if (outline)
        {
            Outline o = go.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.35f);
            // Whole-pixel offsets stay crisp under the reference-resolution scaler.
            o.effectDistance = new Vector2(1f, -1f);
        }
        return t;
    }

    public static Button Btn(Transform parent, Vector2 pos, Vector2 size, Color c, string label, int fontSize, System.Action onClick)
    {
        GameObject go = Panel(parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, c, true, true);
        Button b = go.AddComponent<Button>();
        b.targetGraphic = go.GetComponent<Image>();
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        b.colors = cb;
        Label(go.transform, label, fontSize, Color.white, TextAnchor.MiddleCenter, true);
        b.onClick.AddListener(delegate { Sfx.Play(Snd.Tick, 0.3f); if (onClick != null) onClick(); });
        return b;
    }

    // pill with a colored circle icon on the left
    public static Text Pill(Transform parent, Vector2 anchor, Vector2 offset, Vector2 size, Color bg, Color iconColor, string iconChar, out GameObject root)
    {
        root = Panel(parent, anchor, anchor, offset, size, bg, true, true);
        GameObject ic = Icon(root.transform, Circle(), new Vector2(0f, 0.5f), new Vector2(size.y * 0.55f, 0f), new Vector2(size.y * 0.78f, size.y * 0.78f), iconColor);
        Label(ic.transform, iconChar, Mathf.RoundToInt(size.y * 0.5f), Color.white, TextAnchor.MiddleCenter, true);
        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(root.transform, false);
        RectTransform rt = textArea.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(size.y, 0f); rt.offsetMax = new Vector2(-10f, 0f);
        return Label(textArea.transform, "", Mathf.RoundToInt(size.y * 0.48f), TextDark, TextAnchor.MiddleCenter);
    }
}
