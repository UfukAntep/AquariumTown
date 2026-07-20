using UnityEngine;

public class Depot : MonoBehaviour
{
    public const int Cap = 60;
    public int[] counts = new int[SpeciesInfo.Count];
    TextMesh text;
    Transform crateRoot;
    int crateCount;

    public int Total { get { int n = 0; for (int i = 0; i < counts.Length; i++) n += counts[i]; return n; } }
    public bool HasSpace { get { return Total < Cap; } }

    public static Depot Create(Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject("Depot");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        Depot d = go.AddComponent<Depot>();
        d.Build();
        Game.depot = d;
        return d;
    }

    void Build()
    {
        Material wood = MatLib.Get(new Color(0.62f, 0.45f, 0.28f));
        Material woodD = MatLib.Get(new Color(0.5f, 0.36f, 0.22f));
        B.Prim(PrimitiveType.Cube, "Platform", transform, new Vector3(0f, 0.15f, 0f), Vector3.zero, new Vector3(5.5f, 0.3f, 4.5f), wood);
        B.Prim(PrimitiveType.Cube, "Post1", transform, new Vector3(-2.5f, 1.2f, -2f), Vector3.zero, new Vector3(0.3f, 2.4f, 0.3f), woodD);
        B.Prim(PrimitiveType.Cube, "Post2", transform, new Vector3(2.5f, 1.2f, -2f), Vector3.zero, new Vector3(0.3f, 2.4f, 0.3f), woodD);
        B.Prim(PrimitiveType.Cube, "Roof", transform, new Vector3(0f, 2.5f, -1f), new Vector3(12f, 0f, 0f), new Vector3(5.8f, 0.2f, 3.2f), MatLib.Get(new Color(0.3f, 0.65f, 0.85f)));
        TextMesh depotTitle = B.Text3D("DEPO", transform, new Vector3(0f, 4.15f, 0f), 0.18f, new Color(1f, 0.92f, 0.3f));
        depotTitle.fontStyle = FontStyle.Bold;
        text = B.Text3D("", transform, new Vector3(0f, 3.25f, 0f), 0.125f, Color.white);

        GameObject cr = new GameObject("Crates");
        cr.transform.SetParent(transform, false);
        crateRoot = cr.transform;
        UpdateVisual();
    }

    public Vector3 DropPoint()
    {
        return transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0.4f, Random.Range(-1f, 1f));
    }

    public void StoreVisualArrived(int sp)
    {
        if (!HasSpace) return;
        counts[sp]++;
        Sfx.Play(Snd.Drop, 0.35f);
        UpdateVisual();
    }

    public void Store(int sp)
    {
        if (!HasSpace) return;
        counts[sp]++;
        UpdateVisual();
    }

    public bool TryTakeForTank(out int species)
    {
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i] <= 0) continue;
            Tank t = Game.TankOf(i);
            if (t != null && t.HasSpace)
            {
                counts[i]--;
                species = i;
                UpdateVisual();
                return true;
            }
        }
        species = 0;
        return false;
    }

    void UpdateVisual()
    {
        if (text != null) text.text = Total + "/" + Cap;
        int want = Mathf.Clamp(Mathf.CeilToInt(Total / 5f), 0, 12);
        Material crate = MatLib.Get(new Color(0.85f, 0.65f, 0.35f));
        while (crateCount < want)
        {
            int i = crateCount;
            B.Prim(PrimitiveType.Cube, "Crate", crateRoot,
                new Vector3((i % 3) * 1.1f - 1.1f, 0.7f + (i / 6) * 0.75f, ((i / 3) % 2) * 1.1f - 0.55f),
                new Vector3(0f, Random.Range(-10f, 10f), 0f), new Vector3(0.9f, 0.7f, 0.9f), crate);
            crateCount++;
        }
        while (crateCount > want && crateRoot.childCount > 0)
        {
            Transform c = crateRoot.GetChild(crateRoot.childCount - 1);
            c.SetParent(null);
            Destroy(c.gameObject);
            crateCount--;
        }
    }

    public void LoadSaved()
    {
        for (int i = 0; i < counts.Length; i++)
            counts[i] = Game.gm.LoadDepotCount(i);
        UpdateVisual();
    }
}
