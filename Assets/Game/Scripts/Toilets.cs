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

    class UrineSpot
    {
        public GameObject go;
    }

    List<Unit> units = new List<Unit>();
    List<UrineSpot> urineSpots = new List<UrineSpot>();
    GameObject lockedSign;
    float playerFixTimer;

    public Vector3 EntrancePos { get { return transform.position + new Vector3(9f, 0f, 0f); } }

    public int CleanToiletCount
    {
        get { int n = 0; for (int i = 0; i < units.Count; i++) if (units[i].type == 0 && !units[i].clogged && units[i].dirt < 0.7f) n++; return n; }
    }
    public int CleanSinkCount
    {
        get { int n = 0; for (int i = 0; i < units.Count; i++) if (units[i].type == 1 && units[i].dirt < 0.7f) n++; return n; }
    }
    public bool HasCleanUnit { get { return CleanToiletCount > 0; } }
    public int UrineSpotCount { get { urineSpots.RemoveAll(s => s == null || s.go == null); return urineSpots.Count; } }
    public bool CanStealToilet { get { return CountOf(0) > 0; } }

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
        BuildAreaShell(transform, false);
        TextMesh toiletTitle = B.Text3D("TUVALETLER", transform, new Vector3(3f, 4.1f, 2f), 0.2f, new Color(0.2f, 0.65f, 1f));
        toiletTitle.fontStyle = FontStyle.Bold;

        for (int i = 0; i < Game.gm.toiletCount; i++) AddUnit(0);
        for (int i = 0; i < Game.gm.sinkCount; i++) AddUnit(1);
    }

    // Thin-walled room used both by the locked preview and the purchased annex.
    public static void BuildAreaShell(Transform parent, bool locked)
    {
        Material tile = MatLib.Get(locked ? new Color(0.48f, 0.52f, 0.57f) : new Color(0.85f, 0.92f, 0.95f));
        Material wall = MatLib.Get(locked ? new Color(0.28f, 0.34f, 0.4f) : new Color(0.48f, 0.72f, 0.84f));
        B.Prim(PrimitiveType.Cube, "ToiletFloor", parent, new Vector3(3f, 0.025f, 0f), Vector3.zero,
            new Vector3(15f, 0.05f, 9f), tile);
        B.Prim(PrimitiveType.Cube, "ToiletWallBack", parent, new Vector3(3f, 0.8f, 4.5f), Vector3.zero,
            new Vector3(15f, 1.6f, 0.18f), wall, true);
        B.Prim(PrimitiveType.Cube, "ToiletWallLeft", parent, new Vector3(-4.5f, 0.8f, 0f), Vector3.zero,
            new Vector3(0.18f, 1.6f, 9f), wall, true);
        if (locked)
            B.Prim(PrimitiveType.Cube, "ToiletWallRight", parent, new Vector3(10.5f, 0.8f, 0f), Vector3.zero,
                new Vector3(0.18f, 1.6f, 9f), wall, true);
        else
        {
            B.Prim(PrimitiveType.Cube, "ToiletWallRightBottom", parent, new Vector3(10.5f, 0.8f, -3f), Vector3.zero,
                new Vector3(0.18f, 1.6f, 3f), wall, true);
            B.Prim(PrimitiveType.Cube, "ToiletWallRightTop", parent, new Vector3(10.5f, 0.8f, 3f), Vector3.zero,
                new Vector3(0.18f, 1.6f, 3f), wall, true);
        }

        if (locked)
        {
            B.Prim(PrimitiveType.Cube, "ToiletWallFrontLocked", parent, new Vector3(3f, 0.8f, -4.5f), Vector3.zero,
                new Vector3(15f, 1.6f, 0.18f), wall, true);
            TextMesh lockedTitle = B.Text3D("KILITLI TUVALET ALANI", parent, new Vector3(3f, 4.1f, 0f), 0.19f, Color.white);
            lockedTitle.fontStyle = FontStyle.Bold;
            TextMesh lockedPrice = B.Text3D("$" + B.Money(GameManager.ToiletAreaCost) + "  (Sv " + GameManager.ToiletAreaLevel + ")",
                parent, new Vector3(3f, 3.25f, 0f), 0.15f, new Color(1f, 0.9f, 0.3f));
            lockedPrice.fontStyle = FontStyle.Bold;
        }
        else
        {
            // Front is closed; the three-metre doorway is on the right wall.
            B.Prim(PrimitiveType.Cube, "ToiletWallFront", parent, new Vector3(3f, 0.8f, -4.5f), Vector3.zero,
                new Vector3(15f, 1.6f, 0.18f), wall, true);
        }
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

    public bool TryStealToilet()
    {
        for (int i = units.Count - 1; i >= 0; i--)
        {
            if (units[i].type != 0) continue;
            Unit stolen = units[i];
            units.RemoveAt(i);
            if (stolen.go != null) Destroy(stolen.go);
            Game.gm.toiletCount = Mathf.Max(0, Game.gm.toiletCount - 1);
            return true;
        }
        return false;
    }

    public void RestoreStolenToilet()
    {
        if (Game.gm.toiletCount >= 5) return;
        Game.gm.toiletCount++;
        AddUnit(0);
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
            if (Random.value < 0.32f) SpawnUrine(best.go.transform.position);
            UpdateText(best);
        }
    }

    void SpawnUrine(Vector3 around)
    {
        urineSpots.RemoveAll(s => s == null || s.go == null);
        if (urineSpots.Count >= 8) return;

        Vector2 offset = Random.insideUnitCircle * 0.8f;
        GameObject spot = B.Prim(PrimitiveType.Cylinder, "UrineSpot", transform,
            transform.InverseTransformPoint(around + new Vector3(offset.x, 0.035f, offset.y)), Vector3.zero,
            new Vector3(Random.Range(0.75f, 1.25f), 0.012f, Random.Range(0.75f, 1.25f)),
            MatLib.Glass(new Color(1f, 0.82f, 0.08f, 0.62f)));
        urineSpots.Add(new UrineSpot { go = spot });
    }

    public int DirtiestUnit()
    {
        urineSpots.RemoveAll(s => s == null || s.go == null);
        if (urineSpots.Count > 0) return units.Count;
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
        int spot = i - units.Count;
        if (spot >= 0 && spot < urineSpots.Count && urineSpots[spot] != null && urineSpots[spot].go != null)
            return urineSpots[spot].go.transform.position;
        return i >= 0 && i < units.Count ? units[i].go.transform.position : transform.position;
    }

    public void CleanNearest(Vector3 pos)
    {
        urineSpots.RemoveAll(s => s == null || s.go == null);
        int nearestSpot = -1;
        float nearestSpotDistance = 4f;
        for (int i = 0; i < urineSpots.Count; i++)
        {
            float d = Vector3.Distance(pos, urineSpots[i].go.transform.position);
            if (d < nearestSpotDistance) { nearestSpotDistance = d; nearestSpot = i; }
        }
        if (nearestSpot >= 0)
        {
            Destroy(urineSpots[nearestSpot].go);
            urineSpots.RemoveAt(nearestSpot);
            Sfx.Play(Snd.Collect, 0.35f);
            return;
        }

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
            urineSpots.RemoveAll(s => s == null || s.go == null);
            for (int i = 0; i < urineSpots.Count; i++)
            {
                if (Vector3.Distance(Game.player.transform.position, urineSpots[i].go.transform.position) < 2f)
                {
                    near = true;
                    break;
                }
            }
            for (int i = 0; i < units.Count; i++)
            {
                if (near) break;
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
