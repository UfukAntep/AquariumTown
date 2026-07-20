using System.Collections.Generic;
using UnityEngine;

public class TrashItem : MonoBehaviour
{
    public bool inSea;
    Transform anchor;
    int index;
    bool carried;
    Transform stain;          // pollution slick that slowly spreads in water
    float stainT;
    bool largeTrash;

    public float KillRadius { get { return inSea ? 1.0f + stainT * 2.6f : 0f; } }
    public float StainSize { get { return stainT; } }

    public static TrashItem Create(Vector3 pos, bool sea, bool poop = false)
    {
        GameObject go = new GameObject(sea ? "SeaTrash" : "Trash");
        go.transform.position = pos;
        TrashItem t = go.AddComponent<TrashItem>();
        t.inSea = sea;

        if (poop)
        {
            Material m = MatLib.Get(new Color(0.45f, 0.3f, 0.15f));
            B.Prim(PrimitiveType.Sphere, "P1", go.transform, new Vector3(0f, 0.15f, 0f), Vector3.zero, new Vector3(0.45f, 0.3f, 0.45f), m);
            B.Prim(PrimitiveType.Sphere, "P2", go.transform, new Vector3(0f, 0.35f, 0f), Vector3.zero, new Vector3(0.32f, 0.22f, 0.32f), m);
            B.Prim(PrimitiveType.Sphere, "P3", go.transform, new Vector3(0f, 0.5f, 0f), Vector3.zero, new Vector3(0.2f, 0.15f, 0.2f), m);
        }
        else
        {
            // dropped food litter from the ithappy pack (fallback: gray clump)
            GameObject food = AssetLib.SpawnFood(go.transform, 0.8f);
            if (food != null)
            {
                food.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                food.transform.localEulerAngles = new Vector3(Random.Range(-25f, 25f), Random.Range(0f, 360f), Random.Range(140f, 220f));
            }
            else
            {
                Color c = sea ? new Color(0.42f, 0.22f, 0.08f) : new Color(0.7f, 0.65f, 0.55f);
                B.Prim(PrimitiveType.Sphere, "Ball", go.transform, new Vector3(0f, 0.22f, 0f), Vector3.zero, new Vector3(0.45f, 0.35f, 0.45f), MatLib.Get(c));
                B.Prim(PrimitiveType.Cube, "Bit", go.transform, new Vector3(0.15f, 0.32f, 0.1f), new Vector3(20f, 40f, 10f), Vector3.one * 0.18f, MatLib.Get(c * 0.85f));
            }
        }

        if (sea)
        {
            go.AddComponent<Bobber>().amp = 0.06f;
            // spreading slick, like liquid spilled on a table
            GameObject st = B.Prim(PrimitiveType.Cylinder, "Stain", null, pos + Vector3.up * 0.08f, Vector3.zero,
                new Vector3(0.6f, 0.015f, 0.6f), MatLib.Glass(new Color(0.38f, 0.17f, 0.045f, 0.72f)));
            st.transform.SetParent(go.transform, true);
            t.stain = st.transform;
        }
        return t;
    }

    public static TrashItem FromExisting(GameObject existing)
    {
        if (existing == null) return null;
        TrashItem t = existing.GetComponent<TrashItem>();
        if (t == null) t = existing.AddComponent<TrashItem>();
        t.inSea = false;
        t.largeTrash = true;
        existing.name = "ToppledLoungerTrash";
        return t;
    }

    void Update()
    {
        if (carried && anchor != null)
        {
            Vector3 slot = anchor.position + Vector3.up * (index * 0.32f);
            transform.position = Vector3.Lerp(transform.position, slot, 12f * Time.deltaTime);
            return;
        }
        // pollution spreads slowly outward
        if (inSea && stain != null && stainT < 1f)
        {
            stainT += Time.deltaTime / 75f; // ~75s to full size
            float d = Mathf.Lerp(0.6f, 7f, stainT);
            stain.localScale = new Vector3(d, 0.015f, d);
        }
    }

    public void AttachTo(Transform a, int idx)
    {
        carried = true;
        anchor = a;
        index = idx;
        inSea = false;
        Bobber b = GetComponent<Bobber>();
        if (b != null) Destroy(b);
        if (stain != null) { Destroy(stain.gameObject); stain = null; }
        if (largeTrash) transform.localScale = Vector3.one * 0.62f;
    }

    public void DumpInto(Vector3 binPos)
    {
        carried = false;
        anchor = null;
        StartCoroutine(FlyAndDie(binPos + Vector3.up * 1.2f));
    }

    System.Collections.IEnumerator FlyAndDie(Vector3 to)
    {
        Vector3 from = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.4f;
            transform.position = B.Parabola(from, to, 1.5f, Mathf.Clamp01(t));
            yield return null;
        }
        Destroy(gameObject);
    }
}

// Poop under customers + self-polluting sea whose slicks spread and kill fish.
public class TrashSystem : MonoBehaviour
{
    public Vector3 BinPos;
    List<TrashItem> landItems = new List<TrashItem>();
    List<TrashItem> seaItems = new List<TrashItem>();
    float poopTimer = 15f;
    float seaTimer = 30f;
    float killTimer = 2f;
    float spreadTimer = 10f;
    const int SeaPollutionLevel = 8; // sea starts self-polluting only from here

    public int Count { get { landItems.RemoveAll(i => i == null); return landItems.Count; } }
    public int SeaCount { get { seaItems.RemoveAll(i => i == null); return seaItems.Count; } }
    public int ShopCount
    {
        get
        {
            landItems.RemoveAll(i => i == null);
            int count = 0;
            for (int i = 0; i < landItems.Count; i++) if (landItems[i].transform.position.x <= 8f) count++;
            return count;
        }
    }
    public int BeachCount
    {
        get
        {
            landItems.RemoveAll(i => i == null);
            int count = 0;
            for (int i = 0; i < landItems.Count; i++) if (landItems[i].transform.position.x > 8f) count++;
            return count;
        }
    }

    public static TrashSystem Create(Vector3 binPos, Transform parent)
    {
        GameObject go = new GameObject("TrashSystem");
        if (parent != null) go.transform.SetParent(parent, false);
        TrashSystem t = go.AddComponent<TrashSystem>();
        t.BinPos = binPos;
        t.BuildBin();
        Game.trash = t;
        return t;
    }

    void BuildBin()
    {
        GameObject bin = new GameObject("Bin");
        bin.transform.SetParent(transform, false);
        bin.transform.position = BinPos;
        Material green = MatLib.Get(new Color(0.25f, 0.55f, 0.3f));
        B.Prim(PrimitiveType.Cylinder, "Can", bin.transform, new Vector3(0f, 0.7f, 0f), Vector3.zero, new Vector3(1.2f, 0.7f, 1.2f), green, true);
        B.Prim(PrimitiveType.Cylinder, "Rim", bin.transform, new Vector3(0f, 1.42f, 0f), Vector3.zero, new Vector3(1.3f, 0.05f, 1.3f), MatLib.Get(new Color(0.18f, 0.4f, 0.22f)));
        B.Text3D("COP", bin.transform, new Vector3(0f, 2.2f, 0f), 0.1f, Color.white);
    }

    void Update()
    {
        if (Game.gm == null) return;
        float dt = Time.deltaTime;

        // Inside the shop customers only leave poop. Food litter is reserved for
        // the beach visitor system, so the two kinds of dirt read differently.
        poopTimer -= dt;
        if (poopTimer <= 0f)
        {
            poopTimer = Random.Range(14f, 26f);
            if (ShopCount < 8 && Random.value < Game.gm.PoopChanceMult)
            {
                Customer[] custs = FindObjectsByType<Customer>(FindObjectsSortMode.None);
                if (custs.Length > 0)
                {
                    Vector3 pos = custs[Random.Range(0, custs.Length)].transform.position;
                    pos.y = 0f;
                    landItems.Add(TrashItem.Create(pos, false, true));
                    if (Count == 4 && Game.ui != null)
                        Game.ui.Toast("Magaza kirleniyor! Musteriler rahatsiz olabilir.");
                }
            }
        }

        // sea pollutes itself (only from a mid level on, not from the very start)
        seaTimer -= dt;
        if (seaTimer <= 0f)
        {
            seaTimer = Random.Range(24f, 42f);
            if (Game.gm.Level >= SeaPollutionLevel && SeaCount < 16 && Game.sea != null)
            {
                Rect a = Game.sea.area;
                Vector3 pos = new Vector3(Random.Range(a.xMin + 4f, a.xMin + a.width * 0.5f), 0.5f, Random.Range(a.yMin + 4f, a.yMax - 4f));
                seaItems.Add(TrashItem.Create(pos, true));
                if (SeaCount == 8 && Game.ui != null)
                    Game.ui.Toast("Deniz cok kirlendi! Balik olumleri basladi, temizle!");
            }
        }

        // grown slicks seed new pollution nearby (spreading)
        spreadTimer -= dt;
        if (spreadTimer <= 0f)
        {
            spreadTimer = 18f;
            seaItems.RemoveAll(i => i == null);
            if (SeaCount < 16)
            {
                for (int i = 0; i < seaItems.Count; i++)
                {
                    if (seaItems[i].StainSize > 0.6f && Random.value < 0.4f)
                    {
                        Vector3 p = seaItems[i].transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
                        p.y = 0.5f;
                        if (Game.sea != null && Game.sea.Contains(p))
                        {
                            seaItems.Add(TrashItem.Create(p, true));
                            break;
                        }
                    }
                }
            }
        }

        // pollution kills nearby fish (bigger slick = bigger kill zone)
        killTimer -= dt;
        if (killTimer <= 0f)
        {
            killTimer = 2.5f;
            seaItems.RemoveAll(i => i == null);
            for (int i = 0; i < seaItems.Count; i++)
                if (Game.sea != null && Game.sea.KillOneNear(seaItems[i].transform.position, seaItems[i].KillRadius))
                    break;
        }
    }

    // tutorial: seed some starting mess in the shop
    public void SpawnLandTrash(Vector3 pos, bool poop = false)
    {
        landItems.Add(TrashItem.Create(pos, false, poop));
    }

    public void RegisterLargeTrash(GameObject item)
    {
        TrashItem trash = TrashItem.FromExisting(item);
        if (trash != null && !landItems.Contains(trash)) landItems.Add(trash);
    }

    public void ScatterIntoSea(Vector3 center, int pieces, bool notify = true)
    {
        for (int i = 0; i < pieces; i++)
        {
            Vector3 pos = center + new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
            if (Game.sea != null && Game.sea.Contains(pos))
                seaItems.Add(TrashItem.Create(pos, true));
        }
        if (notify && Game.ui != null) Game.ui.Toast("Copler denize dagildi! Yayilmadan topla!");
    }

    public TrashItem FindNear(Vector3 pos, float radius, bool sea)
    {
        List<TrashItem> list = sea ? seaItems : landItems;
        list.RemoveAll(i => i == null);
        for (int i = 0; i < list.Count; i++)
            if (Vector3.Distance(pos, list[i].transform.position) < radius)
                return list[i];
        return null;
    }

    public TrashItem FindNearestTo(Vector3 pos, bool sea)
    {
        List<TrashItem> list = sea ? seaItems : landItems;
        list.RemoveAll(i => i == null);
        TrashItem best = null;
        float bd = float.MaxValue;
        for (int i = 0; i < list.Count; i++)
        {
            float d = Vector3.Distance(pos, list[i].transform.position);
            if (d < bd) { bd = d; best = list[i]; }
        }
        return best;
    }

    public TrashItem FindAny(bool sea)
    {
        List<TrashItem> list = sea ? seaItems : landItems;
        list.RemoveAll(i => i == null);
        return list.Count > 0 ? list[0] : null;
    }

    public void PickUp(TrashItem t)
    {
        landItems.Remove(t);
        seaItems.Remove(t);
    }
}
