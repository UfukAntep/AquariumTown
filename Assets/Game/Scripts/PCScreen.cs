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
    GameObject summaryGo;
    Text sumTitle;
    Text[] sumVals = new Text[5];
    int collPage, histPage, revPage;
    Transform collRoot, histRoot, revRoot;
    Text collPageText, histPageText, revHeaderText;
    Text sumStars;

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
        UIKit.Btn(bar.transform, new Vector2(-475f, 0f), new Vector2(170f, 48f), UIKit.Orange, "GUNU BITIR", 17, delegate { OpenDaySummary(); });
        GameObject moneyP = UIKit.Panel(bar.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-190f, 0f), new Vector2(170f, 48f), UIKit.Green, true, false);
        moneyText = UIKit.Label(moneyP.transform, "", 22, Color.white, TextAnchor.MiddleCenter, true);
        GameObject clockP = UIKit.Panel(bar.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(160f, 48f), UIKit.BlueDark, true, false);
        clockText = UIKit.Label(clockP.transform, "", 22, Color.white, TextAnchor.MiddleCenter, true);

        // sidebar apps
        string[] menu = { "GELISTIRME", "PERSONEL", "AKVARYUMLAR", "DEKOR", "BOYA", "TEKNOLOJI", "VERITABANI", "GECMIS", "YORUMLAR" };
        Color[] menuCol = { UIKit.Orange, UIKit.Purple, UIKit.Blue, new Color(0.8f, 0.55f, 0.3f), new Color(0.9f, 0.45f, 0.65f), new Color(0.3f, 0.7f, 0.75f), new Color(0.45f, 0.6f, 0.85f), new Color(0.6f, 0.6f, 0.65f), new Color(0.95f, 0.75f, 0.2f) };
        pages = new GameObject[menu.Length];
        tabButtons = new Button[menu.Length];
        for (int i = 0; i < menu.Length; i++)
        {
            int idx = i;
            tabButtons[i] = UIKit.Btn(root.transform, new Vector2(-580f, 254f - i * 54f), new Vector2(230f, 46f), menuCol[i], menu[i], 15, delegate { ShowPage(idx); });
            GameObject page = UIKit.Panel(root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(115f, -35f), new Vector2(1130f, 690f), new Color(1f, 1f, 1f, 0.55f), true, false);
            pages[i] = page;
        }
        UIKit.Btn(root.transform, new Vector2(-580f, -252f), new Vector2(230f, 52f), UIKit.Red, "KAPAT (E)", 16, Close);

        BuildUpgradePage(pages[0].transform);
        BuildStaffPage(pages[1].transform);
        BuildTankPage(pages[2].transform);
        BuildDecorPage(pages[3].transform);
        BuildPaintPage(pages[4].transform);
        BuildTechPage(pages[5].transform);
        BuildCollectionPage(pages[6].transform);
        BuildHistoryPage(pages[7].transform);
        BuildReviewsPage(pages[8].transform);
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
        GameObject starsP = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(680f, 40f), new Color(0f, 0f, 0f, 0.001f), false, false);
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
        Game.gm.ApplySalaries(); // salaries count into today's expense
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
        int perPage = 7;
        int maxPage = Mathf.Max(0, (totalDays - 1) / perPage);
        histPage = Mathf.Clamp(histPage, 0, maxPage);
        if (histPageText != null) histPageText.text = "Sayfa " + (histPage + 1) + "/" + (maxPage + 1);
        if (totalDays <= 0)
        {
            GameObject empty = UIKit.Panel(histRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 80f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(empty.transform, "Henuz gecmis yok. Ilk gununu bitir!", 22, UIKit.TextDark, TextAnchor.MiddleCenter);
            return;
        }
        for (int n = 0; n < perPage; n++)
        {
            int day = totalDays - (histPage * perPage + n); // newest first
            if (day < 1) break;
            int c, f, inc, exp;
            if (!Game.gm.GetHistory(day, out c, out f, out inc, out exp)) continue;
            int net = inc - exp;
            float y = 245f - n * 80f;
            GameObject row = UIKit.Panel(histRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1070f, 72f), Color.white, true, false);
            GameObject dayBadge = UIKit.Panel(row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(110f, 56f), UIKit.Blue, true, false);
            UIKit.Label(dayBadge.transform, "GUN " + day, 18, Color.white, TextAnchor.MiddleCenter, true);
            GameObject la = UIKit.Panel(row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(140f, 0f), new Vector2(700f, 64f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(la.transform, "Musteri: " + c + "   Balik: " + f + "   Gelir: $" + B.Money(inc) + "   Gider: $" + B.Money(exp),
                16, UIKit.TextDark, TextAnchor.MiddleLeft);
            GameObject netP = UIKit.Panel(row.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(190f, 56f), net >= 0 ? new Color(0.85f, 0.95f, 0.85f) : new Color(0.98f, 0.85f, 0.85f), true, false);
            UIKit.Label(netP.transform, "NET " + (net < 0 ? "-$" + B.Money(-net) : "$" + B.Money(net)),
                17, net >= 0 ? new Color(0.15f, 0.55f, 0.2f) : new Color(0.8f, 0.2f, 0.15f), TextAnchor.MiddleCenter);
        }
    }

    // ---------- technology page (compass, map, minimap-nav, remote control) ----------
    static readonly string[] TechNames = { "PUSULA", "HARITA", "GELISMIS NAVIGASYON", "UZAKTAN KONTROL" };
    static readonly string[] TechDescs = {
        "Sol altta calisan pusula + en yakin baligi gosteren ok",
        "M tusuyla acilan oyun haritasi",
        "GTA tarzi MINIMAP + en degerli baligi isaretler (mesafeli)",
        "Dukkani PC'den ac/kapat" };

    void BuildTechPage(Transform page)
    {
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            float y = 215f - i * 122f;
            GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1050f, 108f), Color.white, true, true);
            UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(56f, 0f), new Vector2(56f, 56f), new Color(0.3f, 0.7f, 0.75f));
            GameObject la = UIKit.Panel(card.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(100f, 0f), new Vector2(560f, 96f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(la.transform, TechNames[idx] + "\n" + TechDescs[idx], 16, UIKit.TextDark, TextAnchor.MiddleLeft);
            Button btn = UIKit.Btn(card.transform, new Vector2(380f, 0f), new Vector2(230f, 56f), UIKit.Green, "", 17,
                delegate
                {
                    if (!Game.gm.techOwned[idx])
                    {
                        if (Game.gm.TryBuyTech(idx)) RefreshAll();
                        else Sfx.Play(Snd.Drop, 0.3f);
                    }
                    else if (idx != 3) // owned techs can be toggled on/off
                    {
                        Game.gm.techEnabled[idx] = !Game.gm.techEnabled[idx];
                        Sfx.Play(Snd.Tick, 0.4f);
                        RefreshAll();
                    }
                });
            Text bt = btn.GetComponentInChildren<Text>();
            refreshers.Add(delegate
            {
                if (Game.gm.techOwned[idx])
                {
                    if (idx == 3) { bt.text = "AKTIF"; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
                    else
                    {
                        bool on = Game.gm.techEnabled[idx];
                        bt.text = on ? "ACIK (kapat)" : "KAPALI (ac)";
                        btn.image.color = on ? UIKit.Green : new Color(0.6f, 0.6f, 0.6f);
                    }
                }
                else
                {
                    int cost = GameManager.TechCosts[idx];
                    bt.text = "SATIN AL  $" + B.Money(cost);
                    btn.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
            });
        }

        // remote shop open/close (needs the UZAKTAN KONTROL tech)
        Button shopBtn = UIKit.Btn(page, new Vector2(0f, -295f), new Vector2(500f, 58f), UIKit.Orange, "", 19,
            delegate
            {
                if (!Game.gm.techOwned[3]) { Sfx.Play(Snd.Drop, 0.3f); return; }
                Game.gm.shopOpen = !Game.gm.shopOpen;
                GameBootstrap.UpdateGateBarrier();
                Sfx.Play(Snd.Buy, 0.5f);
                RefreshAll();
            });
        Text sbt = shopBtn.GetComponentInChildren<Text>();
        refreshers.Add(delegate
        {
            if (!Game.gm.techOwned[3]) { sbt.text = "DUKKAN KONTROLU (teknoloji gerekli)"; shopBtn.image.color = new Color(0.6f, 0.6f, 0.6f); }
            else { sbt.text = Game.gm.shopOpen ? "DUKKANI KAPAT" : "DUKKANI AC"; shopBtn.image.color = Game.gm.shopOpen ? UIKit.Red : UIKit.Green; }
        });
    }

    // ---------- first-time shop naming ----------
    // ---------- YORUMLAR (Google-style reviews) ----------
    void BuildReviewsPage(Transform page)
    {
        GameObject header = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(1080f, 70f), new Color(1f, 0.95f, 0.82f), true, true);
        revHeaderText = UIKit.Label(header.transform, "", 22, new Color(0.7f, 0.5f, 0.1f), TextAnchor.MiddleCenter);
        GameObject listP = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(1080f, 500f), new Color(0f, 0f, 0f, 0.001f), false, false);
        revRoot = listP.transform;
        UIKit.Btn(page, new Vector2(-200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "< ONCEKI", 16, delegate { revPage = Mathf.Max(0, revPage - 1); RebuildReviews(); });
        UIKit.Btn(page, new Vector2(200f, -305f), new Vector2(150f, 54f), UIKit.Blue, "SONRAKI >", 16, delegate { revPage++; RebuildReviews(); });
        refreshers.Add(RebuildReviews);
    }

    static string StarStr(int n)
    {
        string s = "";
        for (int i = 0; i < 5; i++) s += i < n ? "*" : "-";
        return s;
    }

    void RebuildReviews()
    {
        if (revRoot == null) return;
        for (int i = revRoot.childCount - 1; i >= 0; i--) Destroy(revRoot.GetChild(i).gameObject);
        if (revHeaderText != null)
            revHeaderText.text = "Toplam " + Game.gm.reviewCount + " yorum   -   Ortalama " + Game.gm.AvgStars.ToString("0.0") + " / 5 yildiz  " + StarStr(Mathf.RoundToInt(Game.gm.AvgStars));
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
            UIKit.Label(top.transform, r.author + "    " + StarStr(r.stars), 17, new Color(0.85f, 0.6f, 0.1f), TextAnchor.MiddleLeft);
            GameObject bot = UIKit.Panel(row.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(84f, 10f), new Vector2(920f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(bot.transform, r.text, 16, UIKit.TextDark, TextAnchor.MiddleLeft);
        }
    }

    void BuildNamingOverlay()
    {
        namingGo = UIKit.Panel(root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1420f, 820f), new Color(0.1f, 0.18f, 0.3f, 0.98f), true, false);
        GameObject box = UIKit.Panel(namingGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(760f, 420f), UIKit.Cream, true, true);
        GameObject band = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 14f), new Vector2(790f, 84f), UIKit.Blue, true, true);
        UIKit.Label(band.transform, "YONETIM PANELINE HOSGELDIN!", 28, Color.white, TextAnchor.MiddleCenter, true);
        GameObject msg = UIKit.Panel(box.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(700f, 80f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(msg.transform, "Hadi dukkanina bir isim verelim!\nBu isim girisin uzerinde ve panelde gorunecek.", 21, UIKit.TextDark, TextAnchor.MiddleCenter);

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
            UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(62f, 62f), tints[i]);
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(240f, 44f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(nameP.transform, UpgInfo.Label(u), 19, UIKit.TextDark, TextAnchor.MiddleCenter);
            GameObject descP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -126f), new Vector2(240f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(descP.transform, UpgInfo.Desc(u), 15, new Color(0.5f, 0.45f, 0.4f), TextAnchor.MiddleCenter);
            GameObject lvlP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(240f, 32f), new Color(0f, 0f, 0f, 0.001f), false, false);
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
            UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(0f, 78f), new Vector2(46f, 46f), role == 6 ? new Color(0.2f, 0.25f, 0.35f) : UIKit.Purple);
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 42f), new Vector2(245f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(nameP.transform, StaffInfo.Names[role], 17, UIKit.TextDark, TextAnchor.MiddleCenter);
            GameObject descP = UIKit.Panel(card.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(245f, 42f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(descP.transform, StaffInfo.Descs[role], 13, new Color(0.5f, 0.45f, 0.4f), TextAnchor.MiddleCenter);
            GameObject cntP = UIKit.Panel(card.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -44f), new Vector2(245f, 24f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text cntT = UIKit.Label(cntP.transform, "", 15, UIKit.Orange, TextAnchor.MiddleCenter);
            Button btn = UIKit.Btn(card.transform, new Vector2(0f, -86f), new Vector2(228f, 46f), UIKit.Green, "", 15,
                delegate { if (Game.gm.TryHireStaff(role)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Text btnT = btn.GetComponentInChildren<Text>();
            refreshers.Add(delegate
            {
                int cnt = Game.gm.staffCounts[role], max = StaffInfo.MaxCount[role];
                cntT.text = "Calisan: " + cnt + "/" + max;
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
            UIKit.Icon(row.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(44f, 44f), SpeciesInfo.MainColor(sp));
            int lvl = Game.gm.tankLevel[sp];
            Tank t = Game.TankOf(sp);
            GameObject la = UIKit.Panel(row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(76f, 0f), new Vector2(600f, 64f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(la.transform, SpeciesInfo.Name(sp) + "   |   Stok: " + (t != null ? t.Count : 0) +
                "   |   Sv " + lvl + "/5   |   Satis x" + Game.gm.TankPriceMult(sp).ToString("0.00"),
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
            UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(48f, 48f), i >= 6 ? UIKit.Blue : new Color(0.8f, 0.55f, 0.3f));
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(340f, 42f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(nameP.transform, DecorInfo.Names[idx], 18, UIKit.TextDark, TextAnchor.MiddleCenter);
            Button btn = UIKit.Btn(card.transform, new Vector2(0f, -60f), new Vector2(260f, 52f), UIKit.Green, "", 17,
                delegate
                {
                    if (!Game.gm.decorOwned[idx] && Game.gm.TrySpend(DecorInfo.Costs[idx]))
                    {
                        Game.gm.decorOwned[idx] = true;
                        GameBootstrap.ApplyDecor(idx);
                        Sfx.Play(Snd.Buy);
                        RefreshAll();
                    }
                    else Sfx.Play(Snd.Drop, 0.3f);
                });
            Text bt = btn.GetComponentInChildren<Text>();
            refreshers.Add(delegate
            {
                if (Game.gm.decorOwned[idx]) { bt.text = "SAHIPSIN"; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
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
