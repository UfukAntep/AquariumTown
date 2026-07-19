using System.Collections.Generic;
using UnityEngine;

// Species aquarium: no capacity limit. Only a handful of fish are shown swimming,
// the rest are counted (extraCount). Can be leveled up (decor + price bonus) and
// can break during earthquakes.
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
    GameObject glassGo, crackGo;
    float repairTime; // repairs are FREE: just stand next to the tank

    public int Count { get { return visualFish.Count + extraCount; } }
    public bool HasSpace { get { return !broken; } }
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
        B.Prim(PrimitiveType.Cylinder, "Water", transform, new Vector3(0f, 1.55f, 0f), Vector3.zero, new Vector3(3.7f, 0.62f, 3.7f), waterM);
        glassGo = B.Prim(PrimitiveType.Cylinder, "Glass", transform, new Vector3(0f, 1.7f, 0f), Vector3.zero, new Vector3(3.9f, 0.85f, 3.9f), glassM);

        CapsuleCollider cc = gameObject.AddComponent<CapsuleCollider>();
        cc.center = new Vector3(0f, 1.3f, 0f);
        cc.radius = 2.1f;
        cc.height = 2.6f;

        B.SpeciesBubble(species, transform, new Vector3(-2.6f, 3.6f, 0f)).AddComponent<Bobber>();
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
            stockText.color = new Color(1f, 0.4f, 0.3f);
        }
        else
        {
            int lvl = Game.gm != null ? Game.gm.tankLevel[species] : 1;
            stockText.text = Count.ToString() + (lvl > 1 ? "  (Sv" + lvl + ")" : "");
            stockText.color = Count == 0 ? new Color(1f, 0.5f, 0.4f) : Color.white;
        }
    }

    public Vector3 RandomWaterPoint()
    {
        Bounds b = WaterBounds;
        Vector2 c = Random.insideUnitCircle * 1.2f;
        return new Vector3(b.center.x + c.x, Random.Range(b.min.y, b.max.y), b.center.z + c.y);
    }

    public void ReserveSlot() { incoming++; }

    public void Receive(Fish f)
    {
        incoming = Mathf.Max(0, incoming - 1);
        if (broken) { f.Die(); return; }
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
    }

    public void AddSaved(int n)
    {
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

    public void AddCount(int n) { extraCount += n; UpdateText(); }

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
        repairTime = 0f;
        glassGo.transform.localEulerAngles = new Vector3(12f, 0f, 8f);
        crackGo = B.Prim(PrimitiveType.Cube, "Crack", transform, new Vector3(0f, 1.7f, -1.9f), new Vector3(0f, 0f, 35f),
            new Vector3(0.1f, 1.4f, 0.1f), MatLib.Get(new Color(0.2f, 0.2f, 0.25f)));
        // fish spill on the floor and flop
        visualFish.RemoveAll(x => x == null);
        int spill = Mathf.Min(Count, 6);
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

    void Update()
    {
        if (Game.gm == null) return; // scene restarting
        if (!broken) return;
        // free repair: stand next to the tank for a few seconds
        if (Game.player != null && Vector3.Distance(Game.player.transform.position, transform.position) < 4.5f)
        {
            repairTime += Time.deltaTime;
            if (stockText != null)
                stockText.text = "TAMIR EDILIYOR... " + Mathf.CeilToInt(RepairDuration - repairTime) + "s";
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
        glassGo.transform.localEulerAngles = Vector3.zero;
        if (crackGo != null) Destroy(crackGo);
        Sfx.Play(Snd.Buy);
        // surviving floppers jump back in
        for (int i = 0; i < floppers.Count; i++)
        {
            Fish f = floppers[i];
            if (f == null) continue;
            ReserveSlot();
            Tank self = this;
            f.FlyTo(RandomWaterPoint(), delegate { if (f != null) self.Receive(f); }, 0.6f);
        }
        floppers.Clear();
        UpdateText();
        if (Game.ui != null) Game.ui.Toast(SpeciesInfo.Name(species) + " tanki tamir edildi!");
    }
}
