using System.Collections.Generic;
using UnityEngine;

public enum TrophyMetric { MarketValue, Customers, Reviews, Species, Level, Staff }

public struct TrophyDefinition
{
    public string title;
    public string description;
    public TrophyMetric metric;
    public long target;

    public TrophyDefinition(string title, string description, TrophyMetric metric, long target)
    {
        this.title = title;
        this.description = description;
        this.metric = metric;
        this.target = target;
    }
}

// Persistent milestones. Unlock state belongs to the save and the popup queue
// waits until the player is not already using another modal screen.
public class TrophySystem : MonoBehaviour
{
    const string Key = "AT3_Trophy_";
    static readonly Queue<int> pending = new Queue<int>();
    float timer;

    public static readonly TrophyDefinition[] All = {
        new TrophyDefinition("Yukselen Girisim", "$100K sirket degerine ulas", TrophyMetric.MarketValue, 100000),
        new TrophyDefinition("Saglam Temeller", "$500K sirket degerine ulas", TrophyMetric.MarketValue, 500000),
        new TrophyDefinition("Milyonluk Akvaryum", "$1M sirket degerine ulas", TrophyMetric.MarketValue, 1000000),
        new TrophyDefinition("Okyanus Imparatorlugu", "$5M sirket degerine ulas", TrophyMetric.MarketValue, 5000000),
        new TrophyDefinition("Dev Marka", "$10M sirket degerine ulas", TrophyMetric.MarketValue, 10000000),
        new TrophyDefinition("Ilk Kalabalik", "100 musteri agirla", TrophyMetric.Customers, 100),
        new TrophyDefinition("Sehrin Gozdesi", "500 musteri agirla", TrophyMetric.Customers, 500),
        new TrophyDefinition("Turist Akini", "1.000 musteri agirla", TrophyMetric.Customers, 1000),
        new TrophyDefinition("Kulaktan Kulaga", "50 yorum al", TrophyMetric.Reviews, 50),
        new TrophyDefinition("Internet Fenomeni", "250 yorum al", TrophyMetric.Reviews, 250),
        new TrophyDefinition("Koleksiyoncu", "20 balik turu ac", TrophyMetric.Species, 20),
        new TrophyDefinition("Deniz Bilgesi", "40 balik turu ac", TrophyMetric.Species, 40),
        new TrophyDefinition("Tum Okyanus", "80 balik turunun tamamini ac", TrophyMetric.Species, 80),
        new TrophyDefinition("Buyuyen Isletme", "Seviye 10 ol", TrophyMetric.Level, 10),
        new TrophyDefinition("Buyuk Patron", "Toplam 10 calisana ulas", TrophyMetric.Staff, 10)
    };

    public static void ClearRuntime() { pending.Clear(); }
    public static bool IsUnlocked(int index) { return PlayerPrefs.GetInt(Key + index, 0) == 1; }

    public static long Current(TrophyDefinition trophy)
    {
        if (Game.gm == null) return 0;
        switch (trophy.metric)
        {
            case TrophyMetric.MarketValue: return Game.gm.CompanyMarketValue;
            case TrophyMetric.Customers: return Game.gm.totalCustomers;
            case TrophyMetric.Reviews: return Game.gm.reviewCount;
            case TrophyMetric.Species: return Game.gm.unlockedCount;
            case TrophyMetric.Level: return Game.gm.Level;
            case TrophyMetric.Staff: return Game.gm.TotalStaffCount();
            default: return 0;
        }
    }

    public static string Progress(TrophyDefinition trophy)
    {
        long current = System.Math.Min(Current(trophy), trophy.target);
        if (trophy.metric == TrophyMetric.MarketValue)
            return "$" + B.Money(current) + " / $" + B.Money(trophy.target);
        return B.Money(current) + " / " + B.Money(trophy.target);
    }

    void Update()
    {
        if (Game.gm == null) return;
        timer -= Time.unscaledDeltaTime;
        if (timer <= 0f)
        {
            timer = 0.75f;
            CheckUnlocks();
        }
        if (pending.Count > 0 && Game.ui != null && !Game.ui.AnyMenuOpen)
            ShowNext();
    }

    void CheckUnlocks()
    {
        for (int i = 0; i < All.Length; i++)
        {
            if (IsUnlocked(i) || Current(All[i]) < All[i].target) continue;
            PlayerPrefs.SetInt(Key + i, 1);
            pending.Enqueue(i);
        }
        if (pending.Count > 0) PlayerPrefs.Save();
    }

    void ShowNext()
    {
        int index = pending.Dequeue();
        TrophyDefinition trophy = All[index];
        Sfx.Play(Snd.LevelUp, 1f);
        Game.ui.ShowInfo("KUPA KAZANDIN!", "★ " + trophy.title + " ★\n\n" + trophy.description +
            "\n\nTum kupalarina Yonetim Paneli > Kupalar bolumunden ulasabilirsin.");
    }
}
