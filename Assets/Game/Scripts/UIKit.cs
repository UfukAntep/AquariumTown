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

    static Sprite rounded, circle, star;

    public static void Clear() { rounded = null; circle = null; star = null; }

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
            o.effectDistance = new Vector2(1.5f, -1.5f);
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
