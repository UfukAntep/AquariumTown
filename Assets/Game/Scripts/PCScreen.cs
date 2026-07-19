using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The register PC — cartoon "operating system" management hub.
public class PCScreen : MonoBehaviour
{
    GameObject root;
    GameObject[] pages;
    Button[] tabButtons;
    Text moneyText, clockText, titleText;
    List<System.Action> refreshers = new List<System.Action>();
    int tankPage;
    Text tankPageText;
    Transform tankListRoot;
    GameObject namingGo;
    InputField nameInput;
    Text namingHeader, namingMessage;
    GameObject summaryGo;
    Text sumTitle;
    Text[] sumVals = new Text[5];
    int collPage, histPage, revPage, techPage;
    Transform collRoot, histRoot, revRoot, techRoot;
    Text collPageText, histPageText, revHeaderText, techPageText;
    Text sumStars;
    Image[] revHeaderStars;
    Text homeShopName, homeCustomers, homeStars, homeReviews, homeValue, homeLevel;
    Transform trophyRoot;
    Text trophyPageText;
    int trophyPage;

    public bool IsOpen { get { return root != null && root.activeSelf; } }

    public static PCScreen Create()
    {
        GameObject go = new GameObject("PCScreen");
        PCScreen pc = go.AddComponent<PCScreen>();
        pc.Build();
        Game.pc = pc;
        return pc;
    }

    void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // window
        root = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1420f, 820f), UIKit.Cream, true, true);

        // title bar
        GameObject bar = UIKit.Panel(root.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 12f), new Vector2(1450f, 74f), UIKit.Blue, true, true);
        GameObject barIcon = UIKit.Icon(bar.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(44f, 44f), UIKit.Orange);
        UIKit.Label(barIcon.transform, "PC", 18, Color.white, TextAnchor.MiddleCenter, true);
        GameObject titleArea = UIKit.Panel(bar.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(600f, 60f), new Color(0f, 0f, 0f, 0.001f), false, false);
        titleText = UIKit.Label(titleArea.transform, "AKVARYUM ISLETIM SISTEMI v2.0", 24, Color.white, TextAnchor.MiddleLeft, true);

        // GUNU BITIR sits just left of the money display
        UIKit.Btn(bar.transform, new Vector2(245f, 0f), new Vector2(170f, 48f), UIKit.Orange, "GUNU BITIR", 17, delegate { OpenDaySummary(); });
        GameObject moneyP = UIKit.Panel(bar.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-190f, 0f), new Vector2(170f, 48f), UIKit.Green, true, false);
        moneyText = UIKit.Label(moneyP.transform, "", 22, Color.white, TextAnchor.MiddleCenter, true);
        GameObject clockP = UIKit.Panel(bar.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(160f, 48f), UIKit.BlueDark, true, false);
        clockText = UIKit.Label(clockP.transform, "", 22, Color.white, TextAnchor.MiddleCenter, true);

        // sidebar apps
        string[] menu = { "ANASAYFA", "GELISTIRME", "PERSONEL", "AKVARYUMLAR", "DEKOR", "BOYA", "TEKNOLOJI", "VERITABANI", "GECMIS", "YORUMLAR", "KUPALAR" };
        Color[] menuCol = { UIKit.Green, UIKit.Orange, UIKit.Purple, UIKit.Blue, new Color(0.8f, 0.55f, 0.3f), new Color(0.9f, 0.45f, 0.65f), new Color(0.3f, 0.7f, 0.75f), new Color(0.45f, 0.6f, 0.85f), new Color(0.6f, 0.6f, 0.65f), new Color(0.95f, 0.75f, 0.2f), new Color(0.95f, 0.58f, 0.15f) };
        pages = new GameObject[menu.Length];
        tabButtons = new Button[menu.Length];
        for (int i = 0; i < menu.Length; i++)
        {
            int idx = i;
            tabButtons[i] = UIKit.Btn(root.transform, new Vector2(-580f, 270f - i * 47f), new Vector2(230f, 40f), menuCol[i], menu[i], 14, delegate { ShowPage(idx); });
            Sprite tabIcon = i == 0 ? GameAssets.ItemIcon(7) : i == menu.Length - 1 ? null : GameAssets.TabIcon(i - 1);
            if (tabIcon != null)
                UIKit.Icon(tabButtons[i].transform, tabIcon, new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(30f, 30f), Color.white);
            else if (i == menu.Length - 1)
                UIKit.Icon(tabButtons[i].transform, UIKit.Trophy(), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(30f, 30f), UIKit.Yellow);
            GameObject page = UIKit.Panel(root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(115f, -35f), new Vector2(1130f, 690f), new Color(1f, 1f, 1f, 0.55f), true, false);
            pages[i] = page;
        }
        UIKit.Btn(root.transform, new Vector2(-580f, -274f), new Vector2(230f, 46f), UIKit.Red, "KAPAT (E)", 15, Close);

        BuildHomePage(pages[0].transform);
        BuildUpgradePage(pages[1].transform);
        BuildStaffPage(pages[2].transform);
        BuildTankPage(pages[3].transform);
        BuildDecorPage(pages[4].transform);
        BuildPaintPage(pages[5].transform);
        BuildTechPage(pages[6].transform);
        BuildCollectionPage(pages[7].transform);
        BuildHistoryPage(pages[8].transform);
        BuildReviewsPage(pages[9].transform);
        BuildTrophyPage(pages[10].transform);
        BuildNamingOverlay();
        BuildDaySummary();

        ShowPage(0);
        root.SetActive(false);
    }

    // ---------- day summary ("GUNU BITIR") ----------
    void BuildDaySummary()
    {
        summaryGo = UIKit.Panel(root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1420f, 820f), new Color(0.08f, 0.14f, 0.25f, 0.97f), true, false);
        GameObject box = UIKit.Panel(summaryGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(720f, 620f), UIKit.Cream, true, true);
        GameObject band = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 14f), new Vector2(750f, 88f), UIKit.Orange, true, true);
        sumTitle = UIKit.Label(band.transform, "", 30, Color.white, TextAnchor.MiddleCenter, true);
        GameObject starsP = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(680f, 40f), new Color(0f, 0f, 0f, 0.001f), false, false);
        sumStars = UIKit.Label(starsP.transform, "", 22, new Color(0.85f, 0.6f, 0.1f), TextAnchor.MiddleCenter);
        string[] names = { "Gelen Musteri", "Satilan Balik", "Toplam Gelir", "Toplam Gider", "NET KAR" };
        for (int i = 0; i < 5; i++)
        {
            float y = -140f - i * 74f;
            Color rowC = i == 4 ? new Color(0.85f, 0.95f, 0.85f) : Color.white;
            GameObject row = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(620f, 66f), rowC, true, false);
            GameObject la = UIKit.Panel(row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(330f, 60f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(la.transform, names[i], 22, UIKit.TextDark, TextAnchor.MiddleLeft);
            GameObject va = UIKit.Panel(row.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(250f, 60f), new Color(0f, 0f, 0f, 0.001f), false, false);
            sumVals[i] = UIKit.Label(va.transform, "", 26, i == 4 ? new Color(0.15f, 0.55f, 0.2f) : UIKit.TextDark, TextAnchor.MiddleRight);
        }
        UIKit.Btn(box.transform, new Vector2(0f, -272f), new Vector2(340f, 60f), UIKit.Green, "YENI GUNE BASLA!", 20,
            delegate
            {
                summaryGo.SetActive(false);
                Game.gm.EndDay();
                Time.timeScale = 1f;
                RefreshAll();
            });
        summaryGo.SetActive(false);
    }

    public void OpenDaySummary(bool auto = false)
    {
        if (summaryGo.activeSelf) return;
        // auto-end (05:00): make sure the PC panel is on screen first
        if (auto && !IsOpen)
        {
            root.SetActive(true);
            root.transform.localScale = Vector3.one;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        Game.gm.ApplySalaries(); // salaries count into today's expense
        if (Game.gm.BankruptcyTriggered)
        {
            root.SetActive(false);
            return;
        }
        summaryGo.SetActive(true);
        summaryGo.transform.SetAsLastSibling();
        sumTitle.text = auto ? "GUN " + Game.gm.dayNumber + " OTOMATIK BITTI" : "GUN " + Game.gm.dayNumber + " OZETI";
        sumStars.text = "Bugun " + Game.gm.dayReviewCount + " yorum   Ortalama: " + Game.gm.DayAvgStars.ToString("0.0") + " / 5 yildiz";
        StopAllCoroutines();
        StartCoroutine(CountUp());
        Sfx.Play(Snd.Buy, 0.7f);
    }

    System.Collections.IEnumerator CountUp()
    {
        int[] vals = { Game.gm.dayCustomers, Game.gm.dayFishSold, Game.gm.dayIncome, Game.gm.dayExpense, Game.gm.dayIncome - Game.gm.dayExpense };
        for (int i = 0; i < 5; i++) sumVals[i].text = "";
        for (int i = 0; i < 5; i++)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 2.2f;
                int v = Mathf.RoundToInt(Mathf.Lerp(0f, vals[i], Mathf.Clamp01(t)));
                sumVals[i].text = i < 2 ? v.ToString() : (i == 4 && v < 0 ? "-$" + B.Money(-v) : "$" + B.Money(v));
                if (Time.frameCount % 4 == 0) Sfx.Play(Snd.Tick, 0.15f);
                yield return null;
            }
            if (i == 4) sumVals[4].color = vals[4] >= 0 ? new Color(0.15f, 0.55f, 0.2f) : new Color(0.8f, 0.2f, 0.15f);
            Sfx.Play(Snd.Cash, 0.3f);
        }
    }

    // ---------- VERITABANI (collection) ----------
    void BuildCollectionPage(Transform page)
    {
        GameObject listP = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 25f), new Vector2(1090f, 560f), new Color(0f, 0f, 0f, 0.001f), false, false);
        collRoot = listP.transform;
        UIKit.Btn(page, new Vector2(-200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "< ONCEKI", 16, delegate { collPage = Mathf.Max(0, collPage - 1); RebuildCollection(); });
        UIKit.Btn(page, new Vector2(200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "SONRAKI >", 16, delegate { collPage++; RebuildCollection(); });
        GameObject pt = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(300f, 40f), new Color(0f, 0f, 0f, 0.001f), false, false);
        collPageText = UIKit.Label(pt.transform, "", 17, UIKit.TextDark, TextAnchor.MiddleCenter);
        refreshers.Add(RebuildCollection);
    }

    void RebuildCollection()
    {
        if (collRoot == null) return;
        for (int i = collRoot.childCount - 1; i >= 0; i--) Destroy(collRoot.GetChild(i).gameObject);
        int perPage = 15;
        int maxPage = (SpeciesInfo.Count - 1) / perPage;
        collPage = Mathf.Clamp(collPage, 0, maxPage);
        int found = 0;
        for (int i = 0; i < SpeciesInfo.Count; i++) if (Game.gm.discovered[i]) found++;
        if (collPageText != null) collPageText.text = "Kesfedilen: " + found + "/" + SpeciesInfo.Count + "   Sayfa " + (collPage + 1) + "/" + (maxPage + 1);
        int start = collPage * perPage;
        for (int n = 0; n < perPage; n++)
        {
            int sp = start + n;
            if (sp >= SpeciesInfo.Count) break;
            bool known = Game.gm.discovered[sp];
            float x = -430f + (n % 5) * 215f;
            float y = 190f - (n / 5) * 190f;
            GameObject card = UIKit.Panel(collRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(200f, 175f), known ? Color.white : new Color(0.5f, 0.5f, 0.55f), true, false);
            Sprite portrait = known ? GameAssets.FishPortrait(sp) : null;
            if (portrait != null)
            {
                GameObject portraitBg = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -48f), new Vector2(116f, 80f), new Color(0.84f, 0.95f, 1f), true, false);
                UIKit.Icon(portraitBg.transform, portrait, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(108f, 72f), Color.white);
            }
            else
                UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(62f, 62f), known ? SpeciesInfo.MainColor(sp) : new Color(0.28f, 0.28f, 0.32f));
            GameObject la = UIKit.Panel(card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(190f, 80f), new Color(0f, 0f, 0f, 0.001f), false, false);
            string cardText = known
                ? "#" + (sp + 1) + " " + SpeciesInfo.Name(sp) + "\n$" + B.Money(SpeciesInfo.Price(sp))
                : "#" + (sp + 1) + "\n? ? ?";
            UIKit.Label(la.transform, cardText, 15, known ? UIKit.TextDark : new Color(0.85f, 0.85f, 0.9f), TextAnchor.MiddleCenter);
        }
    }

    // ---------- GECMIS (day history) ----------
    void BuildHistoryPage(Transform page)
    {
        GameObject listP = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 25f), new Vector2(1090f, 560f), new Color(0f, 0f, 0f, 0.001f), false, false);
        histRoot = listP.transform;
        UIKit.Btn(page, new Vector2(-200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "< ONCEKI", 16, delegate { histPage = Mathf.Max(0, histPage - 1); RebuildHistory(); });
        UIKit.Btn(page, new Vector2(200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "SONRAKI >", 16, delegate { histPage++; RebuildHistory(); });
        GameObject pt = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(240f, 40f), new Color(0f, 0f, 0f, 0.001f), false, false);
        histPageText = UIKit.Label(pt.transform, "", 17, UIKit.TextDark, TextAnchor.MiddleCenter);
        refreshers.Add(RebuildHistory);
    }

    void RebuildHistory()
    {
        if (histRoot == null) return;
        for (int i = histRoot.childCount - 1; i >= 0; i--) Destroy(histRoot.GetChild(i).gameObject);
        int totalDays = Game.gm.dayNumber - 1;
        int maxPage = Mathf.Max(0, totalDays - 1);
        histPage = Mathf.Clamp(histPage, 0, maxPage);
        if (histPageText != null) histPageText.text = totalDays > 0 ? "Ozet " + (histPage + 1) + "/" + totalDays : "";
        if (totalDays <= 0)
        {
            GameObject empty = UIKit.Panel(histRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 80f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(empty.transform, "Henuz gecmis yok. Ilk gununu bitir!", 22, UIKit.TextDark, TextAnchor.MiddleCenter);
            return;
        }
        int day = totalDays - histPage; // newest first, one full summary per page
        int c, f, inc, exp, reviewCount, reviewStars;
        if (Game.gm.GetHistory(day, out c, out f, out inc, out exp, out reviewCount, out reviewStars))
        {
            int net = inc - exp;
            GameObject box = UIKit.Panel(histRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(760f, 540f), new Color(1f, 0.97f, 0.9f), true, true);
            GameObject band = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 8f),
                new Vector2(790f, 70f), UIKit.Orange, true, true);
            UIKit.Label(band.transform, "GUN " + day + " OZETI", 27, Color.white, TextAnchor.MiddleCenter, true);
            float avg = reviewCount > 0 ? (float)reviewStars / reviewCount : 0f;
            GameObject review = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -67f),
                new Vector2(700f, 38f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(review.transform, "Yorum: " + reviewCount + "   Ortalama: " + avg.ToString("0.0") + " / 5 yildiz",
                18, new Color(0.78f, 0.52f, 0.08f), TextAnchor.MiddleCenter);
            string[] labels = { "Gelen Musteri", "Satilan Balik", "Toplam Gelir", "Toplam Gider", "NET KAR" };
            string[] values = { c.ToString(), f.ToString(), "$" + B.Money(inc), "$" + B.Money(exp), net < 0 ? "-$" + B.Money(-net) : "$" + B.Money(net) };
            for (int i = 0; i < labels.Length; i++)
            {
                float y = -125f - i * 70f;
                Color rowColor = i == 4 ? (net >= 0 ? new Color(0.85f, 0.95f, 0.85f) : new Color(0.98f, 0.85f, 0.85f)) : Color.white;
                GameObject row = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(650f, 60f), rowColor, true, false);
                GameObject left = UIKit.Panel(row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(360f, 54f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(left.transform, labels[i], 20, UIKit.TextDark, TextAnchor.MiddleLeft);
                GameObject right = UIKit.Panel(row.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-22f, 0f), new Vector2(220f, 54f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(right.transform, values[i], 22, i == 4 ? (net >= 0 ? new Color(0.15f, 0.55f, 0.2f) : new Color(0.8f, 0.2f, 0.15f)) : UIKit.TextDark, TextAnchor.MiddleRight);
            }
        }
    }

    // ---------- technology page (compass, map, minimap-nav, remote control, auto-radar, auto-shop) ----------
    static readonly string[] TechNames = { "PUSULA", "HARITA", "GELISMIS NAVIGASYON", "UZAKTAN KONTROL", "OTOMATIK RADAR", "OTOMATIK DUKKAN" };
    static readonly string[] TechDescs = {
        "Sol altta calisan pusula + en yakin baligi gosteren ok",
        "M tusuyla acilan oyun haritasi",
        "GTA tarzi MINIMAP + en degerli baligi isaretler (mesafeli)",
        "Dukkani PC'den ac/kapat",
        "Radari uygun baliga otomatik kilitler (oyuncunun yonelmesi yeterlidir)",
        "Dukkani belirlenen saatte otomatik acip kapatir" };

    void BuildTechPage(Transform page)
    {
        GameObject listP = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 65f), new Vector2(1090f, 500f), new Color(0f, 0f, 0f, 0.001f), false, false);
        techRoot = listP.transform;
        UIKit.Btn(page, new Vector2(-200f, -220f), new Vector2(150f, 54f), UIKit.Blue, "< ONCEKI", 16, delegate { techPage = Mathf.Max(0, techPage - 1); RebuildTechPage(); });
        UIKit.Btn(page, new Vector2(200f, -220f), new Vector2(150f, 54f), UIKit.Blue, "SONRAKI >", 16, delegate { techPage++; RebuildTechPage(); });
        GameObject pt = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 107f), new Vector2(200f, 40f), new Color(0f, 0f, 0f, 0.001f), false, false);
        techPageText = UIKit.Label(pt.transform, "", 17, UIKit.TextDark, TextAnchor.MiddleCenter);
        refreshers.Add(RebuildTechPage);

    }

    void RebuildTechPage()
    {
        if (techRoot == null) return;
        for (int i = techRoot.childCount - 1; i >= 0; i--) Destroy(techRoot.GetChild(i).gameObject);
        int perPage = 4;
        int total = TechNames.Length;
        int maxPage = Mathf.Max(0, (total - 1) / perPage);
        techPage = Mathf.Clamp(techPage, 0, maxPage);
        if (techPageText != null) techPageText.text = "Sayfa " + (techPage + 1) + "/" + (maxPage + 1);
        int start = techPage * perPage;
        for (int n = 0; n < perPage; n++)
        {
            int idx = start + n;
            if (idx >= total) break;
            float y = 180f - n * 115f;
            GameObject card = UIKit.Panel(techRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1050f, 108f), Color.white, true, true);
            GameObject techBadge = UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(56f, 0f), new Vector2(64f, 64f), new Color(0.3f, 0.7f, 0.75f));
            Sprite techIcon = GameAssets.ItemIcon((idx + 3) % 8);
            if (techIcon != null) UIKit.Icon(techBadge.transform, techIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40f, 40f), Color.white);
            GameObject la = UIKit.Panel(card.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(100f, 0f), new Vector2(560f, 96f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(la.transform, TechNames[idx] + "\n" + TechDescs[idx], 16, UIKit.TextDark, TextAnchor.MiddleLeft);
            int spc = idx;
            Button btn = UIKit.Btn(card.transform, new Vector2(380f, 0f), new Vector2(230f, 56f), UIKit.Green, "", 17,
                delegate
                {
                    if (!Game.gm.techOwned[spc])
                    {
                        if (Game.gm.TryBuyTech(spc)) RefreshAll();
                        else Sfx.Play(Snd.Drop, 0.3f);
                    }
                    else if (spc == 3)
                    {
                        Game.gm.shopOpen = !Game.gm.shopOpen;
                        GameBootstrap.UpdateGateBarrier();
                        Sfx.Play(Snd.ShopToggle, 0.5f);
                        RefreshAll();
                    }
                    else
                    {
                        Game.gm.techEnabled[spc] = !Game.gm.techEnabled[spc];
                        Sfx.Play(Snd.Tick, 0.4f);
                        RefreshAll();
                    }
                });
            Text bt = btn.GetComponentInChildren<Text>();
            
            if (Game.gm.techOwned[spc])
            {
                if (spc == 3)
                {
                    bt.text = Game.gm.shopOpen ? "DUKKANI KAPAT" : "DUKKANI AC";
                    btn.image.color = Game.gm.shopOpen ? UIKit.Red : UIKit.Green;
                }
                else
                {
                    bool on = Game.gm.techEnabled[spc];
                    bt.text = on ? "ACIK (kapat)" : "KAPALI (ac)";
                    btn.image.color = on ? UIKit.Green : new Color(0.6f, 0.6f, 0.6f);
                }
            }
            else
            {
                int cost = GameManager.TechCosts[spc];
                bt.text = "SATIN AL  $" + B.Money(cost);
                btn.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
            }

            if (spc == 5 && Game.gm.techOwned[5])
            {
                RectTransform buttonRect = btn.GetComponent<RectTransform>();
                buttonRect.anchoredPosition = new Vector2(440f, 0f);
                buttonRect.sizeDelta = new Vector2(150f, 56f);
                GameObject timePanel = UIKit.Panel(card.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(185f, 0f), new Vector2(250f, 92f), new Color(0.94f, 0.97f, 1f), true, false);
                GameObject openLabel = UIKit.Panel(timePanel.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 20f), new Vector2(120f, 28f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(openLabel.transform, "Acilis " + Game.gm.autoOpenTime.ToString("00") + ":00", 13, UIKit.TextDark, TextAnchor.MiddleLeft);
                GameObject closeLabel = UIKit.Panel(timePanel.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, -20f), new Vector2(120f, 28f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(closeLabel.transform, "Kapanis " + Game.gm.autoCloseTime.ToString("00") + ":00", 13, UIKit.TextDark, TextAnchor.MiddleLeft);
                UIKit.Btn(timePanel.transform, new Vector2(63f, 20f), new Vector2(34f, 28f), UIKit.Blue, "-", 13, delegate { if (Game.gm.autoOpenTime > 5) Game.gm.autoOpenTime--; RefreshAll(); });
                UIKit.Btn(timePanel.transform, new Vector2(101f, 20f), new Vector2(34f, 28f), UIKit.Blue, "+", 13, delegate { if (Game.gm.autoOpenTime < Game.gm.autoCloseTime - 1) Game.gm.autoOpenTime++; RefreshAll(); });
                UIKit.Btn(timePanel.transform, new Vector2(63f, -20f), new Vector2(34f, 28f), UIKit.Blue, "-", 13, delegate { if (Game.gm.autoCloseTime > Game.gm.autoOpenTime + 1) Game.gm.autoCloseTime--; RefreshAll(); });
                UIKit.Btn(timePanel.transform, new Vector2(101f, -20f), new Vector2(34f, 28f), UIKit.Blue, "+", 13, delegate { if (Game.gm.autoCloseTime < 24) Game.gm.autoCloseTime++; RefreshAll(); });
            }
        }
    }

    // ---------- first-time shop naming ----------
    // ---------- YORUMLAR (Google-style reviews) ----------
    void BuildReviewsPage(Transform page)
    {
        GameObject header = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(1080f, 70f), new Color(1f, 0.95f, 0.82f), true, true);
        revHeaderText = UIKit.Label(header.transform, "", 22, new Color(0.7f, 0.5f, 0.1f), TextAnchor.MiddleCenter);
        revHeaderText.rectTransform.offsetMin = new Vector2(20f, 0f);
        revHeaderText.rectTransform.offsetMax = new Vector2(-260f, 0f);
        revHeaderStars = new Image[5];
        for (int i = 0; i < revHeaderStars.Length; i++)
        {
            GameObject star = UIKit.Icon(header.transform, UIKit.Star(), new Vector2(1f, 0.5f),
                new Vector2(-212f + i * 38f, 0f), new Vector2(30f, 30f), ReviewStarEmpty);
            Shadow shadow = star.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.45f, 0.28f, 0.05f, 0.2f);
            shadow.effectDistance = new Vector2(0f, -2f);
            revHeaderStars[i] = star.GetComponent<Image>();
        }
        GameObject listP = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(1080f, 500f), new Color(0f, 0f, 0f, 0.001f), false, false);
        revRoot = listP.transform;
        UIKit.Btn(page, new Vector2(-200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "< ONCEKI", 16, delegate { revPage = Mathf.Max(0, revPage - 1); RebuildReviews(); });
        UIKit.Btn(page, new Vector2(200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "SONRAKI >", 16, delegate { revPage++; RebuildReviews(); });
        refreshers.Add(RebuildReviews);
    }

    static readonly Color ReviewStarFilled = new Color(1f, 0.72f, 0.08f);
    static readonly Color ReviewStarEmpty = new Color(0.82f, 0.79f, 0.72f);

    static void AddReviewStars(Transform parent, int rating)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject star = UIKit.Icon(parent, UIKit.Star(), new Vector2(0f, 0.5f),
                new Vector2(252f + i * 28f, 0f), new Vector2(23f, 23f), i < rating ? ReviewStarFilled : ReviewStarEmpty);
            if (i < rating)
            {
                Shadow shadow = star.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.45f, 0.28f, 0.05f, 0.22f);
                shadow.effectDistance = new Vector2(0f, -1.5f);
            }
        }

        GameObject score = UIKit.Panel(parent, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(420f, 0f), new Vector2(54f, 26f), new Color(1f, 0.93f, 0.72f), true, false);
        UIKit.Label(score.transform, rating + "/5", 14, new Color(0.65f, 0.4f, 0.05f), TextAnchor.MiddleCenter);
    }

    void RebuildReviews()
    {
        if (revRoot == null) return;
        for (int i = revRoot.childCount - 1; i >= 0; i--) Destroy(revRoot.GetChild(i).gameObject);
        if (revHeaderText != null)
            revHeaderText.text = "Toplam " + Game.gm.reviewCount + " yorum   -   Ortalama " + Game.gm.AvgStars.ToString("0.0") + " / 5";
        if (revHeaderStars != null)
        {
            int roundedAverage = Mathf.RoundToInt(Game.gm.AvgStars);
            for (int i = 0; i < revHeaderStars.Length; i++)
                if (revHeaderStars[i] != null) revHeaderStars[i].color = i < roundedAverage ? ReviewStarFilled : ReviewStarEmpty;
        }
        int perPage = 5;
        int total = Reviews.recent.Count;
        int maxPage = Mathf.Max(0, (total - 1) / perPage);
        revPage = Mathf.Clamp(revPage, 0, maxPage);
        if (total == 0)
        {
            GameObject empty = UIKit.Panel(revRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 80f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(empty.transform, "Henuz yorum yok. Musteri agirla!", 22, UIKit.TextDark, TextAnchor.MiddleCenter);
            return;
        }
        int start = revPage * perPage;
        for (int n = 0; n < perPage; n++)
        {
            int idx = start + n;
            if (idx >= total) break;
            Reviews.Review r = Reviews.recent[idx];
            float y = 200f - n * 96f;
            GameObject row = UIKit.Panel(revRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1060f, 86f), Color.white, true, false);
            UIKit.Icon(row.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(52f, 52f), UIKit.Blue);
            GameObject top = UIKit.Panel(row.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(84f, -8f), new Vector2(920f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false);
            GameObject authorArea = UIKit.Panel(top.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(220f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(authorArea.transform, r.author, 17, new Color(0.85f, 0.52f, 0.08f), TextAnchor.MiddleLeft);
            AddReviewStars(top.transform, r.stars);
            GameObject bot = UIKit.Panel(row.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(84f, 10f), new Vector2(920f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(bot.transform, r.text, 16, UIKit.TextDark, TextAnchor.MiddleLeft);
        }
    }

    void BuildNamingOverlay()
    {
        namingGo = UIKit.Panel(root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1420f, 820f), new Color(0.1f, 0.18f, 0.3f, 0.98f), true, false);
        GameObject box = UIKit.Panel(namingGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(760f, 420f), UIKit.Cream, true, true);
        GameObject band = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 14f), new Vector2(790f, 84f), UIKit.Blue, true, true);
        namingHeader = UIKit.Label(band.transform, "YONETIM PANELINE HOSGELDIN!", 28, Color.white, TextAnchor.MiddleCenter, true);
        GameObject msg = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(700f, 80f), new Color(0f, 0f, 0f, 0.001f), false, false);
        namingMessage = UIKit.Label(msg.transform, "Hadi dukkanina bir isim verelim!\nBu isim girisin uzerinde ve panelde gorunecek.", 21, UIKit.TextDark, TextAnchor.MiddleCenter);

        // input field
        GameObject field = UIKit.Panel(box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(520f, 72f), Color.white, true, true);
        Text placeholder = UIKit.Label(field.transform, "Dukkan adi yaz...", 24, new Color(0.6f, 0.6f, 0.6f), TextAnchor.MiddleCenter);
        Text inputText = UIKit.Label(field.transform, "", 24, UIKit.TextDark, TextAnchor.MiddleCenter);
        nameInput = field.AddComponent<InputField>();
        nameInput.targetGraphic = field.GetComponent<Image>();
        nameInput.textComponent = inputText;
        nameInput.placeholder = placeholder;
        nameInput.characterLimit = 18;

        UIKit.Btn(box.transform, new Vector2(0f, -150f), new Vector2(300f, 66f), UIKit.Green, "BU ISMI KOY!", 22,
            delegate
            {
                string n = nameInput.text != null ? nameInput.text.Trim() : "";
                if (string.IsNullOrEmpty(n)) n = "Akvaryum Dukkani";
                Game.gm.shopName = n;
                Game.gm.Save();
                RefreshTitle();
                GameBootstrap.ApplyShopName();
                namingGo.SetActive(false);
                RefreshAll();
                if (Game.ui != null) Game.ui.Toast("Hayirli olsun: " + n + "!");
                Sfx.Play(Snd.Buy, 0.7f);
            });
        namingGo.SetActive(false);
    }

    public void RefreshTitle()
    {
        if (titleText == null || Game.gm == null) return;
        titleText.text = string.IsNullOrEmpty(Game.gm.shopName)
            ? "AKVARYUM ISLETIM SISTEMI v2.0"
            : Game.gm.shopName.ToUpper() + "  -  YONETIM PANELI";
    }

    void ShowPage(int idx)
    {
        for (int i = 0; i < pages.Length; i++) pages[i].SetActive(i == idx);
        RefreshAll();
    }

    // ---------- pages ----------
    void BuildHomePage(Transform page)
    {
        GameObject welcome = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(1060f, 92f), UIKit.Blue, true, true);
        homeShopName = UIKit.Label(welcome.transform, "", 28, Color.white, TextAnchor.MiddleLeft, true);
        homeShopName.rectTransform.offsetMin = new Vector2(28f, 0f);
        homeShopName.rectTransform.offsetMax = new Vector2(-250f, 0f);
        UIKit.Btn(welcome.transform, new Vector2(392f, 0f), new Vector2(220f, 52f), UIKit.Orange, "ISMI DEGISTIR", 16, OpenNamingEditor);

        homeCustomers = HomeStat(page, new Vector2(-355f, 125f), "TOPLAM MUSTERI", UIKit.Blue, GameAssets.TabIcon(1));
        homeStars = HomeStat(page, new Vector2(0f, 125f), "YILDIZ ORTALAMASI", UIKit.Yellow, UIKit.Star());
        homeReviews = HomeStat(page, new Vector2(355f, 125f), "TOPLAM YORUM", UIKit.Purple, GameAssets.TabIcon(8));

        GameObject valueCard = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -75f), new Vector2(1060f, 180f), new Color(0.9f, 0.98f, 0.9f), true, true);
        UIKit.Icon(valueCard.transform, UIKit.Star(), new Vector2(0f, 0.5f), new Vector2(82f, 0f), new Vector2(100f, 100f), UIKit.Yellow);
        GameObject valueTitle = UIKit.Panel(valueCard.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(155f, -22f), new Vector2(500f, 38f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(valueTitle.transform, "SIRKET PIYASA DEGERI", 20, new Color(0.18f, 0.5f, 0.25f), TextAnchor.MiddleLeft, true);
        GameObject valueText = UIKit.Panel(valueCard.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(155f, 24f), new Vector2(610f, 88f), new Color(0f, 0f, 0f, 0.001f), false, false);
        homeValue = UIKit.Label(valueText.transform, "", 44, UIKit.TextDark, TextAnchor.MiddleLeft, true);
        GameObject levelText = UIKit.Panel(valueCard.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-35f, 0f), new Vector2(240f, 80f), UIKit.Green, true, false);
        homeLevel = UIKit.Label(levelText.transform, "", 19, Color.white, TextAnchor.MiddleCenter, true);

        GameObject note = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(1040f, 100f), new Color(1f, 0.95f, 0.82f), true, false);
        UIKit.Label(note.transform, "Deger; seviye, gelir, nakit, akvaryumlar, stok, gelistirmeler, teknoloji, dekor, personel ve memnuniyetten hesaplanir.", 17, UIKit.TextDark, TextAnchor.MiddleCenter);
        refreshers.Add(RefreshHome);
    }

    Text HomeStat(Transform page, Vector2 pos, string title, Color color, Sprite icon)
    {
        GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(330f, 180f), Color.white, true, true);
        UIKit.Icon(card.transform, icon != null ? icon : UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(58f, 58f), color);
        GameObject titleArea = UIKit.Panel(card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(300f, 32f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(titleArea.transform, title, 15, UIKit.TextDark, TextAnchor.MiddleCenter, true);
        GameObject valueArea = UIKit.Panel(card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(300f, 54f), new Color(0f, 0f, 0f, 0.001f), false, false);
        return UIKit.Label(valueArea.transform, "", 30, color, TextAnchor.MiddleCenter, true);
    }

    void RefreshHome()
    {
        if (Game.gm == null || homeShopName == null) return;
        homeShopName.text = string.IsNullOrEmpty(Game.gm.shopName) ? "AKVARYUM DUKKANI" : Game.gm.shopName;
        homeCustomers.text = B.Money(Game.gm.totalCustomers);
        homeStars.text = Game.gm.reviewCount > 0 ? Game.gm.AvgStars.ToString("0.0") + " / 5 ★" : "—";
        homeReviews.text = B.Money(Game.gm.reviewCount);
        homeValue.text = "$" + B.Money(Game.gm.CompanyMarketValue);
        homeLevel.text = "SIRKET SEVIYESI\n" + Game.gm.Level;
    }

    void OpenNamingEditor()
    {
        namingHeader.text = "DUKKAN ISMINI DEGISTIR";
        namingMessage.text = "Yeni isim girisin uzerinde, anasayfada ve yonetim panelinde gorunecek.";
        nameInput.text = Game.gm != null ? Game.gm.shopName : "";
        namingGo.SetActive(true);
        namingGo.transform.SetAsLastSibling();
        nameInput.Select();
        nameInput.ActivateInputField();
    }

    void BuildTrophyPage(Transform page)
    {
        GameObject header = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(1060f, 62f), new Color(1f, 0.94f, 0.72f), true, true);
        UIKit.Icon(header.transform, UIKit.Trophy(), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(42f, 42f), UIKit.Yellow);
        UIKit.Label(header.transform, "KUPA KOLEKSIYONU", 23, new Color(0.65f, 0.4f, 0.08f), TextAnchor.MiddleCenter, true);
        GameObject list = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(1090f, 530f), new Color(0f, 0f, 0f, 0.001f), false, false);
        trophyRoot = list.transform;
        UIKit.Btn(page, new Vector2(-210f, -310f), new Vector2(150f, 50f), UIKit.Blue, "< ONCEKI", 15, delegate { trophyPage = Mathf.Max(0, trophyPage - 1); RebuildTrophies(); });
        UIKit.Btn(page, new Vector2(210f, -310f), new Vector2(150f, 50f), UIKit.Blue, "SONRAKI >", 15, delegate { trophyPage++; RebuildTrophies(); });
        GameObject pageArea = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(250f, 38f), new Color(0f, 0f, 0f, 0.001f), false, false);
        trophyPageText = UIKit.Label(pageArea.transform, "", 16, UIKit.TextDark, TextAnchor.MiddleCenter);
        refreshers.Add(RebuildTrophies);
    }

    void RebuildTrophies()
    {
        if (trophyRoot == null) return;
        for (int i = trophyRoot.childCount - 1; i >= 0; i--) Destroy(trophyRoot.GetChild(i).gameObject);
        const int perPage = 12;
        int maxPage = Mathf.Max(0, (TrophySystem.All.Length - 1) / perPage);
        trophyPage = Mathf.Clamp(trophyPage, 0, maxPage);
        int unlockedCount = 0;
        for (int i = 0; i < TrophySystem.All.Length; i++) if (TrophySystem.IsUnlocked(i)) unlockedCount++;
        trophyPageText.text = "Acildi " + unlockedCount + "/" + TrophySystem.All.Length + "   Sayfa " + (trophyPage + 1) + "/" + (maxPage + 1);
        int start = trophyPage * perPage;
        for (int n = 0; n < perPage; n++)
        {
            int index = start + n;
            if (index >= TrophySystem.All.Length) break;
            TrophyDefinition trophy = TrophySystem.All[index];
            bool open = TrophySystem.IsUnlocked(index);
            float x = -355f + (n % 3) * 355f;
            float y = 192f - (n / 3) * 135f;
            GameObject card = UIKit.Panel(trophyRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(335f, 122f), open ? new Color(1f, 0.96f, 0.78f) : new Color(0.72f, 0.73f, 0.77f), true, true);
            UIKit.Icon(card.transform, UIKit.Trophy(), new Vector2(0f, 0.5f), new Vector2(48f, 0f), new Vector2(62f, 62f), open ? UIKit.Yellow : new Color(0.32f, 0.34f, 0.4f));
            GameObject info = UIKit.Panel(card.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(90f, 0f), new Vector2(225f, 104f), new Color(0f, 0f, 0f, 0.001f), false, false);
            string text = (open ? trophy.title : "KILITLI KUPA") + "\n" + trophy.description + "\n" + TrophySystem.Progress(trophy);
            UIKit.Label(info.transform, text, 14, open ? UIKit.TextDark : new Color(0.3f, 0.31f, 0.35f), TextAnchor.MiddleLeft, open);
        }
    }

    void BuildUpgradePage(Transform page)
    {
        Upg[] all = { Upg.Capacity, Upg.MoveSpeed, Upg.SwimSpeed, Upg.RadarSpeed, Upg.RadarRange, Upg.TipChance, Upg.CustSpeed, Upg.ExtraCash };
        Color[] tints = { UIKit.Orange, UIKit.Orange, UIKit.Blue, UIKit.Red, UIKit.Red, UIKit.Green, UIKit.Blue, UIKit.Green };
        for (int i = 0; i < all.Length; i++)
        {
            Upg u = all[i];
            float x = -405f + (i % 4) * 270f;
            float y = 155f - (i / 4) * 320f;
            GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(250f, 290f), Color.white, true, true);
            Sprite itemIcon = GameAssets.ItemIcon(i);
            if (itemIcon != null)
                UIKit.Icon(card.transform, itemIcon, new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(68f, 68f), tints[i]);
            else
                UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(62f, 62f), tints[i]);
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(240f, 44f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(nameP.transform, UpgInfo.Label(u), 19, UIKit.TextDark, TextAnchor.MiddleCenter);
            GameObject descP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -132f), new Vector2(238f, 50f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(descP.transform, UpgInfo.Desc(u), 14, new Color(0.5f, 0.45f, 0.4f), TextAnchor.MiddleCenter);
            GameObject lvlP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -172f), new Vector2(240f, 32f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text lvlT = UIKit.Label(lvlP.transform, "", 16, UIKit.Orange, TextAnchor.MiddleCenter);
            Button btn = UIKit.Btn(card.transform, new Vector2(0f, -92f), new Vector2(214f, 58f), UIKit.Green, "", 18,
                delegate { if (Game.gm.TryBuyUpgrade(u)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Text btnT = btn.GetComponentInChildren<Text>();
            refreshers.Add(delegate
            {
                int lvl = Game.gm.UpgLevel(u), max = UpgInfo.Max(u);
                lvlT.text = "Seviye " + lvl + " / " + max;
                if (lvl >= max) { btnT.text = "MAKS"; btn.interactable = false; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
                else
                {
                    int cost = Game.gm.UpgCost(u);
                    btnT.text = "$" + B.Money(cost);
                    btn.interactable = true;
                    btn.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
            });
        }
    }

    void BuildStaffPage(Transform page)
    {
        // total daily wage banner at the top
        GameObject bannerP = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(1080f, 46f), new Color(0.95f, 0.9f, 0.8f), true, false);
        Text wageT = UIKit.Label(bannerP.transform, "", 18, new Color(0.6f, 0.35f, 0.1f), TextAnchor.MiddleCenter);
        refreshers.Add(delegate { wageT.text = "GUNLUK GIDER: Maas $" + B.Money(Game.gm.TotalDailySalary()) + " + Tuvalet $" + B.Money(Game.gm.ToiletDaily()) + " = $" + B.Money(Game.gm.TotalDailySalary() + Game.gm.ToiletDaily()) + " (her gun sonu odenir)"; });

        // 7 staff roles in a 4-column grid
        for (int r = 0; r < StaffInfo.RoleCount; r++)
        {
            int role = r;
            float x = -405f + (r % 4) * 270f;
            float y = 108f - (r / 4) * 236f;
            GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(255f, 235f), Color.white, true, true);
            GameObject staffBadge = UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(0f, 78f), new Vector2(54f, 54f), role == 6 ? new Color(0.2f, 0.25f, 0.35f) : UIKit.Purple);
            Sprite personIcon = GameAssets.ItemIcon(6);
            if (personIcon != null) UIKit.Icon(staffBadge.transform, personIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(34f, 34f), Color.white);
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 42f), new Vector2(245f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(nameP.transform, StaffInfo.Names[role], 17, UIKit.TextDark, TextAnchor.MiddleCenter);
            GameObject descP = UIKit.Panel(card.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(245f, 42f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(descP.transform, StaffInfo.Descs[role], 13, new Color(0.5f, 0.45f, 0.4f), TextAnchor.MiddleCenter);
            GameObject cntP = UIKit.Panel(card.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -44f), new Vector2(245f, 24f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text cntT = UIKit.Label(cntP.transform, "", 15, UIKit.Orange, TextAnchor.MiddleCenter);
            Button fireBtn = UIKit.Btn(card.transform, new Vector2(-94f, -86f), new Vector2(42f, 46f), UIKit.Red, "-", 22,
                delegate { if (Game.gm.TryFireStaff(role)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Button btn = UIKit.Btn(card.transform, new Vector2(30f, -86f), new Vector2(166f, 46f), UIKit.Green, "", 13,
                delegate { if (Game.gm.TryHireStaff(role)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Text btnT = btn.GetComponentInChildren<Text>();
            refreshers.Add(delegate
            {
                int cnt = Game.gm.staffCounts[role], max = StaffInfo.MaxCount[role];
                cntT.text = "Calisan: " + cnt + "/" + max;
                fireBtn.interactable = cnt > 0;
                fireBtn.image.color = cnt > 0 ? UIKit.Red : new Color(0.65f, 0.65f, 0.65f);
                if (cnt >= max) { btnT.text = "MAKS"; btn.interactable = false; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
                else if (role == 2 && !Game.gm.HasDepot()) { btnT.text = "DEPO GEREKLI"; btn.image.color = UIKit.Orange; btn.interactable = true; }
                else if (role == 5 && Game.gm.toiletCount == 0) { btnT.text = "TUVALET GEREKLI"; btn.image.color = UIKit.Orange; btn.interactable = true; }
                else
                {
                    btnT.text = "ISE AL  $" + B.Money(Game.gm.StaffSalary(role)) + "/gun";
                    btn.interactable = true;
                    btn.image.color = UIKit.Green;
                }
            });
        }

        // toilet + sink purchase rows
        MakeUtilityRow(page, new Vector2(-282f, -300f), "KLOZET (tikanabilir!)",
            delegate () { return Game.gm.toiletCount; }, 5,
            delegate () { return Game.gm.ToiletUnitCost(); },
            delegate () { return Game.gm.TryBuyToilet(); });
        MakeUtilityRow(page, new Vector2(282f, -300f), "LAVABO (el yikama)",
            delegate () { return Game.gm.sinkCount; }, 4,
            delegate () { return Game.gm.SinkUnitCost(); },
            delegate () { return Game.gm.TryBuySink(); });
    }

    void MakeUtilityRow(Transform page, Vector2 pos, string label,
        System.Func<int> count, int max, System.Func<int> cost, System.Func<bool> buy)
    {
        GameObject row = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(545f, 62f), new Color(0.85f, 0.93f, 0.98f), true, true);
        GameObject la = UIKit.Panel(row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(330f, 54f), new Color(0f, 0f, 0f, 0.001f), false, false);
        Text info = UIKit.Label(la.transform, "", 16, UIKit.TextDark, TextAnchor.MiddleLeft);
        Button btn = UIKit.Btn(row.transform, new Vector2(190f, 0f), new Vector2(150f, 50f), UIKit.Green, "", 16,
            delegate { if (buy()) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
        Text btnT = btn.GetComponentInChildren<Text>();
        refreshers.Add(delegate
        {
            info.text = label + "  " + count() + "/" + max;
            if (!Game.gm.toiletAreaOpen) { btnT.text = "ALAN KAPALI"; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
            else if (count() >= max) { btnT.text = "MAKS"; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
            else { btnT.text = "$" + B.Money(cost()); btn.image.color = Game.gm.Money >= cost() ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f); }
        });
    }

    void BuildTankPage(Transform page)
    {
        GameObject listP = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 25f), new Vector2(1090f, 570f), new Color(0f, 0f, 0f, 0.001f), false, false);
        tankListRoot = listP.transform;
        UIKit.Btn(page, new Vector2(-200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "< ONCEKI", 16, delegate { tankPage = Mathf.Max(0, tankPage - 1); RebuildTankList(); });
        UIKit.Btn(page, new Vector2(200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "SONRAKI >", 16, delegate { tankPage++; RebuildTankList(); });
        GameObject pt = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(200f, 40f), new Color(0f, 0f, 0f, 0.001f), false, false);
        tankPageText = UIKit.Label(pt.transform, "", 17, UIKit.TextDark, TextAnchor.MiddleCenter);
        refreshers.Add(RebuildTankList);
    }

    void RebuildTankList()
    {
        if (tankListRoot == null) return;
        for (int i = tankListRoot.childCount - 1; i >= 0; i--) Destroy(tankListRoot.GetChild(i).gameObject);
        int perPage = 7;
        int total = Game.gm.unlockedCount;
        int maxPage = Mathf.Max(0, (total - 1) / perPage);
        tankPage = Mathf.Clamp(tankPage, 0, maxPage);
        if (tankPageText != null) tankPageText.text = "Sayfa " + (tankPage + 1) + "/" + (maxPage + 1);
        int start = tankPage * perPage;
        for (int n = 0; n < perPage; n++)
        {
            int sp = start + n;
            if (sp >= total) break;
            float y = 245f - n * 80f;
            GameObject row = UIKit.Panel(tankListRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1070f, 72f), Color.white, true, false);
            GameObject tankBadge = UIKit.Icon(row.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(48f, 48f), UIKit.Blue);
            Sprite aquariumIcon = GameAssets.TabIcon(2);
            if (aquariumIcon != null) UIKit.Icon(tankBadge.transform, aquariumIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(31f, 31f), Color.white);
            int lvl = Game.gm.tankLevel[sp];
            Tank t = Game.TankOf(sp);
            GameObject la = UIKit.Panel(row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(76f, 0f), new Vector2(600f, 64f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(la.transform, SpeciesInfo.Name(sp) + " Akvaryumu   |   Maks stok: " + (t != null ? t.Count : 0) + "/" + Game.gm.TankCapacity(sp) +
                "   |   Sv " + lvl + "/5   |   Kazanc x" + Game.gm.TankPriceMult(sp).ToString("0.00"),
                16, UIKit.TextDark, TextAnchor.MiddleLeft);
            int spc = sp;
            Button btn = UIKit.Btn(row.transform, new Vector2(430f, 0f), new Vector2(190f, 56f), UIKit.Green, "", 15,
                delegate { if (Game.gm.TryUpgradeTank(spc)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Text bt = btn.GetComponentInChildren<Text>();
            if (lvl >= 5) { bt.text = "MAKS"; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
            else bt.text = "GELISTIR $" + B.Money(Game.gm.TankUpgCost(sp));
        }
    }

    void BuildDecorPage(Transform page)
    {
        for (int i = 0; i < DecorInfo.Count; i++)
        {
            int idx = i;
            float x = -370f + (i % 3) * 370f;
            float y = 220f - (i / 3) * 220f;
            GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(350f, 200f), Color.white, true, true);
            Sprite decorIcon = GameAssets.DecorIcon(i);
            if (decorIcon != null)
                UIKit.Icon(card.transform, decorIcon, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(64f, 64f), i % 2 == 0 ? UIKit.Orange : UIKit.Blue);
            else
                UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(48f, 48f), i >= 6 ? UIKit.Blue : new Color(0.8f, 0.55f, 0.3f));
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(340f, 42f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(nameP.transform, DecorInfo.Names[idx], 18, UIKit.TextDark, TextAnchor.MiddleCenter);
            Button btn = UIKit.Btn(card.transform, new Vector2(0f, -60f), new Vector2(260f, 52f), UIKit.Green, "", 17,
                delegate
                {
                    if (idx == 6 && !Game.gm.decorOwned[8]) { Sfx.Play(Snd.Drop, 0.3f); return; } // Iskele required for Ramp
                    if (Game.gm.decorOwned[idx] && idx == 6)
                    {
                        if (Game.gm.TryUpgradeRamp()) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f);
                        return;
                    }
                    if (Game.gm.decorOwned[idx] && idx == 7)
                    {
                        if (Game.gm.TryUpgradeJetski()) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f);
                        return;
                    }
                    if (!Game.gm.decorOwned[idx] && Game.gm.TrySpend(DecorInfo.Costs[idx]))
                    {
                        Game.gm.decorOwned[idx] = true;
                        if (idx == 6) Game.gm.rampLevel = 1;
                        if (idx == 7) Game.gm.jetskiLevel = 1;
                        GameBootstrap.ApplyDecor(idx);
                        Sfx.Play(Snd.Buy);
                        RefreshAll();
                    }
                    else Sfx.Play(Snd.Drop, 0.3f);
                });
            Text bt = btn.GetComponentInChildren<Text>();
            refreshers.Add(delegate
            {
                if (Game.gm.decorOwned[idx] && idx == 6)
                {
                    int level = Game.gm.rampLevel;
                    bt.text = level >= 5 ? "RAMPA Sv5  MAKS" : "RAMPA Sv" + level + "  GELISTIR $" + B.Money(Game.gm.RampUpgradeCost());
                    btn.image.color = level >= 5 ? new Color(0.65f, 0.65f, 0.65f) : UIKit.Green;
                }
                else if (Game.gm.decorOwned[idx] && idx == 7)
                {
                    int level = Game.gm.jetskiLevel;
                    bt.text = level >= 5 ? "JETSKI Sv5  MAKS" : "JETSKI Sv" + level + "  GELISTIR $" + B.Money(Game.gm.JetskiUpgradeCost());
                    btn.image.color = level >= 5 ? new Color(0.65f, 0.65f, 0.65f) : UIKit.Green;
                }
                else if (Game.gm.decorOwned[idx]) { bt.text = "SAHIPSIN"; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
                else if (idx == 6 && !Game.gm.decorOwned[8])
                {
                    bt.text = "ISKELE GEREKLI";
                    btn.image.color = UIKit.Orange;
                }
                else
                {
                    bt.text = "$" + B.Money(DecorInfo.Costs[idx]);
                    btn.image.color = Game.gm.Money >= DecorInfo.Costs[idx] ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
            });
        }
    }

    void BuildPaintPage(Transform page)
    {
        GameObject fl = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1000f, 50f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(fl.transform, "ZEMIN RENGI  ($250)", 22, UIKit.TextDark, TextAnchor.MiddleCenter);
        for (int i = 0; i < MatLib.FloorStyles.Length; i++)
        {
            int idx = i;
            UIKit.Btn(page, new Vector2(-255f + i * 170f, 165f), new Vector2(140f, 100f), MatLib.FloorStyles[i], "", 14,
                delegate
                {
                    if (Game.gm.floorStyle != idx && Game.gm.TrySpend(250))
                    {
                        Game.gm.floorStyle = idx;
                        GameBootstrap.ApplyPaint();
                        Sfx.Play(Snd.Buy);
                    }
                });
        }
        GameObject wl = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(1000f, 50f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(wl.transform, "DUVAR RENGI  ($250)", 22, UIKit.TextDark, TextAnchor.MiddleCenter);
        for (int i = 0; i < MatLib.WallStyles.Length; i++)
        {
            int idx = i;
            UIKit.Btn(page, new Vector2(-255f + i * 170f, -85f), new Vector2(140f, 100f), MatLib.WallStyles[i], "", 14,
                delegate
                {
                    if (Game.gm.wallStyle != idx && Game.gm.TrySpend(250))
                    {
                        Game.gm.wallStyle = idx;
                        GameBootstrap.ApplyPaint();
                        Sfx.Play(Snd.Buy);
                    }
                });
        }
    }

    // ---------- open/close ----------
    public void Open()
    {
        root.SetActive(true);
        root.transform.localScale = Vector3.one * 0.7f;
        StartCoroutine(PopIn());
        RefreshTitle();
        RefreshAll();
        Sfx.Play(Snd.Collect, 0.4f);
        // first ever visit: name the shop!
        if (Game.gm != null && string.IsNullOrEmpty(Game.gm.shopName))
        {
            namingGo.SetActive(true);
            namingGo.transform.SetAsLastSibling();
        }
    }

    System.Collections.IEnumerator PopIn()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 4f;
            float k = Mathf.Clamp01(t);
            root.transform.localScale = Vector3.one * (0.7f + 0.3f * (1f - (1f - k) * (1f - k)));
            yield return null;
        }
        root.transform.localScale = Vector3.one;
    }

    public void Close()
    {
        root.SetActive(false);
        if (Game.cam != null) Game.cam.ReturnFromPC();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < refreshers.Count; i++) refreshers[i]();
    }

    void Update()
    {
        if (!IsOpen || Game.gm == null) return;
        if (moneyText != null) moneyText.text = "$ " + B.Money(Game.gm.Money);
        if (clockText != null) clockText.text = Game.gm.ClockText();
        // don't close while typing the shop name (E is a letter!)
        if (namingGo != null && namingGo.activeSelf) return;
        if (summaryGo != null && summaryGo.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) Close();
    }
}
