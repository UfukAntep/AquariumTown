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
        Material screen = MatLib.Get(new Color(0.35f, 0.85f, 0.45f));

        B.Prim(PrimitiveType.Cube, "Desk", transform, new Vector3(0f, 0.6f, 0f), Vector3.zero, new Vector3(3.2f, 1.2f, 1.2f), desk, true);
        B.Prim(PrimitiveType.Cube, "Top", transform, new Vector3(0f, 1.28f, 0f), Vector3.zero, new Vector3(3.4f, 0.16f, 1.4f), top);

        // Dedicated payment hardware: compact POS display, card reader, scanner
        // and receipt roll. The management PC lives on the separate manager desk.
        GameObject pos = new GameObject("PaymentPOS");
        pos.transform.SetParent(transform, false);
        pos.transform.localPosition = new Vector3(0.75f, 1.57f, 0f);
        pos.transform.localEulerAngles = new Vector3(-18f, 180f, 0f);
        B.Prim(PrimitiveType.Cube, "POSBody", pos.transform, Vector3.zero, Vector3.zero, new Vector3(0.62f, 0.48f, 0.12f), dark);
        B.Prim(PrimitiveType.Cube, "POSScreen", pos.transform, new Vector3(0f, 0.02f, -0.07f), Vector3.zero, new Vector3(0.48f, 0.29f, 0.02f), screen);
        B.Prim(PrimitiveType.Cube, "CardReader", transform, new Vector3(-0.15f, 1.45f, 0.3f), new Vector3(-8f, 0f, 0f), new Vector3(0.42f, 0.12f, 0.58f), dark);
        B.Prim(PrimitiveType.Cube, "Scanner", transform, new Vector3(-0.78f, 1.42f, 0.1f), Vector3.zero, new Vector3(0.52f, 0.08f, 0.52f), MatLib.Get(new Color(0.2f, 0.3f, 0.42f)));
        B.Prim(PrimitiveType.Cylinder, "ReceiptRoll", transform, new Vector3(1.18f, 1.48f, 0.15f), new Vector3(90f, 0f, 0f), new Vector3(0.18f, 0.16f, 0.18f), top);

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

// Separate management workstation. It is deliberately away from the checkout
// so operating the register and opening the management panel are distinct jobs.
public class ManagerDesk : MonoBehaviour
{
    public Transform monitor;
    public Vector3 InteractionSpot { get { return transform.position + new Vector3(0f, 0f, 1.8f); } }
    public bool PlayerNear
    {
        get { return Game.player != null && Vector3.Distance(Game.player.transform.position, InteractionSpot) < 2.2f; }
    }

    public static ManagerDesk Create(Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject("ManagerDesk");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        ManagerDesk desk = go.AddComponent<ManagerDesk>();
        desk.Build();
        Game.managerDesk = desk;
        return desk;
    }

    void Build()
    {
        Material wood = MatLib.Get(new Color(0.56f, 0.36f, 0.22f));
        Material woodTop = MatLib.Get(new Color(0.75f, 0.5f, 0.28f));
        Material dark = MatLib.Get(new Color(0.12f, 0.15f, 0.22f));
        Material blue = MatLib.Get(new Color(0.25f, 0.58f, 0.95f));
        Material chair = MatLib.Get(new Color(0.18f, 0.32f, 0.48f));

        B.Prim(PrimitiveType.Cube, "ManagerDeskBase", transform, new Vector3(0f, 0.62f, 0f), Vector3.zero, new Vector3(3.6f, 1.2f, 1.45f), wood, true);
        B.Prim(PrimitiveType.Cube, "ManagerDeskTop", transform, new Vector3(0f, 1.28f, 0f), Vector3.zero, new Vector3(3.85f, 0.16f, 1.7f), woodTop);
        B.Prim(PrimitiveType.Cube, "Drawer", transform, new Vector3(-1.15f, 0.78f, 0.74f), Vector3.zero, new Vector3(0.95f, 0.42f, 0.06f), woodTop);

        GameObject mon = new GameObject("ManagementMonitor");
        mon.transform.SetParent(transform, false);
        mon.transform.localPosition = new Vector3(0.45f, 1.82f, 0f);
        mon.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        B.Prim(PrimitiveType.Cube, "Frame", mon.transform, Vector3.zero, Vector3.zero, new Vector3(1.35f, 0.86f, 0.1f), dark);
        B.Prim(PrimitiveType.Cube, "Screen", mon.transform, new Vector3(0f, 0f, -0.06f), Vector3.zero, new Vector3(1.18f, 0.69f, 0.02f), blue);
        B.Prim(PrimitiveType.Cube, "Stand", mon.transform, new Vector3(0f, -0.56f, 0.05f), Vector3.zero, new Vector3(0.17f, 0.35f, 0.17f), dark);
        B.Prim(PrimitiveType.Cube, "Keyboard", transform, new Vector3(0.05f, 1.42f, 0.4f), new Vector3(-5f, 0f, 0f), new Vector3(1.05f, 0.06f, 0.38f), dark);
        B.Prim(PrimitiveType.Sphere, "Mouse", transform, new Vector3(0.82f, 1.43f, 0.4f), Vector3.zero, new Vector3(0.18f, 0.08f, 0.25f), dark);
        monitor = mon.transform;

        B.Prim(PrimitiveType.Cube, "ChairSeat", transform, new Vector3(0f, 0.48f, -1.25f), Vector3.zero, new Vector3(1.05f, 0.18f, 1f), chair);
        B.Prim(PrimitiveType.Cube, "ChairBack", transform, new Vector3(0f, 1.12f, -1.67f), new Vector3(-8f, 0f, 0f), new Vector3(1.05f, 1.15f, 0.16f), chair);
        B.Text3D("YONETIM", transform, new Vector3(0f, 2.85f, 0f), 0.1f, new Color(1f, 0.9f, 0.35f));
    }
}
