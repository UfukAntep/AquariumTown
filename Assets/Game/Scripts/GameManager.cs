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
    public int[] staffLevel = new int[StaffInfo.RoleCount];
    public int staffRadarLevel;
    // New hires have already paid today's salary up-front.
    public int[] staffHiredToday = new int[StaffInfo.RoleCount];
    public int[] shopUpg = new int[9]; // +7 management room expansion, +8 marketing desk
    public int[] tankLevel = new int[SpeciesInfo.Count];
    public bool[] decorOwned = new bool[DecorInfo.Count];
    public bool[] zoneOpen = new bool[5]; // shop expansion areas (zone 0 free)
    public bool[] techOwned = new bool[10];   // 6 generator, 7 shop cameras, 8 golden-fish tracker, 9 worker camera
    public bool[] techEnabled = new bool[10];
    public int generatorLevel;
    public int cameraLevel;
    public bool[] playerWeaponsOwned = new bool[3];
    public bool[] securityWeaponsOwned = new bool[3];
    public int activePlayerWeapon = -1;
    public int activeSecurityWeapon = -1;
    public bool[] discovered = new bool[SpeciesInfo.Count]; // caught at least once (VERITABANI)
    public int jetskiLevel = 1;
    public int rampLevel = 1;
    public string shopName = "";
    public bool freshStart; // true only on the very first session of a save
    public int language = -1; // -1 = not chosen yet -> show language picker

    // reviews (Google-style): running totals + today's tally
    public int reviewCount, reviewStarSum;
    public int dayReviewCount, dayStarSum;
    public int totalCustomers;
    public int totalFishSold;
    public long totalRevenue;
    public float marketingVisitorBonus;
    public string activeMarketingName = "";

    // day cycle
    public int dayNumber = 1;
    public int dayCustomers, dayFishSold, dayIncome, dayExpense;
    public int floorStyle, wallStyle;
    public int toiletCount;
    public int sinkCount;
    public bool toiletAreaOpen;      // toilet annex unlocked (paid on the spot)
    public bool starterBackAreaOpen; // small rear section of the starting shop
    public bool level5QuakeTutorialDone;
    public bool shopOpen = true;
    public float autoOpenTime = 8f;
    public float autoCloseTime = 22f;

    public const int ToiletAreaCost = 1600;
    public const int StarterBackAreaCost = 1800;
    public const int StarterBackAreaLevel = 4;
    public const int ToiletDemandLevel = 10;   // customers start demanding toilets here
    public const int ToiletDailyCost = 40;     // per unit, deducted each day-end
    public float clockMinutes = 9 * 60f;      // 1 real second = 1 game minute

    public const int DepotCost = 2000;
    public const int ToiletAreaLevel = 6;

    // set when "Yeni Oyun" restarts the scene: skip the main menu once
    public static bool SkipMenuOnce;

    float saveTimer;
    bool hadPositiveMoney;
    bool bankruptcyWarningShown;
    bool bankruptcyTriggered;

    public int Level { get { return unlockedCount; } }
    public int Capacity { get { return 5 + upg[(int)Upg.Capacity] * 3; } }
    public float MoveSpeed { get { return 6.5f * Mathf.Pow(1.08f, upg[(int)Upg.MoveSpeed]); } }
    public float SwimSpeed { get { return 4.5f * Mathf.Pow(1.10f, upg[(int)Upg.SwimSpeed]); } }
    public float CatchTimeMult { get { return Mathf.Pow(0.88f, upg[(int)Upg.RadarSpeed]); } }
    public float RadarRange { get { return 3.5f + 0.5f * upg[(int)Upg.RadarRange]; } }
    public float RadarHalfAngle { get { return 30f + 7f * upg[(int)Upg.RadarArea]; } }
    public int TrashCapacity { get { return 5 + upg[(int)Upg.TrashCapacity]; } }
    public float StaffRadarRange { get { return 80f + 25f * staffRadarLevel; } }
    public float TipChance { get { return 0.06f * upg[(int)Upg.TipChance]; } }
    public float CustSpeedMult { get { return Mathf.Pow(1.10f, upg[(int)Upg.CustSpeed]); } }
    public float ExtraCashMult { get { return 1f + 0.10f * upg[(int)Upg.ExtraCash]; } }
    public bool SprintUnlocked { get { return upg[(int)Upg.Sprint] > 0; } }
    public float SprintMultiplier { get { return SprintUnlocked ? 1.45f + 0.12f * (upg[(int)Upg.Sprint] - 1) : 1f; } }
    public int DepotCapacityBonus { get { return 0; } }
    public int DepotCount { get { return HasDepot() ? 1 + Mathf.Clamp(shopUpg[6], 0, 5) : 0; } }
    public float SaleFactor { get { return Mathf.Lerp(0.4f, 1f, Satisfaction / 100f); } }
    public float PoopChanceMult
    {
        get
        {
            float clean = Game.toilets != null ? Game.toilets.CleanToiletCount + Game.toilets.CleanSinkCount * 0.5f : 0f;
            return Mathf.Pow(0.8f, clean) * Mathf.Pow(0.9f, shopUpg[2]);
        }
    }
    public float QuakeResistance { get { return 0.12f * shopUpg[0]; } }
    public float ThiefTimeBonus { get { return 2f * shopUpg[1]; } }
    public int PlayerWeaponDamage { get { int[] values = { 2, 3, 5 }; return activePlayerWeapon < 0 ? 1 : values[Mathf.Clamp(activePlayerWeapon, 0, 2)]; } }
    public float PlayerWeaponRange { get { float[] values = { 3.4f, 3f, 9f }; return activePlayerWeapon < 0 ? 2.8f : values[Mathf.Clamp(activePlayerWeapon, 0, 2)]; } }
    public int SecurityWeaponBonus { get { int[] values = { 1, 2, 4 }; return activeSecurityWeapon < 0 ? 0 : values[Mathf.Clamp(activeSecurityWeapon, 0, 2)]; } }

    public static readonly int[] PlayerWeaponCosts = { 350, 1200, 4500 };
    public static readonly int[] SecurityWeaponCosts = { 600, 1800, 6000 };

    public bool TryBuyOrEquipWeapon(bool security, int weapon)
    {
        if (weapon < 0 || weapon >= 3) return false;
        // Security equipment only has meaning after at least one guard is
        // hired. Keep this rule in the game model as well as the UI so it
        // cannot be bypassed by another caller or a stale button state.
        if (security && (staffCounts == null || staffCounts.Length <= 6 || staffCounts[6] <= 0)) return false;
        bool[] owned = security ? securityWeaponsOwned : playerWeaponsOwned;
        if (!owned[weapon])
        {
            int cost = security ? SecurityWeaponCosts[weapon] : PlayerWeaponCosts[weapon];
            if (!TrySpend(cost)) return false;
            owned[weapon] = true;
            Sfx.Play(Snd.Buy, 0.8f);
        }
        if (security) activeSecurityWeapon = weapon;
        else activePlayerWeapon = weapon;
        Save();
        return true;
    }

    void Awake()
    {
        Game.gm = this;
        if (upg == null || upg.Length < UpgInfo.Count) System.Array.Resize(ref upg, UpgInfo.Count);
        if (shopUpg == null || shopUpg.Length < 9) System.Array.Resize(ref shopUpg, 9);
        if (techOwned == null || techOwned.Length < TechCosts.Length) Array.Resize(ref techOwned, TechCosts.Length);
        if (techEnabled == null || techEnabled.Length < TechCosts.Length) Array.Resize(ref techEnabled, TechCosts.Length);
        for (int i = 0; i < tankLevel.Length; i++) tankLevel[i] = 1;
        for (int i = 0; i < staffLevel.Length; i++) staffLevel[i] = 1;
        Load();
        // Retire older day-end guide state before the one-time first-night lesson.
        if (PlayerPrefs.GetInt(P + "NightGuideV3Migrated", 0) == 0)
        {
            PlayerPrefs.SetInt(P + "EndDayDeskGuideActive", 0);
            PlayerPrefs.SetInt(P + "NightGuideV3Migrated", 1);
            PlayerPrefs.Save();
        }
        // The starter species is available from the first minute of every save,
        // so its database entry must never appear locked.
        discovered[0] = true;
        hadPositiveMoney = Money > 0;
        language = Loc.Lang;
        if (unlockedCount < 1) unlockedCount = 1;
        zoneOpen[0] = true; // starting area = zone 0 (first 20 tanks)
        gameObject.AddComponent<TrophySystem>();
    }

    public bool ZoneOpen(int b) { return b >= 0 && b < zoneOpen.Length && zoneOpen[b]; }
    public int ZoneCost(int b) { return 500 * (int)Mathf.Pow(4, b); } // 2000, 8000, 32K, 128K

    // ---------- technology ----------
    public static readonly int[] TechCosts = { 600, 4000, 40000, 500, 1500, 3000, 1800, 2500, 3500, 5000 };

    public bool TryBuyTech(int i)
    {
        if (i == 6) return TryUpgradeGenerator();
        if (i == 7) return TryUpgradeCameras();
        if (i < 0 || i >= techOwned.Length || techOwned[i]) return false;
        if (i == 9 && (shopUpg.Length <= 5 || shopUpg[5] <= 0))
        {
            if (Game.ui != null) Game.ui.Toast("Personel kamerasi icin Kamera Izleme Masasi gerekli.", 4f);
            return false;
        }
        // Otomatik Dukkan (5) needs Uzaktan Kontrol (3) first
        if (i == 5 && !techOwned[3])
        {
            if (Game.ui != null) Game.ui.Toast("Otomatik Dukkan icin once Uzaktan Kontrol gerekli.", 4f);
            return false;
        }
        if (!TrySpend(TechCosts[i])) return false;
        techOwned[i] = true;
        techEnabled[i] = true;
        if (i == 1 && Game.ui != null) Game.ui.Toast("Harita alindi! Artik M tusuyla haritaya bakabilirsin.");
        Sfx.Play(Snd.Buy);
        return true;
    }

    public bool TechActive(int i) { return techOwned[i] && techEnabled[i]; }

    public int GeneratorUpgradeCost { get { return generatorLevel == 0 ? TechCosts[6] : 1800 + generatorLevel * 1200; } }
    public int CameraUpgradeCost { get { return cameraLevel == 0 ? TechCosts[7] : 2500 + cameraLevel * 1800; } }

    public bool TryUpgradeGenerator()
    {
        if (generatorLevel >= 5 || !TrySpend(GeneratorUpgradeCost)) return false;
        generatorLevel++;
        techOwned[6] = techEnabled[6] = true;
        GeneratorUnit.Ensure();
        if (Game.generator != null) Game.generator.RefreshVisual();
        Sfx.Play(Snd.Buy, 0.8f);
        return true;
    }

    public bool TryUpgradeCameras()
    {
        if (shopUpg.Length <= 5 || shopUpg[5] <= 0)
        {
            if (Game.ui != null) Game.ui.Toast("Once Dukkan Gelistirme'den Kamera Izleme Masasi almalisin.", 4f);
            return false;
        }
        if (cameraLevel >= 5 || !TrySpend(CameraUpgradeCost)) return false;
        cameraLevel++;
        techOwned[7] = techEnabled[7] = true;
        SecurityCameraSystem.Refresh();
        Sfx.Play(Snd.Buy, 0.8f);
        return true;
    }

    // ---------- day cycle / stats ----------
    public bool CustomersAllowed
    {
        get { return clockMinutes >= 6f * 60f && clockMinutes < 22f * 60f; }
    }

    public void MarkDiscovered(int sp)
    {
        if (sp >= 0 && sp < discovered.Length) discovered[sp] = true;
    }

    public void RegisterIncome(int amount, bool fromCustomer)
    {
        dayIncome += amount;
        totalRevenue += Mathf.Max(0, amount);
        if (fromCustomer)
        {
            dayCustomers++;
            dayFishSold++;
            totalFishSold++;
            TryStartSecondTankTutorial();
        }
    }

    void TryStartSecondTankTutorial()
    {
        if (totalFishSold < 5 || unlockedCount >= 2 || Game.ui == null || Game.player == null) return;
        if (PlayerPrefs.GetInt(P + "SecondTankTutorialShown", 0) == 1) return;
        PlayerPrefs.SetInt(P + "SecondTankTutorialShown", 1);
        PlayerPrefs.Save();
        Game.ui.ShowPausedInfo("YENI AKVARYUM ZAMANI!",
            "Ilk 5 balik satisini tamamladin!\n\n" +
            "Simdi ikinci balik turunun akvaryum alanini acabilirsin. Isareti takip edip alanin uzerinde dur.",
            delegate { Game.ui.BeginSecondTankGuide(); });
    }

    public void RegisterCustomerArrival()
    {
        totalCustomers++;
    }

    public long CompanyMarketValue
    {
        get
        {
            long value = Mathf.Max(0, Money) + totalRevenue * 2L + (long)Level * Level * 4000L;
            for (int i = 0; i < upg.Length; i++) value += (long)upg[i] * UpgInfo.BaseCost((Upg)i) * 4L;
            for (int i = 0; i < shopUpg.Length; i++) value += (long)shopUpg[i] * ShopUpgradeCost(i) * 3L;
            for (int i = 0; i < unlockedCount && i < tankLevel.Length; i++)
            {
                value += (long)tankLevel[i] * SpeciesInfo.Price(i) * 10L;
                Tank tank = Game.TankOf(i);
                if (tank != null) value += (long)tank.Count * SpeciesInfo.Price(i);
            }
            for (int i = 0; i < decorOwned.Length; i++) if (decorOwned[i]) value += DecorInfo.Costs[i];
            for (int i = 1; i < zoneOpen.Length; i++) if (zoneOpen[i]) value += ZoneCost(i);
            for (int i = 0; i < techOwned.Length; i++) if (techOwned[i]) value += TechCosts[i];
            value += (long)TotalDailySalary() * 10L + (long)(toiletCount + sinkCount) * 800L;
            double brandMultiplier = 0.75d + Satisfaction / 200d;
            return System.Math.Max(0L, (long)(value * brandMultiplier));
        }
    }

    public int DailyPopularity
    {
        get
        {
            // Estimated maximum visitors between 06:00 and 02:00 when the
            // shop remains open. Growth is steady, but deliberately bounded.
            float brand = Mathf.Sqrt(Mathf.Max(0f, CompanyMarketValue) / 1000f) * 2f;
            float basePopularity = 90f + Level * 6f + Satisfaction * 0.4f + brand;
            return Mathf.Clamp(Mathf.RoundToInt(basePopularity * (1f + Mathf.Max(0f, marketingVisitorBonus))), 100, 900);
        }
    }

    public bool salariesPaid;

    public int ToiletDaily()
    {
        float discount = 1f - 0.08f * shopUpg[3];
        return Mathf.RoundToInt((toiletCount + sinkCount) * ToiletDailyCost * discount);
    }

    public int ShopUpgradeCost(int index)
    {
        int[] bases = { 900, 700, 650, 600, 800, 2200, 850, 2000, 2600 };
        if (index < 0 || index >= shopUpg.Length) return int.MaxValue;
        return bases[index] * (shopUpg[index] + 1);
    }

    public bool TryBuyShopUpgrade(int index)
    {
        int maxLevel = index >= 7 ? 1 : 5;
        if (index < 0 || index >= shopUpg.Length || shopUpg[index] >= maxLevel) return false;
        if (index == 5 && shopUpg[4] <= 0)
        {
            if (Game.ui != null) Game.ui.Toast("Once Yonetim Odasi gelistirmesini almalisin.", 4f);
            return false;
        }
        if (index == 6 && Game.depot == null)
        {
            if (Game.ui != null) Game.ui.Toast("Once depo alanini satin almalisin.", 4f);
            return false;
        }
        if (index == 7 && shopUpg[4] <= 0)
        {
            if (Game.ui != null) Game.ui.Toast("Once Yonetim Odasi gelistirmesini almalisin.", 4f);
            return false;
        }
        // marketing desk (8) no longer needs the office expansion — buy directly
        if (!TrySpend(ShopUpgradeCost(index))) return false;
        shopUpg[index]++;
        if (index == 6 && Game.world != null)
        {
            int newIndex = Game.depots.Count;
            Depot d = Depot.Create(GameBootstrap.DepotPos(newIndex), Game.world, newIndex);
            d.LoadSaved();
        }
        if (index == 4 || index == 5 || index == 7 || index == 8) ManagementRoomSystem.Refresh();
        Sfx.Play(Snd.Buy, 0.8f);
        return true;
    }

    // Pay staff salaries + toilet running costs once per day (day expense).
    public int ApplySalaries()
    {
        if (salariesPaid) return 0;
        salariesPaid = true;
        int wage = SalaryDueAtDayEnd() + ToiletDaily();
        if (wage > 0)
        {
            // Wages are contractual: the full amount is recorded and may put
            // the shop into debt, which is handled by the bankruptcy system.
            Money -= wage;
            dayExpense += wage;
            if (Game.ui != null) Game.ui.OnMoneyChanged();
            CheckBankruptcy();
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
        marketingVisitorBonus = 0f;
        activeMarketingName = "";
        dayCustomers = 0; dayFishSold = 0; dayIncome = 0; dayExpense = 0;
        dayReviewCount = 0; dayStarSum = 0;
        salariesPaid = false;
        for (int i = 0; i < staffHiredToday.Length; i++) staffHiredToday[i] = 0;
        for (int i = 0; i < Game.staff.Count; i++)
            if (Game.staff[i] != null) Game.staff[i].hiredToday = false;
        clockMinutes = 6f * 60f; // day always resumes at 06:00
        shopOpen = false;
        GameBootstrap.UpdateGateBarrier();
        QuestSystem.GenerateDaily();
        Save();
        if (Game.ui != null) Game.ui.Toast("Gun " + dayNumber + " basladi! Dukkan su anda KAPALI.");
    }

    public bool GetHistory(int day, out int c, out int f, out int inc, out int exp, out int reviews, out int reviewStars)
    {
        c = PlayerPrefs.GetInt(P + "H" + day + "_c", -1);
        f = PlayerPrefs.GetInt(P + "H" + day + "_f", 0);
        inc = PlayerPrefs.GetInt(P + "H" + day + "_i", 0);
        exp = PlayerPrefs.GetInt(P + "H" + day + "_e", 0);
        reviews = PlayerPrefs.GetInt(P + "H" + day + "_rc", 0);
        reviewStars = PlayerPrefs.GetInt(P + "H" + day + "_rs", 0);
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

    public void AddOneStarReview()
    {
        reviewCount++; reviewStarSum++;
        dayReviewCount++; dayStarSum++;
        Reviews.AddAttacked();
    }

    // ---------- money ----------
    public void AddMoney(int v)
    {
        Money += v;
        if (Money > 0) hadPositiveMoney = true;
        if (Game.ui != null) Game.ui.OnMoneyChanged();
        CheckBankruptcy();

    }

    public bool TrySpend(int amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        dayExpense += amount;
        if (Game.ui != null) Game.ui.OnMoneyChanged();
        CheckBankruptcy();
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
            CheckBankruptcy();
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
        if (Game.ui != null)
        {
            Game.ui.RefreshLevel();
            Game.ui.Celebrate(sp);
        }
        GameBootstrap.OnSpeciesUnlocked(sp);
    }

    public bool ClaimLevel5QuakeTutorial()
    {
        if (Level != 5 || level5QuakeTutorialDone) return false;
        level5QuakeTutorialDone = true;
        Save();
        return true;
    }

    // ---------- staff (DAILY SALARY model, no upfront fee) ----------
    public int StaffSalary(int role)
    {
        return Mathf.RoundToInt(StaffInfo.Salary[role] * (1f + 0.15f * (staffLevel[role] - 1)));
    }

    public int StaffHireCost(int role)
    {
        int owned = role >= 0 && role < staffCounts.Length ? staffCounts[role] : 0;
        // The first hire is the cheapest. Recruiting and onboarding each
        // additional worker becomes progressively more expensive.
        return Mathf.RoundToInt(StaffSalary(role) * (1f + 0.30f * owned + 0.08f * owned * owned));
    }

    public int StaffTrainingCost(int role) { return StaffInfo.Salary[role] * (staffLevel[role] + 1) * 4; }
    public int StaffRadarUpgradeCost { get { return Mathf.RoundToInt(500f * Mathf.Pow(2f, staffRadarLevel)); } }
    public bool TryUpgradeStaffRadar()
    {
        if (staffRadarLevel >= 5 || !TrySpend(StaffRadarUpgradeCost)) return false;
        staffRadarLevel++;
        Sfx.Play(Snd.Buy);
        Save();
        return true;
    }
    public float StaffSpeedMultiplier(int role) { return 0.85f + 0.2f * (staffLevel[role] - 1); }
    public float StaffWorkTimeMultiplier(int role) { return 1f / (1f + 0.18f * (staffLevel[role] - 1)); }
    public int StaffCapacity(int role)
    {
        int level = staffLevel[role];
        if (role == 1) return 2 + level * 2;
        if (role == 2) return 3 + level * 2;
        if (role == 3 || role == 5 || role == 7) return 2 + level;
        if (role == 4) return 3 + level * 2;
        return 1;
    }

    public bool TryTrainStaff(int role)
    {
        if (role < 0 || role >= staffLevel.Length || staffCounts[role] <= 0 || staffLevel[role] >= 5) return false;
        if (!TrySpend(StaffTrainingCost(role))) return false;
        staffLevel[role]++;
        Sfx.Play(Snd.LevelUp, 0.75f);
        return true;
    }

    public int TotalDailySalary()
    {
        int t = 0;
        for (int r = 0; r < StaffInfo.RoleCount; r++) t += staffCounts[r] * StaffSalary(r);
        return t;
    }

    public int SalaryDueAtDayEnd()
    {
        int total = 0;
        for (int role = 0; role < StaffInfo.RoleCount; role++)
        {
            int prepaid = role < staffHiredToday.Length ? staffHiredToday[role] : 0;
            total += Mathf.Max(0, staffCounts[role] - prepaid) * StaffSalary(role);
        }
        return total;
    }

    public bool TryHireStaff(int role)
    {
        if (role < 0 || role >= StaffInfo.RoleCount) return false;
        if (staffCounts[role] >= StaffInfo.MaxCount[role]) return false;
        if (role == 2 && !HasDepot()) return false;
        if (role == 5 && toiletCount == 0) return false;
        if (role == 8 && generatorLevel <= 0) return false;
        int todaySalary = StaffHireCost(role);
        if (!TrySpend(todaySalary))
        {
            if (Game.ui != null) Game.ui.Toast("Bugunun maasi icin yeterli paran yok: $" + B.Money(todaySalary));
            return false;
        }
        bool firstEmployee = TotalStaffCount() == 0;
        staffCounts[role]++;
        staffHiredToday[role]++;
        Staff.Create(role, Customer.DoorPos, true);
        Sfx.Play(Snd.Buy);
        if (firstEmployee && PlayerPrefs.GetInt(P + "FirstStaffHireInfoV3", 0) == 0)
        {
            PlayerPrefs.SetInt(P + "FirstStaffHireInfoV3", 1);
            PlayerPrefs.Save();
            if (Game.ui != null) Game.ui.ShowPausedInfo("CALISAN VARDIYASI",
                "Calisanlar her sabah 08:00'de ise gelir ve aksam 21:00'de evlerine gider.\n\n" +
                "Ise alirken bugunun maasi hemen odenir; sonraki maaslar gun sonunda kesilir.");
        }
        Save();
        return true;
    }

    public bool TryFireStaff(int role)
    {
        if (role < 0 || role >= staffCounts.Length || staffCounts[role] <= 0) return false;
        Staff removedWorker = null;
        for (int i = Game.staff.Count - 1; i >= 0; i--)
        {
            Staff worker = Game.staff[i];
            if (worker != null && worker.role == role)
            {
                removedWorker = worker;
                Destroy(worker.gameObject);
                break;
            }
        }
        // A same-day dismissal does not refund the salary already paid.
        if (removedWorker != null && removedWorker.hiredToday && staffHiredToday[role] > 0)
            staffHiredToday[role]--;
        staffCounts[role]--;
        Sfx.Play(Snd.Tick, 0.45f);
        Save();
        return true;
    }

    public int TotalStaffCount()
    {
        int total = 0;
        for (int i = 0; i < staffCounts.Length; i++) total += staffCounts[i];
        return total;
    }

    public bool StaffOnShift { get { return clockMinutes >= 8f * 60f && clockMinutes < 21f * 60f; } }

    public int JetskiUpgradeCost() { return 900 + jetskiLevel * 650; }
    public int RampUpgradeCost() { return 300 + rampLevel * 350; }
    public float JetskiSpeedMultiplier { get { return 2.4f + (jetskiLevel - 1) * 0.48f; } }
    public bool TryUpgradeJetski()
    {
        if (!decorOwned[7] || jetskiLevel >= 5 || !TrySpend(JetskiUpgradeCost())) return false;
        jetskiLevel++;
        if (Game.jetski != null) Game.jetski.ApplyLevelVisual();
        Sfx.Play(Snd.Buy);
        return true;
    }
    public bool TryUpgradeRamp()
    {
        if (!decorOwned[6] || rampLevel >= 5 || !TrySpend(RampUpgradeCost())) return false;
        rampLevel++;
        if (Game.ramp != null) Game.ramp.ApplyLevelVisual();
        Sfx.Play(Snd.Buy);
        return true;
    }

    public int BankruptcyLimit { get { return 500 + Level * 250; } }
    public bool BankruptcyTriggered { get { return bankruptcyTriggered; } }

    void CheckBankruptcy()
    {
        if (bankruptcyTriggered) return;
        if (Money <= -BankruptcyLimit)
        {
            bankruptcyTriggered = true;
            Time.timeScale = 0f;
            if (Game.ui != null)
                Game.ui.ShowInfo("OYUN BITTI - IFLAS",
                    "Borcun -$" + B.Money(-Money) + " oldu.\nBu seviyedeki iflas sinirin -$" + B.Money(BankruptcyLimit) + " idi.\nDukkan kapandi ve ana menuye donuyorsun.",
                    FinishBankruptcy);
            else FinishBankruptcy();
        }
        else if (Money <= 0 && (hadPositiveMoney || Money < 0) && !bankruptcyWarningShown)
        {
            bankruptcyWarningShown = true;
            if (Game.ui != null) Game.ui.Toast("DIKKAT: Borcun -$" + B.Money(BankruptcyLimit) + " olursa iflas edeceksin!", 6f);
        }
    }

    void FinishBankruptcy()
    {
        int selectedLanguage = Loc.Lang;
        int[] controls = ControlBindings.Snapshot();
        PlayerPrefs.DeleteAll();
        Loc.Set(selectedLanguage);
        ControlBindings.Restore(controls);
        Time.timeScale = 1f;
        GameBootstrap.GoToMenu();
    }

    public bool HasDepot() { return Game.depots != null && Game.depots.Count > 0; }

    public bool NeedsToilet
    {
        get { return Level >= ToiletDemandLevel && (Game.toilets == null || !Game.toilets.HasCleanUnit); }
    }

    // ---------- tank upgrades ----------
    public int TankUpgCost(int sp) { return SpeciesInfo.Price(sp) * 12 * tankLevel[sp]; }
    public float TankPriceMult(int sp) { return 1f + 0.15f * (tankLevel[sp] - 1); }
    public int TankCapacity(int sp)
    {
        int[] capacities = { 5, 6, 7, 8, 10 };
        return capacities[Mathf.Clamp(tankLevel[sp] - 1, 0, capacities.Length - 1)];
    }

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
        Sfx.SetNightMood(clockMinutes >= 21f * 60f || clockMinutes < 6f * 60f);
        CheckBankruptcy();

        // Only the first night teaches the danger boundary and desk flow.
        if (dayNumber == 1 && prevClock < 22f * 60f && clockMinutes >= 22f * 60f &&
            PlayerPrefs.GetInt(P + "FirstDayNightWarningV3", 0) == 0 && Game.ui != null && Game.player != null)
        {
            PlayerPrefs.SetInt(P + "FirstDayNightWarningV3", 1);
            PlayerPrefs.Save();
            Game.ui.ShowPausedInfo("GECE DENIZ TEHLIKELI",
                "Saat 22:00'dan sonra deniz tehlikelidir. Gununu bitirmeyi unutma.\n\n" +
                "Gun sonu icin yonetim masandaki bilgisayara git.",
                delegate { if (Game.ui != null) Game.ui.BeginFirstNightDeskGuide(); });
        }

        bool crossedStaffDeparture = prevClock < 21f * 60f && clockMinutes >= 21f * 60f;
        if ((crossedStaffDeparture || clockMinutes >= 21f * 60f || clockMinutes < 6f * 60f) && marketingVisitorBonus > 0f)
        {
            marketingVisitorBonus = 0f;
            activeMarketingName = "";
            if (Game.ui != null) Game.ui.Toast("Pazarlama kampanyasi mesaiyle sona erdi.", 3f);
        }
        if (crossedStaffDeparture && TotalStaffCount() > 0 && PlayerPrefs.GetInt(P + "FirstStaffDepartureInfoV3", 0) == 0)
        {
            PlayerPrefs.SetInt(P + "FirstStaffDepartureInfoV3", 1);
            PlayerPrefs.Save();
            if (Game.ui != null) Game.ui.ShowPausedInfo("CALISANLAR PAYDOS ETTI",
                "Saat 21:00 oldu; calisanlar evlerine gidiyor ve her sabah 08:00'de yeniden geliyor.\n\n" +
                "Saat 22:00'dan sonra deniz tehlikelidir. Islerini 22:00'dan once bitir ve gunu bitirmeyi unutma.");
        }

        // Auto shop tech
        if (techOwned[5] && techEnabled[5])
        {
            float openMin = autoOpenTime * 60f;
            float closeMin = autoCloseTime * 60f;
            
            if (prevClock < openMin && clockMinutes >= openMin)
            {
                if (!shopOpen) { shopOpen = true; GameBootstrap.UpdateGateBarrier(); if(Game.ui != null) Game.ui.Toast("Dukkan otomatik olarak ACILDI!"); }
            }
            else if (prevClock < closeMin && clockMinutes >= closeMin)
            {
                if (shopOpen) { shopOpen = false; GameBootstrap.UpdateGateBarrier(); if(Game.ui != null) Game.ui.Toast("Dukkan otomatik olarak KAPATILDI."); }
            }
        }

        // The same real summary flow opens automatically at 02:00.
        if (Game.pc != null && Game.player != null && CrossedAutoDayEnd())
            Game.pc.OpenDaySummary(true);

        saveTimer += Time.deltaTime;
        if (saveTimer > 10f) { saveTimer = 0f; Save(); }
    }

    bool CrossedAutoDayEnd()
    {
        return prevClock < 2f * 60f && clockMinutes >= 2f * 60f;
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
        PlayerPrefs.SetInt(P + "StarterBackArea", starterBackAreaOpen ? 1 : 0);
        PlayerPrefs.SetInt(P + "Level5QuakeTutorial", level5QuakeTutorialDone ? 1 : 0);
        language = Loc.Lang;
        PlayerPrefs.SetInt(P + "Lang", language);
        PlayerPrefs.SetInt(P + "RevCount", reviewCount);
        PlayerPrefs.SetInt(P + "RevSum", reviewStarSum);
        PlayerPrefs.SetInt(P + "TotalCustomers", totalCustomers);
        PlayerPrefs.SetInt(P + "TotalFishSold", totalFishSold);
        PlayerPrefs.SetString(P + "TotalRevenue", totalRevenue.ToString());
        PlayerPrefs.SetFloat(P + "MarketingBonus", marketingVisitorBonus);
        PlayerPrefs.SetString(P + "MarketingName", activeMarketingName ?? "");
        PlayerPrefs.SetInt(P + "Open", shopOpen ? 1 : 0);
        PlayerPrefs.SetFloat(P + "AutoOpenF", autoOpenTime);
        PlayerPrefs.SetFloat(P + "AutoCloseF", autoCloseTime);
        PlayerPrefs.SetInt(P + "JetskiLevel", jetskiLevel);
        PlayerPrefs.SetInt(P + "RampLevel", rampLevel);
        PlayerPrefs.SetInt(P + "GeneratorLevel", generatorLevel);
        PlayerPrefs.SetInt(P + "CameraLevel", cameraLevel);
        PlayerPrefs.SetInt(P + "StaffRadarLevel", staffRadarLevel);
        PlayerPrefs.SetInt(P + "ActivePlayerWeapon", activePlayerWeapon);
        PlayerPrefs.SetInt(P + "ActiveSecurityWeapon", activeSecurityWeapon);
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.SetInt(P + "PlayerWeapon" + i, playerWeaponsOwned[i] ? 1 : 0);
            PlayerPrefs.SetInt(P + "SecurityWeapon" + i, securityWeaponsOwned[i] ? 1 : 0);
        }
        PlayerPrefs.SetFloat(P + "Clock", clockMinutes);
        for (int i = 0; i < upg.Length; i++) PlayerPrefs.SetInt(P + "Upg" + i, upg[i]);
        for (int i = 0; i < shopUpg.Length; i++) PlayerPrefs.SetInt(P + "ShopUpg" + i, shopUpg[i]);
        for (int i = 0; i < staffCounts.Length; i++) PlayerPrefs.SetInt(P + "Staff" + i, staffCounts[i]);
        for (int i = 0; i < staffHiredToday.Length; i++) PlayerPrefs.SetInt(P + "StaffHiredToday" + i, staffHiredToday[i]);
        for (int i = 0; i < staffLevel.Length; i++) PlayerPrefs.SetInt(P + "StaffLevel" + i, staffLevel[i]);
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
        PlayerPrefs.SetInt(P + "HasDepot", HasDepot() ? 1 : 0);
        for (int d = 0; d < Game.depots.Count; d++)
            if (Game.depots[d] != null)
                for (int i = 0; i < SpeciesInfo.Count; i++)
                    PlayerPrefs.SetInt(P + "Dep" + d + "_" + i, Game.depots[d].counts[i]);
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
        starterBackAreaOpen = PlayerPrefs.GetInt(P + "StarterBackArea", 0) == 1;
        level5QuakeTutorialDone = PlayerPrefs.GetInt(P + "Level5QuakeTutorial", 0) == 1;
        language = Loc.Chosen ? Loc.Lang : PlayerPrefs.GetInt(P + "Lang", -1);
        reviewCount = PlayerPrefs.GetInt(P + "RevCount", 0);
        reviewStarSum = PlayerPrefs.GetInt(P + "RevSum", 0);
        shopOpen = PlayerPrefs.GetInt(P + "Open", 1) == 1;
        autoOpenTime = PlayerPrefs.GetFloat(P + "AutoOpenF", PlayerPrefs.GetInt(P + "AutoOpen", 8));
        autoCloseTime = PlayerPrefs.GetFloat(P + "AutoCloseF", PlayerPrefs.GetInt(P + "AutoClose", 22));
        jetskiLevel = Mathf.Clamp(PlayerPrefs.GetInt(P + "JetskiLevel", 1), 1, 5);
        rampLevel = Mathf.Clamp(PlayerPrefs.GetInt(P + "RampLevel", 1), 1, 5);
        generatorLevel = Mathf.Clamp(PlayerPrefs.GetInt(P + "GeneratorLevel", PlayerPrefs.GetInt(P + "Tech6", 0)), 0, 5);
        cameraLevel = Mathf.Clamp(PlayerPrefs.GetInt(P + "CameraLevel", PlayerPrefs.GetInt(P + "Tech7", 0)), 0, 5);
        staffRadarLevel = Mathf.Clamp(PlayerPrefs.GetInt(P + "StaffRadarLevel", 0), 0, 5);
        activePlayerWeapon = Mathf.Clamp(PlayerPrefs.GetInt(P + "ActivePlayerWeapon", -1), -1, 2);
        activeSecurityWeapon = Mathf.Clamp(PlayerPrefs.GetInt(P + "ActiveSecurityWeapon", -1), -1, 2);
        for (int i = 0; i < 3; i++)
        {
            playerWeaponsOwned[i] = PlayerPrefs.GetInt(P + "PlayerWeapon" + i, 0) == 1;
            securityWeaponsOwned[i] = PlayerPrefs.GetInt(P + "SecurityWeapon" + i, 0) == 1;
        }
        if (activePlayerWeapon >= 0 && !playerWeaponsOwned[activePlayerWeapon]) activePlayerWeapon = -1;
        if (activeSecurityWeapon >= 0 && !securityWeaponsOwned[activeSecurityWeapon]) activeSecurityWeapon = -1;
        clockMinutes = PlayerPrefs.GetFloat(P + "Clock", 9 * 60f);
        for (int i = 0; i < upg.Length; i++) upg[i] = PlayerPrefs.GetInt(P + "Upg" + i, 0);
        for (int i = 0; i < shopUpg.Length; i++) shopUpg[i] = Mathf.Clamp(PlayerPrefs.GetInt(P + "ShopUpg" + i, 0), 0, 5);
        for (int i = 0; i < staffCounts.Length; i++) staffCounts[i] = PlayerPrefs.GetInt(P + "Staff" + i, 0);
        for (int i = 0; i < staffHiredToday.Length; i++)
            staffHiredToday[i] = Mathf.Clamp(PlayerPrefs.GetInt(P + "StaffHiredToday" + i, 0), 0, staffCounts[i]);
        for (int i = 0; i < staffLevel.Length; i++) staffLevel[i] = Mathf.Clamp(PlayerPrefs.GetInt(P + "StaffLevel" + i, 1), 1, 5);
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
        totalCustomers = PlayerPrefs.GetInt(P + "TotalCustomers", -1);
        totalFishSold = PlayerPrefs.GetInt(P + "TotalFishSold", -1);
        long parsedRevenue;
        if (!long.TryParse(PlayerPrefs.GetString(P + "TotalRevenue", ""), out parsedRevenue)) parsedRevenue = -1;
        totalRevenue = parsedRevenue;
        marketingVisitorBonus = Mathf.Max(0f, PlayerPrefs.GetFloat(P + "MarketingBonus", 0f));
        activeMarketingName = PlayerPrefs.GetString(P + "MarketingName", "");
        if (totalCustomers < 0 || totalRevenue < 0 || totalFishSold < 0)
        {
            int historicalCustomers = 0;
            int historicalFishSold = 0;
            long historicalRevenue = 0;
            for (int d = 1; d < dayNumber; d++)
            {
                historicalCustomers += Mathf.Max(0, PlayerPrefs.GetInt(P + "H" + d + "_c", 0));
                historicalFishSold += Mathf.Max(0, PlayerPrefs.GetInt(P + "H" + d + "_f", 0));
                historicalRevenue += Mathf.Max(0, PlayerPrefs.GetInt(P + "H" + d + "_i", 0));
            }
            if (totalCustomers < 0) totalCustomers = historicalCustomers + dayCustomers;
            if (totalFishSold < 0) totalFishSold = historicalFishSold + dayFishSold;
            if (totalRevenue < 0) totalRevenue = historicalRevenue + dayIncome;
        }
        string disc = PlayerPrefs.GetString(P + "Disc", "");
        for (int i = 0; i < discovered.Length && i < disc.Length; i++) discovered[i] = disc[i] == '1';
        freshStart = !PlayerPrefs.HasKey(P + "Money"); // brand-new save
        for (int i = 0; i < SpeciesInfo.Count; i++) tankLevel[i] = PlayerPrefs.GetInt(P + "TLvl" + i, 1);
    }

    public bool LoadHasDepot() { return PlayerPrefs.GetInt(P + "HasDepot", 0) == 1; }
    public int LoadTankCount(int sp) { return PlayerPrefs.GetInt(P + "Cnt" + sp, 0); }
    public int LoadDepotCount(int depotIndex, int i)
    {
        string key = P + "Dep" + depotIndex + "_" + i;
        if (PlayerPrefs.HasKey(key)) return PlayerPrefs.GetInt(key, 0);
        return depotIndex == 0 ? PlayerPrefs.GetInt(P + "Dep" + i, 0) : 0;
    }

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
        int selectedLanguage = Loc.Chosen ? Loc.Lang : -1;
        int[] controls = ControlBindings.Snapshot();
        PlayerPrefs.DeleteAll();
        if (selectedLanguage >= 0) Loc.Set(selectedLanguage);
        ControlBindings.Restore(controls);
        GameBootstrap.LaunchGame();
    }

    void OnApplicationQuit() { Save(); }
    void OnApplicationPause(bool paused) { if (paused) Save(); }
}
