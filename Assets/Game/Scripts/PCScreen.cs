using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The register PC — cartoon "operating system" management hub.
public class PCScreen : MonoBehaviour
{
    GameObject root;
    GameObject[] pages;
    Button[] tabButtons;
    GameObject[] tabSelectors;
    Text moneyText, clockText, titleText, closeButtonText;
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
    bool historyDetail;
    int historySelectedDay;
    Transform collRoot, histRoot, revRoot, techRoot;
    Text collPageText, histPageText, revHeaderText, techPageText;
    Text sumStars;
    Image[] revHeaderStars;
    Text homeShopName, homeCustomers, homeStars, homeReviews, homePopularity, homeValue, homeLevel;
    GameObject homeRemoteRoot, homeAutoRoot;
    Text homeRemoteText, homeAutoCheck, homeAutoHint, homeOpenText, homeCloseText;
    Button autoOpenMinus, autoOpenPlus, autoCloseMinus, autoClosePlus;
    Transform trophyRoot;
    Text trophyPageText;
    int trophyPage;
    int currentPage;
    int shopSubPage;
    GameObject[] shopSubPages;
    Text shopSubPageText;
    int selectedFloor = -1, selectedWall = -1;
    Text paintSelectionText;
    Button paintPreviewButton, paintBuyButton;
    Button endDayButton;
    bool paintPreviewing, paintPreviewReturning;
    float paintPreviewReadyAt;
    readonly List<CardPager> cardPagers = new List<CardPager>();

    public bool IsOpen { get { return (root != null && root.activeSelf) || paintPreviewing || paintPreviewReturning; } }

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
        canvas.pixelPerfect = true;
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
        endDayButton = UIKit.Btn(bar.transform, new Vector2(245f, 0f), new Vector2(170f, 48f), UIKit.Red, "GUNU BITIR", 17, delegate { OpenDaySummary(); });
        GameObject moneyP = UIKit.Panel(bar.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-190f, 0f), new Vector2(170f, 48f), UIKit.Green, true, false);
        moneyText = UIKit.Label(moneyP.transform, "", 22, Color.white, TextAnchor.MiddleCenter, true);
        GameObject clockP = UIKit.Panel(bar.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(160f, 48f), UIKit.BlueDark, true, false);
        clockText = UIKit.Label(clockP.transform, "", 22, Color.white, TextAnchor.MiddleCenter, true);

        // sidebar apps
        string[] menu = { "ANASAYFA", "KENDINI GELISTIRME", "MUSTERI GELISTIRME", "SILAH GELISTIRME", "DUKKAN GELISTIRME", "PERSONEL", "PERSONEL EGITIMI", "AKVARYUMLAR", "DEKOR", "BOYA", "TEKNOLOJI", "VERITABANI", "GECMIS", "YORUMLAR", "KUPALAR" };
        Color[] menuCol = { UIKit.Green, UIKit.Orange, new Color(0.25f, 0.68f, 0.55f), UIKit.Red, new Color(0.95f, 0.48f, 0.2f), UIKit.Purple, new Color(0.5f, 0.35f, 0.85f), UIKit.Blue, new Color(0.8f, 0.55f, 0.3f), new Color(0.9f, 0.45f, 0.65f), new Color(0.3f, 0.7f, 0.75f), new Color(0.45f, 0.6f, 0.85f), new Color(0.6f, 0.6f, 0.65f), new Color(0.95f, 0.75f, 0.2f), new Color(0.95f, 0.58f, 0.15f) };
        pages = new GameObject[menu.Length];
        tabButtons = new Button[menu.Length];
        tabSelectors = new GameObject[menu.Length];
        for (int i = 0; i < menu.Length; i++)
        {
            int idx = i;
            // 15 menu items + KAPAT = 16 rows evenly filling the white panel height
            // (top ~290, step ~43.3). All rows share the same width/height.
            tabButtons[i] = UIKit.Btn(root.transform, new Vector2(-580f, 290f - i * 43.3f), new Vector2(236f, 40f), menuCol[i], menu[i], 12, delegate { ShowPage(idx); });
            GameObject selector = UIKit.Panel(tabButtons[i].transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(10f, 0f), new Vector2(34f, 34f), Color.white, true, true);
            UIKit.Label(selector.transform, "▶", 20, menuCol[i], TextAnchor.MiddleCenter, true);
            tabSelectors[i] = selector;
            Sprite tabIcon = i == 7 ? UIKit.Fish() : i == 1 ? UIKit.Person() : SidebarIcon(i);
            if (tabIcon != null)
                UIKit.Icon(tabButtons[i].transform, tabIcon, new Vector2(0f, 0.5f), new Vector2(27f, 0f), new Vector2(34f, 34f), Color.white);
            else if (i == menu.Length - 1)
                UIKit.Icon(tabButtons[i].transform, UIKit.Trophy(), new Vector2(0f, 0.5f), new Vector2(27f, 0f), new Vector2(34f, 34f), UIKit.Yellow);
            GameObject page = UIKit.Panel(root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(115f, -35f), new Vector2(1130f, 690f), new Color(1f, 1f, 1f, 0.55f), true, false);
            pages[i] = page;
        }
        Button closeButton = UIKit.Btn(root.transform, new Vector2(-580f, 290f - 15 * 43.3f), new Vector2(236f, 40f), UIKit.Red, "KAPAT (" + ControlBindings.KeyName(ControlAction.Interact) + ")", 13, Close);
        closeButtonText = closeButton.GetComponentInChildren<Text>();

        BuildHomePage(pages[0].transform);
        BuildUpgradePage(pages[1].transform);
        BuildCustomerUpgradePage(pages[2].transform);
        BuildWeaponPage(pages[3].transform);
        BuildShopUpgradePage(pages[4].transform);
        BuildStaffPage(pages[5].transform);
        BuildStaffTrainingPage(pages[6].transform);
        BuildTankPage(pages[7].transform);
        BuildDecorPage(pages[8].transform);
        BuildPaintPage(pages[9].transform);
        BuildTechPage(pages[10].transform);
        BuildCollectionPage(pages[11].transform);
        BuildHistoryPage(pages[12].transform);
        BuildReviewsPage(pages[13].transform);
        BuildTrophyPage(pages[14].transform);
        BuildNamingOverlay();
        BuildDaySummary();

        ShowPage(0);
        root.SetActive(false);
    }

    Sprite SidebarIcon(int index)
    {
        if (index == 0) return GameAssets.ItemIcon(7);
        if (index == 1 || index == 4) return GameAssets.TabIcon(0);
        if (index == 2) return GameAssets.TabIcon(1);
        if (index == 3) return GameAssets.WeaponIcon;
        if (index == 5) return GameAssets.TabIcon(1);
        if (index == 6) return GameAssets.TrainingIcon;
        if (index >= 7 && index <= 13) return GameAssets.TabIcon(index - 5);
        return null;
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
        // Pressing end-day (or reaching midnight) immediately closes the shop;
        // customers must never keep entering behind the summary overlay.
        if (Game.gm != null && Game.gm.shopOpen)
        {
            Game.gm.shopOpen = false;
            GameBootstrap.UpdateGateBarrier();
        }
        // Midnight auto-end: make sure the PC panel is on screen first.
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
        if (!historyDetail)
        {
            const int perPage = 7;
            int listMaxPage = Mathf.Max(0, (totalDays - 1) / perPage);
            histPage = Mathf.Clamp(histPage, 0, listMaxPage);
            if (histPageText != null) histPageText.text = totalDays > 0 ? "Gecmis  Sayfa " + (histPage + 1) + "/" + (listMaxPage + 1) : "";
            if (totalDays <= 0)
            {
                GameObject empty = UIKit.Panel(histRoot, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(800f,80f), new Color(0,0,0,.001f), false, false);
                UIKit.Label(empty.transform,"Henuz gecmis yok. Ilk gununu bitir!",22,UIKit.TextDark,TextAnchor.MiddleCenter);
                return;
            }
            int start = histPage * perPage;
            for (int n=0;n<perPage;n++)
            {
                int dayNumber = totalDays - start - n;
                if (dayNumber <= 0) break;
                int listCustomers,listFish,listIncome,listExpense,listReviewCount,listReviewStars;
                if (!Game.gm.GetHistory(dayNumber,out listCustomers,out listFish,out listIncome,out listExpense,out listReviewCount,out listReviewStars)) continue;
                int net=listIncome-listExpense;
                GameObject row=UIKit.Panel(histRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,225f-n*76f),new Vector2(1030f,66f),Color.white,true,true);
                GameObject label=UIKit.Panel(row.transform,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(24,0),new Vector2(790f,58f),new Color(0,0,0,.001f),false,false);
                float avg=listReviewCount>0?(float)listReviewStars/listReviewCount:0f;
                UIKit.Label(label.transform,"GUN "+dayNumber+"   |   Musteri "+listCustomers+"   |   Satis "+listFish+"   |   Yildiz "+avg.ToString("0.0")+"   |   Net "+(net<0?"-$"+B.Money(-net):"$"+B.Money(net)),16,UIKit.TextDark,TextAnchor.MiddleLeft);
                int selected=dayNumber;
                UIKit.Btn(row.transform,new Vector2(410,0),new Vector2(170f,46f),UIKit.Blue,"DETAYI AC",14,delegate{historySelectedDay=selected;historyDetail=true;RebuildHistory();});
            }
            return;
        }
        int maxPage = Mathf.Max(0, totalDays - 1);
        histPage = Mathf.Clamp(histPage, 0, maxPage);
        if (histPageText != null) histPageText.text = "Gun " + historySelectedDay + " detayi";
        if (totalDays <= 0)
        {
            GameObject empty = UIKit.Panel(histRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 80f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(empty.transform, "Henuz gecmis yok. Ilk gununu bitir!", 22, UIKit.TextDark, TextAnchor.MiddleCenter);
            return;
        }
        int day = Mathf.Clamp(historySelectedDay, 1, totalDays);
        int c, f, inc, exp, reviewCount, reviewStars;
        if (Game.gm.GetHistory(day, out c, out f, out inc, out exp, out reviewCount, out reviewStars))
        {
            int net = inc - exp;
            GameObject box = UIKit.Panel(histRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(760f, 540f), new Color(1f, 0.97f, 0.9f), true, true);
            UIKit.Btn(box.transform, new Vector2(-275f, -235f), new Vector2(150f, 42f), UIKit.Blue, "< LISTEYE DON", 13, delegate { historyDetail=false; histPage=0; RebuildHistory(); });
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
    static readonly string[] TechNames = { "PUSULA", "HARITA", "GELISMIS NAVIGASYON", "UZAKTAN KONTROL", "OTOMATIK RADAR", "OTOMATIK DUKKAN", "JENERATOR", "GUVENLIK KAMERALARI", "ALTIN BALIK TAKIBI", "ISCI KAMERASI" };
    static readonly string[] TechDescs = {
        "Sol altta calisan pusula + en yakin baligi gosteren ok",
        "M tusuyla acilan oyun haritasi",
        "Gelismis MINIMAP + en degerli baligi isaretler (mesafeli)",
        "Dukkani PC anasayfadan ac/kapat yapilabilir",
        "Radari uygun baliga otomatik kilitler (oyuncunun yonelmesi yeterlidir)",
        "Dukkani belirlenen saatte otomatik acip kapatir, anasayfadan kontrol edilir",
        "Kesintide tank filtrelerini geri getirir; her seviye daha hizli calisir",
        "Her seviye yeni bir bolgeyi izleyen kamera ekler (Kamera Izleme Masasi gerekir)",
        "Altin balik ortaya ciktiginda ona yonelen hareketli bir isaret gosterir",
        "Kamera masasindan personelin gozunden canli olarak ne yaptigini izletir" };
    static readonly int[] TechDisplayOrder = { 0, 1, 3, 5, 4, 8, 6, 7, 9, 2 };

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
        int total = TechDisplayOrder.Length;
        int maxPage = Mathf.Max(0, (total - 1) / perPage);
        techPage = Mathf.Clamp(techPage, 0, maxPage);
        if (techPageText != null) techPageText.text = "Sayfa " + (techPage + 1) + "/" + (maxPage + 1);
        int start = techPage * perPage;
        for (int n = 0; n < perPage; n++)
        {
            int orderIndex = start + n;
            if (orderIndex >= total) break;
            int idx = TechDisplayOrder[orderIndex];
            float y = 180f - n * 115f;
            GameObject card = UIKit.Panel(techRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1050f, 108f), Color.white, true, true);
            GameObject techBadge = UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(56f, 0f), new Vector2(64f, 64f), new Color(0.3f, 0.7f, 0.75f));
            Sprite techIcon = (idx == 7 || idx == 9) ? GameAssets.CameraIcon : GameAssets.ItemIcon((idx + 3) % 8);
            if (techIcon != null) UIKit.Icon(techBadge.transform, techIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40f, 40f), Color.white);
            GameObject la = UIKit.Panel(card.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(100f, 0f), new Vector2(560f, 96f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(la.transform, TechNames[idx] + "\n" + TechDescs[idx], 16, UIKit.TextDark, TextAnchor.MiddleLeft);
            int spc = idx;
            Button btn = UIKit.Btn(card.transform, new Vector2(380f, 0f), new Vector2(230f, 56f), UIKit.Green, "", 17,
                delegate
                {
                    if (spc == 6 || spc == 7)
                    {
                        if (Game.gm.TryBuyTech(spc)) RefreshAll();
                        else Sfx.Play(Snd.Drop, 0.3f);
                    }
                    else if (!Game.gm.techOwned[spc])
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
            
            if (spc == 6 || spc == 7)
            {
                int level = spc == 6 ? Game.gm.generatorLevel : Game.gm.cameraLevel;
                int cost = spc == 6 ? Game.gm.GeneratorUpgradeCost : Game.gm.CameraUpgradeCost;
                if (spc == 7 && (Game.gm.shopUpg.Length <= 5 || Game.gm.shopUpg[5] <= 0))
                {
                    bt.text = "IZLEME MASASI GEREKLI";
                    btn.image.color = UIKit.Orange;
                }
                else if (level >= 5)
                {
                    bt.text = "SEVIYE 5  MAKS";
                    btn.interactable = false;
                    btn.image.color = new Color(0.65f, 0.65f, 0.65f);
                }
                else
                {
                    bt.text = (level == 0 ? "SATIN AL" : "Sv" + level + " > " + (level + 1)) + "  $" + B.Money(cost);
                    btn.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
            }
            else if (spc == 9 && !Game.gm.techOwned[spc] && (Game.gm.shopUpg.Length <= 5 || Game.gm.shopUpg[5] <= 0))
            {
                bt.text = "IZLEME MASASI GEREKLI";
                btn.image.color = UIKit.Orange;
            }
            else if (spc == 5 && !Game.gm.techOwned[5] && !Game.gm.techOwned[3])
            {
                bt.text = "UZAKTAN KONTROL GEREKLI";
                btn.image.color = UIKit.Orange;
            }
            else if (Game.gm.techOwned[spc])
            {
                if (spc == 3 || spc == 5)
                {
                    // shop open/close + auto scheduling are controlled on the HOME page,
                    // so these cards just show "AKTIF" (no toggle here).
                    bt.text = "AKTIF (anasayfadan)";
                    btn.interactable = false;
                    btn.image.color = new Color(0.68f, 0.68f, 0.68f);
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
        // Every section starts from its first page. This prevents returning to
        // a stale second/third page after visiting a different management tab.
        for (int i = 0; i < cardPagers.Count; i++) ShowCardPage(cardPagers[i], 0);
        tankPage = collPage = histPage = revPage = techPage = trophyPage = 0;
        historyDetail = false;
        shopSubPage = 0;
        if (shopSubPages != null) ShowShopSubPage();
        currentPage = idx;
        for (int i = 0; i < pages.Length; i++)
        {
            bool selected = i == idx;
            pages[i].SetActive(selected);
            if (tabSelectors != null && i < tabSelectors.Length && tabSelectors[i] != null)
                tabSelectors[i].SetActive(selected);
            if (tabButtons != null && i < tabButtons.Length && tabButtons[i] != null)
                tabButtons[i].transform.localScale = selected ? new Vector3(1.06f, 1.06f, 1f) : Vector3.one;
        }
        RefreshAll();
    }

    // ---------- pages ----------
    static string FormatHour(float hour)
    {
        int total = Mathf.RoundToInt(hour * 60f);
        return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
    }

    void BuildHomePage(Transform page)
    {
        GameObject welcome = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(1060f, 92f), UIKit.Blue, true, true);
        homeShopName = UIKit.Label(welcome.transform, "", 28, Color.white, TextAnchor.MiddleLeft, true);
        homeShopName.rectTransform.offsetMin = new Vector2(28f, 0f);
        homeShopName.rectTransform.offsetMax = new Vector2(-250f, 0f);
        UIKit.Btn(welcome.transform, new Vector2(392f, 0f), new Vector2(220f, 52f), UIKit.Orange, "ISMI DEGISTIR", 16, OpenNamingEditor);

        homeCustomers = HomeStat(page, new Vector2(-405f, 125f), "TOPLAM MUSTERI", UIKit.Blue, GameAssets.TabIcon(1));
        homeStars = HomeStat(page, new Vector2(-135f, 125f), "YILDIZ ORTALAMASI", UIKit.Yellow, UIKit.Star());
        homeReviews = HomeStat(page, new Vector2(135f, 125f), "TOPLAM YORUM", UIKit.Purple, GameAssets.TabIcon(8));
        homePopularity = HomeStat(page, new Vector2(405f, 125f), "POPULERLIK", UIKit.Orange, GameAssets.TabIcon(1),
            "Bu sizin populerliginizdir. Dukkan 06:00-02:00 boyunca acik kalirsa bir gunde gelebilecek tahmini en yuksek musteri sayisini gosterir.");

        GameObject valueCard = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -75f), new Vector2(1060f, 180f), new Color(0.9f, 0.98f, 0.9f), true, true);
        UIKit.Icon(valueCard.transform, UIKit.Star(), new Vector2(0f, 0.5f), new Vector2(82f, 0f), new Vector2(100f, 100f), UIKit.Yellow);
        GameObject valueTitle = UIKit.Panel(valueCard.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(155f, -22f), new Vector2(500f, 38f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(valueTitle.transform, "SIRKET PIYASA DEGERI", 22, new Color(0.12f, 0.38f, 0.2f), TextAnchor.MiddleLeft);
        GameObject valueText = UIKit.Panel(valueCard.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(155f, 24f), new Vector2(610f, 88f), new Color(0f, 0f, 0f, 0.001f), false, false);
        homeValue = UIKit.Label(valueText.transform, "", 48, new Color(0.22f, 0.16f, 0.13f), TextAnchor.MiddleLeft);
        GameObject levelText = UIKit.Panel(valueCard.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-35f, 0f), new Vector2(240f, 80f), UIKit.Green, true, false);
        homeLevel = UIKit.Label(levelText.transform, "", 19, Color.white, TextAnchor.MiddleCenter, true);

        // value info: single line in the gap between the value card and the auto panel
        GameObject noteBar = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -196f), new Vector2(1040f, 34f), new Color(1f, 0.95f, 0.82f), true, false);
        Text noteLbl = UIKit.Label(noteBar.transform, "Deger; seviye, gelir, nakit, akvaryumlar, stok, gelistirmeler, teknoloji, dekor, personel ve memnuniyetten hesaplanir.", 13, UIKit.TextDark, TextAnchor.MiddleCenter);
        noteLbl.horizontalOverflow = HorizontalWrapMode.Overflow; // force a single row

        // OTOMATIK DUKKAN: compact, well-filled panel with an on/off checkbox + hour steppers
        homeAutoRoot = UIKit.Panel(page, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 24f), new Vector2(720f, 104f), new Color(0.87f, 0.94f, 1f), true, true);
        // checkbox (enable/disable automatic mode = techEnabled[5])
        Button chk = UIKit.Btn(homeAutoRoot.transform, new Vector2(-318f, 26f), new Vector2(46f, 46f), UIKit.Green, "", 26,
            delegate { Game.gm.techEnabled[5] = !Game.gm.techEnabled[5]; Sfx.Play(Snd.Tick, 0.4f); RefreshHome(); });
        homeAutoCheck = chk.GetComponentInChildren<Text>();
        UIKit.Label(UIKit.Panel(homeAutoRoot.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(72f, -12f), new Vector2(400f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false).transform,
            "OTOMATIK DUKKAN", 20, new Color(0.15f, 0.35f, 0.6f), TextAnchor.MiddleLeft);
        homeAutoHint = UIKit.Label(UIKit.Panel(homeAutoRoot.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(72f, -44f), new Vector2(230f, 24f), new Color(0f, 0f, 0f, 0.001f), false, false).transform,
            "", 13, new Color(0.4f, 0.45f, 0.55f), TextAnchor.MiddleLeft);
        // ACILIS group
        UIKit.Label(UIKit.Panel(homeAutoRoot.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(316f, 62f), new Vector2(130f, 26f), new Color(0f, 0f, 0f, 0.001f), false, false).transform,
            "ACILIS", 15, new Color(0.15f, 0.35f, 0.6f), TextAnchor.MiddleLeft);
        homeOpenText = UIKit.Label(UIKit.Panel(homeAutoRoot.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(410f, 62f), new Vector2(70f, 26f), new Color(0f, 0f, 0f, 0.001f), false, false).transform,
            "", 18, UIKit.TextDark, TextAnchor.MiddleCenter);
        autoOpenMinus = UIKit.Btn(homeAutoRoot.transform, new Vector2(316f, 20f), new Vector2(36f, 32f), UIKit.Blue, "-", 16, delegate { if (Game.gm.autoOpenTime > 5f) { Game.gm.autoOpenTime -= .5f; RefreshHome(); } });
        autoOpenPlus = UIKit.Btn(homeAutoRoot.transform, new Vector2(356f, 20f), new Vector2(36f, 32f), UIKit.Blue, "+", 16, delegate { if (Game.gm.autoOpenTime < Game.gm.autoCloseTime - .5f) { Game.gm.autoOpenTime += .5f; RefreshHome(); } });
        // KAPANIS group
        UIKit.Label(UIKit.Panel(homeAutoRoot.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(486f, 62f), new Vector2(130f, 26f), new Color(0f, 0f, 0f, 0.001f), false, false).transform,
            "KAPANIS", 15, new Color(0.15f, 0.35f, 0.6f), TextAnchor.MiddleLeft);
        homeCloseText = UIKit.Label(UIKit.Panel(homeAutoRoot.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(590f, 62f), new Vector2(70f, 26f), new Color(0f, 0f, 0f, 0.001f), false, false).transform,
            "", 18, UIKit.TextDark, TextAnchor.MiddleCenter);
        autoCloseMinus = UIKit.Btn(homeAutoRoot.transform, new Vector2(500f, 20f), new Vector2(36f, 32f), UIKit.Blue, "-", 16, delegate { if (Game.gm.autoCloseTime > Game.gm.autoOpenTime + .5f) { Game.gm.autoCloseTime -= .5f; RefreshHome(); } });
        autoClosePlus = UIKit.Btn(homeAutoRoot.transform, new Vector2(540f, 20f), new Vector2(36f, 32f), UIKit.Blue, "+", 16, delegate { if (Game.gm.autoCloseTime < 24f) { Game.gm.autoCloseTime += .5f; RefreshHome(); } });

        homeRemoteRoot = UIKit.Panel(page, new Vector2(1f,0f), new Vector2(1f,0f), new Vector2(-25f,24f), new Vector2(280f,104f), new Color(.93f,.96f,1f), true, true);
        Button remote = UIKit.Btn(homeRemoteRoot.transform,Vector2.zero,new Vector2(244f,60f),UIKit.Green,"",17,delegate{
            bool autoActive = Game.gm.techOwned.Length > 5 && Game.gm.techOwned[5] && Game.gm.techEnabled[5];
            if (autoActive) { Game.ui.Toast("Otomatik dukkan acikken manuel kontrol kullanilamaz. Ikisi ayni anda calismaz.", 4f); return; }
            Game.gm.shopOpen=!Game.gm.shopOpen;GameBootstrap.UpdateGateBarrier();Sfx.Play(Snd.ShopToggle,.55f);RefreshHome();});
        homeRemoteText = remote.GetComponentInChildren<Text>();
        refreshers.Add(RefreshHome);
    }

    Text HomeStat(Transform page, Vector2 pos, string title, Color color, Sprite icon, string tooltip = null)
    {
        GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(250f, 180f), Color.white, true, true);
        if (!string.IsNullOrEmpty(tooltip)) card.AddComponent<HoverTip>().tip = tooltip;
        UIKit.Icon(card.transform, icon != null ? icon : UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(58f, 58f), color);
        GameObject titleArea = UIKit.Panel(card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(225f, 32f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(titleArea.transform, title, 17, new Color(0.25f, 0.2f, 0.17f), TextAnchor.MiddleCenter);
        GameObject valueArea = UIKit.Panel(card.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(225f, 54f), new Color(0f, 0f, 0f, 0.001f), false, false);
        return UIKit.Label(valueArea.transform, "", 34, color, TextAnchor.MiddleCenter);
    }

    void RefreshHome()
    {
        if (Game.gm == null || homeShopName == null) return;
        homeShopName.text = string.IsNullOrEmpty(Game.gm.shopName) ? "AKVARYUM DUKKANI" : Game.gm.shopName;
        homeCustomers.text = B.Money(Game.gm.totalCustomers);
        homeStars.text = Game.gm.reviewCount > 0 ? Game.gm.AvgStars.ToString("0.0") + " / 5 ★" : "—";
        homeReviews.text = B.Money(Game.gm.reviewCount);
        if (homePopularity != null) homePopularity.text = B.Money(Game.gm.DailyPopularity);
        homeValue.text = "$" + B.Money(Game.gm.CompanyMarketValue);
        homeLevel.text = "SIRKET SEVIYESI\n" + Game.gm.Level;
        bool remote = Game.gm.techOwned.Length > 3 && Game.gm.techOwned[3];
        bool automatic = Game.gm.techOwned.Length > 5 && Game.gm.techOwned[5];
        if (homeRemoteRoot != null) homeRemoteRoot.SetActive(remote);
        if (homeAutoRoot != null) homeAutoRoot.SetActive(automatic);
        bool autoOn = automatic && Game.gm.techEnabled[5];
        Color grayBtn = new Color(0.68f, 0.68f, 0.68f);
        if (homeAutoCheck != null)
        {
            homeAutoCheck.text = autoOn ? "✔" : "";
            homeAutoCheck.transform.parent.GetComponent<Image>().color = autoOn ? UIKit.Green : grayBtn;
        }
        if (homeAutoHint != null) homeAutoHint.text = autoOn ? "Otomatik mod acik" : "Kapali — manuel kullaniliyor";
        // schedule display + steppers dim while automatic mode is OFF (manual in use)
        Color hourCol = autoOn ? UIKit.TextDark : grayBtn;
        if (homeOpenText != null) { homeOpenText.text = FormatHour(Game.gm.autoOpenTime); homeOpenText.color = hourCol; }
        if (homeCloseText != null) { homeCloseText.text = FormatHour(Game.gm.autoCloseTime); homeCloseText.color = hourCol; }
        if (autoOpenMinus != null) { autoOpenMinus.interactable = autoOn; autoOpenMinus.image.color = autoOn ? UIKit.Blue : grayBtn; }
        if (autoOpenPlus != null) { autoOpenPlus.interactable = autoOn; autoOpenPlus.image.color = autoOn ? UIKit.Blue : grayBtn; }
        if (autoCloseMinus != null) { autoCloseMinus.interactable = autoOn; autoCloseMinus.image.color = autoOn ? UIKit.Blue : grayBtn; }
        if (autoClosePlus != null) { autoClosePlus.interactable = autoOn; autoClosePlus.image.color = autoOn ? UIKit.Blue : grayBtn; }
        // manual AC/KAPAT grays out while automatic mode is ON
        if (homeRemoteText != null)
        {
            homeRemoteText.text = Game.gm.shopOpen ? "DUKKANI KAPAT" : "DUKKANI AC";
            Button rb = homeRemoteText.GetComponentInParent<Button>();
            if (rb != null) rb.image.color = autoOn ? grayBtn : (Game.gm.shopOpen ? UIKit.Red : UIKit.Green);
        }
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
        Text trophyTitle = UIKit.Label(header.transform, "KUPA KOLEKSIYONU", 30, new Color(0.36f, 0.2f, 0.04f), TextAnchor.MiddleCenter, false);
        trophyTitle.fontStyle = FontStyle.Bold;
        trophyTitle.resizeTextForBestFit = false;
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
        const int perPage = 9;
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
            float y = 172f - (n / 3) * 172f;
            GameObject card = UIKit.Panel(trophyRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(335f, 154f), open ? new Color(1f, 0.96f, 0.78f) : new Color(0.72f, 0.73f, 0.77f), true, true);
            UIKit.Icon(card.transform, UIKit.Trophy(), new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(72f, 72f), open ? UIKit.Yellow : new Color(0.32f, 0.34f, 0.4f));
            Color textColor = open ? UIKit.TextDark : new Color(0.24f, 0.25f, 0.3f);
            GameObject titleArea = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -14f), new Vector2(220f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text title = UIKit.Label(titleArea.transform, open ? trophy.title : "KILITLI KUPA", 17, textColor, TextAnchor.MiddleLeft, false);
            title.fontStyle = FontStyle.Bold;
            title.resizeTextForBestFit = false;
            GameObject descArea = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -51f), new Vector2(220f, 48f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text desc = UIKit.Label(descArea.transform, trophy.description, 14, textColor, TextAnchor.MiddleLeft, false);
            desc.resizeTextForBestFit = false;
            GameObject progressArea = UIKit.Panel(card.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(96f, 12f), new Vector2(220f, 32f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text progress = UIKit.Label(progressArea.transform, TrophySystem.Progress(trophy), 14, textColor, TextAnchor.MiddleLeft, false);
            progress.fontStyle = FontStyle.Bold;
            progress.resizeTextForBestFit = false;
        }
    }

    class CardPager
    {
        public GameObject[] roots;
        public Text pageText;
        public int current;
    }

    CardPager CreateCardPager(Transform page, int itemCount)
    {
        CardPager pager = new CardPager();
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(itemCount / 6f));
        pager.roots = new GameObject[pageCount];
        for (int i = 0; i < pageCount; i++)
            pager.roots[i] = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -4f), new Vector2(1090f, 530f), new Color(0f, 0f, 0f, 0.001f), false, false);

        if (pageCount > 1)
        {
            UIKit.Btn(page, new Vector2(-205f, -307f), new Vector2(170f, 46f), UIKit.Blue, "< ONCEKI", 14,
                delegate { ShowCardPage(pager, pager.current - 1); });
            UIKit.Btn(page, new Vector2(205f, -307f), new Vector2(170f, 46f), UIKit.Blue, "SONRAKI >", 14,
                delegate { ShowCardPage(pager, pager.current + 1); });
            GameObject pageArea = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f),
                new Vector2(180f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
            pager.pageText = UIKit.Label(pageArea.transform, "", 15, UIKit.TextDark, TextAnchor.MiddleCenter);
        }
        ShowCardPage(pager, 0);
        cardPagers.Add(pager);
        return pager;
    }

    void ShowCardPage(CardPager pager, int pageIndex)
    {
        if (pager == null || pager.roots == null || pager.roots.Length == 0) return;
        pager.current = Mathf.Clamp(pageIndex, 0, pager.roots.Length - 1);
        for (int i = 0; i < pager.roots.Length; i++) pager.roots[i].SetActive(i == pager.current);
        if (pager.pageText != null) pager.pageText.text = "Sayfa " + (pager.current + 1) + "/" + pager.roots.Length;
    }

    Vector2 PagedCardPosition(int index)
    {
        int local = index % 6;
        return new Vector2(-360f + (local % 3) * 360f, 132f - (local / 3) * 260f);
    }

    Text FitText(Text text, int min, int max)
    {
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = min;
        text.resizeTextMaxSize = max;
        return text;
    }

    void BuildUpgradePage(Transform page)
    {
        Upg[] all = { Upg.Capacity, Upg.TrashCapacity, Upg.MoveSpeed, Upg.SwimSpeed, Upg.Sprint, Upg.RadarSpeed, Upg.RadarRange, Upg.RadarArea };
        Color[] tints = { UIKit.Orange, new Color(0.55f, 0.36f, 0.18f), UIKit.Orange, UIKit.Blue, UIKit.Yellow, UIKit.Red, UIKit.Purple, new Color(0.2f, 0.72f, 0.75f) };
        BuildUpgradeCards(page, "KENDINI GELISTIRME  -  Her sayfada 6 buyuk kart", all, tints);
    }

    void BuildCustomerUpgradePage(Transform page)
    {
        Upg[] all = { Upg.TipChance, Upg.CustSpeed, Upg.ExtraCash };
        Color[] tints = { UIKit.Green, UIKit.Blue, new Color(0.2f, 0.65f, 0.35f) };
        BuildUpgradeCards(page, "MUSTERI GELISTIRME  -  Memnuniyet ve satis gelistirmeleri", all, tints);
    }

    void BuildUpgradeCards(Transform page, string title, Upg[] all, Color[] tints)
    {
        GameObject header = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(1060f, 58f), new Color(1f, 0.92f, 0.8f), true, false);
        UIKit.Label(header.transform, title, 18, new Color(0.55f, 0.3f, 0.1f), TextAnchor.MiddleCenter);
        CardPager pager = CreateCardPager(page, all.Length);
        for (int i = 0; i < all.Length; i++)
        {
            Upg u = all[i];
            GameObject card = UIKit.Panel(pager.roots[i / 6].transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), PagedCardPosition(i), new Vector2(340f, 240f), Color.white, true, true);
            Sprite itemIcon = u == Upg.Sprint ? UIKit.Speed() : GameAssets.ItemIcon((int)u);
            if (itemIcon == null)
            {
                if (u == Upg.Sprint) itemIcon = UIKit.Speed();
                else if (u == Upg.TrashCapacity) itemIcon = UIKit.Poop();
                else if (u == Upg.RadarArea) itemIcon = UIKit.Star();
                else itemIcon = UIKit.Circle();
            }
            if (itemIcon != null)
                UIKit.Icon(card.transform, itemIcon, new Vector2(0f, 1f), new Vector2(46f, -45f), new Vector2(54f, 54f), tints[i]);
            else
                UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0f, 1f), new Vector2(46f, -45f), new Vector2(52f, 52f), tints[i]);
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, -24f), new Vector2(235f, 42f), new Color(0f, 0f, 0f, 0.001f), false, false);
            FitText(UIKit.Label(nameP.transform, UpgInfo.Label(u), 17, UIKit.TextDark, TextAnchor.MiddleLeft), 12, 17);
            GameObject descP = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -79f), new Vector2(285f, 58f), new Color(0f, 0f, 0f, 0.001f), false, false);
            FitText(UIKit.Label(descP.transform, UpgInfo.Desc(u), 14, new Color(0.5f, 0.45f, 0.4f), TextAnchor.MiddleLeft), 11, 14);
            GameObject lvlP = UIKit.Panel(card.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 65f), new Vector2(285f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text lvlT = FitText(UIKit.Label(lvlP.transform, "", 14, tints[i], TextAnchor.MiddleLeft), 11, 14);
            Button btn = UIKit.Btn(card.transform, new Vector2(0f, -88f), new Vector2(300f, 48f), UIKit.Green, "", 16,
                delegate { if (Game.gm.TryBuyUpgrade(u)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Text btnT = btn.GetComponentInChildren<Text>();
            FitText(btnT, 11, 16);
            refreshers.Add(delegate
            {
                int lvl = Game.gm.UpgLevel(u), max = UpgInfo.Max(u);
                lvlT.text = u == Upg.Sprint && lvl == 0 ? "Kilitli  |  Shift henuz etkisiz" : "Seviye " + lvl + " / " + max;
                if (lvl >= max) { btnT.text = "MAKS"; btn.interactable = false; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
                else
                {
                    int cost = Game.gm.UpgCost(u);
                    btnT.text = u == Upg.Sprint && lvl == 0 ? "KILIDI AC  $" + B.Money(cost) : "GELISTIR  $" + B.Money(cost);
                    btn.interactable = Game.gm.Money >= cost;
                    btn.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
            });
        }
    }

    void BuildWeaponPage(Transform page)
    {
        GameObject header = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(1060f, 58f), new Color(1f, 0.9f, 0.88f), true, false);
        UIKit.Label(header.transform, "SILAH GELISTIRME  -  Satin aldigin ekipmanlardan birini aktif sec", 18, new Color(0.55f, 0.18f, 0.14f), TextAnchor.MiddleCenter);
        string[] weaponNames = { "SOPA", "BICAK", "SILAH" };
        string[] playerDesc = { "+2 hasar  |  Orta menzil", "+3 hasar  |  Hizli yakin saldiri", "+5 hasar  |  Uzak menzil" };
        string[] guardDesc = { "+1 guvenlik hasari", "+2 guvenlik hasari", "+4 guvenlik hasari" };
        Color[] colors = { new Color(0.72f, 0.46f, 0.22f), new Color(0.4f, 0.58f, 0.72f), UIKit.Red };
        for (int group = 0; group < 2; group++)
        {
            GameObject band = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, group == 0 ? 220f : -92f), new Vector2(1040f, 40f), group == 0 ? new Color(1f, 0.94f, 0.82f) : new Color(0.9f, 0.88f, 1f), true, false);
            UIKit.Label(band.transform, group == 0 ? "OYUNCU EKIPMANI" : "GUVENLIK PERSONELI EKIPMANI", 16, group == 0 ? new Color(0.55f, 0.3f, 0.1f) : new Color(0.32f, 0.2f, 0.55f), TextAnchor.MiddleCenter);
            for (int w = 0; w < 3; w++)
            {
                int weapon = w;
                bool security = group == 1;
                float x = -360f + w * 360f;
                float y = group == 0 ? 84f : -228f;
                GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(330f, 220f), Color.white, true, true);
                Sprite icon = w == 0 ? UIKit.Baton() : GameAssets.WeaponItemIcon(w);
                if (icon != null) UIKit.Icon(card.transform, icon, new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(62f, 62f), colors[w]);
                GameObject nameArea = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(300f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(nameArea.transform, weaponNames[w], 18, UIKit.TextDark, TextAnchor.MiddleCenter);
                GameObject descArea = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -118f), new Vector2(300f, 32f), new Color(0f, 0f, 0f, 0.001f), false, false);
                UIKit.Label(descArea.transform, security ? guardDesc[w] : playerDesc[w], 13, new Color(0.44f, 0.38f, 0.34f), TextAnchor.MiddleCenter);
                Button button = UIKit.Btn(card.transform, new Vector2(0f, -68f), new Vector2(250f, 48f), UIKit.Green, "", 15,
                    delegate
                    {
                        if (Game.gm.TryBuyOrEquipWeapon(security, weapon)) RefreshAll();
                        else Sfx.Play(Snd.Drop, 0.35f);
                    });
                Text buttonText = button.GetComponentInChildren<Text>();
                FitText(buttonText, 10, 15);
                refreshers.Add(delegate
                {
                    bool owned = security ? Game.gm.securityWeaponsOwned[weapon] : Game.gm.playerWeaponsOwned[weapon];
                    bool active = (security ? Game.gm.activeSecurityWeapon : Game.gm.activePlayerWeapon) == weapon;
                    int cost = security ? GameManager.SecurityWeaponCosts[weapon] : GameManager.PlayerWeaponCosts[weapon];
                    if (security && Game.gm.staffCounts[6] <= 0)
                    {
                        buttonText.text = "ONCE GUVENLIK ISE AL";
                        button.interactable = false;
                        button.image.color = new Color(0.65f, 0.65f, 0.65f);
                    }
                    else if (active)
                    {
                        buttonText.text = "AKTIF";
                        button.interactable = true;
                        button.image.color = UIKit.Green;
                    }
                    else if (owned)
                    {
                        buttonText.text = "AKTIF ET";
                        button.interactable = true;
                        button.image.color = UIKit.Blue;
                    }
                    else
                    {
                        buttonText.text = "SATIN AL  $" + B.Money(cost);
                        button.interactable = Game.gm.Money >= cost;
                        button.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                    }
                });
            }
        }
    }

    void BuildShopUpgradePage(Transform page)
    {
        GameObject header = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(1060f, 64f), new Color(1f, 0.93f, 0.8f), true, false);
        UIKit.Label(header.transform, "DUKKAN ALTYAPISI  -  Kalici koruma ve isletme gelistirmeleri", 19, new Color(0.55f, 0.3f, 0.12f), TextAnchor.MiddleCenter);
        shopSubPages = new GameObject[2];
        for (int p = 0; p < shopSubPages.Length; p++)
        {
            shopSubPages[p] = new GameObject("ShopUpgradePage_" + (p + 1), typeof(RectTransform));
            shopSubPages[p].transform.SetParent(page, false);
            RectTransform rect = shopSubPages[p].GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }
        string[] names = { "DUKKAN SAGLAMLASTIRMA", "HIRSIZ ALARMI", "HIJYEN SISTEMI", "ENERJI VERIMLILIGI", "YONETIM ODASI", "KAMERA IZLEME MASASI" };
        string[] desc = {
            "Her seviye tanklarin depremde kirilmama sansini %12 artirir.",
            "Her seviye hirsizi yakalamak icin 2 saniye daha kazandirir.",
            "Her seviye magazada kaka birakilma ihtimalini %10 azaltir.",
            "Her seviye tuvaletlerin gunluk giderini %8 azaltir.",
            "Her seviye yonetim odasi duvarlarinin yuksekligini artirir.",
            "Guvenlik kameralarini satin almak ve goruntulemek icin gereken masa."
        };
        Color[] colors = { UIKit.Orange, UIKit.Red, UIKit.Green, UIKit.Blue, UIKit.Purple, new Color(0.2f, 0.65f, 0.78f) };
        for (int i = 0; i < names.Length; i++)
        {
            int idx = i;
            float x = -360f + (i % 3) * 360f;
            float y = 135f - (i / 3) * 265f;
            GameObject card = UIKit.Panel(shopSubPages[0].transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(340f, 240f), Color.white, true, true);
            UIKit.Icon(card.transform, GameAssets.ItemIcon((i + 3) % 8), new Vector2(0f, 1f), new Vector2(45f, -48f), new Vector2(52f, 52f), colors[i]);
            GameObject title = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, -30f), new Vector2(245f, 44f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(title.transform, names[i], 15, UIKit.TextDark, TextAnchor.MiddleLeft);
            GameObject detail = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -82f), new Vector2(285f, 62f), new Color(0f, 0f, 0f, 0.001f), false, false);
            UIKit.Label(detail.transform, desc[i], 12, new Color(0.42f, 0.38f, 0.35f), TextAnchor.MiddleLeft);
            GameObject levelArea = UIKit.Panel(card.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 72f), new Vector2(190f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text levelText = UIKit.Label(levelArea.transform, "", 15, colors[i], TextAnchor.MiddleLeft);
            Button buy = UIKit.Btn(card.transform, new Vector2(62f, -76f), new Vector2(190f, 48f), colors[i], "", 13,
                delegate { if (Game.gm.TryBuyShopUpgrade(idx)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.35f); });
            Text buyText = buy.GetComponentInChildren<Text>();
            refreshers.Add(delegate
            {
                int level = Game.gm.shopUpg[idx];
                levelText.text = "Seviye " + level + " / 5";
                if (idx == 5 && Game.gm.shopUpg[4] <= 0)
                {
                    buyText.text = "YONETIM ODASI GEREKLI";
                    buy.interactable = true;
                    buy.image.color = UIKit.Orange;
                }
                else if (level >= 5) { buyText.text = "MAKS SEVIYE"; buy.interactable = false; buy.image.color = new Color(0.65f, 0.65f, 0.65f); }
                else
                {
                    int cost = Game.gm.ShopUpgradeCost(idx);
                    buyText.text = "GELISTIR  $" + B.Money(cost);
                    buy.interactable = true;
                    buy.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
            });
        }

        // page 2: all five fixtures in the same 3-column grid as page 1 (top row + bottom row)
        MakeUtilityCard(shopSubPages[1].transform, new Vector2(-360f, 135f), "KLOZET", "Musterilerin tuvalet ihtiyacini karsilar; kirlenebilir ve tikanabilir.", GameAssets.ItemIcon(6),
            delegate () { return Game.gm.toiletCount; }, 5,
            delegate () { return Game.gm.ToiletUnitCost(); },
            delegate () { return Game.gm.TryBuyToilet(); });
        MakeUtilityCard(shopSubPages[1].transform, new Vector2(0f, 135f), "LAVABO", "El yikama imkani verir ve tuvalet alaninin hijyenini destekler.", GameAssets.ItemIcon(5),
            delegate () { return Game.gm.sinkCount; }, 4,
            delegate () { return Game.gm.SinkUnitCost(); },
            delegate () { return Game.gm.TryBuySink(); });
        MakeDepotCapacityCard(shopSubPages[1].transform, new Vector2(360f, 135f));
        MakeOneTimeShopCard(shopSubPages[1].transform, new Vector2(-360f, -130f), 7,
            "YONETIM ODASI GENISLETMESI", "Yonetim odasini sag tarafa dogru buyutur ve yeni is masalarina yer acar.", UIKit.Purple);
        MakeOneTimeShopCard(shopSubPages[1].transform, new Vector2(0f, -130f), 8,
            "PAZARLAMA MASASI", "15 farkli reklam kampanyasi, okul gezisi ve influencer davetleri yonetilir.", UIKit.Orange);
        UIKit.Btn(page, new Vector2(-190f, -307f), new Vector2(160f, 46f), UIKit.Blue, "< ONCEKI", 14, delegate { shopSubPage = Mathf.Max(0, shopSubPage - 1); ShowShopSubPage(); });
        UIKit.Btn(page, new Vector2(190f, -307f), new Vector2(160f, 46f), UIKit.Blue, "SONRAKI >", 14, delegate { shopSubPage = Mathf.Min(shopSubPages.Length - 1, shopSubPage + 1); ShowShopSubPage(); });
        GameObject pageArea = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(180f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
        shopSubPageText = UIKit.Label(pageArea.transform, "", 15, UIKit.TextDark, TextAnchor.MiddleCenter);
        ShowShopSubPage();
    }

    void MakeOneTimeShopCard(Transform page, Vector2 pos, int index, string label, string description, Color tint)
    {
        GameObject card = UIKit.Panel(page, new Vector2(.5f,.5f), new Vector2(.5f,.5f), pos, new Vector2(340f,240f), Color.white, true, true);
        GameObject badge = UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(.5f,1f), new Vector2(0,-48f), new Vector2(66f,66f), tint);
        UIKit.Icon(badge.transform, index == 8 ? UIKit.Star() : GameAssets.ItemIcon(3), new Vector2(.5f,.5f), Vector2.zero, new Vector2(42f,42f), Color.white);
        GameObject title = UIKit.Panel(card.transform,new Vector2(.5f,1f),new Vector2(.5f,1f),new Vector2(0,-92f),new Vector2(320f,34f),new Color(0,0,0,.001f),false,false);
        FitText(UIKit.Label(title.transform,label,17,UIKit.TextDark,TextAnchor.MiddleCenter),11,17);
        GameObject detail = UIKit.Panel(card.transform,new Vector2(.5f,1f),new Vector2(.5f,1f),new Vector2(0,-132f),new Vector2(310f,48f),new Color(0,0,0,.001f),false,false);
        FitText(UIKit.Label(detail.transform,description,13,new Color(.44f,.39f,.35f),TextAnchor.MiddleCenter),10,13);
        Button button=UIKit.Btn(card.transform,new Vector2(72f,-90f),new Vector2(170f,48f),UIKit.Green,"",13,delegate {
            bool req=index==7 ? Game.gm.shopUpg[4]>0 : true; // marketing desk (8) needs no prerequisite
            if(!req){ Game.ui.Toast("Once YONETIM ODASI gerekli.",3f); return; }
            if(Game.gm.TryBuyShopUpgrade(index))RefreshAll(); else Sfx.Play(Snd.Drop,.3f); });
        Text bt=button.GetComponentInChildren<Text>();
        refreshers.Add(delegate {
            bool owned=Game.gm.shopUpg[index]>0;
            bool requirement=index==7 ? Game.gm.shopUpg[4]>0 : true; // marketing (8) always available
            int cost=Game.gm.ShopUpgradeCost(index);
            if(owned){ bt.text="SAHIPSIN"; button.interactable=false; button.image.color=new Color(.65f,.65f,.65f); }
            else if(!requirement){ bt.text="YONETIM ODASI GEREKLI"; button.interactable=true; button.image.color=UIKit.Orange; }
            else { bt.text="SATIN AL  $"+B.Money(cost); button.interactable=true; button.image.color=Game.gm.Money>=cost?UIKit.Green:new Color(.65f,.65f,.65f); }
        });
    }

    void ShowShopSubPage()
    {
        if (shopSubPages == null) return;
        shopSubPage = Mathf.Clamp(shopSubPage, 0, shopSubPages.Length - 1);
        for (int i = 0; i < shopSubPages.Length; i++) shopSubPages[i].SetActive(i == shopSubPage);
        if (shopSubPageText != null) shopSubPageText.text = "Sayfa " + (shopSubPage + 1) + "/" + shopSubPages.Length;
    }

    void MakeUtilityCard(Transform page, Vector2 pos, string label, string description, Sprite icon,
        System.Func<int> count, int max, System.Func<int> cost, System.Func<bool> buy)
    {
        GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(340f, 240f), Color.white, true, true);
        GameObject badge = UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(66f, 66f), UIKit.Blue);
        if (icon != null) UIKit.Icon(badge.transform, icon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(42f, 42f), Color.white);
        GameObject titleArea = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(320f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(titleArea.transform, label, 18, UIKit.TextDark, TextAnchor.MiddleCenter);
        GameObject detailArea = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -132f), new Vector2(310f, 48f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(detailArea.transform, description, 13, new Color(0.44f, 0.39f, 0.35f), TextAnchor.MiddleCenter);
        GameObject levelArea = UIKit.Panel(card.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(22f, 62f), new Vector2(150f, 36f), new Color(0f, 0f, 0f, 0.001f), false, false);
        Text info = UIKit.Label(levelArea.transform, "", 15, UIKit.Blue, TextAnchor.MiddleLeft);
        Button button = UIKit.Btn(card.transform, new Vector2(72f, -90f), new Vector2(170f, 48f), UIKit.Green, "", 14,
            delegate { if (buy()) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
        Text buttonText = button.GetComponentInChildren<Text>();
        refreshers.Add(delegate
        {
            info.text = "Adet " + count() + " / " + max;
            if (!Game.gm.toiletAreaOpen) { buttonText.text = "ALAN KAPALI"; button.image.color = new Color(0.65f, 0.65f, 0.65f); }
            else if (count() >= max) { buttonText.text = "MAKS"; button.image.color = new Color(0.65f, 0.65f, 0.65f); }
            else
            {
                int price = cost();
                buttonText.text = "SATIN AL  $" + B.Money(price);
                button.image.color = Game.gm.Money >= price ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
            }
        });
    }

    void MakeDepotCapacityCard(Transform page, Vector2 pos)
    {
        GameObject card = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(340f, 240f), Color.white, true, true);
        GameObject badge = UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(66f, 66f), UIKit.Orange);
        Sprite icon = GameAssets.ItemIcon(0);
        if (icon != null) UIKit.Icon(badge.transform, icon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(42f, 42f), Color.white);
        GameObject title = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(320f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(title.transform, "YENI DEPO ALANI", 18, UIKit.TextDark, TextAnchor.MiddleCenter);
        GameObject detail = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -132f), new Vector2(310f, 48f), new Color(0f, 0f, 0f, 0.001f), false, false);
        FitText(UIKit.Label(detail.transform, "Her seviye sahil yoluna 90 kapasiteli yeni bir depo ekler.", 13, new Color(0.44f, 0.39f, 0.35f), TextAnchor.MiddleCenter), 10, 13);
        GameObject levelArea = UIKit.Panel(card.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(22f, 62f), new Vector2(150f, 36f), new Color(0f, 0f, 0f, 0.001f), false, false);
        Text info = UIKit.Label(levelArea.transform, "", 15, UIKit.Orange, TextAnchor.MiddleLeft);
        Button button = UIKit.Btn(card.transform, new Vector2(72f, -90f), new Vector2(170f, 48f), UIKit.Green, "", 14,
            delegate { if (Game.gm.TryBuyShopUpgrade(6)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
        Text buttonText = button.GetComponentInChildren<Text>();
        refreshers.Add(delegate
        {
            int level = Game.gm.shopUpg[6];
            info.text = "Seviye " + level + "/5";
            if (Game.depot == null)
            {
                buttonText.text = "ALAN KAPALI";
                button.interactable = false;
                button.image.color = new Color(0.65f, 0.65f, 0.65f);
            }
            else if (level >= 5)
            {
                buttonText.text = "MAKS";
                button.interactable = false;
                button.image.color = new Color(0.65f, 0.65f, 0.65f);
            }
            else
            {
                int cost = Game.gm.ShopUpgradeCost(6);
                buttonText.text = "YENI DEPO $" + B.Money(cost);
                button.interactable = Game.gm.Money >= cost;
                button.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
            }
        });
    }

    void BuildStaffPage(Transform page)
    {
        // total daily wage banner at the top
        GameObject bannerP = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(1080f, 46f), new Color(0.95f, 0.9f, 0.8f), true, false);
        Text wageT = UIKit.Label(bannerP.transform, "", 18, new Color(0.6f, 0.35f, 0.1f), TextAnchor.MiddleCenter);
        refreshers.Add(delegate
        {
            int due = Game.gm.SalaryDueAtDayEnd();
            wageT.text = "GUN SONUNDA ODENECEK: Maas $" + B.Money(due) + " + Tuvalet $" + B.Money(Game.gm.ToiletDaily()) +
                " = $" + B.Money(due + Game.gm.ToiletDaily()) + "  |  Yeni calisanin bugunku maasi ise alirken kesilir";
        });

        CardPager pager = CreateCardPager(page, StaffInfo.RoleCount);
        for (int r = 0; r < StaffInfo.RoleCount; r++)
        {
            int role = r;
            GameObject card = UIKit.Panel(pager.roots[r / 6].transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), PagedCardPosition(r), new Vector2(340f, 240f), Color.white, true, true);
            GameObject staffBadge = UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0f, 1f), new Vector2(46f, -45f), new Vector2(52f, 52f), StaffColor(role));
            Sprite personIcon = GameAssets.ItemIcon(6);
            if (personIcon != null) UIKit.Icon(staffBadge.transform, personIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(38f, 38f), Color.white);
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, -24f), new Vector2(235f, 42f), new Color(0f, 0f, 0f, 0.001f), false, false);
            FitText(UIKit.Label(nameP.transform, StaffInfo.Names[role], 16, UIKit.TextDark, TextAnchor.MiddleLeft), 11, 16);
            GameObject descP = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -80f), new Vector2(285f, 58f), new Color(0f, 0f, 0f, 0.001f), false, false);
            FitText(UIKit.Label(descP.transform, StaffInfo.Descs[role], 14, new Color(0.5f, 0.45f, 0.4f), TextAnchor.MiddleLeft), 10, 14);
            GameObject cntP = UIKit.Panel(card.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 65f), new Vector2(285f, 30f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text cntT = FitText(UIKit.Label(cntP.transform, "", 15, StaffColor(role), TextAnchor.MiddleLeft), 11, 15);
            Button fireBtn = UIKit.Btn(card.transform, new Vector2(-126f, -88f), new Vector2(44f, 48f), UIKit.Red, "-", 20,
                delegate { if (Game.gm.TryFireStaff(role)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Button btn = UIKit.Btn(card.transform, new Vector2(26f, -88f), new Vector2(244f, 48f), UIKit.Green, "", 14,
                delegate { if (Game.gm.TryHireStaff(role)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Text btnT = btn.GetComponentInChildren<Text>();
            FitText(btnT, 10, 14);
            refreshers.Add(delegate
            {
                int cnt = Game.gm.staffCounts[role], max = StaffInfo.MaxCount[role];
                cntT.text = "Calisan: " + cnt + "/" + max;
                fireBtn.interactable = cnt > 0;
                fireBtn.image.color = cnt > 0 ? UIKit.Red : new Color(0.65f, 0.65f, 0.65f);
                if (cnt >= max) { btnT.text = "MAKS"; btn.interactable = false; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
                else if (role == 2 && !Game.gm.HasDepot()) { btnT.text = "DEPO GEREKLI"; btn.image.color = UIKit.Orange; btn.interactable = true; }
                else if (role == 5 && Game.gm.toiletCount == 0) { btnT.text = "TUVALET GEREKLI"; btn.image.color = UIKit.Orange; btn.interactable = true; }
                else if (role == 8 && Game.gm.generatorLevel <= 0) { btnT.text = "JENERATOR GEREKLI"; btn.image.color = UIKit.Orange; btn.interactable = true; }
                else
                {
                    int hireCost = Game.gm.StaffHireCost(role);
                    btnT.text = "ISE AL  $" + B.Money(hireCost) + " BUGUN";
                    btn.interactable = Game.gm.Money >= hireCost;
                    btn.image.color = Game.gm.Money >= hireCost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
            });
        }

    }

    Color StaffColor(int role)
    {
        Color[] colors = {
            new Color(0.25f, 0.72f, 0.38f), new Color(0.18f, 0.68f, 0.82f), new Color(0.95f, 0.58f, 0.18f), new Color(0.45f, 0.75f, 0.28f),
            new Color(0.16f, 0.5f, 0.9f), new Color(0.65f, 0.42f, 0.88f), new Color(0.18f, 0.25f, 0.38f), new Color(0.95f, 0.72f, 0.18f), new Color(0.95f, 0.62f, 0.1f)
        };
        return colors[Mathf.Clamp(role, 0, colors.Length - 1)];
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

    void BuildStaffTrainingPage(Transform page)
    {
        GameObject banner = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(1080f, 56f), new Color(0.9f, 0.86f, 1f), true, false);
        FitText(UIKit.Label(banner.transform, "PERSONEL EGITIMI  -  Her sayfada 6 buyuk kart", 18, new Color(0.27f, 0.16f, 0.52f), TextAnchor.MiddleCenter), 13, 18);
        string[] benefits = {
            "Daha hizli odeme alir", "Daha hizli yakalar, daha cok balik tasir", "Daha hizli yurur, daha cok kasa tasir",
            "Daha hizli yurur, daha cok cop toplar", "Daha hizli yuzer, daha cok deniz copu toplar", "Tuvaletleri daha hizli temizler",
            "Her seviye vurus basina +1 hasar; daha hizli kosar", "Sahilde daha hizli ve daha uzun sure temizlik yapar", "Jeneratoru daha hizli devreye alir"
        };
        CardPager pager = CreateCardPager(page, StaffInfo.RoleCount + 1);
        for (int r = 0; r < StaffInfo.RoleCount; r++)
        {
            int role = r;
            GameObject card = UIKit.Panel(pager.roots[r / 6].transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), PagedCardPosition(r), new Vector2(340f, 240f), Color.white, true, true);
            GameObject badge = UIKit.Icon(card.transform, UIKit.Circle(), new Vector2(0f, 1f), new Vector2(46f, -45f), new Vector2(52f, 52f), StaffColor(role));
            Sprite icon = GameAssets.ItemIcon(6);
            if (icon != null) UIKit.Icon(badge.transform, icon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(38f, 38f), Color.white);
            GameObject nameArea = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, -24f), new Vector2(235f, 42f), new Color(0f, 0f, 0f, 0.001f), false, false);
            FitText(UIKit.Label(nameArea.transform, StaffInfo.Names[role], 16, UIKit.TextDark, TextAnchor.MiddleLeft, false), 11, 16);
            GameObject benefitArea = UIKit.Panel(card.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -80f), new Vector2(285f, 58f), new Color(0f, 0f, 0f, 0.001f), false, false);
            FitText(UIKit.Label(benefitArea.transform, benefits[role], 14, new Color(0.42f, 0.36f, 0.32f), TextAnchor.MiddleLeft), 10, 14);
            GameObject statArea = UIKit.Panel(card.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 64f), new Vector2(285f, 38f), new Color(0f, 0f, 0f, 0.001f), false, false);
            Text stats = FitText(UIKit.Label(statArea.transform, "", 14, StaffColor(role), TextAnchor.MiddleLeft, false), 10, 14);
            Button train = UIKit.Btn(card.transform, new Vector2(0f, -88f), new Vector2(300f, 48f), UIKit.Green, "", 15,
                delegate { if (Game.gm.TryTrainStaff(role)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.35f); });
            Text trainText = train.GetComponentInChildren<Text>();
            FitText(trainText, 11, 15);
            refreshers.Add(delegate
            {
                int level = Game.gm.staffLevel[role];
                int cap = Game.gm.StaffCapacity(role);
                string capacity = (role >= 1 && role <= 4) || role == 7 ? "  Kapasite " + cap : "";
                stats.text = "Seviye " + level + "/5" + capacity + "\nMaas $" + B.Money(Game.gm.StaffSalary(role)) + "/gun";
                if (Game.gm.staffCounts[role] <= 0)
                {
                    statArea.SetActive(false);
                    trainText.text = "ONCE ISE AL";
                    train.interactable = false;
                    train.image.color = new Color(0.65f, 0.65f, 0.65f);
                }
                else if (level >= 5)
                {
                    statArea.SetActive(true);
                    trainText.text = "MAKS SEVIYE";
                    train.interactable = false;
                    train.image.color = new Color(0.65f, 0.65f, 0.65f);
                }
                else
                {
                    statArea.SetActive(true);
                    int cost = Game.gm.StaffTrainingCost(role);
                    trainText.text = "EGIT  $" + B.Money(cost);
                    train.interactable = true;
                    train.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
            });
        }

        int radarIndex = StaffInfo.RoleCount;
        GameObject radarCard = UIKit.Panel(pager.roots[radarIndex / 6].transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), PagedCardPosition(radarIndex), new Vector2(340f, 240f), Color.white, true, true);
        UIKit.Icon(radarCard.transform, GameAssets.ItemIcon(3) ?? UIKit.Circle(), new Vector2(0f, 1f), new Vector2(46f, -45f), new Vector2(54f, 54f), new Color(0.2f, 0.72f, 0.75f));
        GameObject radarName = UIKit.Panel(radarCard.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, -24f), new Vector2(235f, 42f), new Color(0f, 0f, 0f, 0.001f), false, false);
        FitText(UIKit.Label(radarName.transform, "PERSONEL RADARI", 16, UIKit.TextDark, TextAnchor.MiddleLeft), 11, 16);
        GameObject radarDesc = UIKit.Panel(radarCard.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -80f), new Vector2(285f, 58f), new Color(0f, 0f, 0f, 0.001f), false, false);
        FitText(UIKit.Label(radarDesc.transform, "Avcilarin uzak ve degerli baliklari bulma alanini genisletir", 14, new Color(0.42f, 0.36f, 0.32f), TextAnchor.MiddleLeft), 10, 14);
        GameObject radarStat = UIKit.Panel(radarCard.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 64f), new Vector2(285f, 38f), new Color(0f, 0f, 0f, 0.001f), false, false);
        Text radarStats = FitText(UIKit.Label(radarStat.transform, "", 14, new Color(0.2f, 0.72f, 0.75f), TextAnchor.MiddleLeft), 10, 14);
        Button radarButton = UIKit.Btn(radarCard.transform, new Vector2(0f, -88f), new Vector2(300f, 48f), UIKit.Green, "", 15,
            delegate { if (Game.gm.TryUpgradeStaffRadar()) RefreshAll(); else Sfx.Play(Snd.Drop, 0.35f); });
        Text radarButtonText = radarButton.GetComponentInChildren<Text>();
        refreshers.Add(delegate
        {
            int level = Game.gm.staffRadarLevel;
            radarStats.text = "Seviye " + level + "/5  |  Menzil " + Mathf.RoundToInt(Game.gm.StaffRadarRange) + "m";
            if (level >= 5)
            {
                radarButtonText.text = "MAKS SEVIYE";
                radarButton.interactable = false;
                radarButton.image.color = new Color(0.65f, 0.65f, 0.65f);
            }
            else
            {
                int cost = Game.gm.StaffRadarUpgradeCost;
                radarButtonText.text = "GELISTIR  $" + B.Money(cost);
                radarButton.interactable = Game.gm.Money >= cost;
                radarButton.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
            }
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
        int perPage = 6;
        int total = Game.gm.unlockedCount;
        int maxPage = Mathf.Max(0, (total - 1) / perPage);
        tankPage = Mathf.Clamp(tankPage, 0, maxPage);
        if (tankPageText != null) tankPageText.text = "Sayfa " + (tankPage + 1) + "/" + (maxPage + 1);
        int start = tankPage * perPage;
        for (int n = 0; n < perPage; n++)
        {
            int sp = start + n;
            if (sp >= total) break;
            float y = 230f - n * 94f;
            GameObject row = UIKit.Panel(tankListRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1070f, 84f), Color.white, true, false);
            GameObject tankBadge = UIKit.Icon(row.transform, UIKit.Circle(), new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(76f, 76f), new Color(0.78f, 0.92f, 1f));
            Sprite fishIcon = GameAssets.FishPortrait(sp);
            if (fishIcon != null) UIKit.Icon(tankBadge.transform, fishIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(72f, 58f), Color.white);
            int lvl = Game.gm.tankLevel[sp];
            Tank t = Game.TankOf(sp);
            GameObject la = UIKit.Panel(row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(580f, 70f), new Color(0f, 0f, 0f, 0.001f), false, false);
            int currentSalePrice = Mathf.Max(1, Mathf.RoundToInt(SpeciesInfo.Price(sp) * Game.gm.ExtraCashMult * Game.gm.TankPriceMult(sp) * Game.gm.SaleFactor));
            UIKit.Label(la.transform, SpeciesInfo.Name(sp) + " Akvaryumu   |   Satis fiyati: $" + B.Money(currentSalePrice) + "   |   Maks stok: " + (t != null ? t.Count : 0) + "/" + Game.gm.TankCapacity(sp) +
                "   |   Sv " + lvl + "/5   |   Kazanc x" + Game.gm.TankPriceMult(sp).ToString("0.00"),
                16, UIKit.TextDark, TextAnchor.MiddleLeft);
            int spc = sp;
            Button btn = UIKit.Btn(row.transform, new Vector2(430f, 0f), new Vector2(190f, 56f), UIKit.Green, "", 15,
                delegate { if (Game.gm.TryUpgradeTank(spc)) RefreshAll(); else Sfx.Play(Snd.Drop, 0.3f); });
            Text bt = btn.GetComponentInChildren<Text>();
            if (lvl >= 5) { bt.text = "MAKS"; btn.interactable = false; btn.image.color = new Color(0.65f, 0.65f, 0.65f); }
            else
            {
                int cost = Game.gm.TankUpgCost(sp);
                bt.text = "GELISTIR $" + B.Money(cost);
                btn.interactable = true;
                btn.image.color = Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
            }
        }
    }

    void BuildDecorPage(Transform page)
    {
        GameObject header = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(1060f, 58f), new Color(1f, 0.92f, 0.8f), true, false);
        UIKit.Label(header.transform, "DEKOR  -  Her sayfada 6 buyuk kart", 18, new Color(0.55f, 0.3f, 0.1f), TextAnchor.MiddleCenter);
        CardPager pager = CreateCardPager(page, DecorInfo.Count);
        int[] decorOrder = { 0, 4, 2, 6, 1, 5, 3, 7, 8 };
        for (int i = 0; i < DecorInfo.Count; i++)
        {
            int idx = i < decorOrder.Length ? decorOrder[i] : i;
            GameObject card = UIKit.Panel(pager.roots[i / 6].transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), PagedCardPosition(i), new Vector2(340f, 240f), Color.white, true, true);
            Sprite decorIcon = GameAssets.DecorIcon(idx);
            if (decorIcon != null)
                UIKit.Icon(card.transform, decorIcon, new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(76f, 76f), idx % 2 == 0 ? UIKit.Orange : UIKit.Blue);
            else
                UIKit.Icon(card.transform, UIKit.Star(), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(66f, 66f), idx >= 6 ? UIKit.Blue : new Color(0.8f, 0.55f, 0.3f));
            GameObject nameP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -102f), new Vector2(310f, 48f), new Color(0f, 0f, 0f, 0.001f), false, false);
            FitText(UIKit.Label(nameP.transform, DecorInfo.Names[idx], 18, UIKit.TextDark, TextAnchor.MiddleCenter), 12, 18);
            GameObject detailP = UIKit.Panel(card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -142f), new Vector2(300f, 34f), new Color(0f, 0f, 0f, 0.001f), false, false);
            FitText(UIKit.Label(detailP.transform, idx >= 6 ? "Etkilesimli dis alan donanimi" : "Dukkan ve cevre dekorasyonu", 13, new Color(0.5f, 0.45f, 0.4f), TextAnchor.MiddleCenter), 10, 13);
            Button btn = UIKit.Btn(card.transform, new Vector2(0f, -88f), new Vector2(300f, 48f), UIKit.Green, "", 16,
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
            FitText(bt, 10, 16);
            refreshers.Add(delegate
            {
                if (Game.gm.decorOwned[idx] && idx == 6)
                {
                    int level = Game.gm.rampLevel;
                    bt.text = level >= 5 ? "RAMPA Sv5  MAKS" : "RAMPA Sv" + level + "  GELISTIR $" + B.Money(Game.gm.RampUpgradeCost());
                    int cost = Game.gm.RampUpgradeCost();
                    btn.image.color = level >= 5 ? new Color(0.65f, 0.65f, 0.65f) : Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
                }
                else if (Game.gm.decorOwned[idx] && idx == 7)
                {
                    int level = Game.gm.jetskiLevel;
                    bt.text = level >= 5 ? "JETSKI Sv5  MAKS" : "JETSKI Sv" + level + "  GELISTIR $" + B.Money(Game.gm.JetskiUpgradeCost());
                    int cost = Game.gm.JetskiUpgradeCost();
                    btn.image.color = level >= 5 ? new Color(0.65f, 0.65f, 0.65f) : Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
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
        GameObject title = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(1060f, 60f), new Color(1f, 0.92f, 0.95f), true, false);
        UIKit.Label(title.transform, "BOYA ATOLYESI  -  Once sec, sonra on izle veya satin al", 20, new Color(0.55f, 0.2f, 0.38f), TextAnchor.MiddleCenter);
        GameObject fl = UIKit.Panel(page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(1000f, 44f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(fl.transform, "ZEMIN RENKLERI", 20, UIKit.TextDark, TextAnchor.MiddleCenter);
        for (int i = 0; i < MatLib.FloorStyles.Length; i++)
        {
            int idx = i;
            UIKit.Btn(page, new Vector2(-255f + i * 170f, 150f), new Vector2(140f, 92f), MatLib.FloorStyles[i], "SEC", 14,
                delegate { selectedFloor = idx; RefreshPaintSelection(); Sfx.Play(Snd.Tick, 0.35f); });
        }
        GameObject wl = UIKit.Panel(page, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), new Vector2(1000f, 44f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(wl.transform, "DUVAR RENKLERI", 20, UIKit.TextDark, TextAnchor.MiddleCenter);
        for (int i = 0; i < MatLib.WallStyles.Length; i++)
        {
            int idx = i;
            UIKit.Btn(page, new Vector2(-255f + i * 170f, -80f), new Vector2(140f, 92f), MatLib.WallStyles[i], "SEC", 14,
                delegate { selectedWall = idx; RefreshPaintSelection(); Sfx.Play(Snd.Tick, 0.35f); });
        }
        GameObject selection = UIKit.Panel(page, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 108f), new Vector2(760f, 52f), new Color(1f, 0.95f, 0.82f), true, false);
        paintSelectionText = UIKit.Label(selection.transform, "", 16, UIKit.TextDark, TextAnchor.MiddleCenter);
        paintPreviewButton = UIKit.Btn(page, new Vector2(-145f, -282f), new Vector2(250f, 54f), UIKit.Blue, "ON IZLE", 17, StartPaintPreview);
        paintBuyButton = UIKit.Btn(page, new Vector2(145f, -282f), new Vector2(250f, 54f), UIKit.Green, "SATIN AL", 17, BuySelectedPaint);
        refreshers.Add(RefreshPaintSelection);
    }

    int SelectedFloorStyle { get { return selectedFloor >= 0 ? selectedFloor : Game.gm.floorStyle; } }
    int SelectedWallStyle { get { return selectedWall >= 0 ? selectedWall : Game.gm.wallStyle; } }

    int PaintSelectionCost()
    {
        int cost = 0;
        if (selectedFloor >= 0 && selectedFloor != Game.gm.floorStyle) cost += 250;
        if (selectedWall >= 0 && selectedWall != Game.gm.wallStyle) cost += 250;
        return cost;
    }

    void RefreshPaintSelection()
    {
        if (paintSelectionText == null || Game.gm == null) return;
        int cost = PaintSelectionCost();
        string floor = selectedFloor < 0 ? "Zemin: mevcut" : "Zemin: renk " + (selectedFloor + 1);
        string wall = selectedWall < 0 ? "Duvar: mevcut" : "Duvar: renk " + (selectedWall + 1);
        paintSelectionText.text = floor + "     |     " + wall + "     |     Toplam: $" + B.Money(cost);
        bool changed = cost > 0;
        paintPreviewButton.interactable = selectedFloor >= 0 || selectedWall >= 0;
        paintPreviewButton.image.color = paintPreviewButton.interactable ? UIKit.Blue : new Color(0.65f, 0.65f, 0.65f);
        paintBuyButton.interactable = changed;
        paintBuyButton.image.color = changed && Game.gm.Money >= cost ? UIKit.Green : new Color(0.65f, 0.65f, 0.65f);
        paintBuyButton.GetComponentInChildren<Text>().text = changed ? "SATIN AL  $" + B.Money(cost) : "DEGISIKLIK YOK";
    }

    void StartPaintPreview()
    {
        if (selectedFloor < 0 && selectedWall < 0) return;
        GameBootstrap.PreviewPaint(SelectedFloorStyle, SelectedWallStyle);
        paintPreviewing = true;
        paintPreviewReturning = false;
        paintPreviewReadyAt = Time.unscaledTime + 0.85f;
        root.SetActive(false);
        if (Game.cam != null) Game.cam.ReturnFromPC();
        if (Game.ui != null) Game.ui.Toast("BOYA ON IZLEMESI - PC'ye donmek icin herhangi bir tusa bas", 30f);
    }

    void BuySelectedPaint()
    {
        int cost = PaintSelectionCost();
        if (cost <= 0 || !Game.gm.TrySpend(cost)) { Sfx.Play(Snd.Drop, 0.35f); return; }
        if (selectedFloor >= 0) Game.gm.floorStyle = selectedFloor;
        if (selectedWall >= 0) Game.gm.wallStyle = selectedWall;
        GameBootstrap.ApplyPaint();
        selectedFloor = selectedWall = -1;
        Game.gm.Save();
        Sfx.Play(Snd.Buy);
        RefreshAll();
    }

    System.Collections.IEnumerator ReturnFromPaintPreview()
    {
        paintPreviewReturning = true;
        paintPreviewing = false;
        GameBootstrap.ApplyPaint();
        yield return new WaitForSecondsRealtime(0.1f);
        if (Game.cam != null)
        {
            bool finished = false;
            Game.cam.ZoomToPC(delegate { finished = true; });
            float timeout = Time.unscaledTime + 2f;
            while (!finished && Time.unscaledTime < timeout) yield return null;
        }
        root.SetActive(true);
        root.transform.localScale = Vector3.one;
        ShowPage(9);
        paintPreviewReturning = false;
        if (Game.ui != null) Game.ui.Toast("On izleme kapandi; satin almazsan eski renkler korunur.", 3f);
    }

    // ---------- open/close ----------
    public void Open()
    {
        if (root != null && root.activeSelf) return;
        if (closeButtonText != null) closeButtonText.text = "KAPAT (" + ControlBindings.KeyName(ControlAction.Interact) + ")";
        root.SetActive(true);
        ShowPage(0);
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
        if (paintPreviewing || paintPreviewReturning) return;
        GameBootstrap.ApplyPaint();
        root.SetActive(false);
        if (Game.managerDesk != null) Game.managerDesk.ReleasePlayer();
        if (Game.cam != null) Game.cam.ReturnFromPC();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < refreshers.Count; i++) refreshers[i]();
    }

    void Update()
    {
        if (!IsOpen || Game.gm == null) return;
        if (paintPreviewing)
        {
            if (!paintPreviewReturning && Time.unscaledTime >= paintPreviewReadyAt && Input.anyKeyDown)
                StartCoroutine(ReturnFromPaintPreview());
            return;
        }
        if (paintPreviewReturning) return;
        if (moneyText != null) moneyText.text = "$ " + B.Money(Game.gm.Money);
        if (clockText != null) clockText.text = Game.gm.ClockText();
        if (endDayButton != null)
        {
            bool evening = Game.gm.clockMinutes >= 19f * 60f;
            float pulse = evening ? 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.09f : 1f;
            endDayButton.transform.localScale = Vector3.one * pulse;
        }
        // don't close while typing the shop name (E is a letter!)
        if (namingGo != null && namingGo.activeSelf) return;
        if (summaryGo != null && summaryGo.activeSelf) return;
        if (ControlBindings.Down(ControlAction.Interact) || Input.GetKeyDown(KeyCode.Escape)) Close();
    }
}
