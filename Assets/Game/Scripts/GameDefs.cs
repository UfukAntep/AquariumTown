using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// ---------- Global refs ----------
public static class Game
{
    public static GameManager gm;
    public static PlayerController player;
    public static CashRegister register;
    public static UIManager ui;
    public static PCScreen pc;
    public static Sea sea;
    public static Depot depot;
    public static TrashSystem trash;
    public static Toilets toilets;
    public static CameraRig cam;
    public static EventManager events;
    public static Transform world;
    public static List<Tank> tanks = new List<Tank>();
    public static List<Staff> staff = new List<Staff>();

    public static void Clear()
    {
        gm = null; player = null; register = null; ui = null; pc = null; sea = null;
        depot = null; trash = null; toilets = null; cam = null; events = null; world = null;
        tanks = new List<Tank>();
        staff = new List<Staff>();
    }

    public static Tank TankOf(int sp)
    {
        for (int i = 0; i < tanks.Count; i++) if (tanks[i].species == sp) return tanks[i];
        return null;
    }

    public static int TotalStock()
    {
        int n = 0;
        for (int i = 0; i < tanks.Count; i++) n += tanks[i].Count;
        return n;
    }

    public static int StockedTankCount()
    {
        int n = 0;
        for (int i = 0; i < tanks.Count; i++) if (tanks[i].HasStock) n++;
        return n;
    }
}

// ---------- 80 procedural species ----------
public static class SpeciesInfo
{
    public const int Count = 80;

    static bool init;
    static string[] names = new string[Count];
    static int[] prices = new int[Count];
    static int[] plotCosts = new int[Count];
    static float[] catchTimes = new float[Count];
    static Color[] mains = new Color[Count];
    static Color[] accents = new Color[Count];
    static float[] sizes = new float[Count];

    static readonly string[] adjs = { "Altin", "Mercan", "Inci", "Zumrut", "Yakut", "Safir", "Gece", "Seker", "Firtina", "Kral" };
    static readonly string[] nouns = { "Palyaco", "Denizati", "Kaplumbaga", "Yengec", "Istakoz", "Murekkep", "Balon", "Koi" };

    static void EnsureInit()
    {
        if (init) return;
        init = true;
        System.Random rnd = new System.Random(4242);
        for (int i = 0; i < Count; i++)
        {
            names[i] = adjs[(i / 8) % adjs.Length] + " " + nouns[i % 8];
            prices[i] = Mathf.Max(5, Mathf.RoundToInt(5f * Mathf.Pow(1.17f, i)));
            plotCosts[i] = i == 0 ? 0 : prices[i] * 8 + 20;
            catchTimes[i] = Mathf.Min(1f + 0.045f * i, 5f);
            float h = (i * 0.61803f) % 1f;
            mains[i] = Color.HSVToRGB(h, 0.55f + (float)rnd.NextDouble() * 0.3f, 0.8f + (float)rnd.NextDouble() * 0.2f);
            accents[i] = Color.HSVToRGB((h + 0.35f + (float)rnd.NextDouble() * 0.2f) % 1f, 0.5f, 0.95f);
            sizes[i] = 0.85f + (float)rnd.NextDouble() * 0.5f + (i / (float)Count) * 0.5f;
        }
    }

    // name matches the actual model the species maps to (consistency), with a
    // rarity adjective; falls back to the procedural name if the pack is absent
    public static string Name(int i)
    {
        EnsureInit();
        string animal = AssetLib.AnimalName(i);
        if (!string.IsNullOrEmpty(animal)) return adjs[(i / 8) % adjs.Length] + " " + animal;
        return names[i];
    }
    public static int Price(int i) { EnsureInit(); return prices[i]; }
    public static int PlotCost(int i) { EnsureInit(); return plotCosts[i]; }
    public static float CatchTime(int i) { EnsureInit(); return catchTimes[i]; }
    public static Color MainColor(int i) { EnsureInit(); return mains[i]; }
    public static int ReqLevel(int i) { return i + 1; }

    public static Transform Build(int id, Transform parent, float scale)
    {
        EnsureInit();
        GameObject root = new GameObject("Model_" + id);
        root.transform.SetParent(parent, false);
        Transform t = root.transform;
        Material main = MatLib.Get(mains[id]);
        Material acc = MatLib.Get(accents[id]);
        Material dark = MatLib.Get(new Color(0.1f, 0.1f, 0.12f));
        float k = scale * sizes[id];
        int body = id % 8;

        switch (body)
        {
            case 0: // clown-style striped fish
                B.Prim(PrimitiveType.Sphere, "Body", t, Vector3.zero, Vector3.zero, new Vector3(0.5f * k, 0.45f * k, 0.9f * k), main);
                B.Prim(PrimitiveType.Cube, "Stripe1", t, new Vector3(0f, 0f, 0.2f * k), Vector3.zero, new Vector3(0.52f * k, 0.46f * k, 0.12f * k), acc);
                B.Prim(PrimitiveType.Cube, "Stripe2", t, new Vector3(0f, 0f, -0.15f * k), Vector3.zero, new Vector3(0.48f * k, 0.42f * k, 0.12f * k), acc);
                B.Prim(PrimitiveType.Cube, "Tail", t, new Vector3(0f, 0f, -0.55f * k), new Vector3(0f, 0f, 45f), new Vector3(0.06f * k, 0.3f * k, 0.3f * k), acc);
                break;
            case 1: // seahorse
                B.Prim(PrimitiveType.Capsule, "Body", t, Vector3.zero, new Vector3(20f, 0f, 0f), new Vector3(0.3f * k, 0.5f * k, 0.3f * k), main);
                B.Prim(PrimitiveType.Sphere, "Head", t, new Vector3(0f, 0.5f * k, 0.15f * k), Vector3.zero, Vector3.one * 0.3f * k, main);
                B.Prim(PrimitiveType.Cube, "Snout", t, new Vector3(0f, 0.5f * k, 0.38f * k), Vector3.zero, new Vector3(0.1f * k, 0.1f * k, 0.3f * k), acc);
                B.Prim(PrimitiveType.Cube, "Tail", t, new Vector3(0f, -0.5f * k, -0.1f * k), new Vector3(45f, 0f, 0f), new Vector3(0.12f * k, 0.12f * k, 0.35f * k), main);
                break;
            case 2: // turtle
                B.Prim(PrimitiveType.Sphere, "Shell", t, Vector3.zero, Vector3.zero, new Vector3(0.8f * k, 0.4f * k, 0.95f * k), acc);
                B.Prim(PrimitiveType.Sphere, "Head", t, new Vector3(0f, 0f, 0.6f * k), Vector3.zero, Vector3.one * 0.32f * k, main);
                B.Prim(PrimitiveType.Cube, "FlipFL", t, new Vector3(0.45f * k, -0.05f * k, 0.3f * k), new Vector3(0f, 30f, 0f), new Vector3(0.4f * k, 0.08f * k, 0.2f * k), main);
                B.Prim(PrimitiveType.Cube, "FlipFR", t, new Vector3(-0.45f * k, -0.05f * k, 0.3f * k), new Vector3(0f, -30f, 0f), new Vector3(0.4f * k, 0.08f * k, 0.2f * k), main);
                break;
            case 3: // crab
                B.Prim(PrimitiveType.Sphere, "Body", t, Vector3.zero, Vector3.zero, new Vector3(0.8f * k, 0.35f * k, 0.6f * k), main);
                B.Prim(PrimitiveType.Sphere, "ClawL", t, new Vector3(0.5f * k, 0.05f * k, 0.35f * k), Vector3.zero, Vector3.one * 0.3f * k, acc);
                B.Prim(PrimitiveType.Sphere, "ClawR", t, new Vector3(-0.5f * k, 0.05f * k, 0.35f * k), Vector3.zero, Vector3.one * 0.3f * k, acc);
                B.Prim(PrimitiveType.Sphere, "EyeL", t, new Vector3(0.15f * k, 0.25f * k, 0.25f * k), Vector3.zero, Vector3.one * 0.12f * k, dark);
                B.Prim(PrimitiveType.Sphere, "EyeR", t, new Vector3(-0.15f * k, 0.25f * k, 0.25f * k), Vector3.zero, Vector3.one * 0.12f * k, dark);
                break;
            case 4: // lobster
                B.Prim(PrimitiveType.Capsule, "Body", t, Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(0.35f * k, 0.55f * k, 0.35f * k), main);
                B.Prim(PrimitiveType.Cube, "TailFan", t, new Vector3(0f, 0f, -0.65f * k), Vector3.zero, new Vector3(0.45f * k, 0.08f * k, 0.25f * k), acc);
                B.Prim(PrimitiveType.Capsule, "ClawL", t, new Vector3(0.28f * k, 0f, 0.6f * k), new Vector3(90f, 0f, 0f), new Vector3(0.18f * k, 0.25f * k, 0.18f * k), main);
                B.Prim(PrimitiveType.Capsule, "ClawR", t, new Vector3(-0.28f * k, 0f, 0.6f * k), new Vector3(90f, 0f, 0f), new Vector3(0.18f * k, 0.25f * k, 0.18f * k), main);
                break;
            case 5: // squid
                B.Prim(PrimitiveType.Capsule, "Mantle", t, new Vector3(0f, 0f, 0.15f * k), new Vector3(90f, 0f, 0f), new Vector3(0.4f * k, 0.5f * k, 0.4f * k), main);
                for (int i = 0; i < 4; i++)
                    B.Prim(PrimitiveType.Cube, "Tent" + i, t, new Vector3((i - 1.5f) * 0.12f * k, 0f, -0.5f * k), Vector3.zero, new Vector3(0.07f * k, 0.07f * k, 0.5f * k), acc);
                B.Prim(PrimitiveType.Cube, "FinL", t, new Vector3(0.25f * k, 0f, 0.55f * k), new Vector3(0f, 35f, 0f), new Vector3(0.3f * k, 0.05f * k, 0.2f * k), main);
                B.Prim(PrimitiveType.Cube, "FinR", t, new Vector3(-0.25f * k, 0f, 0.55f * k), new Vector3(0f, -35f, 0f), new Vector3(0.3f * k, 0.05f * k, 0.2f * k), main);
                break;
            case 6: // puffer
                B.Prim(PrimitiveType.Sphere, "Body", t, Vector3.zero, Vector3.zero, Vector3.one * 0.7f * k, main);
                for (int i = 0; i < 8; i++)
                {
                    float a = i * 45f * Mathf.Deg2Rad;
                    Vector3 dir = new Vector3(Mathf.Cos(a) * 0.8f, 0.35f + 0.25f * ((i % 2) * 2 - 1) * 0.4f, Mathf.Sin(a) * 0.8f);
                    B.Prim(PrimitiveType.Cube, "Spike" + i, t, dir.normalized * 0.35f * k, Vector3.zero, Vector3.one * 0.1f * k, acc);
                }
                B.Prim(PrimitiveType.Sphere, "EyeL", t, new Vector3(0.18f * k, 0.15f * k, 0.3f * k), Vector3.zero, Vector3.one * 0.12f * k, dark);
                B.Prim(PrimitiveType.Sphere, "EyeR", t, new Vector3(-0.18f * k, 0.15f * k, 0.3f * k), Vector3.zero, Vector3.one * 0.12f * k, dark);
                break;
            default: // koi
                B.Prim(PrimitiveType.Sphere, "Body", t, Vector3.zero, Vector3.zero, new Vector3(0.5f * k, 0.45f * k, 1.05f * k), main);
                B.Prim(PrimitiveType.Sphere, "Patch1", t, new Vector3(0.1f * k, 0.18f * k, 0.15f * k), Vector3.zero, new Vector3(0.3f * k, 0.15f * k, 0.35f * k), acc);
                B.Prim(PrimitiveType.Sphere, "Patch2", t, new Vector3(-0.08f * k, 0.18f * k, -0.25f * k), Vector3.zero, new Vector3(0.25f * k, 0.12f * k, 0.28f * k), acc);
                B.Prim(PrimitiveType.Cube, "Tail", t, new Vector3(0f, 0f, -0.65f * k), new Vector3(0f, 0f, 45f), new Vector3(0.06f * k, 0.35f * k, 0.35f * k), acc);
                break;
        }
        return t;
    }
}

// ---------- Upgrades ----------
public enum Upg
{
    Capacity = 0, MoveSpeed = 1, SwimSpeed = 2, RadarSpeed = 3, RadarRange = 4,
    TipChance = 5, CustSpeed = 6, ExtraCash = 7
}

public static class UpgInfo
{
    public static readonly int Count = 8;
    static readonly string[] label = { "CANTA", "KOSU HIZI", "YUZME HIZI", "RADAR HIZI", "RADAR MENZILI", "BAHSIS", "MUSTERI HIZI", "EKSTRA PARA" };
    static readonly string[] desc = { "+3 tasima", "+%8 hiz", "+%10 hiz", "+%12 tarama", "+0.5 menzil", "+%6 bahsis", "+%10 hiz", "+%10 satis" };
    static readonly int[] baseCost = { 40, 60, 60, 80, 80, 120, 100, 150 };
    static readonly float[] mult = { 1.9f, 1.8f, 1.8f, 1.9f, 1.9f, 2.0f, 1.9f, 2.1f };
    static readonly int[] max = { 15, 12, 12, 10, 10, 8, 8, 10 };

    public static string Label(Upg u) { return label[(int)u]; }
    public static string Desc(Upg u) { return desc[(int)u]; }
    public static int BaseCost(Upg u) { return baseCost[(int)u]; }
    public static float Mult(Upg u) { return mult[(int)u]; }
    public static int Max(Upg u) { return max[(int)u]; }
}

// ---------- Staff roles ----------
public static class StaffInfo
{
    public const int RoleCount = 8;
    // roles: 0 cashier,1 fisher,2 carrier,3 janitor,4 sea cleaner,5 toilet cleaner,6 security,7 beach cleaner
    public static readonly string[] Names = { "KASIYER", "AVCI", "TASIYICI", "TEMIZLIKCI", "DENIZ TEMIZLIGI", "TUVALETCI", "GUVENLIK", "SAHIL TEMIZLIGI" };
    public static readonly string[] Descs = {
        "Kasada calisir (maks 1)", "Denizden balik toplar", "Depodan tanklara tasir",
        "Magaza coplerini toplar", "Denizdeki copleri toplar", "Tuvaletleri temizler",
        "Hirsizlari dover ve caldigini geri koyar", "Sahildeki copleri toplar" };
    // paid as a DAILY SALARY (deducted every day-end), not a one-time fee
    public static readonly int[] Salary = { 120, 260, 200, 90, 170, 110, 300, 140 };
    public static readonly int[] MaxCount = { 1, 6, 4, 4, 4, 3, 3, 3 };
}

// ---------- Decor & equipment items (bought on the PC) ----------
public static class DecorInfo
{
    public const int Count = 8;
    public static readonly string[] Names = { "Palmiyeler", "Fiskiye", "Balon Kemeri", "Balik Heykeli", "Kirmizi Hali", "Fener Direkleri", "Ziplama Rampasi", "Jetski" };
    public static readonly int[] Costs = { 500, 1200, 800, 2500, 600, 1500, 800, 3500 };
}

// ---------- Materials & textures ----------
public static class MatLib
{
    static Dictionary<Color, Material> solid = new Dictionary<Color, Material>();
    static Dictionary<Color, Material> glass = new Dictionary<Color, Material>();
    static Material waterMat, grassMat, beachMat;
    public static Material FloorMat, WallMat; // paintable

    public static void Clear()
    {
        solid.Clear(); glass.Clear();
        waterMat = null; grassMat = null; beachMat = null; FloorMat = null; WallMat = null;
    }

    static Shader Lit()
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        return s;
    }

    public static Material Get(Color c)
    {
        Material m;
        if (solid.TryGetValue(c, out m) && m != null) return m;
        m = new Material(Lit());
        m.color = c;
        m.SetFloat("_Smoothness", 0.25f);
        solid[c] = m;
        return m;
    }

    public static Material Glass(Color c)
    {
        Material m;
        if (glass.TryGetValue(c, out m) && m != null) return m;
        m = new Material(Lit());
        MakeTransparent(m);
        m.color = c;
        m.SetFloat("_Smoothness", 0.75f);
        glass[c] = m;
        return m;
    }

    static void MakeTransparent(Material m)
    {
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent;
    }

    public static readonly Color[] FloorStyles = {
        new Color(0.99f, 0.87f, 0.64f), new Color(0.85f, 0.9f, 0.95f),
        new Color(0.95f, 0.8f, 0.85f), new Color(0.8f, 0.92f, 0.8f) };
    public static readonly Color[] WallStyles = {
        new Color(0.3f, 0.75f, 0.9f), new Color(0.9f, 0.6f, 0.4f),
        new Color(0.6f, 0.5f, 0.9f), new Color(0.4f, 0.8f, 0.6f) };

    public static Material Floor()
    {
        if (FloorMat != null) return FloorMat;
        Texture2D tex = TriangleTex(Color.white, new Color(0.94f, 0.94f, 0.94f));
        FloorMat = new Material(Lit());
        FloorMat.mainTexture = tex;
        FloorMat.color = FloorStyles[0];
        FloorMat.SetFloat("_Smoothness", 0.1f);
        FloorMat.mainTextureScale = new Vector2(20f, 20f);
        return FloorMat;
    }

    public static Material Wall()
    {
        if (WallMat != null) return WallMat;
        WallMat = new Material(Lit());
        WallMat.color = WallStyles[0];
        WallMat.SetFloat("_Smoothness", 0.3f);
        return WallMat;
    }

    public static Material Water()
    {
        if (waterMat != null) return waterMat;
        Texture2D tex = CellTex(new Color(0.35f, 0.78f, 0.95f), new Color(0.47f, 0.86f, 1f));
        waterMat = new Material(Lit());
        waterMat.mainTexture = tex;
        waterMat.color = Color.white;
        waterMat.SetFloat("_Smoothness", 0.55f);
        waterMat.mainTextureScale = new Vector2(22f, 22f);
        return waterMat;
    }

    public static Material Grass()
    {
        if (grassMat != null) return grassMat;
        Texture2D tex = CellTex(new Color(0.55f, 0.85f, 0.4f), new Color(0.63f, 0.9f, 0.46f));
        grassMat = new Material(Lit());
        grassMat.mainTexture = tex;
        grassMat.color = Color.white;
        grassMat.SetFloat("_Smoothness", 0.1f);
        grassMat.mainTextureScale = new Vector2(10f, 10f);
        return grassMat;
    }

    public static Material Beach()
    {
        if (beachMat != null) return beachMat;
        beachMat = new Material(Lit());
        beachMat.color = new Color(0.99f, 0.94f, 0.75f);
        beachMat.SetFloat("_Smoothness", 0.1f);
        return beachMat;
    }

    static Texture2D TriangleTex(Color a, Color b)
    {
        int n = 64;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGB24, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float fx = (float)x / n, fy = (float)y / n;
                tex.SetPixel(x, y, (fx + fy) % 1f < 0.5f ? a : b);
            }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D CellTex(Color a, Color b)
    {
        int n = 64;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGB24, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float v = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                tex.SetPixel(x, y, Color.Lerp(a, b, Mathf.Round(v * 3f) / 3f));
            }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }
}

// ---------- Build helpers ----------
public static class B
{
    static Font font;
    public static Font UIFont
    {
        get
        {
            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
                if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            }
            return font;
        }
    }

    public static GameObject Prim(PrimitiveType t, string name, Transform parent, Vector3 pos, Vector3 euler, Vector3 scale, Material m, bool keepCollider = false)
    {
        GameObject go = GameObject.CreatePrimitive(t);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localEulerAngles = euler;
        go.transform.localScale = scale;
        if (m != null) go.GetComponent<MeshRenderer>().sharedMaterial = m;
        if (!keepCollider)
        {
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
        }
        return go;
    }

    public static TextMesh Text3D(string txt, Transform parent, Vector3 localPos, float size, Color c, bool billboard = true)
    {
        GameObject go = new GameObject("Text3D");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        TextMesh tm = go.AddComponent<TextMesh>();
        tm.font = UIFont;
        tm.fontSize = 64;
        tm.characterSize = size;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = c;
        tm.text = txt;
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (UIFont != null) mr.sharedMaterial = UIFont.material;
        if (billboard) go.AddComponent<Billboard>();
        return tm;
    }

    public static Vector3 Parabola(Vector3 a, Vector3 b, float height, float t)
    {
        Vector3 p = Vector3.Lerp(a, b, t);
        p.y += 4f * height * t * (1f - t);
        return p;
    }

    public static string Money(int v)
    {
        if (v >= 1000000) return (v / 1000000f).ToString("0.##") + "M";
        if (v >= 1000) return (v / 1000f).ToString("0.##") + "K";
        return v.ToString();
    }

    public static Mesh FanMesh(float angleDeg, float radius, int segments = 14)
    {
        Mesh mesh = new Mesh();
        Vector3[] verts = new Vector3[segments + 2];
        int[] tris = new int[segments * 3];
        verts[0] = Vector3.zero;
        float half = angleDeg * 0.5f * Mathf.Deg2Rad;
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.Lerp(-half, half, (float)i / segments);
            verts[i + 1] = new Vector3(Mathf.Sin(a) * radius, 0f, Mathf.Cos(a) * radius);
        }
        for (int i = 0; i < segments; i++)
        {
            tris[i * 3] = 0; tris[i * 3 + 1] = i + 1; tris[i * 3 + 2] = i + 2;
        }
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    public static GameObject SpeciesBubble(int sp, Transform parent, Vector3 localPos)
    {
        GameObject root = new GameObject("Bubble");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        Prim(PrimitiveType.Sphere, "Bg", root.transform, Vector3.zero, Vector3.zero, new Vector3(1.5f, 1.5f, 0.25f), MatLib.Get(Color.white));
        // same model source as the tank fish, so the bubble matches what's inside
        GameObject q = AssetLib.SpawnSeaAnimal(sp, root.transform, 1f);
        Transform model = q != null ? q.transform : SpeciesInfo.Build(sp, root.transform, 0.8f);
        model.localPosition = new Vector3(0f, 0f, -0.25f);
        model.localEulerAngles = new Vector3(0f, 90f, 0f);
        root.AddComponent<Billboard>();
        return root;
    }

    public static GameObject Stickman(Transform parent, Color c, float scale = 1f)
    {
        GameObject v = new GameObject("Stick");
        v.transform.SetParent(parent, false);
        Material m = MatLib.Get(c);
        Prim(PrimitiveType.Capsule, "Body", v.transform, new Vector3(0f, 0.9f * scale, 0f), Vector3.zero, new Vector3(0.7f, 0.65f, 0.7f) * scale, m);
        Prim(PrimitiveType.Sphere, "Head", v.transform, new Vector3(0f, 1.8f * scale, 0f), Vector3.zero, Vector3.one * 0.52f * scale, m);
        return v;
    }
}

public class Billboard : MonoBehaviour
{
    static Camera cam;
    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}

public class Bobber : MonoBehaviour
{
    public float amp = 0.15f, speed = 2f;
    float phase; Vector3 basePos;
    void Start() { basePos = transform.localPosition; phase = Random.value * 6f; }
    void Update() { transform.localPosition = basePos + Vector3.up * Mathf.Sin(Time.time * speed + phase) * amp; }
}

public class Spinner : MonoBehaviour
{
    public float speed = 60f;
    void Update() { transform.Rotate(Vector3.up, speed * Time.unscaledDeltaTime); }
}
