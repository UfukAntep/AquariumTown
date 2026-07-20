using System.Collections.Generic;
using UnityEngine;

// Species aquarium with level-based capacity. Reservations count toward capacity
// so several workers cannot overfill the same tank while fish are in flight.
public class Tank : MonoBehaviour
{
    const int VisualCap = 8;
    public int species;
    public bool broken;

    List<Fish> visualFish = new List<Fish>();
    List<Fish> floppers = new List<Fish>();
    int extraCount;
    int incoming;
    TextMesh stockText;
    Transform decorRoot;
    GameObject glassGo, waterGo, shardRoot;
    bool filterRunning = true;
    float repairTime; // repairs are FREE: just stand next to the tank
    int bulletHits, bulletHitsToBreak;

    public int Count { get { return visualFish.Count + extraCount; } }
    public int MaxCapacity { get { return Game.gm != null ? Game.gm.TankCapacity(species) : 5; } }
    public bool HasSpace { get { return !broken && Count + incoming < MaxCapacity; } }
    public bool HasStock { get { return !broken && Count > 0; } }
    public Vector3 FrontPoint { get { return transform.position + new Vector3(0f, 0f, -3f); } }
    const float RepairDuration = 3f;

    Bounds WaterBounds
    {
        get { return new Bounds(transform.position + Vector3.up * 1.6f, new Vector3(2.6f, 0.9f, 2.6f)); }
    }

    public static Tank Create(int sp, Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject("Tank_" + sp);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        Tank tank = go.AddComponent<Tank>();
        tank.species = sp;
        tank.Build();
        Game.tanks.Add(tank);
        return tank;
    }

    void Build()
    {
        Material baseM = MatLib.Get(new Color(0.2f, 0.75f, 0.85f));
        Material ribbon = MatLib.Get(new Color(1f, 0.85f, 0.2f));
        Material glassM = MatLib.Glass(new Color(0.7f, 0.92f, 1f, 0.3f));
        Material waterM = MatLib.Glass(new Color(0.35f, 0.75f, 0.95f, 0.5f));

        B.Prim(PrimitiveType.Cylinder, "Base", transform, new Vector3(0f, 0.45f, 0f), Vector3.zero, new Vector3(4.2f, 0.45f, 4.2f), baseM);
        B.Prim(PrimitiveType.Cylinder, "Ribbon", transform, new Vector3(0f, 0.72f, 0f), Vector3.zero, new Vector3(4.25f, 0.06f, 4.25f), ribbon);
        waterGo = B.Prim(PrimitiveType.Cylinder, "Water", transform, new Vector3(0f, 1.55f, 0f), Vector3.zero, new Vector3(3.7f, 0.62f, 3.7f), waterM);
        glassGo = B.Prim(PrimitiveType.Cylinder, "Glass", transform, new Vector3(0f, 1.7f, 0f), Vector3.zero, new Vector3(3.9f, 0.85f, 3.9f), glassM);

        CapsuleCollider cc = gameObject.AddComponent<CapsuleCollider>();
        cc.center = new Vector3(0f, 1.3f, 0f);
        cc.radius = 2.1f;
        cc.height = 2.6f;

        B.SpeciesBubble(species, transform, new Vector3(-2.6f, 3.6f, 0f)).AddComponent<Bobber>();
        // Keep the tank itself unobstructed: high-resolution glyphs provide
        // clarity without the large dark plate used by the previous revision.
        stockText = B.Text3D("", transform, new Vector3(0f, 3.1f, 0f), 0.12f, Color.white);

        GameObject dr = new GameObject("Decor");
        dr.transform.SetParent(transform, false);
        decorRoot = dr.transform;
        ApplyLevelDecor();
        UpdateText();
    }

    // Decorations grow with tank level (bought on the PC).
    public void ApplyLevelDecor()
    {
        for (int i = decorRoot.childCount - 1; i >= 0; i--) Destroy(decorRoot.GetChild(i).gameObject);
        int lvl = Game.gm != null ? Game.gm.tankLevel[species] : 1;
        Material plant = MatLib.Get(new Color(0.3f, 0.7f, 0.4f));
        Material flower = MatLib.Get(new Color(1f, 0.5f, 0.7f));
        Material coral = MatLib.Get(new Color(1f, 0.6f, 0.3f));
        Material gold = MatLib.Get(new Color(1f, 0.85f, 0.2f));
        if (lvl >= 2)
        {
            B.Prim(PrimitiveType.Capsule, "Plant1", decorRoot, new Vector3(1f, 1.4f, 0.8f), Vector3.zero, new Vector3(0.15f, 0.5f, 0.15f), plant);
            B.Prim(PrimitiveType.Capsule, "Plant2", decorRoot, new Vector3(-1f, 1.4f, -0.7f), Vector3.zero, new Vector3(0.15f, 0.45f, 0.15f), plant);
        }
        if (lvl >= 3)
        {
            B.Prim(PrimitiveType.Sphere, "Flower1", decorRoot, new Vector3(1f, 1.95f, 0.8f), Vector3.zero, Vector3.one * 0.25f, flower);
            B.Prim(PrimitiveType.Cube, "Coral", decorRoot, new Vector3(0f, 1.35f, 1f), new Vector3(0f, 30f, 0f), new Vector3(0.3f, 0.5f, 0.3f), coral);
        }
        if (lvl >= 4)
        {
            B.Prim(PrimitiveType.Sphere, "Pearl", decorRoot, new Vector3(-0.9f, 1.3f, 0.9f), Vector3.zero, Vector3.one * 0.35f, MatLib.Get(new Color(0.95f, 0.95f, 1f)));
            B.Prim(PrimitiveType.Cylinder, "Column", decorRoot, new Vector3(0f, 2.6f, 0f), Vector3.zero, new Vector3(0.15f, 0.25f, 0.15f), gold);
        }
        if (lvl >= 5)
        {
            B.Prim(PrimitiveType.Sphere, "Crown1", decorRoot, new Vector3(0f, 3f, 0f), Vector3.zero, Vector3.one * 0.4f, gold).AddComponent<Bobber>();
            B.Prim(PrimitiveType.Cylinder, "GoldRibbon", decorRoot, new Vector3(0f, 0.95f, 0f), Vector3.zero, new Vector3(4.3f, 0.05f, 4.3f), gold);
        }
    }

    void UpdateText()
    {
        if (stockText == null) return;
        if (broken)
        {
            stockText.text = "KIRIK! Tamir icin yaninda dur";
            stockText.color = new Color(1f, 0.9f, 0.22f);
        }
        else if (!filterRunning)
        {
            stockText.text = "FILTRE DURDU! Jeneratoru calistir";
            stockText.color = new Color(1f, 0.72f, 0.2f);
        }
        else
        {
            int lvl = Game.gm != null ? Game.gm.tankLevel[species] : 1;
            stockText.text = Count + " / " + MaxCapacity + (lvl > 1 ? "  (Sv" + lvl + ")" : "");
            stockText.color = Count == 0 ? new Color(1f, 0.5f, 0.4f) :
                (Count >= MaxCapacity ? new Color(1f, 0.85f, 0.25f) : Color.white);
        }
    }

    public Vector3 RandomWaterPoint()
    {
        Bounds b = WaterBounds;
        Vector2 c = Random.insideUnitCircle * 1.2f;
        return new Vector3(b.center.x + c.x, Random.Range(b.min.y, b.max.y), b.center.z + c.y);
    }

    public bool ReserveSlot()
    {
        if (!HasSpace) return false;
        incoming++;
        return true;
    }

    public void Receive(Fish f)
    {
        incoming = Mathf.Max(0, incoming - 1);
        if (broken) { f.Die(); return; }
        if (Count >= MaxCapacity)
        {
            // Defensive fallback. Normal delivery paths reserve first, but this
            // keeps capacity strict if a future caller forgets to do so.
            if (Game.depot != null && Game.depot.HasSpace) Game.depot.Store(f.species);
            f.Die();
            UpdateText();
            return;
        }
        if (visualFish.Count < VisualCap)
        {
            visualFish.Add(f);
            f.SetInTank(WaterBounds);
        }
        else
        {
            extraCount++;
            Destroy(f.gameObject);
        }
        UpdateText();
        Sfx.Play(Snd.Drop, 0.4f);
        TryTeachShopOpening();
    }

    void TryTeachShopOpening()
    {
        if (species != 0 || Game.gm == null || Game.ui == null) return;
        if (PlayerPrefs.GetInt("AT3_FirstTankShopGuideShown", 0) == 1) return;
        PlayerPrefs.SetInt("AT3_FirstTankShopGuideShown", 1);
        if (Game.gm.shopOpen)
        {
            PlayerPrefs.SetInt("AT3_FirstTankShopGuideDone", 1);
            PlayerPrefs.Save();
            return;
        }
        PlayerPrefs.Save();
        Game.ui.ShowPausedInfo("DUKKANI ACMA ZAMANI!",
            "Ilk baligin akvaryumda! Musterilerin gelebilmesi icin kapidaki AC / KAPAT tabelasindan dukkani ac.\n\n" +
            "Tabelayi gosteren isareti takip et ve yaninda E'ye bas.",
            delegate { Game.ui.BeginShopGateGuide(); });
    }

    public void AddSaved(int n)
    {
        n = Mathf.Min(n, MaxCapacity);
        for (int i = 0; i < n; i++)
        {
            if (visualFish.Count < VisualCap)
            {
                Fish f = Fish.Create(species, RandomWaterPoint());
                f.SetInTank(WaterBounds);
                visualFish.Add(f);
            }
            else extraCount++;
        }
        UpdateText();
    }

    public int AddCount(int n)
    {
        int accepted = Mathf.Clamp(n, 0, Mathf.Max(0, MaxCapacity - Count));
        extraCount += accepted;
        UpdateText();
        return accepted;
    }

    public int TakeForCustomer()
    {
        if (broken) return 0;
        visualFish.RemoveAll(x => x == null);
        if (Count == 0) return 0;
        if (extraCount > 0) extraCount--;
        else
        {
            Fish f = visualFish[visualFish.Count - 1];
            visualFish.RemoveAt(visualFish.Count - 1);
            Destroy(f.gameObject);
        }
        UpdateText();
        float price = SpeciesInfo.Price(species) * Game.gm.ExtraCashMult * Game.gm.TankPriceMult(species) * Game.gm.SaleFactor;
        return Mathf.Max(1, Mathf.RoundToInt(price));
    }

    // ---------- earthquake damage ----------
    public void Break()
    {
        if (broken) return;
        broken = true;
        Sfx.Play(Snd.GlassBreak, 0.9f);
        repairTime = 0f;
        glassGo.SetActive(false);
        if (waterGo != null) waterGo.SetActive(false);
        SpawnGlassShards();
        // fish spill on the floor and flop
        visualFish.RemoveAll(x => x == null);
        int spill = Mathf.Min(Count, VisualCap);
        extraCount = Mathf.Max(0, Count - visualFish.Count); // keep bookkeeping
        for (int i = 0; i < spill && visualFish.Count > 0; i++)
        {
            Fish f = visualFish[visualFish.Count - 1];
            visualFish.RemoveAt(visualFish.Count - 1);
            Vector3 g = transform.position + new Vector3(Random.Range(-3.5f, 3.5f), 0.25f, Random.Range(-4.5f, -2.5f));
            f.SetFlop(g);
            floppers.Add(f);
        }
        extraCount = 0; // the rest escaped/lost
        UpdateText();
    }

    public bool HitByBullet()
    {
        if (broken) return false;
        if (bulletHitsToBreak <= 0) bulletHitsToBreak = Random.Range(1, 3);
        bulletHits++;
        if (bulletHits >= bulletHitsToBreak)
        {
            Break();
            return true;
        }
        if (Game.ui != null) Game.ui.Toast(SpeciesInfo.Name(species) + " akvaryumu catladi! Bir mermi daha kirabilir.", 3f);
        Sfx.Play(Snd.Crash, 0.55f);
        return false;
    }

    public void SetFilterRunning(bool running)
    {
        filterRunning = running;
        UpdateText();
    }

    public void LoseFishFromFilterFailure()
    {
        if (broken || Count <= 0) return;
        visualFish.RemoveAll(f => f == null);
        if (extraCount > 0) extraCount--;
        else if (visualFish.Count > 0)
        {
            Fish fish = visualFish[visualFish.Count - 1];
            visualFish.RemoveAt(visualFish.Count - 1);
            if (fish != null) fish.Die();
        }
        UpdateText();
        if (Game.ui != null) Game.ui.Toast(SpeciesInfo.Name(species) + " akvaryumunda filtresizlikten bir balik oldu!", 4f);
    }

    void SpawnGlassShards()
    {
        if (shardRoot != null) Destroy(shardRoot);
        shardRoot = new GameObject("ShatteredGlass");
        shardRoot.transform.SetParent(transform, false);
        Material glass = MatLib.Glass(new Color(0.68f, 0.9f, 1f, 0.62f));
        const int shardCount = 22;
        for (int i = 0; i < shardCount; i++)
        {
            float angle = i * Mathf.PI * 2f / shardCount + Random.Range(-0.12f, 0.12f);
            Vector3 local = new Vector3(Mathf.Cos(angle) * 1.85f, Random.Range(0.85f, 2.5f), Mathf.Sin(angle) * 1.85f);
            GameObject shard = B.Prim(PrimitiveType.Cube, "GlassShard_" + i, shardRoot.transform, local,
                new Vector3(Random.Range(-35f, 35f), -angle * Mathf.Rad2Deg, Random.Range(-35f, 35f)),
                new Vector3(Random.Range(0.28f, 0.72f), Random.Range(0.45f, 1.05f), Random.Range(0.035f, 0.09f)), glass);
            GlassShard motion = shard.AddComponent<GlassShard>();
            Vector3 outward = new Vector3(Mathf.Cos(angle), Random.Range(0.55f, 1.15f), Mathf.Sin(angle));
            motion.Launch(outward * Random.Range(3.2f, 6.4f), Random.Range(-480f, 480f));
        }
    }

    void Update()
    {
        if (Game.gm == null) return; // scene restarting
        if (!broken) return;
        // free repair: stand next to the tank for a few seconds
        if (Game.player != null && Vector3.Distance(Game.player.transform.position, transform.position) < 4.5f)
        {
            repairTime += Time.deltaTime;
            if (stockText != null)
            {
                stockText.text = "TAMIR EDILIYOR... " + Mathf.CeilToInt(RepairDuration - repairTime) + "s";
                stockText.color = Color.white;
            }
            if (repairTime >= RepairDuration) Repair();
        }
        else repairTime = 0f;
        // floppers die after a while
        for (int i = floppers.Count - 1; i >= 0; i--)
        {
            if (floppers[i] == null) { floppers.RemoveAt(i); continue; }
            if (floppers[i].flopTimer > 40f)
            {
                floppers[i].Die();
                floppers.RemoveAt(i);
            }
        }
    }

    void Repair()
    {
        broken = false;
        bulletHits = 0;
        bulletHitsToBreak = 0;
        glassGo.SetActive(true);
        glassGo.transform.localEulerAngles = Vector3.zero;
        if (waterGo != null) waterGo.SetActive(true);
        if (shardRoot != null) { Destroy(shardRoot); shardRoot = null; }
        Sfx.Play(Snd.Repair, 0.75f);
        // surviving floppers jump back in
        for (int i = 0; i < floppers.Count; i++)
        {
            Fish f = floppers[i];
            if (f == null) continue;
            if (!ReserveSlot()) continue;
            Tank self = this;
            f.FlyTo(RandomWaterPoint(), delegate { if (f != null) self.Receive(f); }, 0.6f);
        }
        floppers.Clear();
        UpdateText();
        if (Game.ui != null) Game.ui.Toast(SpeciesInfo.Name(species) + " tanki tamir edildi!");
    }
}

public class GlassShard : MonoBehaviour
{
    Vector3 velocity;
    float spin;
    bool settled;

    public void Launch(Vector3 initialVelocity, float spinSpeed)
    {
        velocity = initialVelocity;
        spin = spinSpeed;
    }

    void Update()
    {
        if (settled) return;
        velocity.y -= 16f * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.Rotate(new Vector3(spin, spin * 0.7f, spin * 0.35f) * Time.deltaTime, Space.Self);
        if (transform.position.y <= 0.08f)
        {
            Vector3 position = transform.position;
            position.y = 0.08f;
            transform.position = position;
            settled = true;
        }
    }
}
