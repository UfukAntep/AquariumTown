using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// ---------- Global refs ----------
public static class Game
{
    public static GameManager gm;
    public static PlayerController player;
    public static CashRegister register;
    public static ManagerDesk managerDesk;
    public static UIManager ui;
    public static PCScreen pc;
    public static Sea sea;
    public static Depot depot;
    public static TrashSystem trash;
    public static Toilets toilets;
    public static CameraRig cam;
    public static EventManager events;
    public static Jetski jetski;
    public static RampZone ramp;
    public static GeneratorUnit generator;
    public static CameraDeskUnit cameraDesk;
    public static Transform world;
    public static List<Tank> tanks = new List<Tank>();
    public static List<Staff> staff = new List<Staff>();

    public static void Clear()
    {
        gm = null; player = null; register = null; managerDesk = null; ui = null; pc = null; sea = null;
        depot = null; trash = null; toilets = null; cam = null; events = null; jetski = null; ramp = null; generator = null; cameraDesk = null; world = null;
        tanks = new List<Tank>();
        staff = new List<Staff>();
        TrophySystem.ClearRuntime();
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

    public static void ReturnFishToStorage(int species)
    {
        Tank tank = TankOf(species);
        if (tank != null && tank.AddCount(1) == 1) return;
        if (depot != null && depot.HasSpace) depot.Store(species);
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
    TipChance = 5, CustSpeed = 6, ExtraCash = 7, Sprint = 8
}

public enum ControlAction
{
    Forward, Backward, Left, Right, Punch, Interact, Camera, Map
}

// Player-configurable primary controls. Arrow keys remain permanent secondary
// movement bindings so keyboard movement is always recoverable.
public static class ControlBindings
{
    const string P = "AT3_Control_";
    static readonly KeyCode[] defaults = {
        KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space,
        KeyCode.E, KeyCode.C, KeyCode.M
    };
    public static readonly string[] Names = {
        "ILERI", "GERI", "SOLA", "SAGA", "YUMRUK", "ETKILESIM", "KAMERA", "HARITA"
    };

    public static KeyCode Key(ControlAction action)
    {
        int i = (int)action;
        return (KeyCode)PlayerPrefs.GetInt(P + i, (int)defaults[i]);
    }

    public static void Set(ControlAction action, KeyCode key)
    {
        KeyCode previous = Key(action);
        for (int i = 0; i < defaults.Length; i++)
            if (i != (int)action && Key((ControlAction)i) == key)
                PlayerPrefs.SetInt(P + i, (int)previous);
        PlayerPrefs.SetInt(P + (int)action, (int)key);
        PlayerPrefs.Save();
    }

    public static bool Down(ControlAction action) { return Input.GetKeyDown(Key(action)); }
    public static bool Held(ControlAction action) { return Input.GetKey(Key(action)); }
    public static int MoveMouseButton { get { return PlayerPrefs.GetInt(P + "MouseMove", 0); } }
    public static int PunchMouseButton { get { return MoveMouseButton == 0 ? 1 : 0; } }
    public static void SwapMouseButtons()
    {
        PlayerPrefs.SetInt(P + "MouseMove", MoveMouseButton == 0 ? 1 : 0);
        PlayerPrefs.Save();
    }
    public static string KeyName(ControlAction action) { return Key(action).ToString(); }
    public static string MouseName(int button) { return button == 0 ? "SOL TIK" : "SAG TIK"; }
    public static int[] Snapshot()
    {
        int[] values = new int[defaults.Length + 1];
        for (int i = 0; i < defaults.Length; i++) values[i] = (int)Key((ControlAction)i);
        values[defaults.Length] = MoveMouseButton;
        return values;
    }
    public static void Restore(int[] values)
    {
        if (values == null || values.Length < defaults.Length + 1) return;
        for (int i = 0; i < defaults.Length; i++) PlayerPrefs.SetInt(P + i, values[i]);
        PlayerPrefs.SetInt(P + "MouseMove", values[defaults.Length]);
        PlayerPrefs.Save();
    }
}

public static class UpgInfo
{
    public static readonly int Count = 9;
    static readonly string[] label = { "CANTA", "KOSU HIZI", "YUZME HIZI", "RADAR HIZI", "RADAR MENZILI", "BAHSIS", "MUSTERI HIZI", "EKSTRA PARA", "DEPAR" };
    static readonly string[] desc = {
        "+3 balik tasima kapasitesi", "+%8 kosu hizi", "+%10 yuzme hizi", "+%12 deniz tarama hizi",
        "+0.5 deniz radar menzili", "+%6 musteri bahsis sansi", "+%10 musteri hareket hizi", "+%10 daha fazla satis geliri",
        "Shift basiliyken hizli kos; her seviye depari hizlandirir" };
    static readonly int[] baseCost = { 40, 60, 60, 80, 80, 120, 100, 150, 220 };
    static readonly float[] mult = { 1.9f, 1.8f, 1.8f, 1.9f, 1.9f, 2.0f, 1.9f, 2.1f, 2.2f };
    static readonly int[] max = { 15, 12, 12, 10, 10, 8, 8, 10, 5 };

    public static string Label(Upg u) { return label[(int)u]; }
    public static string Desc(Upg u) { return desc[(int)u]; }
    public static int BaseCost(Upg u) { return baseCost[(int)u]; }
    public static float Mult(Upg u) { return mult[(int)u]; }
    public static int Max(Upg u) { return max[(int)u]; }
}

// ---------- Staff roles ----------
public static class StaffInfo
{
    public const int RoleCount = 9;
    // roles: 0 cashier,1 fisher,2 carrier,3 janitor,4 sea cleaner,5 toilet cleaner,6 security,7 beach cleaner,8 electrician
    public static readonly string[] Names = { "KASIYER", "AVCI", "TASIYICI", "TEMIZLIKCI", "DENIZ TEMIZLIGI", "TUVALETCI", "GUVENLIK", "SAHIL TEMIZLIGI", "ELEKTRIK TEKNISYENI" };
    public static readonly string[] Descs = {
        "Kasada calisir (maks 1)", "Denizden balik toplar", "Depodan tanklara tasir",
        "Magaza coplerini toplar", "Denizdeki copleri toplar", "Tuvaletleri temizler",
        "Hirsizlari dover ve caldigini geri koyar", "Sahildeki copleri toplar", "Kesintide jeneratore kosup elektrigi geri getirir" };
    // paid as a DAILY SALARY (deducted every day-end), not a one-time fee
    public static readonly int[] Salary = { 120, 260, 200, 90, 170, 110, 300, 140, 240 };
    public static readonly int[] MaxCount = { 1, 6, 4, 4, 4, 3, 3, 3, 2 };
}

// ---------- Decor & equipment items (bought on the PC) ----------
public static class DecorInfo
{
    public const int Count = 9;
    public static readonly string[] Names = { "Palmiyeler", "Fiskiye", "Balon Kemeri", "Balik Heykeli", "Kirmizi Hali", "Fener Direkleri", "Ziplama Rampasi", "Jetski", "Iskele" };
    public static readonly int[] Costs = { 500, 1200, 800, 2500, 600, 1500, 800, 3500, 5000 };
}

// ---------- Materials & textures ----------
public static class MatLib
{
    static Dictionary<Color, Material> solid = new Dictionary<Color, Material>();
    static Dictionary<Color, Material> glass = new Dictionary<Color, Material>();
    static Material waterMat, grassMat, beachMat, runtimeLit, runtimeLitTransparent;
    public static Material FloorMat, WallMat; // paintable

    public static void Clear()
    {
        solid.Clear(); glass.Clear();
        waterMat = null; grassMat = null; beachMat = null; FloorMat = null; WallMat = null;
        runtimeLit = null; runtimeLitTransparent = null;
    }

    static Shader Lit()
    {
        if (runtimeLit == null) runtimeLit = Resources.Load<Material>("RuntimeLit");
        Shader s = runtimeLit != null ? runtimeLit.shader : null;
        if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Hidden/InternalErrorShader");
        return s;
    }

    static Material NewLitMaterial(bool transparent = false)
    {
        Material template;
        if (transparent)
        {
            if (runtimeLitTransparent == null)
                runtimeLitTransparent = Resources.Load<Material>("RuntimeLitTransparent");
            template = runtimeLitTransparent;
        }
        else
        {
            if (runtimeLit == null) runtimeLit = Resources.Load<Material>("RuntimeLit");
            template = runtimeLit;
        }
        if (template != null) return new Material(template);

        Shader shader = Lit();
        if (shader != null) return new Material(shader);

        // Last-resort protection for aggressively stripped player builds. The
        // built-in default material is packaged by Unity with the player.
        Material fallback = Resources.GetBuiltinResource<Material>("Default-Material.mat");
        if (fallback != null) return new Material(fallback);
        throw new System.InvalidOperationException("No runtime material shader is available.");
    }

    public static Material Get(Color c)
    {
        Material m;
        if (solid.TryGetValue(c, out m) && m != null) return m;
        m = NewLitMaterial();
        m.color = c;
        m.SetFloat("_Smoothness", 0.25f);
        solid[c] = m;
        return m;
    }

    public static Material Glass(Color c)
    {
        Material m;
        if (glass.TryGetValue(c, out m) && m != null) return m;
        m = NewLitMaterial(true);
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
        FloorMat = NewLitMaterial();
        FloorMat.mainTexture = tex;
        FloorMat.color = FloorStyles[0];
        FloorMat.SetFloat("_Smoothness", 0.1f);
        FloorMat.mainTextureScale = new Vector2(20f, 20f);
        return FloorMat;
    }

    public static Material Wall()
    {
        if (WallMat != null) return WallMat;
        WallMat = NewLitMaterial();
        WallMat.color = WallStyles[0];
        WallMat.SetFloat("_Smoothness", 0.3f);
        return WallMat;
    }

    public static Material Water()
    {
        if (waterMat != null) return waterMat;
        Texture2D tex = CellTex(new Color(0.35f, 0.78f, 0.95f), new Color(0.47f, 0.86f, 1f));
        waterMat = NewLitMaterial();
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
        grassMat = NewLitMaterial();
        grassMat.mainTexture = tex;
        grassMat.color = Color.white;
        grassMat.SetFloat("_Smoothness", 0.1f);
        grassMat.mainTextureScale = new Vector2(10f, 10f);
        return grassMat;
    }

    public static Material Beach()
    {
        if (beachMat != null) return beachMat;
        beachMat = NewLitMaterial();
        beachMat.color = new Color(0.99f, 0.94f, 0.75f);
        beachMat.SetFloat("_Smoothness", 0.1f);
        return beachMat;
    }

    static Texture2D TriangleTex(Color a, Color b)
    {
        int n = 256;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGB24, true);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float fx = (float)x / n, fy = (float)y / n;
                tex.SetPixel(x, y, (fx + fy) % 1f < 0.5f ? a : b);
            }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 8;
        return tex;
    }

    static Texture2D CellTex(Color a, Color b)
    {
        int n = 256;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGB24, true);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float v = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                tex.SetPixel(x, y, Color.Lerp(a, b, Mathf.Round(v * 3f) / 3f));
            }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 8;
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
        go.AddComponent<AutoLocalizeTextMesh>();
        tm.font = UIFont;
        // Render twice as many glyph pixels while preserving the exact same
        // world-space dimensions. Nearby signs no longer dissolve into a
        // low-resolution dynamic-font atlas.
        tm.fontSize = 128;
        tm.characterSize = size * 0.5f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = c;
        tm.text = txt;
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (UIFont != null) mr.sharedMaterial = UIFont.material;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;
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

    public static string Money(long v)
    {
        if (v >= 1000000000L) return (v / 1000000000d).ToString("0.##") + "B";
        if (v >= 1000000L) return (v / 1000000d).ToString("0.##") + "M";
        if (v >= 1000L) return (v / 1000d).ToString("0.##") + "K";
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
        MakePreviewModelVivid(model);
        root.AddComponent<Billboard>();
        return root;
    }

    // Tank labels are display graphics, so keep their fish readable regardless of
    // the sun direction or night lighting used by the 3D world.
    static void MakePreviewModelVivid(Transform model)
    {
        Material unlitTemplate = Resources.Load<Material>("RuntimeUnlit");
        Shader unlit = unlitTemplate != null ? unlitTemplate.shader : null;
        if (unlit == null) unlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlit == null) unlit = Shader.Find("Unlit/Texture");

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        for (int r = 0; r < renderers.Length; r++)
        {
            Material[] source = renderers[r].sharedMaterials;
            Material[] vivid = new Material[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                Material src = source[i];
                if (src == null) continue;

                Texture texture = null;
                if (src.HasProperty("_BaseMap")) texture = src.GetTexture("_BaseMap");
                if (texture == null && src.HasProperty("_MainTex")) texture = src.GetTexture("_MainTex");
                Color tint = src.HasProperty("_BaseColor") ? src.GetColor("_BaseColor") :
                    (src.HasProperty("_Color") ? src.GetColor("_Color") : Color.white);
                // Imported fish textures expect a white material tint. A gray
                // tint muddies every colour in the icon even with unlit shading.
                Color brightTint = Color.white;
                brightTint.a = tint.a;

                vivid[i] = unlit != null ? new Material(unlit) : new Material(src);
                if (vivid[i].HasProperty("_BaseMap")) vivid[i].SetTexture("_BaseMap", texture);
                if (vivid[i].HasProperty("_MainTex")) vivid[i].SetTexture("_MainTex", texture);
                if (vivid[i].HasProperty("_BaseColor")) vivid[i].SetColor("_BaseColor", brightTint);
                if (vivid[i].HasProperty("_Color")) vivid[i].SetColor("_Color", brightTint);
                if (unlit == null && vivid[i].HasProperty("_EmissionColor"))
                {
                    vivid[i].EnableKeyword("_EMISSION");
                    vivid[i].SetColor("_EmissionColor", brightTint * 0.45f);
                }
            }
            renderers[r].sharedMaterials = vivid;
            renderers[r].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderers[r].receiveShadows = false;
        }
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
