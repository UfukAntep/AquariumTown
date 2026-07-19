using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int Money;
    public float Satisfaction = 80f;          // 0..100, affects sale prices
    public int[] upg = new int[UpgInfo.Count];
    public int unlockedCount = 1;             // species 0..unlockedCount-1 are unlocked; Level == unlockedCount
    public int[] staffCounts = new int[StaffInfo.RoleCount];
    public int[] tankLevel = new int[SpeciesInfo.Count];
    public bool[] decorOwned = new bool[DecorInfo.Count];
    public bool[] zoneOpen = new bool[5]; // shop expansion areas (zone 0 free)
    public bool[] techOwned = new bool[4];   // 0 pusula, 1 harita, 2 navigasyon(minimap), 3 uzaktan kontrol
    public bool[] techEnabled = new bool[4]; // owned techs can be toggled on/off
    public bool[] discovered = new bool[SpeciesInfo.Count]; // caught at least once (VERITABANI)
    public string shopName = "";
    public bool freshStart; // true only on the very first session of a save
    public int language = -1; // -1 = not chosen yet -> show language picker

    // reviews (Google-style): running totals + today's tally
    public int reviewCount, reviewStarSum;
    public int dayReviewCount, dayStarSum;

    // day cycle
    public int dayNumber = 1;
    public int dayCustomers, dayFishSold, dayIncome, dayExpense;
    int lateInfoDay = -1;
    public int floorStyle, wallStyle;
    public int toiletCount;
    public int sinkCount;
    public bool toiletAreaOpen;      // toilet annex unlocked (paid on the spot)
    public bool shopOpen = true;

    public const int ToiletAreaCost = 3000;
    public const int ToiletDemandLevel = 10;   // customers start demanding toilets here
    public const int ToiletDailyCost = 40;     // per unit, deducted each day-end
    public float clockMinutes = 9 * 60f;      // 1 real second = 1 game minute

    public const int DepotCost = 2000;
    public const int ToiletAreaLevel = 6;

    // set when "Yeni Oyun" restarts the scene: skip the main menu once
    public static bool SkipMenuOnce;

    float saveTimer;

    public int Level { get { return unlockedCount; } }
    public int Capacity { get { return 6 + upg[(int)Upg.Capacity] * 3; } }
    public float MoveSpeed { get { return 6.5f * Mathf.Pow(1.08f, upg[(int)Upg.MoveSpeed]); } }
    public float SwimSpeed { get { return 4.5f * Mathf.Pow(1.10f, upg[(int)Upg.SwimSpeed]); } }
    public float CatchTimeMult { get { return Mathf.Pow(0.88f, upg[(int)Upg.RadarSpeed]); } }
    public float RadarRange { get { return 3.5f + 0.5f * upg[(int)Upg.RadarRange]; } }
    public float TipChance { get { return 0.06f * upg[(int)Upg.TipChance]; } }
    public float CustSpeedMult { get { return Mathf.Pow(1.10f, upg[(int)Upg.CustSpeed]); } }
    public float ExtraCashMult { get { return 1f + 0.10f * upg[(int)Upg.ExtraCash]; } }
    public float SaleFactor { get { return Mathf.Lerp(0.4f, 1f, Satisfaction / 100f); } }
    public float PoopChanceMult
    {
        get
        {
            float clean = Game.toilets != null ? Game.toilets.CleanToiletCount + Game.toilets.CleanSinkCount * 0.5f : 0f;
            return Mathf.Pow(0.8f, clean);
        }
    }

    void Awake()
    {
        Game.gm = this;
        for (int i = 0; i < tankLevel.Length; i++) tankLevel[i] = 1;
        Load();
        if (unlockedCount < 1) unlockedCount = 1;
        zoneOpen[0] = true; // starting area = zone 0 (first 20 tanks)
    }

    public bool ZoneOpen(int b) { return b >= 0 && b < zoneOpen.Length && zoneOpen[b]; }
    public int ZoneCost(int b) { return 500 * (int)Mathf.Pow(4, b); } // 2000, 8000, 32K, 128K

    // ---------- technology ----------
    public static readonly int[] TechCosts = { 600, 4000, 40000, 2500 };

    public bool TryBuyTech(int i)
    {
        if (i < 0 || i >= techOwned.Length || techOwned[i]) return false;
        if (!TrySpend(TechCosts[i])) return false;
        techOwned[i] = true;
        techEnabled[i] = true;
        if (i == 1 && Game.ui != null) Game.ui.Toast("Harita alindi! Artik M tusuyla haritaya bakabilirsin.");
        Sfx.Play(Snd.Buy);
        return true;
    }

    public bool TechActive(int i) { return techOwned[i] && techEnabled[i]; }

    // ---------- day cycle / stats ----------
    public bool CustomersAllowed
    {
        get { return clockMinutes >= 8f * 60f && clockMinutes < 22f * 60f; }
    }

    public void MarkDiscovered(int sp)
    {
        if (sp >= 0 && sp < discovered.Length) discovered[sp] = true;
    }

    public void RegisterIncome(int amount, bool fromCustomer)
    {
        dayIncome += amount;
        if (fromCustomer) { dayCustomers++; dayFishSold++; }
    }

    public bool salariesPaid;

    public int ToiletDaily() { return (toiletCount + sinkCount) * ToiletDailyCost; }

    // Pay staff salaries + toilet running costs once per day (day expense).
    public int ApplySalaries()
    {
        if (salariesPaid) return 0;
        salariesPaid = true;
        int wage = TotalDailySalary() + ToiletDaily();
        if (wage > 0)
        {
            int paid = Mathf.Min(wage, Money);
            Money -= paid;
            dayExpense += paid;
            if (Game.ui != null) Game.ui.OnMoneyChanged();
            if (paid < wage && Game.ui != null)
                Game.ui.Toast("Giderleri tam odeyemedin! Personel mutsuz.");
        }
        return wage;
    }

    public void EndDay()
    {
        ApplySalaries();
        // history record
        int d = dayNumber;
        PlayerPrefs.SetInt(P + "H" + d + "_c", dayCustomers);
        PlayerPrefs.SetInt(P + "H" + d + "_f", dayFishSold);
        PlayerPrefs.SetInt(P + "H" + d + "_i", dayIncome);
        PlayerPrefs.SetInt(P + "H" + d + "_e", dayExpense);
        PlayerPrefs.SetInt(P + "H" + d + "_rc", dayReviewCount);
        PlayerPrefs.SetInt(P + "H" + d + "_rs", dayStarSum);
        dayNumber++;
        dayCustomers = 0; dayFishSold = 0; dayIncome = 0; dayExpense = 0;
        dayReviewCount = 0; dayStarSum = 0;
        salariesPaid = false;
        clockMinutes = 6f * 60f; // day always resumes at 06:00
        QuestSystem.GenerateDaily();
        Save();
        if (Game.ui != null) Game.ui.Toast("Gun " + dayNumber + " basladi! Gunaydin!");
    }

    public bool GetHistory(int day, out int c, out int f, out int inc, out int exp)
    {
        c = PlayerPrefs.GetInt(P + "H" + day + "_c", -1);
        f = PlayerPrefs.GetInt(P + "H" + day + "_f", 0);
        inc = PlayerPrefs.GetInt(P + "H" + day + "_i", 0);
        exp = PlayerPrefs.GetInt(P + "H" + day + "_e", 0);
        return c >= 0;
    }

    // ---------- reviews ----------
    public float AvgStars { get { return reviewCount > 0 ? (float)reviewStarSum / reviewCount : 0f; } }
    public float DayAvgStars { get { return dayReviewCount > 0 ? (float)dayStarSum / dayReviewCount : 0f; } }

    // a departing customer leaves a star rating based on their satisfaction
    public int AddReview(float custSatisfaction, bool happy)
    {
        int stars = Mathf.Clamp(Mathf.RoundToInt(custSatisfaction / 20f) + (happy ? 0 : -1), 1, 5);
        reviewCount++; reviewStarSum += stars;
        dayReviewCount++; dayStarSum += stars;
        Reviews.Add(stars, custSatisfaction);
        return stars;
    }

    // ---------- money ----------
    public void AddMoney(int v)
    {
        Money += v;
        if (Game.ui != null) Game.ui.OnMoneyChanged();
    }

    public bool TrySpend(int amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        dayExpense += amount;
        if (Game.ui != null) Game.ui.OnMoneyChanged();
        return true;
    }

    public int SpendTick(int wanted)
    {
        int take = Mathf.Min(wanted, Money);
        if (take > 0)
        {
            Money -= take;
            dayExpense += take;
            if (Game.ui != null) Game.ui.OnMoneyChanged();
        }
        return take;
    }

    // ---------- satisfaction ----------
    public void AddSatisfaction(float d)
    {
        Satisfaction = Mathf.Clamp(Satisfaction + d, 5f, 100f);
        if (Game.ui != null) Game.ui.OnSatisfactionChanged(d);
    }

    // ---------- upgrades ----------
    public int UpgLevel(Upg u) { return upg[(int)u]; }
    public int UpgCost(Upg u) { return Mathf.RoundToInt(UpgInfo.BaseCost(u) * Mathf.Pow(UpgInfo.Mult(u), upg[(int)u])); }

    public bool TryBuyUpgrade(Upg u)
    {
        if (upg[(int)u] >= UpgInfo.Max(u)) return false;
        if (!TrySpend(UpgCost(u))) return false;
        upg[(int)u]++;
        Sfx.Play(Snd.Buy);
        return true;
    }

    // ---------- species / level ----------
    public bool IsUnlocked(int sp) { return sp < unlockedCount; }

    public void UnlockNext()
    {
        if (unlockedCount >= SpeciesInfo.Count) return;
        int sp = unlockedCount;
        unlockedCount++;
        Sfx.Play(Snd.Buy);
        if (Game.ui != null)
        {
            Game.ui.RefreshLevel();
            Game.ui.Celebrate(sp);
        }
        GameBootstrap.OnSpeciesUnlocked(sp);
    }

    // ---------- staff (DAILY SALARY model, no upfront fee) ----------
    public int StaffSalary(int role) { return StaffInfo.Salary[role]; }

    public int TotalDailySalary()
    {
        int t = 0;
        for (int r = 0; r < StaffInfo.RoleCount; r++) t += staffCounts[r] * StaffInfo.Salary[r];
        return t;
    }

    public bool TryHireStaff(int role)
    {
        if (staffCounts[role] >= StaffInfo.MaxCount[role]) return false;
        if (role == 2 && !HasDepot()) return false;
        if (role == 5 && toiletCount == 0) return false;
        // no upfront cost — you just commit to paying the daily salary at day-end
        staffCounts[role]++;
        Staff.Create(role, Customer.DoorPos);
        Sfx.Play(Snd.Buy);
        return true;
    }

    public bool HasDepot() { return Game.depot != null; }

    public bool NeedsToilet
    {
        get { return Level >= ToiletDemandLevel && (Game.toilets == null || !Game.toilets.HasCleanUnit); }
    }

    // ---------- tank upgrades ----------
    public int TankUpgCost(int sp) { return SpeciesInfo.Price(sp) * 12 * tankLevel[sp]; }
    public float TankPriceMult(int sp) { return 1f + 0.15f * (tankLevel[sp] - 1); }

    public bool TryUpgradeTank(int sp)
    {
        if (tankLevel[sp] >= 5) return false;
        if (!TrySpend(TankUpgCost(sp))) return false;
        tankLevel[sp]++;
        Tank t = Game.TankOf(sp);
        if (t != null) t.ApplyLevelDecor();
        Sfx.Play(Snd.Buy);
        return true;
    }

    // ---------- toilets ----------
    public int ToiletUnitCost() { return 800 + toiletCount * 700; }
    public int SinkUnitCost() { return 400 + sinkCount * 350; }

    public bool TryBuyToilet()
    {
        if (!toiletAreaOpen) return false;
        if (toiletCount >= 5) return false;
        if (!TrySpend(ToiletUnitCost())) return false;
        toiletCount++;
        if (Game.toilets != null) Game.toilets.AddUnit(0);
        Sfx.Play(Snd.Buy);
        return true;
    }

    public bool TryBuySink()
    {
        if (!toiletAreaOpen) return false;
        if (sinkCount >= 4) return false;
        if (!TrySpend(SinkUnitCost())) return false;
        sinkCount++;
        if (Game.toilets != null) Game.toilets.AddUnit(1);
        Sfx.Play(Snd.Buy);
        return true;
    }

    // ---------- clock ----------
    float prevClock;
    void Update()
    {
        prevClock = clockMinutes;
        clockMinutes += Time.deltaTime;
        if (clockMinutes >= 1440f) clockMinutes -= 1440f;

        // 22:00 -> shop night: tell the player how to end the day (once per day)
        if (clockMinutes >= 22f * 60f && clockMinutes < 23f * 60f && lateInfoDay != dayNumber && Game.ui != null && Game.player != null)
        {
            lateInfoDay = dayNumber;
            Game.ui.ShowInfo("SAAT 22:00 - DUKKAN KAPANDI!",
                "Bu saatten sonra musteri gelmez.\n" +
                "Kasadaki PC'ye git ve ustteki 'GUNU BITIR' tusuna bas:\n" +
                "gunun ozetini gor, maaslari ode, yeni gune baslat!\n" +
                "(Basmazsan saat 05:00'te gun otomatik biter.)");
        }

        // 05:00 auto-end: if the player never ended the day, do it for them
        if (Game.pc != null && Game.player != null && CrossedFive())
            Game.pc.OpenDaySummary(true);

        saveTimer += Time.deltaTime;
        if (saveTimer > 10f) { saveTimer = 0f; Save(); }
    }

    bool CrossedFive()
    {
        float five = 5f * 60f;
        // detect the moment the clock ticks past 05:00 (wrap-aware)
        if (prevClock < five && clockMinutes >= five && clockMinutes < five + 60f) return true;
        if (prevClock > clockMinutes && five <= clockMinutes && clockMinutes < five + 60f) return true;
        return false;
    }

    public string ClockText()
    {
        int h = (int)(clockMinutes / 60f), m = (int)(clockMinutes % 60f);
        return h.ToString("00") + ":" + m.ToString("00");
    }

    public bool IsNight { get { return clockMinutes < 6 * 60f || clockMinutes > 20 * 60f; } }

    // ---------- cheats (also see DevCheats.cs) ----------
    public void CheatMoney(int amount) { AddMoney(amount); }

    public void CheatSetLevel(int target)
    {
        target = Mathf.Clamp(target, 1, SpeciesInfo.Count);
        while (unlockedCount < target)
        {
            int sp = unlockedCount;
            unlockedCount++;
            GameBootstrap.OnSpeciesUnlocked(sp);
        }
        if (Game.ui != null) Game.ui.RefreshLevel();
    }

    // ---------- persistence ----------
    const string P = "AT3_";
    public static bool SaveExists() { return PlayerPrefs.HasKey(P + "Money"); }

    public void Save()
    {
        PlayerPrefs.SetInt(P + "Money", Money);
        PlayerPrefs.SetFloat(P + "Sat", Satisfaction);
        PlayerPrefs.SetInt(P + "Unlocked", unlockedCount);
        PlayerPrefs.SetInt(P + "Floor", floorStyle);
        PlayerPrefs.SetInt(P + "Wall", wallStyle);
        PlayerPrefs.SetInt(P + "Toilets", toiletCount);
        PlayerPrefs.SetInt(P + "Sinks", sinkCount);
        PlayerPrefs.SetInt(P + "ToiletArea", toiletAreaOpen ? 1 : 0);
        PlayerPrefs.SetInt(P + "Lang", language);
        PlayerPrefs.SetInt(P + "RevCount", reviewCount);
        PlayerPrefs.SetInt(P + "RevSum", reviewStarSum);
        PlayerPrefs.SetInt(P + "Open", shopOpen ? 1 : 0);
        PlayerPrefs.SetFloat(P + "Clock", clockMinutes);
        for (int i = 0; i < upg.Length; i++) PlayerPrefs.SetInt(P + "Upg" + i, upg[i]);
        for (int i = 0; i < staffCounts.Length; i++) PlayerPrefs.SetInt(P + "Staff" + i, staffCounts[i]);
        for (int i = 0; i < decorOwned.Length; i++) PlayerPrefs.SetInt(P + "Decor" + i, decorOwned[i] ? 1 : 0);
        for (int i = 0; i < zoneOpen.Length; i++) PlayerPrefs.SetInt(P + "Zone" + i, zoneOpen[i] ? 1 : 0);
        for (int i = 0; i < techOwned.Length; i++)
        {
            PlayerPrefs.SetInt(P + "Tech" + i, techOwned[i] ? 1 : 0);
            PlayerPrefs.SetInt(P + "TechOn" + i, techEnabled[i] ? 1 : 0);
        }
        PlayerPrefs.SetString(P + "ShopName", shopName);
        PlayerPrefs.SetInt(P + "Day", dayNumber);
        PlayerPrefs.SetInt(P + "DayC", dayCustomers);
        PlayerPrefs.SetInt(P + "DayF", dayFishSold);
        PlayerPrefs.SetInt(P + "DayI", dayIncome);
        PlayerPrefs.SetInt(P + "DayE", dayExpense);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < discovered.Length; i++) sb.Append(discovered[i] ? '1' : '0');
        PlayerPrefs.SetString(P + "Disc", sb.ToString());
        for (int i = 0; i < unlockedCount && i < SpeciesInfo.Count; i++)
        {
            PlayerPrefs.SetInt(P + "TLvl" + i, tankLevel[i]);
            Tank t = Game.TankOf(i);
            if (t != null) PlayerPrefs.SetInt(P + "Cnt" + i, t.Count);
        }
        PlayerPrefs.SetInt(P + "HasDepot", Game.depot != null ? 1 : 0);
        if (Game.depot != null)
            for (int i = 0; i < SpeciesInfo.Count; i++)
                if (Game.depot.counts[i] > 0) PlayerPrefs.SetInt(P + "Dep" + i, Game.depot.counts[i]);
        PlayerPrefs.SetString(P + "LastSave", DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();
    }

    void Load()
    {
        Money = PlayerPrefs.GetInt(P + "Money", 0);
        Satisfaction = PlayerPrefs.GetFloat(P + "Sat", 80f);
        unlockedCount = PlayerPrefs.GetInt(P + "Unlocked", 1);
        floorStyle = PlayerPrefs.GetInt(P + "Floor", 0);
        wallStyle = PlayerPrefs.GetInt(P + "Wall", 0);
        toiletCount = PlayerPrefs.GetInt(P + "Toilets", 0);
        sinkCount = PlayerPrefs.GetInt(P + "Sinks", 0);
        toiletAreaOpen = PlayerPrefs.GetInt(P + "ToiletArea", 0) == 1;
        language = PlayerPrefs.GetInt(P + "Lang", -1);
        reviewCount = PlayerPrefs.GetInt(P + "RevCount", 0);
        reviewStarSum = PlayerPrefs.GetInt(P + "RevSum", 0);
        shopOpen = PlayerPrefs.GetInt(P + "Open", 1) == 1;
        clockMinutes = PlayerPrefs.GetFloat(P + "Clock", 9 * 60f);
        for (int i = 0; i < upg.Length; i++) upg[i] = PlayerPrefs.GetInt(P + "Upg" + i, 0);
        for (int i = 0; i < staffCounts.Length; i++) staffCounts[i] = PlayerPrefs.GetInt(P + "Staff" + i, 0);
        for (int i = 0; i < decorOwned.Length; i++) decorOwned[i] = PlayerPrefs.GetInt(P + "Decor" + i, 0) == 1;
        for (int i = 0; i < zoneOpen.Length; i++) zoneOpen[i] = PlayerPrefs.GetInt(P + "Zone" + i, 0) == 1;
        for (int i = 0; i < techOwned.Length; i++)
        {
            techOwned[i] = PlayerPrefs.GetInt(P + "Tech" + i, 0) == 1;
            techEnabled[i] = PlayerPrefs.GetInt(P + "TechOn" + i, 1) == 1;
        }
        shopName = PlayerPrefs.GetString(P + "ShopName", "");
        dayNumber = PlayerPrefs.GetInt(P + "Day", 1);
        dayCustomers = PlayerPrefs.GetInt(P + "DayC", 0);
        dayFishSold = PlayerPrefs.GetInt(P + "DayF", 0);
        dayIncome = PlayerPrefs.GetInt(P + "DayI", 0);
        dayExpense = PlayerPrefs.GetInt(P + "DayE", 0);
        string disc = PlayerPrefs.GetString(P + "Disc", "");
        for (int i = 0; i < discovered.Length && i < disc.Length; i++) discovered[i] = disc[i] == '1';
        freshStart = !PlayerPrefs.HasKey(P + "Money"); // brand-new save
        for (int i = 0; i < SpeciesInfo.Count; i++) tankLevel[i] = PlayerPrefs.GetInt(P + "TLvl" + i, 1);
    }

    public bool LoadHasDepot() { return PlayerPrefs.GetInt(P + "HasDepot", 0) == 1; }
    public int LoadTankCount(int sp) { return PlayerPrefs.GetInt(P + "Cnt" + sp, 0); }
    public int LoadDepotCount(int i) { return PlayerPrefs.GetInt(P + "Dep" + i, 0); }

    public int ComputeOfflineEarnings()
    {
        if (staffCounts[1] == 0) return 0;
        string s = PlayerPrefs.GetString(P + "LastSave", "");
        long ticks;
        if (!long.TryParse(s, out ticks)) return 0;
        double minutes = (DateTime.UtcNow - new DateTime(ticks)).TotalMinutes;
        if (minutes < 1) return 0;
        return Mathf.Min(5000, (int)(minutes * 15 * staffCounts[1]));
    }

    public static void NewGame()
    {
        PlayerPrefs.DeleteAll();
        GameBootstrap.LaunchGame();
    }

    void OnApplicationQuit() { Save(); }
    void OnApplicationPause(bool paused) { if (paused) Save(); }
}
