using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Cartoon-styled HUD + menus (uses UIKit for rounded panels, stars, pills).
public class UIManager : MonoBehaviour
{
    Text moneyText, levelText, satText, clockText, dirtText, capacityText, toastText, hintText, promptText, thiefText;
    RectTransform moneyRect, satRect;
    GameObject satRoot, promptRoot;
    Image satIcon;
    GameObject toastGo, cameraTutorialGo, thiefGo, celebrationGo, mainMenuGo, pauseGo, infoGo;
    Text celebTitle, celebSub, infoTitle, infoBody;
    GameObject infoTrophyVisual;
    RectTransform infoBodyRect;
    System.Action infoAction;
    GameObject celebModel;
    Camera celebCam;
    Transform celebStage;
    float toastTimer, punchT = 1f, satFlashT = 1f, celebTimer;
    Color satFlashColor = Color.white;
    GameObject tipGo;
    Text tipText;
    GameObject langGo;
    // tech widgets
    GameObject compassGo, minimapGo, mapGo, questGo;
    RectTransform compassRose, mapPlayerDot;
    Camera minimapCam;
    RectTransform minimapThiefDot;
    Transform mapDynamic;
    Text[] questTexts = new Text[3];
    float questTimer;

    public bool MainMenuOpen { get { return mainMenuGo != null && mainMenuGo.activeSelf; } }
    public bool PauseOpen { get { return pauseGo != null && pauseGo.activeSelf; } }
    public bool CelebrationOpen { get { return celebrationGo != null && celebrationGo.activeSelf; } }
    public bool InfoOpen { get { return infoGo != null && infoGo.activeSelf; } }
    public bool MapOpen { get { return mapGo != null && mapGo.activeSelf; } }
    public bool LangOpen { get { return langGo != null && langGo.activeSelf; } }
    public bool AnyMenuOpen { get { return MainMenuOpen || PauseOpen || CelebrationOpen || InfoOpen || MapOpen || LangOpen || (Game.pc != null && Game.pc.IsOpen); } }

    public static UIManager Create()
    {
        GameObject go = new GameObject("UI");
        UIManager ui = go.AddComponent<UIManager>();
        ui.Build();
        Game.ui = ui;
        return ui;
    }

    void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ----- top-left: star level badge -----
        GameObject starGo = UIKit.Icon(transform, UIKit.Star(), new Vector2(0f, 1f), new Vector2(70f, -70f), new Vector2(120f, 120f), UIKit.Yellow);
        starGo.AddComponent<Shadow>().effectDistance = new Vector2(0f, -5f);
        GameObject starInner = UIKit.Icon(starGo.transform, UIKit.Star(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(92f, 92f), new Color(1f, 0.9f, 0.45f));
        levelText = UIKit.Label(starInner.transform, "1", 34, UIKit.TextDark, TextAnchor.MiddleCenter, true);

        // ----- top-right: money / satisfaction / clock pills -----
        GameObject moneyRoot;
        moneyText = UIKit.Pill(transform, new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(220f, 56f), UIKit.Cream, UIKit.Green, "$", out moneyRoot);
        moneyRect = moneyRoot.GetComponent<RectTransform>();

        satText = UIKit.Pill(transform, new Vector2(1f, 1f), new Vector2(-16f, -80f), new Vector2(220f, 50f), UIKit.Cream, UIKit.Green, "", out satRoot);
        satRect = satRoot.GetComponent<RectTransform>();
        satIcon = satRoot.transform.GetChild(0).GetComponent<Image>();
        Sprite satisfactionIcon = GameAssets.TabIcon(8);
        if (satisfactionIcon != null) UIKit.Icon(satRoot.transform.GetChild(0), satisfactionIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 28f), Color.white);

        GameObject clockRoot;
        clockText = UIKit.Pill(transform, new Vector2(1f, 1f), new Vector2(-16f, -138f), new Vector2(220f, 46f), UIKit.Cream, UIKit.Blue, "", out clockRoot);
        Sprite timeIcon = GameAssets.TabIcon(7);
        if (timeIcon != null) UIKit.Icon(clockRoot.transform.GetChild(0), timeIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(27f, 27f), Color.white);

        GameObject dirtRoot;
        dirtText = UIKit.Pill(transform, new Vector2(1f, 1f), new Vector2(-16f, -192f), new Vector2(220f, 46f), UIKit.Cream, new Color(0.55f, 0.4f, 0.25f), "", out dirtRoot);
        UIKit.Icon(dirtRoot.transform.GetChild(0), UIKit.Poop(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 30f), Color.white);

        GameObject capacityRoot;
        capacityText = UIKit.Pill(transform, new Vector2(1f, 1f), new Vector2(-16f, -246f), new Vector2(220f, 46f), UIKit.Cream, UIKit.Orange, "", out capacityRoot);
        Sprite bagIcon = GameAssets.ItemIcon(0);
        if (bagIcon != null) UIKit.Icon(capacityRoot.transform.GetChild(0), bagIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(29f, 29f), Color.white);

        // hover tooltips for the HUD pills
        AddTip(moneyRoot, "PARA: Kasandaki toplam para.\nMusteriler odedikce artar, gelistirmelere harcanir.");
        AddTip(satRoot, "MEMNUNIYET: Musteri mutlulugu.\nDusukse baliklar cok daha ucuza satilir!");
        AddTip(clockRoot, "SAAT: Oyun saati ve dukkanin acik/kapali durumu.\nGece isiklar kisilir.");
        AddTip(dirtRoot, "KIRLILIK: Magaza + deniz kirliligi.\nYukselirse musteriler kacar, denizde baliklar olur!");
        AddTip(capacityRoot, "BALIK TASIMA: Uzerindeki balik sayisi ve canta kapasitesi.");

        // tooltip panel
        tipGo = UIKit.Panel(transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -306f), new Vector2(340f, 92f), new Color(0.15f, 0.15f, 0.22f, 0.95f), true, true);
        tipText = UIKit.Label(tipGo.transform, "", 16, Color.white, TextAnchor.MiddleCenter);
        tipGo.SetActive(false);

        // ----- bottom: hint bar -----
        GameObject hint = UIKit.Panel(transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(780f, 46f), new Color(0.1f, 0.12f, 0.2f, 0.75f), true, false);
        hintText = UIKit.Label(hint.transform, "", 20, Color.white, TextAnchor.MiddleCenter);

        // ----- bottom-right: interaction prompt -----
        promptRoot = UIKit.Panel(transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 76f), new Vector2(330f, 58f), UIKit.Orange, true, true);
        promptText = UIKit.Label(promptRoot.transform, "", 22, Color.white, TextAnchor.MiddleCenter, true);
        promptRoot.SetActive(false);

        // ----- toast -----
        toastGo = UIKit.Panel(transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(780f, 58f), UIKit.Green, true, true);
        toastText = UIKit.Label(toastGo.transform, "", 22, Color.white, TextAnchor.MiddleCenter, true);
        toastGo.SetActive(false);

        // Persistent first-session camera lesson. It deliberately has no timer
        // and disappears only after the player actually presses C.
        cameraTutorialGo = UIKit.Panel(transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -232f), new Vector2(850f, 64f), UIKit.Blue, true, true);
        UIKit.Label(cameraTutorialGo.transform, Loc.T("CAMERA_TUTORIAL"), 20, Color.white, TextAnchor.MiddleCenter, true);
        cameraTutorialGo.SetActive(false);

        // ----- thief timer -----
        thiefGo = UIKit.Panel(transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(440f, 64f), UIKit.Red, true, true);
        thiefText = UIKit.Label(thiefGo.transform, "", 26, Color.white, TextAnchor.MiddleCenter, true);
        thiefGo.SetActive(false);

        BuildCelebration();
        BuildMainMenu();
        BuildPause();
        BuildInfo();
        BuildLanguagePicker();
        BuildCompass();
        BuildMinimap();
        BuildMap();
        BuildQuests();
    }

    // ---------- compass (PUSULA tech) ----------
    void BuildCompass()
    {
        compassGo = UIKit.Panel(transform, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(100f, 100f), new Vector2(140f, 140f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Icon(compassGo.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140f, 140f), UIKit.Cream).AddComponent<Shadow>().effectDistance = new Vector2(0f, -4f);
        UIKit.Icon(compassGo.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120f, 120f), new Color(0.92f, 0.96f, 1f));
        GameObject rose = new GameObject("Rose");
        rose.transform.SetParent(compassGo.transform, false);
        compassRose = rose.AddComponent<RectTransform>();
        compassRose.anchorMin = new Vector2(0.5f, 0.5f); compassRose.anchorMax = new Vector2(0.5f, 0.5f);
        compassRose.sizeDelta = Vector2.zero;
        string[] dirs = { "K", "D", "G", "B" }; // kuzey dogu guney bati
        Vector2[] offs = { new Vector2(0f, 44f), new Vector2(44f, 0f), new Vector2(0f, -44f), new Vector2(-44f, 0f) };
        for (int i = 0; i < 4; i++)
        {
            GameObject d = UIKit.Panel(compassRose, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), offs[i], new Vector2(36f, 36f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(d.transform, dirs[i], 22, i == 0 ? UIKit.Red : UIKit.TextDark, TextAnchor.MiddleCenter, true);
        }
        // fixed needle = the direction the camera faces
        UIKit.Panel(compassGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(7f, 42f), UIKit.Red, true, false);
        compassGo.SetActive(false);
    }

    // ---------- minimap (GELISMIS NAVIGASYON tech) ----------
    void BuildMinimap()
    {
        minimapGo = UIKit.Panel(transform, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(160f, 330f), new Vector2(250f, 250f), new Color(0f, 0f, 0f, 0.001f), false, false);
        GameObject rim = UIKit.Icon(minimapGo.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250f, 250f), UIKit.Cream);
        rim.AddComponent<Shadow>().effectDistance = new Vector2(0f, -4f);
        GameObject maskGo = UIKit.Icon(minimapGo.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(232f, 232f), Color.white);
        maskGo.AddComponent<Mask>().showMaskGraphic = true;
        RenderTexture rt = new RenderTexture(256, 256, 16);
        GameObject rawGo = new GameObject("MiniRT");
        rawGo.transform.SetParent(maskGo.transform, false);
        RawImage raw = rawGo.AddComponent<RawImage>();
        raw.texture = rt;
        RectTransform rrt = rawGo.GetComponent<RectTransform>();
        rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
        rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;
        UIKit.Icon(minimapGo.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(14f, 14f), UIKit.Red);
        GameObject thiefDot = UIKit.Icon(maskGo.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(20f, 20f), UIKit.Red);
        thiefDot.AddComponent<Outline>().effectColor = Color.white;
        minimapThiefDot = thiefDot.GetComponent<RectTransform>();
        thiefDot.SetActive(false);
        GameObject n = UIKit.Panel(minimapGo.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 6f), new Vector2(30f, 30f), UIKit.Blue, true, false);
        UIKit.Label(n.transform, "K", 16, Color.white, TextAnchor.MiddleCenter, true);

        GameObject camGo = new GameObject("MinimapCam");
        camGo.transform.SetParent(transform, false);
        minimapCam = camGo.AddComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = 26f;
        minimapCam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = new Color(0.5f, 0.85f, 1f);
        minimapCam.cullingMask = ~((1 << 5) | (1 << 30));
        minimapCam.targetTexture = rt;
        minimapCam.enabled = false;
        minimapGo.SetActive(false);
    }

    // ---------- full map on M (HARITA tech) ----------
    const float MapSX = 4.2f, MapSZ = 2.4f;
    static Vector2 WorldToMap(Vector3 w)
    {
        return new Vector2((w.x - 41.5f) * MapSX, (w.z - 65f) * MapSZ);
    }

    void BuildMap()
    {
        mapGo = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 4000f), new Color(0f, 0f, 0f, 0.7f), false, false);
        GameObject card = UIKit.Panel(mapGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1080f, 800f), UIKit.Cream, true, true);
        GameObject band = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 12f), new Vector2(1110f, 76f), UIKit.Blue, true, true);
        UIKit.Label(band.transform, "HARITA  (M ile kapat)", 28, Color.white, TextAnchor.MiddleCenter, true);
        UIKit.Btn(card.transform, new Vector2(510f, 370f), new Vector2(60f, 60f), UIKit.Red, "X", 24, delegate { mapGo.SetActive(false); UpdateCursor(false); });

        GameObject area = UIKit.Panel(card.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(990f, 660f), new Color(0.85f, 0.93f, 0.8f), true, false);
        Transform a = area.transform;
        // static geography
        MapRect(a, new Rect(26f, -60f, 114f, 150f), new Color(0.45f, 0.82f, 1f));   // sea
        MapRect(a, new Rect(8f, -60f, 12f, 150f), new Color(0.6f, 0.88f, 0.5f));    // grass
        MapRect(a, new Rect(20f, -60f, 6f, 150f), new Color(0.98f, 0.92f, 0.7f));   // beach
        MapRect(a, new Rect(-58f, -2f, 66f, 78f), new Color(0.99f, 0.87f, 0.64f));  // shop
        MapRect(a, new Rect(20f, 30.3f, 10f, 3.4f), new Color(0.7f, 0.5f, 0.3f));   // dock
        GameObject dyn = new GameObject("Dynamic");
        dyn.transform.SetParent(a, false);
        RectTransform drt = dyn.AddComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.5f, 0.5f); drt.anchorMax = new Vector2(0.5f, 0.5f);
        drt.sizeDelta = Vector2.zero;
        mapDynamic = dyn.transform;
        GameObject pd = UIKit.Icon(a, UIKit.Circle(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(18f, 18f), UIKit.Red);
        mapPlayerDot = pd.GetComponent<RectTransform>();
        mapGo.SetActive(false);
    }

    void MapRect(Transform parent, Rect w, Color c)
    {
        Vector2 mid = WorldToMap(new Vector3(w.x + w.width * 0.5f, 0f, w.y + w.height * 0.5f));
        UIKit.Panel(parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), mid, new Vector2(w.width * MapSX, w.height * MapSZ), c, false, false);
    }

    void OpenMap()
    {
        mapGo.SetActive(true);
        UpdateCursor(true);
        // dynamic markers: locked zones, tanks, spots
        for (int i = mapDynamic.childCount - 1; i >= 0; i--) Destroy(mapDynamic.GetChild(i).gameObject);
        for (int b = 1; b < 4; b++)
            if (!Game.gm.ZoneOpen(b))
                MapRect(mapDynamic, new Rect(-58f, GameBootstrap.ZoneZ0(b), 56f, GameBootstrap.ZoneZ1(b) - GameBootstrap.ZoneZ0(b)), new Color(0.4f, 0.4f, 0.45f, 0.85f));
        for (int i = 0; i < Game.tanks.Count; i++)
            UIKit.Icon(mapDynamic, UIKit.Circle(), new Vector2(0.5f, 0.5f), WorldToMap(Game.tanks[i].transform.position), new Vector2(10f, 10f), new Color(0.3f, 0.85f, 1f));
        if (Game.register != null)
            UIKit.Icon(mapDynamic, UIKit.Circle(), new Vector2(0.5f, 0.5f), WorldToMap(Game.register.transform.position), new Vector2(14f, 14f), UIKit.Orange);
        if (Game.trash != null)
            UIKit.Icon(mapDynamic, UIKit.Circle(), new Vector2(0.5f, 0.5f), WorldToMap(Game.trash.BinPos), new Vector2(12f, 12f), UIKit.Green);
        Sfx.Play(Snd.Tick, 0.5f);
    }

    // ---------- daily quests panel ----------
    void BuildQuests()
    {
        questGo = UIKit.Panel(transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -150f), new Vector2(300f, 152f), new Color(1f, 0.97f, 0.9f, 0.9f), true, true);
        GameObject band = UIKit.Panel(questGo.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 8f), new Vector2(312f, 40f), UIKit.Purple, true, false);
        UIKit.Label(band.transform, "GUNLUK GOREVLER", 17, Color.white, TextAnchor.MiddleCenter, true);
        for (int i = 0; i < 3; i++)
        {
            GameObject row = UIKit.Panel(questGo.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f - i * 36f), new Vector2(284f, 32f), new Color(0f, 0f, 0f, 0.001f), false, false);
            questTexts[i] = UIKit.Label(row.transform, "", 15, UIKit.TextDark, TextAnchor.MiddleLeft);
        }
        questGo.SetActive(false);
    }

    void AddTip(GameObject root, string tip)
    {
        HoverTip h = root.AddComponent<HoverTip>();
        h.tip = tip;
    }

    public void ShowTip(string tip)
    {
        if (tipGo == null) return;
        tipGo.SetActive(!string.IsNullOrEmpty(tip));
        tipText.text = tip;
    }

    // ---------- info dialog (tutorial messages) ----------
    void BuildInfo()
    {
        infoGo = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 4000f), new Color(0f, 0f, 0f, 0.55f), false, false);
        GameObject box = UIKit.Panel(infoGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 400f), UIKit.Cream, true, true);
        GameObject band = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 14f), new Vector2(750f, 80f), UIKit.Blue, true, true);
        infoTitle = UIKit.Label(band.transform, "", 30, Color.white, TextAnchor.MiddleCenter, true);
        GameObject bodyP = UIKit.Panel(box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -5f), new Vector2(660f, 200f), new Color(0f, 0f, 0f, 0.001f), false, false);
        infoBodyRect = bodyP.GetComponent<RectTransform>();
        infoBody = UIKit.Label(bodyP.transform, "", 22, UIKit.TextDark, TextAnchor.MiddleCenter);
        infoTrophyVisual = UIKit.Panel(box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 82f), new Vector2(150f, 105f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Panel(infoTrophyVisual.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(86f, 54f), UIKit.Yellow, true, true);
        UIKit.Panel(infoTrophyVisual.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(18f, 34f), UIKit.Orange, true, false);
        UIKit.Panel(infoTrophyVisual.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -43f), new Vector2(70f, 15f), UIKit.Orange, true, false);
        UIKit.Icon(infoTrophyVisual.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(-51f, 18f), new Vector2(42f, 42f), UIKit.Orange);
        UIKit.Icon(infoTrophyVisual.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(51f, 18f), new Vector2(42f, 42f), UIKit.Orange);
        infoTrophyVisual.transform.SetSiblingIndex(infoBodyRect.GetSiblingIndex());
        infoTrophyVisual.SetActive(false);
        UIKit.Btn(box.transform, new Vector2(0f, -155f), new Vector2(260f, 62f), UIKit.Green, Loc.T("OK"), 22,
            delegate
            {
                infoGo.SetActive(false);
                System.Action action = infoAction;
                infoAction = null;
                if (action != null) action();
                else UpdateCursor(false);
            });
        infoGo.SetActive(false);
    }

    public void ShowInfo(string title, string body)
    {
        ShowInfo(title, body, null);
    }

    public void ShowInfo(string title, string body, System.Action onClose)
    {
        bool trophy = title != null && title.Contains("KUPA");
        if (infoTrophyVisual != null) infoTrophyVisual.SetActive(trophy);
        if (infoBodyRect != null)
        {
            infoBodyRect.anchoredPosition = new Vector2(0f, trophy ? -55f : -5f);
            infoBodyRect.sizeDelta = new Vector2(660f, trophy ? 145f : 200f);
        }
        infoTitle.text = title;
        infoBody.text = body;
        infoAction = onClose;
        infoGo.SetActive(true);
        UpdateCursor(true);
        Sfx.Play(Snd.Collect, 0.4f);
    }

    // ---------- language picker (20 languages with country-accurate flags) ----------
    void BuildLanguagePicker()
    {
        langGo = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 4000f), new Color(0.1f, 0.35f, 0.6f, 0.98f), false, false);
        GameObject band = UIKit.Panel(langGo.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(760f, 80f), UIKit.Orange, true, true);
        UIKit.Label(band.transform, Loc.T("SELECT_LANGUAGE"), 30, Color.white, TextAnchor.MiddleCenter, true);
        // 20 flag buttons in a 5 x 4 grid
        for (int i = 0; i < Loc.Names.Length; i++)
        {
            int idx = i;
            float x = -520f + (i % 5) * 260f;
            float y = 190f - (i / 5) * 155f;
            GameObject card = UIKit.Panel(langGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(240f, 130f), UIKit.Cream, true, true);
            // Each country uses its real stripe/cross/crescent layout.
            GameObject flag = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(96f, 62f), Color.white, true, false);
            Mask mask = flag.AddComponent<Mask>();
            mask.showMaskGraphic = true;
            BuildFlag(flag.transform, Loc.FlagCodes[idx]);
            Button b = UIKit.Btn(card.transform, new Vector2(0f, -42f), new Vector2(220f, 44f), UIKit.Blue, Loc.Names[idx], 16,
                delegate
                {
                    Loc.Set(idx);
                    if (Game.gm != null) Game.gm.Save();
                    langGo.SetActive(false);
                    GameBootstrap.SoftRestart();
                });
        }
        langGo.SetActive(false);
    }

    static Sprite flagTriangle;

    static GameObject FlagRect(Transform parent, Vector2 pos, Vector2 size, Color color, float angle = 0f)
    {
        GameObject go = UIKit.Panel(parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, color, false, false);
        go.transform.localEulerAngles = new Vector3(0f, 0f, angle);
        return go;
    }

    static Sprite FlagTriangleSprite()
    {
        if (flagTriangle != null) return flagTriangle;
        const int n = 64;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
                tex.SetPixel(x, y, x <= n - 1 - Mathf.Abs(y - n * 0.5f) * 2f ? Color.white : Color.clear);
        tex.Apply();
        flagTriangle = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f);
        return flagTriangle;
    }

    static void FlagSymbol(Transform parent, string symbol, Vector2 pos, int size, Color color)
    {
        GameObject area = UIKit.Panel(parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos,
            new Vector2(30f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(area.transform, symbol, size, color, TextAnchor.MiddleCenter, true);
    }

    static void FlagTriangle(Transform parent, Color color)
    {
        GameObject tri = UIKit.Icon(parent, FlagTriangleSprite(), new Vector2(0.5f, 0.5f), new Vector2(-24f, 0f), new Vector2(48f, 60f), color);
        tri.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
    }

    static void BuildFlag(Transform f, string code)
    {
        Color red = new Color(0.82f, 0.05f, 0.1f), blue = new Color(0.03f, 0.2f, 0.55f);
        Color brightBlue = new Color(0.05f, 0.35f, 0.75f), yellow = new Color(1f, 0.82f, 0.05f);
        FlagRect(f, Vector2.zero, new Vector2(94f, 60f), Color.white);
        switch (code)
        {
            case "GB":
                FlagRect(f, Vector2.zero, new Vector2(94f, 60f), blue);
                FlagRect(f, Vector2.zero, new Vector2(112f, 8f), Color.white, 32f);
                FlagRect(f, Vector2.zero, new Vector2(112f, 8f), Color.white, -32f);
                FlagRect(f, Vector2.zero, new Vector2(94f, 17f), Color.white);
                FlagRect(f, Vector2.zero, new Vector2(17f, 60f), Color.white);
                FlagRect(f, Vector2.zero, new Vector2(94f, 9f), red);
                FlagRect(f, Vector2.zero, new Vector2(9f, 60f), red);
                break;
            case "TR":
                FlagRect(f, Vector2.zero, new Vector2(94f, 60f), red);
                UIKit.Icon(f, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(-12f, 0f), new Vector2(31f, 31f), Color.white);
                UIKit.Icon(f, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(-6f, 0f), new Vector2(25f, 25f), red);
                FlagSymbol(f, "★", new Vector2(12f, 0f), 17, Color.white);
                break;
            case "DE": FlagRect(f, new Vector2(0f, 20f), new Vector2(94f, 20f), Color.black); FlagRect(f, Vector2.zero, new Vector2(94f, 20f), red); FlagRect(f, new Vector2(0f, -20f), new Vector2(94f, 20f), yellow); break;
            case "FR": FlagRect(f, new Vector2(-31f, 0f), new Vector2(32f, 60f), blue); FlagRect(f, new Vector2(31f, 0f), new Vector2(32f, 60f), red); break;
            case "ES": FlagRect(f, new Vector2(0f, 22f), new Vector2(94f, 16f), red); FlagRect(f, Vector2.zero, new Vector2(94f, 28f), yellow); FlagRect(f, new Vector2(0f, -22f), new Vector2(94f, 16f), red); FlagSymbol(f, "●", new Vector2(-20f, 0f), 13, red); break;
            case "IT": FlagRect(f, new Vector2(-31f, 0f), new Vector2(32f, 60f), new Color(0.02f, 0.5f, 0.22f)); FlagRect(f, new Vector2(31f, 0f), new Vector2(32f, 60f), red); break;
            case "PT": FlagRect(f, new Vector2(-28f, 0f), new Vector2(38f, 60f), new Color(0.02f, 0.45f, 0.2f)); FlagRect(f, new Vector2(19f, 0f), new Vector2(57f, 60f), red); FlagSymbol(f, "●", new Vector2(-9f, 0f), 18, yellow); break;
            case "NL": FlagRect(f, new Vector2(0f, 20f), new Vector2(94f, 20f), red); FlagRect(f, new Vector2(0f, -20f), new Vector2(94f, 20f), blue); break;
            case "PL": FlagRect(f, new Vector2(0f, -15f), new Vector2(94f, 30f), new Color(0.86f, 0.03f, 0.18f)); break;
            case "RU": FlagRect(f, Vector2.zero, new Vector2(94f, 20f), brightBlue); FlagRect(f, new Vector2(0f, -20f), new Vector2(94f, 20f), red); break;
            case "UA": FlagRect(f, new Vector2(0f, 15f), new Vector2(94f, 30f), brightBlue); FlagRect(f, new Vector2(0f, -15f), new Vector2(94f, 30f), yellow); break;
            case "RO": FlagRect(f, new Vector2(-31f, 0f), new Vector2(32f, 60f), blue); FlagRect(f, Vector2.zero, new Vector2(32f, 60f), yellow); FlagRect(f, new Vector2(31f, 0f), new Vector2(32f, 60f), red); break;
            case "CZ": FlagRect(f, new Vector2(0f, -15f), new Vector2(94f, 30f), red); FlagTriangle(f, blue); break;
            case "SE": FlagRect(f, Vector2.zero, new Vector2(94f, 60f), brightBlue); FlagRect(f, new Vector2(-10f, 0f), new Vector2(11f, 60f), yellow); FlagRect(f, Vector2.zero, new Vector2(94f, 11f), yellow); break;
            case "DK": FlagRect(f, Vector2.zero, new Vector2(94f, 60f), red); FlagRect(f, new Vector2(-10f, 0f), new Vector2(10f, 60f), Color.white); FlagRect(f, Vector2.zero, new Vector2(94f, 10f), Color.white); break;
            case "NO": FlagRect(f, Vector2.zero, new Vector2(94f, 60f), red); FlagRect(f, new Vector2(-10f, 0f), new Vector2(13f, 60f), Color.white); FlagRect(f, Vector2.zero, new Vector2(94f, 13f), Color.white); FlagRect(f, new Vector2(-10f, 0f), new Vector2(7f, 60f), blue); FlagRect(f, Vector2.zero, new Vector2(94f, 7f), blue); break;
            case "FI": FlagRect(f, new Vector2(-10f, 0f), new Vector2(10f, 60f), brightBlue); FlagRect(f, Vector2.zero, new Vector2(94f, 10f), brightBlue); break;
            case "ID": FlagRect(f, new Vector2(0f, 15f), new Vector2(94f, 30f), red); break;
            case "VN": FlagRect(f, Vector2.zero, new Vector2(94f, 60f), red); FlagSymbol(f, "★", Vector2.zero, 25, yellow); break;
            case "PH": FlagRect(f, new Vector2(0f, 15f), new Vector2(94f, 30f), brightBlue); FlagRect(f, new Vector2(0f, -15f), new Vector2(94f, 30f), red); FlagTriangle(f, Color.white); FlagSymbol(f, "★", new Vector2(-31f, 0f), 14, yellow); break;
        }
    }

    public void ShowLanguagePicker() { langGo.SetActive(true); langGo.transform.SetAsLastSibling(); UpdateCursor(true); }

    // hide every transient overlay (called when the game world starts, item 14)
    public void CloseTransient()
    {
        if (celebrationGo) celebrationGo.SetActive(false);
        if (infoGo) infoGo.SetActive(false);
        if (mapGo) mapGo.SetActive(false);
        if (pauseGo) pauseGo.SetActive(false);
        if (langGo) langGo.SetActive(false);
        if (tipGo) tipGo.SetActive(false);
        if (thiefGo) thiefGo.SetActive(false);
        if (mainMenuGo) mainMenuGo.SetActive(false);
        Time.timeScale = 1f;
    }

    // re-apply translated labels to the rebuilt-on-demand menus
    void RelabelMenus()
    {
        // menus are rebuilt each scene phase, so just refresh the visible ones
        if (mainMenuGo != null && mainMenuGo.activeSelf) { Destroy(mainMenuGo); BuildMainMenu(); mainMenuGo.SetActive(true); }
    }

    // ---------- celebration ----------
    void BuildCelebration()
    {
        celebrationGo = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 4000f), new Color(0.08f, 0.15f, 0.3f, 0.8f), false, false);
        // star burst behind the card
        for (int i = 0; i < 8; i++)
        {
            float ang = i * 45f * Mathf.Deg2Rad;
            UIKit.Icon(celebrationGo.transform, UIKit.Star(), new Vector2(0.5f, 0.5f),
                new Vector2(Mathf.Cos(ang) * 430f, 90f + Mathf.Sin(ang) * 250f), new Vector2(70f, 70f), new Color(1f, 0.85f, 0.3f, 0.6f));
        }
        // big celebration card: title band, text, LIVE 3D portrait of the new
        // species in the middle, DEVAM at the very bottom
        GameObject box = UIKit.Panel(celebrationGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(800f, 620f), UIKit.Cream, true, true);
        GameObject band = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 16f), new Vector2(840f, 100f), UIKit.Orange, true, true);
        celebTitle = UIKit.Label(band.transform, "TEBRIKLER!", 44, Color.white, TextAnchor.MiddleCenter, true);
        GameObject sp = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(740f, 100f), new Color(0f, 0f, 0f, 0.001f), false, false);
        celebSub = UIKit.Label(sp.transform, "", 28, UIKit.TextDark, TextAnchor.MiddleCenter);

        // species portrait (render-texture of the live model)
        RenderTexture rt = new RenderTexture(512, 512, 16);
        GameObject stageGo = new GameObject("CelebStage");
        stageGo.transform.SetParent(transform, false);
        stageGo.transform.position = new Vector3(0f, -250f, 0f);
        celebStage = stageGo.transform;
        GameObject camGo = new GameObject("CelebCam");
        camGo.transform.SetParent(stageGo.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 0f, -4f);
        celebCam = camGo.AddComponent<Camera>();
        celebCam.orthographic = true;
        celebCam.orthographicSize = 1.35f;
        celebCam.clearFlags = CameraClearFlags.SolidColor;
        celebCam.backgroundColor = new Color(0.75f, 0.9f, 1f, 1f);
        celebCam.cullingMask = 1 << 30;
        celebCam.targetTexture = rt;
        celebCam.enabled = false;

        GameObject portrait = UIKit.Panel(box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(270f, 270f), Color.white, true, true);
        GameObject rawGo = new GameObject("Portrait");
        rawGo.transform.SetParent(portrait.transform, false);
        RawImage raw = rawGo.AddComponent<RawImage>();
        raw.texture = rt;
        RectTransform rrt = rawGo.GetComponent<RectTransform>();
        rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
        rrt.offsetMin = new Vector2(8f, 8f); rrt.offsetMax = new Vector2(-8f, -8f);

        UIKit.Btn(box.transform, new Vector2(0f, -255f), new Vector2(300f, 70f), UIKit.Green, "DEVAM!", 26, delegate { EndCelebration(); });
        celebrationGo.SetActive(false);
    }

    static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++) SetLayerRecursive(t.GetChild(i), layer);
    }

    public void Celebrate(int sp)
    {
        celebTitle.text = "TEBRIKLER!  SEVIYE " + Game.gm.Level + "!";
        celebSub.text = "Yeni tur acildi:\n" + SpeciesInfo.Name(sp) + "  ($" + B.Money(SpeciesInfo.Price(sp)) + ")";
        celebrationGo.SetActive(true);
        celebTimer = 8f;
        UpdateCursor(true);
        Sfx.Play(Snd.LevelUp, 0.95f);

        // spawn the new species on the hidden portrait stage
        if (celebModel != null) Destroy(celebModel);
        celebModel = new GameObject("CelebModel");
        celebModel.transform.SetParent(celebStage, false);
        celebModel.transform.localPosition = Vector3.zero;
        GameObject quirky = AssetLib.SpawnSeaAnimal(sp, celebModel.transform, 2.2f);
        if (quirky == null)
        {
            Transform m = SpeciesInfo.Build(sp, celebModel.transform, 1.5f);
            m.localRotation = Quaternion.Euler(0f, 160f, 0f);
        }
        celebModel.AddComponent<Spinner>().speed = 70f;
        SetLayerRecursive(celebModel.transform, 30);
        if (celebCam != null) celebCam.enabled = true;
    }

    void EndCelebration()
    {
        celebrationGo.SetActive(false);
        if (celebModel != null) Destroy(celebModel);
        if (celebCam != null) celebCam.enabled = false;
        UpdateCursor(false);
    }

    // ---------- main menu ----------
    void BuildMainMenu()
    {
        mainMenuGo = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 4000f), UIKit.Blue, false, false);
        // decorative bubbles
        for (int i = 0; i < 14; i++)
        {
            float x = Random.Range(-700f, 700f), y = Random.Range(-420f, 420f);
            float s = Random.Range(24f, 90f);
            UIKit.Icon(mainMenuGo.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(s, s), new Color(1f, 1f, 1f, 0.08f));
        }
        // logo card
        GameObject logo = UIKit.Panel(mainMenuGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(860f, 190f), UIKit.Cream, true, true);
        UIKit.Icon(logo.transform, UIKit.Star(), new Vector2(0f, 0.5f), new Vector2(80f, 20f), new Vector2(90f, 90f), UIKit.Yellow);
        UIKit.Icon(logo.transform, UIKit.Star(), new Vector2(1f, 0.5f), new Vector2(-80f, 20f), new Vector2(90f, 90f), UIKit.Yellow);
        GameObject tArea = UIKit.Panel(logo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), new Vector2(700f, 90f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(tArea.transform, "AQUARIUM TOWN", 62, UIKit.Orange, TextAnchor.MiddleCenter, true);
        GameObject sArea = UIKit.Panel(logo.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(700f, 40f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(sArea.transform, Loc.T("SLOGAN"), 22, UIKit.TextDark, TextAnchor.MiddleCenter);

        bool hasSave = GameManager.SaveExists();
        float y0 = hasSave ? 30f : -10f;
        if (hasSave)
            UIKit.Btn(mainMenuGo.transform, new Vector2(0f, y0), new Vector2(380f, 74f), UIKit.Green, Loc.T("CONTINUE"), 26,
                delegate { GameBootstrap.LaunchGame(); });
        UIKit.Btn(mainMenuGo.transform, new Vector2(0f, y0 - 92f), new Vector2(380f, 74f), UIKit.Orange, Loc.T("NEW_GAME"), 26, delegate { GameManager.NewGame(); });
        UIKit.Btn(mainMenuGo.transform, new Vector2(0f, y0 - 184f), new Vector2(380f, 74f), UIKit.Red, Loc.T("QUIT"), 26, delegate { Application.Quit(); });
        GameObject foot = UIKit.Panel(mainMenuGo.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(700f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(foot.transform, "v2  -  80 tur, sonsuz akvaryum", 16, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleCenter);
    }

    // menu is its own lightweight scene now; the world animates behind it
    public void ShowMainMenu()
    {
        mainMenuGo.SetActive(true);
        UpdateCursor(true);
        if (!Loc.Chosen) ShowLanguagePicker();
    }

    // ---------- pause (Esc) ----------
    void BuildPause()
    {
        pauseGo = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000f, 4000f), new Color(0f, 0f, 0f, 0.6f), false, false);
        GameObject box = UIKit.Panel(pauseGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 540f), UIKit.Cream, true, true);
        GameObject band = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 14f), new Vector2(510f, 76f), UIKit.Blue, true, true);
        UIKit.Label(band.transform, Loc.T("PAUSED"), 30, Color.white, TextAnchor.MiddleCenter, true);
        UIKit.Btn(box.transform, new Vector2(0f, 98f), new Vector2(380f, 58f), UIKit.Green, Loc.T("RESUME"), 22, delegate { TogglePause(); });
        UIKit.Btn(box.transform, new Vector2(0f, 34f), new Vector2(380f, 58f), UIKit.Purple, Loc.T("LANGUAGE"), 22, delegate { ShowLanguagePicker(); });
        Button sndBtn = null;
        sndBtn = UIKit.Btn(box.transform, new Vector2(0f, -30f), new Vector2(380f, 58f), UIKit.Blue, Sfx.Muted ? Loc.T("SOUND_OFF") : Loc.T("SOUND_ON"), 22,
            delegate
            {
                Sfx.Muted = !Sfx.Muted;
                AudioListener.volume = Sfx.Muted ? 0f : 1f;
                sndBtn.GetComponentInChildren<Text>().text = Sfx.Muted ? Loc.T("SOUND_OFF") : Loc.T("SOUND_ON");
            });
        Button camBtn = null;
        camBtn = UIKit.Btn(box.transform, new Vector2(0f, -94f), new Vector2(380f, 58f), UIKit.Blue, CameraModeLabel(), 20,
            delegate
            {
                if (Game.cam != null) Game.cam.TogglePlayerMode();
                camBtn.GetComponentInChildren<Text>().text = CameraModeLabel();
                UpdateCursor(true);
            });
        UIKit.Btn(box.transform, new Vector2(0f, -158f), new Vector2(380f, 58f), UIKit.Orange, Loc.T("SAVE_MENU"), 20,
            delegate
            {
                Game.gm.Save();
                GameBootstrap.GoToMenu();
            });
        pauseGo.SetActive(false);
    }

    string CameraModeLabel()
    {
        return Loc.T("CAMERA_VIEW") + ": " + (Game.cam != null && Game.cam.IsTPS ? "TPS" : Loc.T("TOP_DOWN"));
    }

    public void TogglePause()
    {
        bool open = !pauseGo.activeSelf;
        pauseGo.SetActive(open);
        Time.timeScale = open ? 0f : 1f;
        if (open) Game.gm.Save();
        UpdateCursor(open);
    }

    void UpdateCursor(bool menuOpen)
    {
        if (menuOpen || Game.cam == null || !Game.cam.IsTPS)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // ---------- HUD ----------
    public void OnMoneyChanged()
    {
        if (moneyText != null && Game.gm != null)
            moneyText.text = B.Money(Game.gm.Money);
    }

    public void MoneyPunch()
    {
        punchT = 0f;
        float s = Game.gm.Satisfaction;
        moneyText.color = s > 70f ? new Color(0.1f, 0.55f, 0.15f) : s > 40f ? new Color(0.75f, 0.55f, 0.05f) : new Color(0.85f, 0.2f, 0.15f);
    }

    public void OnSatisfactionChanged(float delta)
    {
        satFlashT = 0f;
        satFlashColor = delta >= 0f ? UIKit.Green : UIKit.Red;
    }

    public void RefreshLevel()
    {
        if (levelText != null && Game.gm != null) levelText.text = Game.gm.Level.ToString();
        if (Game.toilets != null) Game.toilets.OnLevelChanged();
    }

    public void Toast(string msg, float duration = 3.5f)
    {
        toastText.text = msg;
        toastGo.SetActive(true);
        toastTimer = duration;
    }

    public void ShowCameraTutorial()
    {
        if (cameraTutorialGo != null && PlayerPrefs.GetInt("AT3_CameraTutorialDone", 0) == 0)
            cameraTutorialGo.SetActive(true);
    }

    public void HideCameraTutorial()
    {
        if (cameraTutorialGo != null) cameraTutorialGo.SetActive(false);
        PlayerPrefs.SetInt("AT3_CameraTutorialDone", 1);
        PlayerPrefs.Save();
    }

    public void SetPrompt(string msg)
    {
        promptRoot.SetActive(!string.IsNullOrEmpty(msg));
        promptText.text = msg;
    }

    public void ShowThiefTimer(float t)
    {
        thiefGo.SetActive(true);
        thiefText.text = "HIRSIZ KACIYOR!  " + Mathf.Max(0f, t).ToString("0.0") + "s";
    }

    public void HideThiefTimer() { if (thiefGo != null) thiefGo.SetActive(false); }

    void Update()
    {
        if (Game.gm == null) return;
        OnMoneyChanged();

        if (Input.GetKeyDown(KeyCode.Escape) && !MainMenuOpen && !MapOpen && (Game.pc == null || !Game.pc.IsOpen))
            TogglePause();

        // M: world map (HARITA tech)
        if (Input.GetKeyDown(KeyCode.M) && Game.gm.TechActive(1) && !MainMenuOpen && !PauseOpen && (Game.pc == null || !Game.pc.IsOpen))
        {
            if (MapOpen) { mapGo.SetActive(false); UpdateCursor(false); }
            else OpenMap();
        }
        if (MapOpen && Game.player != null && mapPlayerDot != null)
            mapPlayerDot.anchoredPosition = WorldToMap(Game.player.transform.position);

        UpdateTechWidgets();

        if (punchT < 1f)
        {
            punchT += Time.unscaledDeltaTime * 3f;
            moneyRect.localScale = Vector3.one * (1f + Mathf.Sin(Mathf.Clamp01(punchT) * Mathf.PI) * 0.22f);
        }

        float sat = Game.gm.Satisfaction;
        satText.text = "%" + Mathf.RoundToInt(sat);
        Color baseCol = sat > 70f ? UIKit.Green : sat > 40f ? UIKit.Orange : UIKit.Red;
        if (satIcon != null) satIcon.color = baseCol;
        if (satFlashT < 1f)
        {
            satFlashT += Time.unscaledDeltaTime * 2f;
            satText.color = Color.Lerp(satFlashColor, UIKit.TextDark, satFlashT);
            satRect.localScale = Vector3.one * (1f + Mathf.Sin(Mathf.Clamp01(satFlashT) * Mathf.PI) * 0.18f);
        }
        else satText.color = UIKit.TextDark;

        clockText.text = Game.gm.ClockText() + (Game.gm.shopOpen ? " ACIK" : " KAPALI");
        if (capacityText != null && Game.player != null)
            capacityText.text = Game.player.CarryCount + "/" + Game.gm.Capacity;

        // pollution %: shop litter + sea slicks
        if (dirtText != null && Game.trash != null)
        {
            int dirt = Mathf.Clamp(Game.trash.Count * 8 + Game.trash.SeaCount * 5, 0, 100);
            dirtText.text = "%" + dirt;
            dirtText.color = dirt < 30 ? UIKit.TextDark : dirt < 60 ? UIKit.Orange : UIKit.Red;
        }

        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            if (toastTimer <= 0f) toastGo.SetActive(false);
        }

        if (CelebrationOpen)
        {
            celebTimer -= Time.unscaledDeltaTime;
            if (celebTimer <= 0f) EndCelebration();
        }

        UpdateHint();
    }

    void UpdateTechWidgets()
    {
        bool inGame = Game.player != null;
        bool compassOn = inGame && Game.gm.TechActive(0);
        bool navOn = inGame && Game.gm.TechActive(2);

        if (compassGo != null && compassGo.activeSelf != compassOn) compassGo.SetActive(compassOn);
        if (compassOn && compassRose != null && Camera.main != null)
            compassRose.localRotation = Quaternion.Euler(0f, 0f, Camera.main.transform.eulerAngles.y);

        if (minimapGo != null && minimapGo.activeSelf != navOn) minimapGo.SetActive(navOn);
        if (minimapCam != null)
        {
            minimapCam.enabled = navOn;
            if (navOn && Game.player != null)
                minimapCam.transform.position = Game.player.transform.position + Vector3.up * 90f;
        }
        if (minimapThiefDot != null)
        {
            Thief thief = navOn && Game.player != null ? Thief.Nearest(Game.player.transform.position) : null;
            bool show = thief != null;
            minimapThiefDot.gameObject.SetActive(show);
            if (show)
            {
                Vector3 delta = thief.transform.position - Game.player.transform.position;
                minimapThiefDot.anchoredPosition = new Vector2(delta.x, delta.z) * (232f / 52f);
            }
        }

        // quests
        questTimer -= Time.unscaledDeltaTime;
        if (questTimer <= 0f)
        {
            questTimer = 0.5f;
            bool showQ = inGame && QuestSystem.quests.Length > 0;
            if (questGo != null && questGo.activeSelf != showQ) questGo.SetActive(showQ);
            if (showQ)
                for (int i = 0; i < 3 && i < QuestSystem.quests.Length; i++)
                {
                    questTexts[i].text = (QuestSystem.quests[i].done ? "+ " : "- ") + QuestSystem.quests[i].Line();
                    questTexts[i].color = QuestSystem.quests[i].done ? new Color(0.2f, 0.6f, 0.25f) : UIKit.TextDark;
                }
        }
    }

    void UpdateHint()
    {
        if (hintText == null || Game.player == null) return;
        string h;
        if (Game.register != null && Game.register.PileAmount > 0)
            h = "Kasadaki PARALARI topla!";
        else if (Game.trash != null && Game.trash.SeaCount >= 8)
            h = "Deniz cok kirli! Lekeler buyumeden copleri topla!";
        else if (Game.player.Swimming)
            h = "Baliga yaklas, radar dolunca yakalarsin!";
        else if (Game.player.CarryCount > 0)
            h = "Baliklari KENDI TURUNUN akvaryumuna birak!";
        else if (Game.TotalStock() == 0)
            h = "Denize git ve balik yakala! (iskeleden atlayabilirsin)";
        else if (Game.register != null && !Game.register.HasOperator)
            h = "Musteriler bekliyor: kasanin arkasina gec veya kasiyer al!";
        else
            h = Loc.T("CONTROL_HINT");
        hintText.text = h;
    }
}

// Hover tooltip helper for HUD pills.
public class HoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tip;
    public void OnPointerEnter(PointerEventData e) { if (Game.ui != null) Game.ui.ShowTip(tip); }
    public void OnPointerExit(PointerEventData e) { if (Game.ui != null) Game.ui.ShowTip(null); }
}
