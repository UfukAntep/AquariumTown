using System;
using UnityEngine;
using UnityEngine.UI;

// The "MEKANIK VERITABANI" desk in the management room. Shows which game
// mechanics have been discovered (revealed, with a description) and which are
// still hidden (a mysterious "???" with a teasing hint). Open from the start.
public class MechanicsDeskUnit : MonoBehaviour
{
    struct Mech { public string name; public string desc; public string hint; public Func<bool> unlocked; }

    static Mech[] list;
    GameObject viewer;
    Transform grid;

    public bool PlayerNear(Vector3 position) { return Vector3.Distance(position, transform.position) < 3.4f; }
    public bool ViewerOpen { get { return viewer != null && viewer.activeSelf; } }

    static void BuildList()
    {
        if (list != null) return;
        list = new Mech[]
        {
            M("BALIK AVI", "Denizde radarla balik yakala, tanklara tasi.", "Denize gir...", () => true),
            M("MUSTERI SATISI", "Musteriler tanktan balik alir, kasada oder.", "Dukkani ac...", () => true),
            M("TEMIZLIK", "Yerdeki ve denizdeki copleri topla, cop kutusuna at.", "Etraf kirlenince...", () => true),
            M("DEPO SISTEMI", "Fazla baliklari depoda sakla, tasiyici tanklara dagitir.", "Bir alan satin al...", () => Game.depot != null),
            M("TUVALET & HIJYEN", "Klozet/lavabo ile musteri memnuniyetini koru.", "Musteriler rahatsiz olunca...", () => Game.gm != null && Game.gm.toiletAreaOpen),
            M("PERSONEL", "Kasiyer, avci, guvenlik gibi calisanlari ise al.", "Isler yogunlasinca...", () => AnyStaff()),
            M("YONETIM ODASI", "Kendi ofisini kur, is masalari ekle.", "Buyudukce...", () => Shop(4)),
            M("KAMERA SISTEMI", "Guvenlik kameralariyla dukkani canli izle.", "Yonetim odasindan sonra...", () => Shop(5)),
            M("PAZARLAMA", "Reklam kampanyalari ile populerligi artir.", "Bir masa lazim...", () => Shop(8)),
            M("SAHIL ZIYARETCILERI", "Turistler gelir, sahili kirletir; temizlemen gerekir.", "Seviye 15'te...", () => Lvl(15)),
            M("DENIZ KIRLILIGI", "Deniz kendiliginden kirlenir, baliklar olur.", "Seviye 8'de...", () => Lvl(8)),
            M("DEPREM OLAYI", "Tanklar kirilabilir; hemen tamir etmen gerekir.", "Seviye 5'te...", () => Lvl(5)),
            M("HIRSIZ", "Kilikli hirsiz para/balik calar; yakala!", "Zenginlesince...", () => Lvl(4)),
            M("KOPEKBALIGI", "Denizde saldiran kopekbaligindan kac.", "Suda dikkat et...", () => Lvl(3)),
            M("GECE DENIZI", "Gece yarisindan sonra sadece kopekbaliklari cikar.", "Gece denize girme...", () => Lvl(2)),
            M("TEKNOLOJILER", "Pusula, harita, navigasyon, otomatik dukkan...", "PC teknoloji sekmesi...", () => AnyTech()),
            M("ALTIN BALIK", "Nadir altin balik cok deger katar.", "Sansliysan...", () => Lvl(2)),
            M("JETSKI & RAMPA", "Jetski ve ziplama rampasi ile denizde hizlan.", "Dekordan al...", () => Decor()),
        };
    }

    static Mech M(string n, string d, string h, Func<bool> u) { Mech m; m.name = n; m.desc = d; m.hint = h; m.unlocked = u; return m; }
    static bool AnyStaff() { if (Game.gm == null) return false; for (int i = 0; i < Game.gm.staffCounts.Length; i++) if (Game.gm.staffCounts[i] > 0) return true; return false; }
    static bool AnyTech() { if (Game.gm == null) return false; for (int i = 0; i < Game.gm.techOwned.Length; i++) if (Game.gm.techOwned[i]) return true; return false; }
    static bool Decor() { if (Game.gm == null) return false; for (int i = 0; i < Game.gm.decorOwned.Length; i++) if (Game.gm.decorOwned[i]) return true; return false; }
    static bool Shop(int i) { return Game.gm != null && Game.gm.shopUpg.Length > i && Game.gm.shopUpg[i] > 0; }
    static bool Lvl(int n) { return Game.gm != null && Game.gm.Level >= n; }

    public void Open()
    {
        BuildList();
        EnsureViewer();
        RebuildCards();
        viewer.SetActive(true);
        viewer.transform.SetAsLastSibling();
        if (Game.cam != null) { /* leave camera as-is; overlay is fullscreen */ }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Sfx.Play(Snd.Tick, 0.5f);
    }

    void Close()
    {
        if (viewer != null) viewer.SetActive(false);
        if (Game.ui == null || !Game.ui.AnyMenuOpen) { }
        if (Game.cam == null || !Game.cam.IsTPS) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }

    void EnsureViewer()
    {
        if (viewer != null) return;
        viewer = new GameObject("MechanicsDatabaseViewer");
        Canvas canvas = viewer.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 46;
        CanvasScaler scaler = viewer.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;
        viewer.AddComponent<GraphicRaycaster>();

        // dark "sci-fi database" backdrop
        UIKit.Panel(viewer.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2000f, 1200f), new Color(0.05f, 0.03f, 0.1f, 0.97f), false, false);
        GameObject window = UIKit.Panel(viewer.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 800f), new Color(0.1f, 0.08f, 0.2f, 1f), true, true);
        GameObject band = UIKit.Panel(window.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 12f), new Vector2(1430f, 84f), new Color(0.35f, 0.2f, 0.6f), true, true);
        UIKit.Label(band.transform, "MEKANIK VERITABANI", 34, new Color(0.9f, 0.8f, 1f), TextAnchor.MiddleCenter, true);
        UIKit.Icon(band.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(56f, 0f), new Vector2(52f, 52f), new Color(0.6f, 0.4f, 1f));

        UIKit.Btn(window.transform, new Vector2(640f, 356f), new Vector2(60f, 60f), UIKit.Red, "X", 26, Close);

        GameObject sub = UIKit.Panel(window.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -74f), new Vector2(1200f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(sub.transform, "Kesfettikce sistemler acilir. Kilitli olanlar sana ne bekledigini fisildar...", 17, new Color(0.7f, 0.65f, 0.85f), TextAnchor.MiddleCenter);

        GameObject gridGo = UIKit.Panel(window.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(1320f, 620f), new Color(0f, 0f, 0f, 0.001f), false, false);
        grid = gridGo.transform;
    }

    void RebuildCards()
    {
        for (int i = grid.childCount - 1; i >= 0; i--) Destroy(grid.GetChild(i).gameObject);
        int cols = 6;
        float cw = 210f, ch = 150f, gapx = 6f, gapy = 8f;
        int unlockedCount = 0;
        for (int i = 0; i < list.Length; i++) if (SafeCheck(list[i])) unlockedCount++;

        for (int i = 0; i < list.Length; i++)
        {
            bool on = SafeCheck(list[i]);
            int col = i % cols, row = i / cols;
            float x = -(cols - 1) * 0.5f * (cw + gapx) + col * (cw + gapx);
            float y = 230f - row * (ch + gapy);
            Color cardCol = on ? new Color(0.2f, 0.5f, 0.85f) : new Color(0.14f, 0.1f, 0.22f);
            GameObject card = UIKit.Panel(grid, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(cw, ch), cardCol, true, true);
            if (on)
            {
                UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(34f, 34f), new Color(0.4f, 0.9f, 0.5f));
                GameObject nm = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(200f, 26f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(nm.transform, list[i].name, 15, Color.white, TextAnchor.MiddleCenter, true);
                GameObject ds = UIKit.Panel(card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(196f, 74f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(ds.transform, list[i].desc, 12, new Color(0.9f, 0.95f, 1f), TextAnchor.MiddleCenter);
            }
            else
            {
                UIKit.Label(UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(180f, 46f), new Color(0f, 0f, 0f, 0.001f), false, false).transform,
                    "?", 44, new Color(0.6f, 0.45f, 0.9f), TextAnchor.MiddleCenter, true);
                GameObject hn = UIKit.Panel(card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(190f, 66f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(hn.transform, "??? — " + list[i].hint, 12, new Color(0.65f, 0.6f, 0.8f), TextAnchor.MiddleCenter);
            }
        }

        // progress bar at the bottom of the window
        GameObject prog = UIKit.Panel(grid, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(1200f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(prog.transform, "Kesfedilen mekanikler: " + unlockedCount + " / " + list.Length, 17, new Color(0.85f, 0.75f, 1f), TextAnchor.MiddleCenter, true);
    }

    static bool SafeCheck(Mech m)
    {
        try { return m.unlocked != null && m.unlocked(); } catch { return false; }
    }

    void Update()
    {
        if (ViewerOpen && (Input.GetKeyDown(KeyCode.Escape) || ControlBindings.Down(ControlAction.Interact)))
            Close();
    }

    void OnDestroy()
    {
        if (viewer != null) Destroy(viewer);
        if (Game.mechanicsDesk == this) Game.mechanicsDesk = null;
    }
}
