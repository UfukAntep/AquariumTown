using UnityEngine;

// Purchasable plot: stand on it, money drains in.
public class BuyZone : MonoBehaviour
{
    public int cost;

    int paid;
    float payCarry;
    System.Func<bool> prereq;
    System.Action onBought;
    TextMesh text;
    Transform fillDisc;
    Transform plotFill; // green area that rises across the pad as you pay
    GameObject root;
    bool visible = true;
    bool hideUntilPrereq;
    bool lastCanBuy;

    // Tank plot: brown pad + dark fish silhouette + name. Only the next couple are visible.
    public static BuyZone CreatePlot(int sp, Vector3 pos)
    {
        BuyZone z = CreateBase("Plot_" + sp, SpeciesInfo.PlotCost(sp), pos,
            delegate ()
            {
                bool starterAreaOpen = sp < 10 || sp >= 20 || Game.gm.starterBackAreaOpen;
                return Game.gm.unlockedCount >= sp && Game.gm.unlockedCount <= sp + 1 &&
                    Game.gm.ZoneOpen(sp / 20) && starterAreaOpen;
            },
            delegate { Game.gm.UnlockNext(); });
        z.hideUntilPrereq = true;
        Material pad = MatLib.Get(new Color(0.72f, 0.58f, 0.42f));
        Material shadow = MatLib.Get(new Color(0.35f, 0.28f, 0.2f));
        Material corner = MatLib.Get(Color.white);
        B.Prim(PrimitiveType.Cube, "Pad", z.root.transform, new Vector3(0f, 0.03f, 0f), Vector3.zero, new Vector3(4.6f, 0.05f, 4.6f), pad);
        // silhouette uses the SAME model that will fill the tank (consistency)
        GameObject q = AssetLib.SpawnSeaAnimal(sp, z.root.transform, 2.6f);
        Transform sil = q != null ? q.transform : SpeciesInfo.Build(sp, z.root.transform, 2.4f);
        sil.localPosition = new Vector3(0f, 0.12f, 0f);
        sil.localEulerAngles = new Vector3(0f, -90f, 0f);
        sil.localScale = new Vector3(sil.localScale.x, sil.localScale.y * 0.05f, sil.localScale.z);
        foreach (MeshRenderer mr in sil.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = shadow;
        foreach (SkinnedMeshRenderer smr in sil.GetComponentsInChildren<SkinnedMeshRenderer>()) smr.sharedMaterial = shadow;
        // keep it a static flat shadow (no swim animation)
        QuirkyMotion qm = sil.GetComponent<QuirkyMotion>();
        if (qm != null) Object.Destroy(qm);
        Animator an = sil.GetComponentInChildren<Animator>();
        if (an != null) an.enabled = false;
        for (int i = 0; i < 4; i++)
        {
            float sx = (i % 2) * 2 - 1, sz = (i / 2) * 2 - 1;
            B.Prim(PrimitiveType.Cube, "Corner", z.root.transform, new Vector3(sx * 2.2f, 0.08f, sz * 2.2f), Vector3.zero, new Vector3(0.5f, 0.08f, 0.5f), corner);
        }
        // green progress area filling the pad from the front (like the reference)
        GameObject fill = B.Prim(PrimitiveType.Cube, "PlotFill", z.root.transform, new Vector3(0f, 0.06f, -2.3f), Vector3.zero,
            new Vector3(4.6f, 0.03f, 0.01f), MatLib.Get(new Color(0.45f, 0.85f, 0.3f)));
        z.plotFill = fill.transform;
        TextMesh name = B.Text3D(SpeciesInfo.Name(sp), z.root.transform, new Vector3(0f, 2.7f, 0f), 0.09f, new Color(1f, 0.95f, 0.7f));
        return z;
    }

    public static BuyZone CreateGeneric(string label, int cost, Vector3 pos, Color color,
        System.Func<bool> prereq, System.Action onBought)
    {
        BuyZone z = CreateBase(label, cost, pos, prereq, onBought);
        Color dim = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f);
        B.Prim(PrimitiveType.Cylinder, "Base", z.root.transform, new Vector3(0f, 0.03f, 0f), Vector3.zero, new Vector3(3f, 0.03f, 3f), MatLib.Get(dim));
        GameObject fill = B.Prim(PrimitiveType.Cylinder, "Fill", z.root.transform, new Vector3(0f, 0.07f, 0f), Vector3.zero, new Vector3(0.01f, 0.03f, 0.01f), MatLib.Get(color));
        z.fillDisc = fill.transform;
        B.Text3D(label, z.root.transform, new Vector3(0f, 2.6f, 0f), 0.1f, Color.white);
        return z;
    }

    static BuyZone CreateBase(string name, int cost, Vector3 pos, System.Func<bool> prereq, System.Action onBought)
    {
        GameObject go = new GameObject("BuyZone_" + name);
        go.transform.position = pos;
        BuyZone z = go.AddComponent<BuyZone>();
        z.cost = cost;
        z.prereq = prereq;
        z.onBought = onBought;
        z.root = new GameObject("Visual");
        z.root.transform.SetParent(go.transform, false);
        z.text = B.Text3D("", z.root.transform, new Vector3(0f, 2f, 0f), 0.12f, Color.white);
        z.lastCanBuy = prereq == null || prereq();
        z.UpdateText();
        return z;
    }

    void UpdateText()
    {
        int remaining = Mathf.Max(0, cost - paid);
        bool canBuy = prereq == null || prereq();
        if (text != null) text.text = "$" + B.Money(remaining) + (canBuy ? "" : "\nKILITLI");
    }

    void Update()
    {
        if (Game.gm == null || Game.player == null) return; // scene restarting
        bool canBuy = prereq == null || prereq();
        if (canBuy != lastCanBuy) { lastCanBuy = canBuy; UpdateText(); }
        bool show = !hideUntilPrereq || canBuy;
        if (show != visible)
        {
            visible = show;
            root.SetActive(show);
        }
        if (!visible) return;

        Vector3 p = Game.player.transform.position; p.y = 0f;
        Vector3 me = transform.position; me.y = 0f;
        if (canBuy && Vector3.Distance(p, me) < 2.2f && paid < cost)
        {
            float rate = Mathf.Max(30f, cost / 1.8f);
            payCarry += rate * Time.deltaTime;
            int want = Mathf.FloorToInt(payCarry);
            if (want > 0)
            {
                payCarry -= want;
                want = Mathf.Min(want, cost - paid);
                int got = Game.gm.SpendTick(want);
                if (got > 0)
                {
                    paid += got;
                    UpdateText();
                    float frac = Mathf.Clamp01((float)paid / cost);
                    if (fillDisc != null)
                        fillDisc.localScale = new Vector3(3f * frac, 0.031f, 3f * frac);
                    if (plotFill != null)
                    {
                        float depth = 4.6f * frac;
                        plotFill.localScale = new Vector3(4.6f, 0.03f, Mathf.Max(0.01f, depth));
                        plotFill.localPosition = new Vector3(0f, 0.06f, -2.3f + depth * 0.5f);
                    }
                }
            }
            if (paid >= cost)
            {
                Sfx.Play(Snd.Buy);
                System.Action cb = onBought;
                Destroy(gameObject);
                if (cb != null) cb();
            }
        }
    }
}
