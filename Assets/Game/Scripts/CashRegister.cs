using System.Collections.Generic;
using UnityEngine;

public class CashRegister : MonoBehaviour
{
    public int PileAmount;
    public bool CashierPresent;

    List<Customer> queue = new List<Customer>();
    Transform pileRoot;
    List<GameObject> bills = new List<GameObject>();
    TextMesh pileText, needText;
    bool playerAtSpot;
    public Transform monitor;

    bool billRoutineRunning;

    public Vector3 OperatorSpot { get { return transform.position + new Vector3(0f, 0f, 2f); } }
    public bool HasOperator { get { return CashierPresent || playerAtSpot; } }
    public bool PlayerAtSpot { get { return playerAtSpot; } }

    public static CashRegister Create(Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject("CashRegister");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        CashRegister r = go.AddComponent<CashRegister>();
        r.Build();
        Game.register = r;
        return r;
    }

    void Build()
    {
        Material desk = MatLib.Get(new Color(0.5f, 0.33f, 0.22f));
        Material top = MatLib.Get(new Color(0.95f, 0.95f, 0.98f));
        Material dark = MatLib.Get(new Color(0.12f, 0.12f, 0.16f));
        Material screen = MatLib.Get(new Color(0.25f, 0.55f, 0.95f));

        B.Prim(PrimitiveType.Cube, "Desk", transform, new Vector3(0f, 0.6f, 0f), Vector3.zero, new Vector3(3.2f, 1.2f, 1.2f), desk, true);
        B.Prim(PrimitiveType.Cube, "Top", transform, new Vector3(0f, 1.28f, 0f), Vector3.zero, new Vector3(3.4f, 0.16f, 1.4f), top);

        // PC monitor on the desk (interactable with E)
        GameObject mon = new GameObject("Monitor");
        mon.transform.SetParent(transform, false);
        mon.transform.localPosition = new Vector3(0.8f, 1.75f, 0f);
        mon.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        B.Prim(PrimitiveType.Cube, "Frame", mon.transform, Vector3.zero, Vector3.zero, new Vector3(1.1f, 0.75f, 0.08f), dark);
        B.Prim(PrimitiveType.Cube, "Screen", mon.transform, new Vector3(0f, 0f, -0.05f), Vector3.zero, new Vector3(0.95f, 0.6f, 0.02f), screen);
        B.Prim(PrimitiveType.Cube, "Stand", mon.transform, new Vector3(0f, -0.5f, 0.05f), Vector3.zero, new Vector3(0.15f, 0.3f, 0.15f), dark);
        B.Prim(PrimitiveType.Cube, "Keyboard", transform, new Vector3(0.1f, 1.4f, 0.35f), Vector3.zero, new Vector3(0.8f, 0.05f, 0.3f), dark);
        monitor = mon.transform;

        B.Text3D("KASA", transform, new Vector3(0f, 2.9f, 0f), 0.12f, new Color(1f, 0.95f, 0.5f));
        needText = B.Text3D("", transform, new Vector3(0f, 2.3f, 0f), 0.09f, new Color(1f, 0.6f, 0.5f));

        GameObject pr = new GameObject("Pile");
        pr.transform.SetParent(transform, false);
        pr.transform.localPosition = new Vector3(-3.6f, 0f, -1.2f);
        pileRoot = pr.transform;
        pileText = B.Text3D("", pileRoot, new Vector3(0f, 3.2f, 0f), 0.12f, new Color(0.55f, 1f, 0.55f));
    }

    // ---------- queue ----------
    public int Join(Customer c) { queue.Add(c); return queue.Count - 1; }
    public void Leave(Customer c) { queue.Remove(c); }
    public int IndexOf(Customer c) { return queue.IndexOf(c); }
    public Vector3 QueueSlot(int i) { return transform.position + new Vector3(0f, 0f, -2f - i * 1.3f); }

    public int QueueCount { get { return queue.Count; } }

    // ---------- neat single-bill money pile (like the reference) ----------
    public void Pay(int amount, bool fromCustomer = true)
    {
        PileAmount += amount;
        Game.gm.RegisterIncome(amount, fromCustomer);
        if (fromCustomer) QuestSystem.OnSell();
        Sfx.Play(Snd.Cash, 0.4f);
        if (!billRoutineRunning) StartCoroutine(TickBills());
        if (pileText != null) pileText.text = "$" + B.Money(PileAmount);
    }

    Vector3 BillSlot(int idx)
    {
        // Every object is exactly $1. A wider grid keeps large payments tidy.
        int perLayer = 80;
        int layer = idx / perLayer;
        int inLayer = idx % perLayer;
        int col = inLayer % 10, row = inLayer / 10;
        return new Vector3(col * 0.38f - 1.71f, 0.05f + layer * 0.08f, row * 0.52f - 1.82f);
    }

    System.Collections.IEnumerator TickBills()
    {
        billRoutineRunning = true;
        Material green = MatLib.Get(new Color(0.32f, 0.8f, 0.42f));
        Material greenD = MatLib.Get(new Color(0.24f, 0.65f, 0.34f));
        while (bills.Count < PileAmount)
        {
            int idx = bills.Count;
            Vector3 slot = BillSlot(idx);
            // cash model from the PinkTea safe pack; fallback: green cube
            GameObject b = AssetLib.SpawnMoneyBill(pileRoot);
            if (b != null)
            {
                b.transform.localPosition = slot + Vector3.up * 1.5f;
                b.transform.localEulerAngles = new Vector3(0f, Random.Range(-4f, 4f), 0f);
            }
            else
            {
                b = B.Prim(PrimitiveType.Cube, "Bill", pileRoot, slot + Vector3.up * 1.5f,
                    new Vector3(0f, Random.Range(-4f, 4f), 0f), new Vector3(0.58f, 0.06f, 0.94f),
                    (idx % 2 == 0) ? green : greenD);
            }
            bills.Add(b);
            StartCoroutine(DropBill(b.transform, slot));
            Sfx.Play(Snd.Tick, 0.25f);
            yield return new WaitForSeconds(0.018f);
        }
        billRoutineRunning = false;
    }

    System.Collections.IEnumerator DropBill(Transform b, Vector3 to)
    {
        float t = 0f;
        Vector3 from = b != null ? b.localPosition : Vector3.zero;
        while (t < 1f && b != null)
        {
            t += Time.deltaTime * 6f;
            b.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }

    public int StealAll()
    {
        int amount = PileAmount;
        PileAmount = 0;
        ClearBills();
        return amount;
    }

    void ClearBills()
    {
        for (int i = 0; i < bills.Count; i++) if (bills[i] != null) Destroy(bills[i]);
        bills.Clear();
        billRoutineRunning = false;
        if (pileText != null) pileText.text = "";
    }

    void Update()
    {
        if (Game.player == null) return;
        playerAtSpot = Vector3.Distance(Game.player.transform.position, OperatorSpot) < 2.2f;

        if (needText != null)
            needText.text = (queue.Count > 0 && !HasOperator) ? "Kasaya birisi lazim!" : "";

        if (PileAmount > 0 && Vector3.Distance(Game.player.transform.position, pileRoot.position) < 2.8f)
        {
            Game.gm.AddMoney(PileAmount);
            if (Game.ui != null) Game.ui.MoneyPunch();
            PileAmount = 0;
            Sfx.Play(Snd.MoneyPickup, 0.85f);
            ClearBills();
        }
    }
}
