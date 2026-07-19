using System.Collections.Generic;
using UnityEngine;

// Toilet annex: buy klozet (toilet) and lavabo (sink) units on the PC.
// Toilets randomly CLOG ("TIKANDI!") and units get dirty with use;
// the player (stand next to it) or the toilet cleaner fixes them.
public class Toilets : MonoBehaviour
{
    class Unit
    {
        public int type;          // 0 = klozet, 1 = lavabo
        public float dirt;        // 0..1
        public bool clogged;
        public float clogTimer;
        public GameObject go;
        public TextMesh text;
        public GameObject clogBlob;
    }

    List<Unit> units = new List<Unit>();
    GameObject lockedSign;
    float playerFixTimer;

    public Vector3 EntrancePos { get { return transform.position + new Vector3(3f, 0f, -4.5f); } }

    public int CleanToiletCount
    {
        get { int n = 0; for (int i = 0; i < units.Count; i++) if (units[i].type == 0 && !units[i].clogged && units[i].dirt < 0.7f) n++; return n; }
    }
    public int CleanSinkCount
    {
        get { int n = 0; for (int i = 0; i < units.Count; i++) if (units[i].type == 1 && units[i].dirt < 0.7f) n++; return n; }
    }
    public bool HasCleanUnit { get { return CleanToiletCount > 0; } }

    public static Toilets Create(Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject("Toilets");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        Toilets t = go.AddComponent<Toilets>();
        t.Build();
        Game.toilets = t;
        return t;
    }

    void Build()
    {
        Material tile = MatLib.Get(new Color(0.85f, 0.92f, 0.95f));
        B.Prim(PrimitiveType.Cube, "Floor", transform, new Vector3(3f, 0.02f, 0f), Vector3.zero, new Vector3(15f, 0.04f, 9f), tile);
        B.Text3D("TUVALETLER", transform, new Vector3(3f, 3.6f, 2f), 0.13f, new Color(0.4f, 0.7f, 0.95f));

        for (int i = 0; i < Game.gm.toiletCount; i++) AddUnit(0);
        for (int i = 0; i < Game.gm.sinkCount; i++) AddUnit(1);
    }

    public void OnLevelChanged()
    {
        if (lockedSign != null) { Destroy(lockedSign); lockedSign = null; }
    }

    int CountOf(int type)
    {
        int n = 0;
        for (int i = 0; i < units.Count; i++) if (units[i].type == type) n++;
        return n;
    }

    public void AddUnit(int type)
    {
        Unit u = new Unit();
        u.type = type;
        int idx = CountOf(type);
        GameObject go = new GameObject(type == 0 ? "Klozet" : "Lavabo");
        go.transform.SetParent(transform, false);
        // toilets along the back row, sinks along the front row
        go.transform.localPosition = type == 0
            ? new Vector3(idx * 2.4f - 2.5f, 0f, 2.2f)
            : new Vector3(idx * 2.4f - 2f, 0f, -2.2f);
        u.go = go;

        Material wall = MatLib.Get(new Color(0.55f, 0.75f, 0.85f));
        Material white = MatLib.Get(Color.white);
        Material chrome = MatLib.Get(new Color(0.75f, 0.78f, 0.82f));
        if (type == 0)
        {
            B.Prim(PrimitiveType.Cube, "Back", go.transform, new Vector3(0f, 1.2f, 0.9f), Vector3.zero, new Vector3(1.9f, 2.4f, 0.15f), wall);
            B.Prim(PrimitiveType.Cube, "SideL", go.transform, new Vector3(-0.95f, 1.2f, 0.3f), Vector3.zero, new Vector3(0.12f, 2.4f, 1.4f), wall);
            B.Prim(PrimitiveType.Cube, "SideR", go.transform, new Vector3(0.95f, 1.2f, 0.3f), Vector3.zero, new Vector3(0.12f, 2.4f, 1.4f), wall);
            B.Prim(PrimitiveType.Cube, "Tank", go.transform, new Vector3(0f, 0.9f, 0.7f), Vector3.zero, new Vector3(0.7f, 0.6f, 0.3f), white);
            B.Prim(PrimitiveType.Cylinder, "Bowl", go.transform, new Vector3(0f, 0.4f, 0.3f), Vector3.zero, new Vector3(0.65f, 0.35f, 0.65f), white);
        }
        else
        {
            B.Prim(PrimitiveType.Cube, "Column", go.transform, new Vector3(0f, 0.45f, 0f), Vector3.zero, new Vector3(0.3f, 0.9f, 0.3f), white);
            B.Prim(PrimitiveType.Cube, "Basin", go.transform, new Vector3(0f, 0.95f, 0f), Vector3.zero, new Vector3(0.9f, 0.2f, 0.7f), white);
            B.Prim(PrimitiveType.Cube, "Tap", go.transform, new Vector3(0f, 1.2f, 0.25f), Vector3.zero, new Vector3(0.08f, 0.3f, 0.08f), chrome);
            B.Prim(PrimitiveType.Cube, "Mirror", go.transform, new Vector3(0f, 1.9f, 0.4f), Vector3.zero, new Vector3(0.8f, 0.8f, 0.05f), MatLib.Glass(new Color(0.7f, 0.85f, 1f, 0.7f)));
        }
        u.text = B.Text3D("", go.transform, new Vector3(0f, 2.9f, 0f), 0.08f, Color.white);
        u.clogTimer = Random.Range(60f, 150f);
        units.Add(u);
        UpdateText(u);
    }

    // customer uses the facilities
    public void Use()
    {
        Unit best = null;
        for (int i = 0; i < units.Count; i++)
        {
            Unit u = units[i];
            if (u.clogged || u.dirt >= 0.7f) continue;
            if (best == null || u.dirt < best.dirt) best = u;
        }
        if (best != null)
        {
            best.dirt = Mathf.Min(1f, best.dirt + Random.Range(0.12f, 0.28f));
            UpdateText(best);
        }
    }

    public int DirtiestUnit()
    {
        int best = -1;
        float worst = 0.5f;
        for (int i = 0; i < units.Count; i++)
        {
            float score = units[i].dirt + (units[i].clogged ? 1f : 0f);
            if (score >= worst && score > 0.49f) { worst = score; best = i; }
        }
        return best;
    }

    public Vector3 UnitPos(int i)
    {
        return i >= 0 && i < units.Count ? units[i].go.transform.position : transform.position;
    }

    public void CleanNearest(Vector3 pos)
    {
        Unit best = null;
        float bd = 4f;
        for (int i = 0; i < units.Count; i++)
        {
            float d = Vector3.Distance(pos, units[i].go.transform.position);
            if (d < bd && (units[i].dirt >= 0.5f || units[i].clogged)) { bd = d; best = units[i]; }
        }
        if (best != null)
        {
            best.dirt = 0f;
            best.clogged = false;
            best.clogTimer = Random.Range(60f, 150f);
            if (best.clogBlob != null) { Destroy(best.clogBlob); best.clogBlob = null; }
            UpdateText(best);
            Sfx.Play(Snd.Collect, 0.35f);
        }
    }

    void UpdateText(Unit u)
    {
        if (u.text == null) return;
        if (u.clogged) { u.text.text = "TIKANDI!"; u.text.color = new Color(1f, 0.35f, 0.3f); }
        else if (u.dirt >= 0.7f) { u.text.text = "KIRLI"; u.text.color = new Color(1f, 0.6f, 0.35f); }
        else { u.text.text = u.type == 0 ? "WC" : "Lavabo"; u.text.color = new Color(0.65f, 1f, 0.7f); }
    }

    void Update()
    {
        if (Game.gm == null) return;
        float dt = Time.deltaTime;

        // toilets clog over time
        for (int i = 0; i < units.Count; i++)
        {
            Unit u = units[i];
            if (u.type != 0 || u.clogged) continue;
            u.clogTimer -= dt;
            if (u.clogTimer <= 0f)
            {
                u.clogged = true;
                u.clogBlob = B.Prim(PrimitiveType.Sphere, "Clog", u.go.transform, new Vector3(0f, 0.72f, 0.3f), Vector3.zero,
                    new Vector3(0.55f, 0.3f, 0.55f), MatLib.Get(new Color(0.45f, 0.32f, 0.18f)));
                UpdateText(u);
                if (Game.ui != null) Game.ui.Toast("Bir tuvalet TIKANDI! Basina git ve temizle (veya tuvaletci tut).");
            }
        }

        // player can fix by standing next to a broken/dirty unit
        if (Game.player != null)
        {
            bool near = false;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].clogged && units[i].dirt < 0.5f) continue;
                if (Vector3.Distance(Game.player.transform.position, units[i].go.transform.position) < 2f) { near = true; break; }
            }
            if (near)
            {
                playerFixTimer += dt;
                if (playerFixTimer > 2.2f)
                {
                    playerFixTimer = 0f;
                    CleanNearest(Game.player.transform.position);
                    if (Game.ui != null) Game.ui.Toast("Tuvalet temizlendi!");
                }
            }
            else playerFixTimer = 0f;
        }
    }
}
