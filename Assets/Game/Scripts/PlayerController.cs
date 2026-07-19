using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform stackAnchor;
    public Transform visual;
    public Jetski jetski; // mounted jetski (bought on the PC)
    public bool Swimming { get; private set; }

    CharacterController cc;
    List<Fish> carried = new List<Fish>();
    List<TrashItem> heldTrash = new List<TrashItem>();
    TextMesh carryText;
    float depositTimer;
    bool wasSwimming;

    // radar
    Fish target;
    float scanProgress;
    Transform fan;
    Transform barBg, barFill;

    // ramp / shark launch
    bool launched;
    Vector3 launchFrom, launchTo;
    float launchT;

    // joystick
    Vector2 dragOrigin;
    bool dragging;

    public int CarryCount { get { return carried.Count; } }
    public bool IsFull { get { return carried.Count >= Game.gm.Capacity; } }

    public static PlayerController Create(Vector3 pos)
    {
        GameObject go = new GameObject("Player");
        go.transform.position = pos;
        go.tag = "Player";
        PlayerController p = go.AddComponent<PlayerController>();
        Game.player = p;
        return p;
    }

    void Awake()
    {
        cc = gameObject.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.45f;
        cc.center = new Vector3(0f, 1f, 0f);

        GameObject v = new GameObject("Visual");
        v.transform.SetParent(transform, false);
        visual = v.transform;

        // cute capsule character (like the reference game)
        Material suit = MatLib.Get(new Color(1f, 0.45f, 0.3f));
        Material pack = MatLib.Get(new Color(0.95f, 0.6f, 0.15f));
        Material head = MatLib.Get(new Color(1f, 0.35f, 0.3f));
        B.Prim(PrimitiveType.Capsule, "Body", visual, new Vector3(0f, 1f, 0f), Vector3.zero, new Vector3(0.85f, 0.72f, 0.85f), suit);
        B.Prim(PrimitiveType.Sphere, "Head", visual, new Vector3(0f, 2f, 0f), Vector3.zero, Vector3.one * 0.65f, head);
        B.Prim(PrimitiveType.Cube, "Backpack", visual, new Vector3(0f, 1.2f, -0.42f), Vector3.zero, new Vector3(0.55f, 0.7f, 0.3f), pack);
        B.Prim(PrimitiveType.Cube, "Snorkel", visual, new Vector3(0.3f, 2.25f, 0f), Vector3.zero, new Vector3(0.08f, 0.5f, 0.08f), MatLib.Get(new Color(0.2f, 0.5f, 1f)));

        GameObject anchor = new GameObject("StackAnchor");
        anchor.transform.SetParent(transform, false);
        anchor.transform.localPosition = new Vector3(0f, 2.7f, 0f);
        stackAnchor = anchor.transform;

        carryText = B.Text3D("", transform, new Vector3(0f, 3.4f, 0f), 0.1f, Color.white);
        BuildRadarVisuals();

        // compass / navigation arrow (unlocked via PC technology)
        GameObject comp = new GameObject("Compass");
        comp.transform.SetParent(transform, false);
        comp.transform.localPosition = new Vector3(0f, 4f, 0f);
        comp.AddComponent<CompassArrow>();
    }

    void BuildRadarVisuals()
    {
        GameObject fanGo = new GameObject("RadarFan");
        MeshFilter mf = fanGo.AddComponent<MeshFilter>();
        mf.mesh = B.FanMesh(55f, 1f);
        MeshRenderer mr = fanGo.AddComponent<MeshRenderer>();
        mr.sharedMaterial = MatLib.Glass(new Color(1f, 0.25f, 0.25f, 0.35f));
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        fan = fanGo.transform;
        fan.gameObject.SetActive(false);

        GameObject barRoot = new GameObject("ScanBar");
        barBg = B.Prim(PrimitiveType.Cube, "Bg", barRoot.transform, Vector3.zero, Vector3.zero, new Vector3(1.2f, 0.18f, 0.02f), MatLib.Get(Color.white)).transform;
        barFill = B.Prim(PrimitiveType.Cube, "Fill", barRoot.transform, new Vector3(-0.6f, 0f, -0.02f), Vector3.zero, new Vector3(0.01f, 0.14f, 0.02f), MatLib.Get(new Color(0.3f, 0.9f, 0.3f))).transform;
        barRoot.AddComponent<Billboard>();
        barBg.parent.gameObject.SetActive(false);
    }

    void UpdateCarryText()
    {
        if (carryText == null || Game.gm == null) return;
        carryText.text = carried.Count > 0 ? carried.Count + "/" + Game.gm.Capacity : "";
        carryText.color = IsFull ? new Color(1f, 0.45f, 0.35f) : Color.white;
    }

    Vector2 ReadInput()
    {
        if (Game.ui != null && Game.ui.AnyMenuOpen) return Vector2.zero;
        Vector2 mv = Vector2.zero;
        mv.x = Input.GetAxisRaw("Horizontal");
        mv.y = Input.GetAxisRaw("Vertical");

        // drag joystick only in top-down mode
        if (Game.cam == null || !Game.cam.IsTPS)
        {
            if (Input.GetMouseButtonDown(0)) { dragging = true; dragOrigin = Input.mousePosition; }
            if (Input.GetMouseButtonUp(0)) dragging = false;
            if (dragging && Input.GetMouseButton(0))
            {
                Vector2 delta = (Vector2)Input.mousePosition - dragOrigin;
                float max = Screen.height * 0.12f;
                if (delta.magnitude > max) dragOrigin = (Vector2)Input.mousePosition - delta.normalized * max;
                Vector2 j = delta / max;
                if (j.magnitude > 0.15f) mv += j;
            }
        }
        return Vector2.ClampMagnitude(mv, 1f);
    }

    public void LaunchTo(Vector3 to)
    {
        launched = true;
        launchFrom = transform.position;
        launchTo = to;
        launchT = 0f;
        Sfx.Play(Snd.Splash, 0.6f);
    }

    // Shark attack: lose carried fish + some money, get tossed back to the beach.
    public void SharkHit()
    {
        for (int i = 0; i < carried.Count; i++)
            if (carried[i] != null) carried[i].Die();
        carried.Clear();
        int loss = Mathf.Min(Game.gm.Money, 30 * Game.gm.Level);
        if (loss > 0) Game.gm.SpendTick(loss);
        Game.ui.Toast("KOPEKBALIGI SALDIRDI! Baliklar kacti" + (loss > 0 ? ", -$" + B.Money(loss) + " kayip!" : "!"));
        if (Game.cam != null) Game.cam.Shake(0.8f, 0.6f);
        LaunchTo(new Vector3(22f, 0f, Mathf.Clamp(transform.position.z, -4f, 44f)));
        UpdateCarryText();
    }

    void Update()
    {
        if (Game.gm == null) return;
        float dt = Time.deltaTime;

        if (launched)
        {
            launchT += dt / 0.9f;
            if (launchT >= 1f) launched = false;
            cc.enabled = false;
            transform.position = B.Parabola(launchFrom, launchTo, 4f, Mathf.Clamp01(launchT));
            cc.enabled = true;
            visual.Rotate(Vector3.right, 500f * dt);
            return;
        }

        Swimming = Game.sea != null && Game.sea.Contains(transform.position);

        // entering the sea after midnight = shark-infested waters!
        if (Swimming && !wasSwimming && Game.sea.SharkNight && Game.ui != null)
            Game.ui.Toast("GECE DENIZI TEHLIKELI! Sadece kopekbaliklari var, cikmadan saldirirlar!");

        // carrying trash into the sea pollutes it!
        if (Swimming && !wasSwimming && heldTrash.Count > 0)
        {
            int pieces = heldTrash.Count * 3;
            for (int i = 0; i < heldTrash.Count; i++)
                if (heldTrash[i] != null) Destroy(heldTrash[i].gameObject);
            heldTrash.Clear();
            Game.trash.ScatterIntoSea(transform.position, pieces);
        }
        wasSwimming = Swimming;

        Vector2 mv = ReadInput();
        Vector3 dir = new Vector3(mv.x, 0f, mv.y);
        if (Game.cam != null && Game.cam.IsTPS)
            dir = Quaternion.Euler(0f, Game.cam.TPSYaw, 0f) * dir; // camera-relative TPS controls
        float speed = Swimming ? Game.gm.SwimSpeed * (jetski != null ? 2.6f : 1f) : Game.gm.MoveSpeed;
        cc.Move((dir * speed + Vector3.down * 15f) * dt);

        if (dir.sqrMagnitude > 0.01f)
            visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(dir), 12f * dt);
        // swim tilt (upright while riding the jetski)
        Quaternion pose = (Swimming && jetski == null) ? Quaternion.Euler(40f, visual.eulerAngles.y, 0f) : Quaternion.Euler(0f, visual.eulerAngles.y, 0f);
        visual.rotation = Quaternion.Slerp(visual.rotation, pose, 6f * dt);
        // depth feel: the body sinks below the water surface while swimming
        float dipY = Swimming ? (jetski != null ? -0.35f : -1.05f) : 0f;
        Vector3 vlp = visual.localPosition;
        vlp.y = Mathf.Lerp(vlp.y, dipY, 6f * dt);
        visual.localPosition = vlp;

        UpdateRadar(dt);
        UpdateDeposits(dt);
        UpdateTrash();
        UpdateInteractions();
        UpdateCarryText();
    }

    // ---------- E interactions + prompt ----------
    void UpdateInteractions()
    {
        if (Game.ui == null) return;
        string prompt = null;

        bool menuOpen = Game.ui.AnyMenuOpen;
        if (!menuOpen)
        {
            if (Game.register != null && Game.register.PlayerAtSpot)
                prompt = "E : PC'yi Ac";
            else if (GameBootstrap.NearGateSign(transform.position))
                prompt = Game.gm.shopOpen ? "E : Dukkani KAPAT" : "E : Dukkani AC";
        }
        Game.ui.SetPrompt(prompt);

        if (prompt != null && Input.GetKeyDown(KeyCode.E))
        {
            if (Game.register != null && Game.register.PlayerAtSpot)
            {
                Game.ui.SetPrompt(null);
                Game.cam.ZoomToPC(delegate { Game.pc.Open(); });
            }
            else
            {
                Game.gm.shopOpen = !Game.gm.shopOpen;
                GameBootstrap.UpdateGateBarrier();
                Game.ui.Toast(Game.gm.shopOpen ? "Dukkan ACILDI! Musteriler gelebilir." : "Dukkan KAPATILDI. Musteri gelmeyecek.");
                Sfx.Play(Snd.Buy, 0.5f);
            }
        }
    }

    // ---------- radar catching ----------
    // The radar looks where the PLAYER faces (not auto-lock): only fish inside
    // the aim cone can be scanned, so aiming matters.
    void UpdateRadar(float dt)
    {
        bool canScan = Swimming && !IsFull && (Game.ui == null || !Game.ui.AnyMenuOpen);

        // aim = facing direction
        Vector3 aim = visual.forward;
        aim.y = 0f;
        if (aim.sqrMagnitude < 0.01f) aim = Vector3.forward;
        aim.Normalize();

        // cone visual follows the aim while swimming
        fan.gameObject.SetActive(canScan);
        if (canScan)
        {
            float r = Game.gm.RadarRange;
            fan.position = transform.position + Vector3.up * 0.62f;
            fan.rotation = Quaternion.LookRotation(aim);
            fan.localScale = new Vector3(r, 1f, r);
        }

        Fish newTarget = null;
        if (canScan)
        {
            float range = Game.gm.RadarRange;
            // keep the current target while it stays roughly inside the cone
            if (target != null && target.state == Fish.State.Wild &&
                Vector3.Distance(transform.position, target.transform.position) <= range * 1.15f &&
                Vector3.Angle(aim, Flat(target.transform.position - transform.position)) < 45f)
                newTarget = target;
            else
                newTarget = Game.sea.FindTargetInCone(transform.position, aim, range, 30f);
        }

        if (newTarget != target)
        {
            if (target != null) target.scanner = null;
            target = newTarget;
            scanProgress = 0f;
        }

        bool active = target != null;
        barBg.parent.gameObject.SetActive(active);
        if (!active) return;

        target.scanner = transform;

        // locked species: show required level on the fish instead of progress
        bool locked = !target.golden && !Game.gm.IsUnlocked(target.species);
        if (locked)
        {
            target.SetBadgeVisible(true, SpeciesInfo.ReqLevel(target.species));
            barBg.parent.gameObject.SetActive(false);
            scanProgress = 0f;
            return;
        }
        barBg.parent.gameObject.SetActive(true);

        float time = SpeciesInfo.CatchTime(target.species) * Game.gm.CatchTimeMult;
        if (target.golden) time = 1.5f;
        scanProgress += dt / time;

        Transform barRoot = barBg.parent;
        barRoot.position = target.transform.position + Vector3.up * 1.1f;
        float w = Mathf.Clamp01(scanProgress) * 1.2f;
        barFill.localScale = new Vector3(w, 0.14f, 0.02f);
        barFill.localPosition = new Vector3(-0.6f + w * 0.5f, 0f, -0.02f);

        if (scanProgress >= 1f)
        {
            Fish f = target;
            f.scanner = null;
            target = null;
            scanProgress = 0f;
            Game.sea.Remove(f);
            if (f.golden)
            {
                int bonus = 100 * Game.gm.Level;
                Game.gm.AddMoney(bonus);
                Game.ui.MoneyPunch();
                Game.ui.Toast("ALTIN BALIK! +$" + B.Money(bonus));
                Sfx.Play(Snd.Collect, 1f);
                f.Die();
            }
            else Collect(f);
        }
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

    void Collect(Fish f)
    {
        f.FlyTo(stackAnchor.position + Vector3.up * (carried.Count * 0.32f), null, 0.3f);
        carried.Add(f);
        StartCoroutine(CarryAfter(f, 0.3f));
        Game.gm.MarkDiscovered(f.species); // VERITABANI entry
        QuestSystem.OnCatch();
        Sfx.Play(Snd.Catch);
    }

    System.Collections.IEnumerator CarryAfter(Fish f, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (f != null && carried.Contains(f))
            f.SetCarried(stackAnchor, carried.IndexOf(f));
    }

    // ---------- deposits ----------
    void UpdateDeposits(float dt)
    {
        depositTimer -= dt;
        if (depositTimer > 0f || carried.Count == 0) return;

        for (int i = 0; i < Game.tanks.Count; i++)
        {
            Tank t = Game.tanks[i];
            if (!t.HasSpace) continue;
            if (Vector3.Distance(transform.position, t.transform.position) > 4.2f) continue;
            int idx = FindCarried(t.species);
            if (idx < 0) continue;
            depositTimer = 0.13f;
            Fish f = carried[idx];
            carried.RemoveAt(idx);
            Reindex();
            t.ReserveSlot();
            Fish captured = f;
            Tank tank = t;
            f.FlyTo(t.RandomWaterPoint(), delegate { if (captured != null) tank.Receive(captured); }, 0.5f);
            return;
        }

        if (Game.depot != null && Vector3.Distance(transform.position, Game.depot.transform.position) < 3.5f && Game.depot.HasSpace)
        {
            depositTimer = 0.13f;
            int last = carried.Count - 1;
            Fish f = carried[last];
            carried.RemoveAt(last);
            Reindex();
            int sp = f.species;
            f.FlyTo(Game.depot.DropPoint(), delegate { Game.depot.StoreVisualArrived(sp); if (f != null) Destroy(f.gameObject); }, 0.5f);
        }
    }

    int FindCarried(int sp)
    {
        for (int i = carried.Count - 1; i >= 0; i--)
            if (carried[i].species == sp) return i;
        return -1;
    }

    void Reindex()
    {
        for (int i = 0; i < carried.Count; i++) carried[i].SetCarryIndex(i);
    }

    // ---------- compass / navigation tech ----------
    // (component lives on a child of the player)

    // ---------- trash ----------
    void UpdateTrash()
    {
        if (Game.trash == null) return;

        if (heldTrash.Count < 3)
        {
            TrashItem t = Game.trash.FindNear(transform.position, 1.5f, Swimming);
            if (t != null)
            {
                Game.trash.PickUp(t);
                heldTrash.Add(t);
                t.AttachTo(stackAnchor, carried.Count + heldTrash.Count - 1);
                Sfx.Play(Snd.Drop, 0.4f);
            }
        }

        if (heldTrash.Count > 0 && Vector3.Distance(transform.position, Game.trash.BinPos) < 2.4f)
        {
            QuestSystem.OnTrash(heldTrash.Count);
            for (int i = 0; i < heldTrash.Count; i++)
                if (heldTrash[i] != null) heldTrash[i].DumpInto(Game.trash.BinPos);
            heldTrash.Clear();
            Sfx.Play(Snd.Collect, 0.5f);
        }
    }
}

// PUSULA (tech 0): golden arrow above the player pointing at the NEAREST catchable fish.
// GELISMIS NAVIGASYON (tech 1): points at the MOST VALUABLE fish + shows distance.
public class CompassArrow : MonoBehaviour
{
    Transform arrowRoot;
    TextMesh distText;
    bool built;

    void Build()
    {
        built = true;
        arrowRoot = new GameObject("Arrow").transform;
        arrowRoot.SetParent(transform, false);
        Material gold = MatLib.Get(new Color(1f, 0.8f, 0.15f));
        B.Prim(PrimitiveType.Cube, "Shaft", arrowRoot, new Vector3(0f, 0f, 0.1f), Vector3.zero, new Vector3(0.14f, 0.14f, 0.7f), gold);
        B.Prim(PrimitiveType.Cube, "HeadL", arrowRoot, new Vector3(0.14f, 0f, 0.5f), new Vector3(0f, 45f, 0f), new Vector3(0.12f, 0.14f, 0.4f), gold);
        B.Prim(PrimitiveType.Cube, "HeadR", arrowRoot, new Vector3(-0.14f, 0f, 0.5f), new Vector3(0f, -45f, 0f), new Vector3(0.12f, 0.14f, 0.4f), gold);
        distText = B.Text3D("", transform, new Vector3(0f, 0.7f, 0f), 0.09f, new Color(1f, 0.9f, 0.4f));
    }

    void Update()
    {
        if (Game.gm == null || Game.sea == null) return;
        bool nav = Game.gm.techOwned[1];
        bool comp = Game.gm.techOwned[0];
        if (!nav && !comp)
        {
            if (built) { arrowRoot.gameObject.SetActive(false); distText.gameObject.SetActive(false); }
            return;
        }
        if (!built) Build();

        Fish target = Game.sea.CompassTarget(transform.position, nav);
        bool show = target != null;
        arrowRoot.gameObject.SetActive(show);
        distText.gameObject.SetActive(show && nav);
        if (!show) return;

        Vector3 to = target.transform.position - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 0.01f)
            arrowRoot.rotation = Quaternion.Slerp(arrowRoot.rotation, Quaternion.LookRotation(to.normalized), 8f * Time.deltaTime);
        if (nav)
            distText.text = SpeciesInfo.Name(target.species) + "  " + Mathf.RoundToInt(to.magnitude) + "m";
    }
}
