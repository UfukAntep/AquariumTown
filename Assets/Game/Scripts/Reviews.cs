using System.Collections.Generic;
using UnityEngine;

// Google-style customer reviews. Comments are picked by star rating, so a happy
// (high-satisfaction) day produces good reviews and a bad day produces bad ones.
public static class Reviews
{
    public struct Review { public int stars; public string text; public string author; }

    public static List<Review> recent = new List<Review>();

    static readonly string[] authors = {
        "Ayse K.", "Mehmet T.", "Deniz Y.", "Zeynep A.", "Can B.", "Elif S.",
        "Burak D.", "Selin M.", "Ahmet R.", "Gizem O.", "Emre C.", "Nil U." };

    static readonly string[] good = {
        "Harika bir akvaryum, baliklar cok saglikli!",
        "Cocuklar bayildi, kesinlikle tekrar gelecegiz.",
        "Temiz ve guzel bir yer. Tavsiye ederim!",
        "Cok cesit var, herkese oneririm.",
        "Personel cok ilgili, mekan tertemiz." };
    static readonly string[] mid = {
        "Fena degil ama biraz kalabalikti.",
        "Guzel ama fiyatlar biraz yuksek.",
        "Idare eder, gelistirilebilir.",
        "Baliklar guzeldi ama sira uzundu." };
    static readonly string[] bad = {
        "Cok kirliydi, rahatsiz oldum!",
        "Tuvalet bile yoktu, berbat.",
        "Kasada kimse yoktu, cok bekledim.",
        "Balik yoktu, bos yere geldim." };

    public static void Clear() { recent = new List<Review>(); }

    public static void Add(int stars, float satisfaction)
    {
        Review r = new Review();
        r.stars = stars;
        r.author = authors[Random.Range(0, authors.Length)];
        string[] pool = stars >= 4 ? good : stars >= 3 ? mid : bad;
        r.text = pool[Random.Range(0, pool.Length)];
        recent.Insert(0, r);
        if (recent.Count > 40) recent.RemoveAt(recent.Count - 1);
    }

    public static void AddAttacked()
    {
        Review r = new Review();
        r.stars = 1;
        r.author = authors[Random.Range(0, authors.Length)];
        r.text = "Dukkan sahibi bana vurdu! Bir daha asla gelmem.";
        recent.Insert(0, r);
        if (recent.Count > 40) recent.RemoveAt(recent.Count - 1);
    }
}
