using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const int TrashCarryLimit = 5;
    public Transform stackAnchor;
    public Transform visual;
    public Jetski jetski; // mounted jetski (bought on the PC)
    public bool Swimming { get; private set; }

    CharacterController cc;
    List<Fish> carried = new List<Fish>();
    List<TrashItem> heldTrash = new List<TrashItem>();
    GameObject trashGuideArrow;
    TextMesh carryText;
    float depositTimer;
    bool wasSwimming;
    float attackCooldown;
    bool deskSeated;
    float deskInteractionBlockedUntil;
    bool dumpingTrash;

    // radar
    Fish target;
    float scanProgress;
    Transform fan;
    Transform barBg, barFill;

    // ramp / shark launch
    bool launched;
    Vector3 launchFrom, launchTo;
    float launchT;
    float launchHeight = 4f;

    // joystick
    Vector2 dragOrigin;
    bool dragging;

    public int CarryCount { get { return carried.Count; } }
    public int HeldTrashCount { get { return heldTrash.Count; } }
    public bool IsFull { get { return carried.Count >= Game.gm.Capacity; } }
    public bool CanUseDeskInteraction { get { return !deskSeated && Time.unscaledTime >= deskInteractionBlockedUntil; } }

    public void SitAtDesk(Vector3 seatPosition, Quaternion deskRotation)
    {
        if (!CanUseDeskInteraction) return;
        deskSeated = true;
        if (cc != null) cc.enabled = false;
        transform.position = seatPosition;
        transform.rotation = deskRotation;
        if (visual != null)
        {
            visual.localPosition = new Vector3(0f, -0.42f, 0f);
            visual.localRotation = Quaternion.identity;
        }
    }

    public void LeaveDesk()
    {
        deskSeated = false;
        deskInteractionBlockedUntil = Time.unscaledTime + 0.35f;
        if (visual != null)
        {
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
        }
        if (cc != null) cc.enabled = true;
    }

    public void StandAtRegister(Vector3 operatorPosition, Vector3 registerPosition)
    {
        if (cc != null) cc.enabled = false;
        transform.position = operatorPosition;
        if (cc != null) cc.enabled = true;
        Vector3 face = registerPosition - operatorPosition;
        face.y = 0f;
        if (face.sqrMagnitude > 0.01f && visual != null)
            visual.rotation = Quaternion.LookRotation(face.normalized, Vector3.up);
    }

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
        if (ControlBindings.Held(ControlAction.Left) || Input.GetKey(KeyCode.LeftArrow)) mv.x -= 1f;
        if (ControlBindings.Held(ControlAction.Right) || Input.GetKey(KeyCode.RightArrow)) mv.x += 1f;
        if (ControlBindings.Held(ControlAction.Forward) || Input.GetKey(KeyCode.UpArrow)) mv.y += 1f;
        if (ControlBindings.Held(ControlAction.Backward) || Input.GetKey(KeyCode.DownArrow)) mv.y -= 1f;

        // drag joystick only in top-down mode
        if (Game.cam == null || !Game.cam.IsTPS)
        {
            int moveButton = ControlBindings.MoveMouseButton;
            if (Input.GetMouseButtonDown(moveButton)) { dragging = true; dragOrigin = Input.mousePosition; }
            if (Input.GetMouseButtonUp(moveButton)) dragging = false;
            if (dragging && Input.GetMouseButton(moveButton))
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

    public void LaunchTo(Vector3 to, float height = 4f)
    {
        launched = true;
        launchFrom = transform.position;
        launchTo = to;
        launchT = 0f;
        launchHeight = height;
        Sfx.Play(Snd.Throw, 0.75f);
    }

    // Shark attack: lose carried fish + some money, get tossed back to the beach.
    public void SharkHit()
    {
        Jetski hitJetski = jetski;
        if (hitJetski != null) hitJetski.BreakFromShark();
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
        attackCooldown -= dt;

        if (launched)
        {
            launchT += dt / 0.9f;
            if (launchT >= 1f) launched = false;
            cc.enabled = false;
            transform.position = B.Parabola(launchFrom, launchTo, launchHeight, Mathf.Clamp01(launchT));
            cc.enabled = true;
            visual.Rotate(Vector3.right, 500f * dt);
            return;
        }

        if (deskSeated)
        {
            // The management UI owns input while seated; do not feed gravity
            // or movement into the temporarily disabled controller.
            UpdateCarryText();
            return;
        }

        Swimming = Game.sea != null && Game.sea.Contains(transform.position);

        // entering the sea after midnight = shark-infested waters!
        if (Swimming && !wasSwimming && Game.sea.SharkNight && Game.ui != null)
            Game.ui.Toast("GECE DENIZI TEHLIKELI! Sadece kopekbaliklari var, cikmadan saldirirlar!");
        if (Swimming && !wasSwimming) Sfx.Play(Snd.Splash, 0.45f);

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
        float speed = Swimming ? (jetski != null ? Game.gm.MoveSpeed * Game.gm.JetskiSpeedMultiplier : Game.gm.SwimSpeed) : Game.gm.MoveSpeed;
        if (!Swimming && Game.gm.SprintUnlocked && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            speed *= Game.gm.SprintMultiplier;
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
        UpdateAttack();
        UpdateDeposits(dt);
        UpdateTrash();
        UpdateInteractions();
        UpdateCarryText();
    }

    void UpdateAttack()
    {
        bool punchPressed = Input.GetMouseButtonDown(ControlBindings.PunchMouseButton) || ControlBindings.Down(ControlAction.Punch);
        if (attackCooldown > 0f || !punchPressed ||
            (Game.ui != null && Game.ui.AnyMenuOpen)) return;
        attackCooldown = Game.gm != null && Game.gm.activePlayerWeapon == 2 ? 0.62f : 0.48f;
        StartCoroutine(PunchVisual());

        Vector3 facing = visual != null ? visual.forward : transform.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude < 0.01f) facing = transform.forward;
        facing.Normalize();

        // A selected gun never auto-locks to a nearby target. Every press fires
        // a real projectile exactly where the player is looking, with full
        // friendly-fire consequences for customers and the player's own tanks.
        if (Game.gm != null && Game.gm.activePlayerWeapon == 2)
        {
            PlayerBullet.Spawn(transform.position + Vector3.up * 1.3f + facing * 0.75f, facing);
            return;
        }
        float best = Game.gm != null ? Game.gm.PlayerWeaponRange : 2.8f;
        Thief hitThief = null;
        Customer hitCustomer = null;
        Thief[] thieves = FindObjectsByType<Thief>(FindObjectsSortMode.None);
        for (int i = 0; i < thieves.Length; i++)
        {
            if (!thieves[i].IsRevealed) continue;
            Vector3 to = thieves[i].transform.position - transform.position; to.y = 0f;
            float d = to.magnitude;
            if (d < best && (d < 1f || Vector3.Angle(facing, to) < 75f))
            { best = d; hitThief = thieves[i]; hitCustomer = null; }
        }
        Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
        for (int i = 0; i < customers.Length; i++)
        {
            Vector3 to = customers[i].transform.position - transform.position; to.y = 0f;
            float d = to.magnitude;
            if (d < best && (d < 1f || Vector3.Angle(facing, to) < 75f))
            { best = d; hitCustomer = customers[i]; hitThief = null; }
        }

        if (hitThief != null) hitThief.PlayerHit(transform.position, Game.gm != null ? Game.gm.PlayerWeaponDamage : 1);
        else if (hitCustomer != null) hitCustomer.HitByPlayer(transform.position);
        else Sfx.Play(Snd.Throw, 0.28f);
    }

    System.Collections.IEnumerator PunchVisual()
    {
        int weapon = Game.gm != null ? Game.gm.activePlayerWeapon : -1;
        PrimitiveType primitive = weapon < 0 ? PrimitiveType.Sphere : PrimitiveType.Cube;
        Vector3 scale = weapon < 0 ? Vector3.one * 0.34f : weapon == 0 ? new Vector3(0.16f, 0.16f, 1.15f) : weapon == 1 ? new Vector3(0.12f, 0.08f, 0.82f) : new Vector3(0.28f, 0.22f, 0.72f);
        Color color = weapon < 0 ? new Color(1f, 0.72f, 0.55f) : weapon == 0 ? new Color(0.42f, 0.24f, 0.1f) : weapon == 1 ? new Color(0.82f, 0.9f, 0.96f) : new Color(0.12f, 0.14f, 0.18f);
        GameObject fist = B.Prim(primitive, weapon < 0 ? "Punch" : "EquippedWeapon", visual, new Vector3(0.45f, 1.3f, 0.35f), Vector3.zero, scale, MatLib.Get(color));
        float t = 0f;
        while (t < 1f && fist != null)
        {
            t += Time.deltaTime * (weapon == 2 ? 10f : 7f);
            float reach = weapon == 2 ? 1.6f : 1.15f;
            fist.transform.localPosition = new Vector3(0.45f, 1.3f, 0.35f + Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * reach);
            if (weapon == 0 || weapon == 1) fist.transform.localRotation = Quaternion.Euler(Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * 55f, 0f, 0f);
            yield return null;
        }
        if (fist != null) Destroy(fist);
    }

    // ---------- E interactions + prompt ----------
    void UpdateInteractions()
    {
        if (Game.ui == null) return;
        string prompt = null;

        if (Game.managerDesk != null && Game.managerDesk.PlayerNear &&
            PlayerPrefs.GetInt("AT3_EndDayDeskGuideActive", 0) == 1)
            Game.ui.CompleteEndDayDeskGuide();

        bool menuOpen = Game.ui.AnyMenuOpen;
        if (!menuOpen)
        {
            string interactKey = ControlBindings.KeyName(ControlAction.Interact);
            if (Game.generator != null && Game.generator.CanUse && Game.generator.PlayerNear(transform.position))
                prompt = interactKey + " : Jeneratoru calistir";
            else if (Game.ramp != null && Game.ramp.Broken && Game.ramp.PlayerNear(transform.position))
                prompt = interactKey + " : Rampayi tamir et  $" + B.Money(Game.ramp.RepairCost);
            else if (Game.jetski != null && Game.jetski.Broken && Game.jetski.PlayerNear(transform.position))
                prompt = interactKey + " : Jetski tamir et  $" + B.Money(Game.jetski.RepairCost);
            else if (Game.cameraDesk != null && Game.cameraDesk.PlayerNear(transform.position))
                prompt = interactKey + " : Guvenlik kameralarini izle";
            else if (Game.managerDesk != null && Game.managerDesk.PlayerNear)
                prompt = interactKey + " : Yonetim PC'sini Ac";
            else if (Game.register != null && Game.register.PlayerNear(transform.position))
                prompt = interactKey + " : Odeme noktasinda bekle";
            else if (GameBootstrap.NearGateSign(transform.position))
                prompt = Game.gm.shopOpen ? interactKey + " : Dukkani KAPAT" : interactKey + " : Dukkani AC";
        }
        Game.ui.SetPrompt(prompt);

        if (prompt != null && ControlBindings.Down(ControlAction.Interact))
        {
            if (Game.generator != null && Game.generator.CanUse && Game.generator.PlayerNear(transform.position))
            {
                Game.generator.StartByPlayer();
            }
            else if (Game.ramp != null && Game.ramp.Broken && Game.ramp.PlayerNear(transform.position))
            {
                if (!Game.ramp.TryRepair()) Game.ui.Toast("Rampa tamiri icin yeterli paran yok.");
            }
            else if (Game.jetski != null && Game.jetski.Broken && Game.jetski.PlayerNear(transform.position))
            {
                Game.jetski.TryRepair();
            }
            else if (Game.cameraDesk != null && Game.cameraDesk.PlayerNear(transform.position))
            {
                Game.ui.SetPrompt(null);
                Game.cameraDesk.OpenViewer();
            }
            else if (Game.managerDesk != null && Game.managerDesk.PlayerNear)
            {
                Game.ui.SetPrompt(null);
                Game.ui.CompleteEndDayDeskGuide();
                Game.managerDesk.SeatPlayer();
                Game.cam.ZoomToPC(delegate { Game.pc.Open(); });
            }
            else if (Game.register != null && Game.register.PlayerNear(transform.position))
            {
                StandAtRegister(Game.register.OperatorSpot, Game.register.transform.position);
                if (Game.register.QueueCount == 0)
                    Game.ui.Toast("Bekleyen musteri yok; kasada bekliyorsun.", 3f);
                else
                    Game.ui.Toast("Odeme aliniyor; kasada bekle.", 3f);
            }
            else
            {
                Game.gm.shopOpen = !Game.gm.shopOpen;
                GameBootstrap.UpdateGateBarrier();
                Game.ui.Toast(Game.gm.shopOpen ? "Dukkan ACILDI! Musteriler gelebilir." : "Dukkan KAPATILDI. Musteri gelmeyecek.");
                Sfx.Play(Snd.ShopToggle, 0.65f);
            }
        }
    }

    // ---------- radar catching ----------
    // The radar looks where the PLAYER faces (not auto-lock): only fish inside
    // the aim cone can be scanned, so aiming matters.
    void UpdateRadar(float dt)
    {
        // Golden fish is an instant touch bonus and never needs an empty bag
        // slot. This also fixes the old full-bag/radar deadlock.
        if (Swimming && Game.sea != null)
        {
            Fish golden = Game.sea.FindGoldenNear(transform.position, 2.1f);
            if (golden != null)
            {
                if (target == golden) { target.scanner = null; target = null; scanProgress = 0f; }
                Game.sea.Remove(golden);
                int bonus = 100 * Game.gm.Level;
                Game.gm.AddMoney(bonus);
                Game.ui.MoneyPunch();
                Game.ui.Toast("ALTIN BALIK! +$" + B.Money(bonus));
                Sfx.Play(Snd.Collect, 1f);
                golden.Die();
            }
        }
        bool canScan = Swimming && !IsFull && (Game.ui == null || !Game.ui.AnyMenuOpen);

        // aim = facing direction
        Vector3 aim = visual.forward;
        aim.y = 0f;
        if (aim.sqrMagnitude < 0.01f) aim = Vector3.forward;
        aim.Normalize();

        Fish newTarget = null;
        if (canScan)
        {
            float range = Game.gm.RadarRange;
            bool autoRadar = Game.gm.techOwned[4] && Game.gm.techEnabled[4];
            
            // keep the current target while it stays roughly inside the cone (or just inside range if auto-radar is on)
            if (target != null && target.state == Fish.State.Wild &&
                Vector3.Distance(transform.position, target.transform.position) <= range * 1.15f &&
                (autoRadar || Vector3.Angle(aim, Flat(target.transform.position - transform.position)) < 45f))
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
        
        // cone visual follows the aim (or locks to target if auto-radar is on)
        fan.gameObject.SetActive(canScan);
        if (canScan)
        {
            float r = Game.gm.RadarRange;
            fan.position = transform.position + Vector3.up * 0.62f;
            fan.localScale = new Vector3(r, 1f, r);
            
            if (target != null && Game.gm.techOwned[4] && Game.gm.techEnabled[4])
            {
                Vector3 toTarget = Flat(target.transform.position - transform.position);
                if (toTarget.sqrMagnitude > 0.01f)
                    fan.rotation = Quaternion.LookRotation(toTarget.normalized);
            }
            else
            {
                fan.rotation = Quaternion.LookRotation(aim);
            }
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
            if (!t.ReserveSlot()) continue;
            depositTimer = 0.13f;
            Fish f = carried[idx];
            carried.RemoveAt(idx);
            Reindex();
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

        if (!dumpingTrash && heldTrash.Count < TrashCarryLimit)
        {
            TrashItem t = Game.trash.FindNear(transform.position, 1.5f, Swimming);
            if (t != null)
            {
                Game.trash.PickUp(t);
                heldTrash.Add(t);
                t.AttachTo(stackAnchor, carried.Count + heldTrash.Count - 1);
                Sfx.Play(Snd.TrashPickup, 0.45f);
                StartTrashGuideIfNeeded();
            }
        }

        if (!dumpingTrash && heldTrash.Count > 0 && Vector3.Distance(transform.position, Game.trash.BinPos) < 2.4f)
        {
            StartCoroutine(DumpTrashOneByOne());
        }

        UpdateTrashGuide();
    }

    System.Collections.IEnumerator DumpTrashOneByOne()
    {
        dumpingTrash = true;
        int count = heldTrash.Count;
        QuestSystem.OnTrash(count);
        while (heldTrash.Count > 0)
        {
            int index = heldTrash.Count - 1;
            TrashItem item = heldTrash[index];
            heldTrash.RemoveAt(index);
            if (item != null) item.DumpInto(Game.trash.BinPos);
            Sfx.Play(Snd.TrashDump, 0.42f);
            yield return new WaitForSeconds(0.16f);
        }
        dumpingTrash = false;
        CompleteTrashGuide();
    }

    void StartTrashGuideIfNeeded()
    {
        if (PlayerPrefs.GetInt("AT3_BinTutorialDone", 0) == 1) return;
        // Direction is now communicated solely by the screen-edge HUD guide.
        // Remove a world arrow left alive by an in-Editor script reload.
        if (trashGuideArrow != null) { Destroy(trashGuideArrow); trashGuideArrow = null; }
    }

    void UpdateTrashGuide()
    {
        if (Game.trash == null) return;
        if (trashGuideArrow != null) { Destroy(trashGuideArrow); trashGuideArrow = null; }
    }

    void CompleteTrashGuide()
    {
        // The bin guide is learned after the FIRST successful dump and must
        // never appear again on this save.
        if (PlayerPrefs.GetInt("AT3_BinTutorialDone", 0) == 0)
        {
            PlayerPrefs.SetInt("AT3_BinTutorialDone", 1);
            if (trashGuideArrow != null) Destroy(trashGuideArrow);
        }

        // Camera controls remain a separate lesson: show them only after every
        // piece of the starter cleanup has been collected and dumped.
        if (Game.trash != null && Game.trash.Count == 0 &&
            PlayerPrefs.GetInt("AT3_StarterCleanupDone", 0) == 0)
        {
            PlayerPrefs.SetInt("AT3_StarterCleanupDone", 1);
            if (Game.ui != null) Game.ui.ShowCameraTutorial();
        }
        PlayerPrefs.Save();
    }
}

public class PlayerBullet : MonoBehaviour
{
    Vector3 velocity;
    float life = 3.5f;

    public static void Spawn(Vector3 from, Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.forward;
        direction.Normalize();
        GameObject go = B.Prim(PrimitiveType.Sphere, "PlayerBullet", null, from, Vector3.zero, Vector3.one * 0.2f,
            MatLib.Get(new Color(1f, 0.82f, 0.16f)));
        PlayerBullet bullet = go.AddComponent<PlayerBullet>();
        bullet.velocity = direction * 25f;
        Sfx.Play(Snd.Alarm, 0.32f);
    }

    void Update()
    {
        if (Game.gm == null) { Destroy(gameObject); return; }
        float dt = Time.deltaTime;
        Vector3 next = transform.position + velocity * dt;

        Thief[] thieves = FindObjectsByType<Thief>(FindObjectsSortMode.None);
        for (int i = 0; i < thieves.Length; i++)
        {
            if (thieves[i] != null && thieves[i].IsRevealed && DistanceToSegment(thieves[i].transform.position + Vector3.up, transform.position, next) < 0.9f)
            {
                thieves[i].PlayerHit(transform.position - velocity.normalized, Game.gm.PlayerWeaponDamage);
                Destroy(gameObject);
                return;
            }
        }

        Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
        for (int i = 0; i < customers.Length; i++)
        {
            if (customers[i] != null && !customers[i].IsDead && DistanceToSegment(customers[i].transform.position + Vector3.up, transform.position, next) < 0.85f)
            {
                customers[i].HitByPlayer(transform.position - velocity.normalized);
                if (Game.ui != null) Game.ui.Toast("Yanlislikla musteriye ates ettin! 1 yildizli yorum birakti.", 4f);
                Destroy(gameObject);
                return;
            }
        }

        for (int i = 0; i < Game.tanks.Count; i++)
        {
            Tank tank = Game.tanks[i];
            if (tank != null && !tank.broken && DistanceToSegment(tank.transform.position + Vector3.up * 1.2f, transform.position, next) < 2.05f)
            {
                tank.Break(); // the player's own bullet breaks it immediately
                if (Game.ui != null) Game.ui.Toast("Kendi akvaryumunu vurdun! Akvaryum kirildi.", 4f);
                Destroy(gameObject);
                return;
            }
        }

        RaycastHit hit;
        if (Physics.Linecast(transform.position, next, out hit))
        {
            Tank tank = hit.collider != null ? hit.collider.GetComponentInParent<Tank>() : null;
            if (tank != null && !tank.broken)
            {
                tank.Break();
                if (Game.ui != null) Game.ui.Toast("Kendi akvaryumunu vurdun! Akvaryum kirildi.", 4f);
            }
            Destroy(gameObject);
            return;
        }

        transform.position = next;
        life -= dt;
        if (life <= 0f) Destroy(gameObject);
    }

    static float DistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float denominator = ab.sqrMagnitude;
        if (denominator < 0.0001f) return Vector3.Distance(point, a);
        float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / denominator);
        return Vector3.Distance(point, a + ab * t);
    }
}

public class TrashBinGuideArrow : MonoBehaviour
{
    public Transform player;
    Transform arrowVisual;
    TextMesh label;

    void Start()
    {
        // A guaranteed high-contrast arrow. The former catalog prefab had an
        // incompatible pivot/orientation and could be invisible from top-down.
        arrowVisual = new GameObject("ArrowVisual").transform;
        arrowVisual.SetParent(transform, false);
        Material yellow = MatLib.Get(new Color(1f, 0.78f, 0.08f));
        Material dark = MatLib.Get(new Color(0.12f, 0.2f, 0.16f));
        B.Prim(PrimitiveType.Cube, "ArrowShadow", arrowVisual, new Vector3(0f, -0.09f, 0.45f), Vector3.zero,
            new Vector3(0.34f, 0.09f, 1.5f), dark);
        B.Prim(PrimitiveType.Cube, "Shaft", arrowVisual, new Vector3(0f, 0f, 0.4f), Vector3.zero,
            new Vector3(0.28f, 0.18f, 1.35f), yellow);
        B.Prim(PrimitiveType.Cube, "HeadL", arrowVisual, new Vector3(0.38f, 0f, 1.08f), new Vector3(0f, 45f, 0f),
            new Vector3(0.27f, 0.18f, 0.85f), yellow);
        B.Prim(PrimitiveType.Cube, "HeadR", arrowVisual, new Vector3(-0.38f, 0f, 1.08f), new Vector3(0f, -45f, 0f),
            new Vector3(0.27f, 0.18f, 0.85f), yellow);
        label = B.Text3D("", transform, new Vector3(0f, 1.05f, 0f), 0.115f, Color.white);
    }

    void Update()
    {
        if (player == null || Game.trash == null) { Destroy(gameObject); return; }
        Vector3 to = Game.trash.BinPos - player.position;
        to.y = 0f;
        Vector3 dir = to.sqrMagnitude > 0.01f ? to.normalized : Vector3.forward;
        float travel = Mathf.PingPong(Time.time * 1.8f, 0.8f);
        transform.position = player.position + Vector3.up * (4.1f + Mathf.Sin(Time.time * 4f) * 0.22f) + dir * travel;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        if (arrowVisual != null)
            arrowVisual.localScale = Vector3.one * (1.25f + Mathf.Sin(Time.time * 6f) * 0.16f);
        if (label != null) label.text = Loc.T("TRASH_BIN") + "  " + Mathf.CeilToInt(to.magnitude) + "m";
    }
}

// Purchased technology: exists only while the event's golden fish is still
// swimming free, so the screen cleans itself up as soon as it is caught/lost.
public class GoldenFishGuideArrow : MonoBehaviour
{
    Fish target;
    Transform arrowVisual;
    TextMesh label;

    public static void Create(Fish fish)
    {
        if (fish == null || Game.player == null) return;
        GoldenFishGuideArrow old = FindFirstObjectByType<GoldenFishGuideArrow>();
        if (old != null) Destroy(old.gameObject);
        GameObject go = new GameObject("GoldenFishGuideArrow");
        GoldenFishGuideArrow guide = go.AddComponent<GoldenFishGuideArrow>();
        guide.target = fish;
    }

    void Start()
    {
        arrowVisual = new GameObject("ArrowVisual").transform;
        arrowVisual.SetParent(transform, false);
        Material gold = MatLib.Get(new Color(1f, 0.68f, 0.03f));
        Material glow = MatLib.Get(new Color(1f, 0.95f, 0.25f));
        B.Prim(PrimitiveType.Cube, "Shaft", arrowVisual, new Vector3(0f, 0f, 0.4f), Vector3.zero,
            new Vector3(0.3f, 0.2f, 1.4f), gold);
        B.Prim(PrimitiveType.Cube, "HeadL", arrowVisual, new Vector3(0.4f, 0f, 1.1f), new Vector3(0f, 45f, 0f),
            new Vector3(0.3f, 0.2f, 0.9f), glow);
        B.Prim(PrimitiveType.Cube, "HeadR", arrowVisual, new Vector3(-0.4f, 0f, 1.1f), new Vector3(0f, -45f, 0f),
            new Vector3(0.3f, 0.2f, 0.9f), glow);
        label = B.Text3D("", transform, new Vector3(0f, 1.05f, 0f), 0.115f, new Color(1f, 0.9f, 0.2f));
        label.fontStyle = FontStyle.Bold;
    }

    void Update()
    {
        if (Game.player == null || target == null || !target.golden || target.state != Fish.State.Wild)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 to = target.transform.position - Game.player.transform.position;
        to.y = 0f;
        Vector3 dir = to.sqrMagnitude > 0.01f ? to.normalized : Vector3.forward;
        float travel = Mathf.PingPong(Time.time * 2.1f, 0.9f);
        transform.position = Game.player.transform.position + Vector3.up * (4.2f + Mathf.Sin(Time.time * 4.5f) * 0.25f) + dir * travel;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        if (arrowVisual != null) arrowVisual.localScale = Vector3.one * (1.2f + Mathf.Sin(Time.time * 7f) * 0.15f);
        if (label != null) label.text = "ALTIN BALIK  " + Mathf.CeilToInt(to.magnitude) + "m";
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
