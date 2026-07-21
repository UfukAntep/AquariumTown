using System.Collections.Generic;
using UnityEngine;

// Hired staff. Roles: 0 Cashier, 1 Fisher, 2 Carrier, 3 Janitor, 4 SeaCleaner, 5 ToiletCleaner.
public class Staff : MonoBehaviour
{
    public int role;
    public bool hiredToday;

    enum S { Idle, MoveToWork, Working, MoveToDeliver, Delivering }
    S state = S.Idle;

    Transform visual, stackAnchor;
    List<Fish> carriedFish = new List<Fish>();
    List<TrashItem> carriedTrash = new List<TrashItem>();
    int carriedCrates;
    List<GameObject> crateVis = new List<GameObject>();
    int crateSpecies;
    Fish chaseTarget;
    Tank targetTank;
    Vector3 moveTarget;
    float timer;
    bool shiftVisible = true;
    bool shiftWorkDiscarded;
    float MoveSpeed { get { return 4.2f * (Game.gm != null ? Game.gm.StaffSpeedMultiplier(role) : 1f); } }
    public Transform ViewRoot { get { return visual != null ? visual : transform; } }
    public string CurrentTask
    {
        get
        {
            if (Game.gm != null && !Game.gm.StaffOnShift) return "Mesai disinda";
            switch (state)
            {
                case S.MoveToWork: return "Goreve gidiyor";
                case S.Working: return StaffInfo.Descs[role];
                case S.MoveToDeliver: return "Teslimata gidiyor";
                case S.Delivering: return "Teslim ediyor";
                default: return "Yeni gorev bekliyor";
            }
        }
    }

    public static Staff Create(int role, Vector3 pos, bool hiredToday = false)
    {
        GameObject go = new GameObject("Staff_" + StaffInfo.Names[role]);
        go.transform.position = pos;
        Staff w = go.AddComponent<Staff>();
        w.role = role;
        w.hiredToday = hiredToday;
        w.Build();
        Game.staff.Add(w);
        return w;
    }

    void Build()
    {
        Color[] cols = {
            new Color(0.7f, 0.4f, 0.9f), new Color(0.25f, 0.7f, 0.6f), new Color(0.9f, 0.6f, 0.2f),
            new Color(0.5f, 0.65f, 0.5f), new Color(0.2f, 0.6f, 0.85f), new Color(0.8f, 0.75f, 0.5f),
            new Color(0.2f, 0.28f, 0.42f), new Color(0.25f, 0.75f, 0.72f), new Color(0.95f, 0.78f, 0.18f) };
        GameObject vroot = new GameObject("Visual");
        vroot.transform.SetParent(transform, false);
        visual = vroot.transform;
        B.Stickman(visual, cols[role]);
        B.Prim(PrimitiveType.Cube, "Cap", visual, new Vector3(0f, 2.1f, 0.02f), Vector3.zero, new Vector3(0.5f, 0.13f, 0.5f), MatLib.Get(Color.white));
        TextMesh tag = B.Text3D(StaffInfo.Names[role], transform, new Vector3(0f, 2.8f, 0f), 0.07f, cols[role]);
        tag.fontStyle = FontStyle.Bold;

        GameObject anchor = new GameObject("StackAnchor");
        anchor.transform.SetParent(transform, false);
        anchor.transform.localPosition = new Vector3(0f, 2.45f, 0f);
        stackAnchor = anchor.transform;
    }

    bool MoveTo(Vector3 t, float dt)
    {
        t.y = 0f;
        Vector3 pos = transform.position; pos.y = 0f;
        Vector3 to = t - pos;
        if (to.magnitude < 0.35f) return true;
        Vector3 dir = to.normalized;
        transform.position += dir * MoveSpeed * dt;
        visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(dir), 10f * dt);
        return false;
    }

    void Update()
    {
        if (Game.gm == null || Game.register == null) return; // scene restarting
        float dt = Time.deltaTime;
        if (!Game.gm.StaffOnShift)
        {
            if (!shiftWorkDiscarded)
            {
                DiscardCarriedWork();
                shiftWorkDiscarded = true;
            }
            Vector3 home = Customer.DoorPos + new Vector3(7f + role * 0.25f, 0f, -12f);
            if (MoveTo(home, dt) && shiftVisible) SetShiftVisible(false);
            return;
        }
        if (!shiftVisible)
        {
            transform.position = Customer.DoorPos + new Vector3(role * 0.2f, 0f, 0f);
            SetShiftVisible(true);
            state = S.Idle;
            timer = 0f;
        }
        shiftWorkDiscarded = false;
        switch (role)
        {
            case 0: TickCashier(dt); break;
            case 1: TickFisher(dt); break;
            case 2: TickCarrier(dt); break;
            case 3: TickJanitor(dt); break;
            case 4: TickSeaCleaner(dt); break;
            case 5: TickToiletCleaner(dt); break;
            case 6: TickSecurity(dt); break;
            case 7: TickBeachCleaner(dt); break;
            case 8: TickElectrician(dt); break;
        }
    }

    void SetShiftVisible(bool visible)
    {
        shiftVisible = visible;
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = visible;
    }

    void DiscardCarriedWork()
    {
        if (role == 0 && Game.register != null) Game.register.CashierPresent = false;
        for (int i = 0; i < carriedFish.Count; i++)
            if (carriedFish[i] != null) Destroy(carriedFish[i].gameObject);
        carriedFish.Clear();
        for (int i = 0; i < carriedTrash.Count; i++)
            if (carriedTrash[i] != null) Destroy(carriedTrash[i].gameObject);
        carriedTrash.Clear();
        ClearCrates();
        chaseTarget = null;
        targetTank = null;
        state = S.Idle;
        timer = 0f;
    }

    void OnDestroy()
    {
        Game.staff.Remove(this);
    }

    public void BlastHit(Vector3 origin)
    {
        for (int i = 0; i < carriedFish.Count; i++)
            if (carriedFish[i] != null) carriedFish[i].DieFromPollution();
        carriedFish.Clear();
        ClearCrates();
        Vector3 away = transform.position - origin; away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
        StartCoroutine(BlastKnockRoutine(away.normalized));
    }

    System.Collections.IEnumerator BlastKnockRoutine(Vector3 away)
    {
        float elapsed = 0f;
        while (elapsed < 0.75f)
        {
            elapsed += Time.deltaTime;
            transform.position += away * (7f * (1f - elapsed / 0.75f)) * Time.deltaTime;
            transform.position += Vector3.up * Mathf.Sin(elapsed / 0.75f * Mathf.PI) * 2f * Time.deltaTime;
            visual.Rotate(Vector3.right, 430f * Time.deltaTime);
            yield return null;
        }
        Vector3 grounded = transform.position; grounded.y = 0f; transform.position = grounded;
        visual.rotation = Quaternion.identity;
        state = S.Idle;
    }

    // ---- beach cleaner: clears shore/beach litter ----
    void TickBeachCleaner(float dt) { TickCleaner(dt, false, Game.gm.StaffCapacity(7)); }

    // ---- security guard: chase & beat thieves ----
    Thief chaseThief;
    float punchTimer;
    Vector3 patrolTarget;

    void TickSecurity(float dt)
    {
        if (chaseThief == null)
        {
            chaseThief = Thief.Nearest(transform.position);
            if (chaseThief == null)
            {
                if (IsFirstSecurity())
                {
                    // The first guard is visibly stationed at the front door.
                    Vector3 doorPost = Customer.GateInside + new Vector3(2.5f, 0f, -1.5f);
                    if (MoveTo(doorPost, dt))
                    {
                        Vector3 face = Customer.DoorPos - transform.position; face.y = 0f;
                        if (face.sqrMagnitude > 0.01f) visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(face), 6f * dt);
                    }
                }
                else
                {
                    // Extra guards patrol the sales floor instead of crowding the gate.
                    if (Vector3.Distance(transform.position, patrolTarget) < 1.5f || patrolTarget == Vector3.zero)
                        patrolTarget = new Vector3(Random.Range(-17f, 5f), 0f, Random.Range(-3f, 23f));
                    MoveTo(patrolTarget, dt);
                }
                return;
            }
        }
        if (chaseThief == null) return;
        float d = Vector3.Distance(transform.position, chaseThief.transform.position);
        float attackRange = Game.gm.activeSecurityWeapon == 2 ? 8f : 1.6f;
        if (d > attackRange) MoveTo(chaseThief.transform.position, dt);
        else
        {
            // beat it up!
            punchTimer -= dt;
            visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 25f) * 20f);
            if (punchTimer <= 0f)
            {
                punchTimer = Mathf.Lerp(0.7f, 0.28f, (Game.gm.staffLevel[6] - 1) / 4f);
                Sfx.Play(Snd.Punch, 0.7f);
                StartCoroutine(SecurityWeaponVisual());
                int damage = Mathf.Clamp(Game.gm.staffLevel[6], 1, 5) + Game.gm.SecurityWeaponBonus;
                if (chaseThief.TakeHit(damage))
                    chaseThief = null; // caught: thief returned loot & fled
            }
        }
    }

    System.Collections.IEnumerator SecurityWeaponVisual()
    {
        int weapon = Game.gm != null ? Game.gm.activeSecurityWeapon : -1;
        PrimitiveType primitive = weapon < 0 ? PrimitiveType.Sphere : PrimitiveType.Cube;
        Vector3 scale = weapon < 0 ? Vector3.one * 0.3f : weapon == 0 ? new Vector3(0.15f, 0.15f, 1.05f) : weapon == 1 ? new Vector3(0.11f, 0.07f, 0.75f) : new Vector3(0.26f, 0.2f, 0.68f);
        Color color = weapon < 0 ? new Color(1f, 0.72f, 0.55f) : weapon == 0 ? new Color(0.42f, 0.24f, 0.1f) : weapon == 1 ? new Color(0.82f, 0.9f, 0.96f) : new Color(0.12f, 0.14f, 0.18f);
        GameObject item = B.Prim(primitive, "SecurityWeaponStrike", visual, new Vector3(0.42f, 1.3f, 0.35f), Vector3.zero, scale, MatLib.Get(color));
        float t = 0f;
        while (t < 1f && item != null)
        {
            t += Time.deltaTime * 8f;
            item.transform.localPosition = new Vector3(0.42f, 1.3f, 0.35f + Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * 1.1f);
            yield return null;
        }
        if (item != null) Destroy(item);
    }

    bool IsFirstSecurity()
    {
        for (int i = 0; i < Game.staff.Count; i++)
            if (Game.staff[i] != null && Game.staff[i].role == 6) return Game.staff[i] == this;
        return true;
    }

    void TickElectrician(float dt)
    {
        if (Game.events == null || !Game.events.PowerOutageActive || Game.generator == null)
        {
            Vector3 standby = new Vector3(5.5f, 0f, -1f);
            MoveTo(standby, dt);
            return;
        }
        if (MoveTo(Game.generator.InteractionSpot, dt))
            Game.generator.StartByTechnician(Game.gm.staffLevel[8]);
    }

    // ---- cashier ----
    void TickCashier(float dt)
    {
        Vector3 spot = Game.register.OperatorSpotFor(this);
        bool there = MoveTo(spot, dt);
        Game.register.CashierPresent = there;
        if (there)
        {
            Vector3 face = Game.register.transform.position - transform.position; face.y = 0f;
            if (face.sqrMagnitude > 0.01f)
                visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(face), 6f * dt);
        }
    }

    // ---- fisher ----
    void TickFisher(float dt)
    {
        switch (state)
        {
            case S.Idle:
                int fishCapacity = Game.gm.StaffCapacity(1);
                if (carriedFish.Count >= fishCapacity) { state = S.MoveToDeliver; break; }
                chaseTarget = Game.sea != null ? Game.sea.FindWorkerTarget(transform.position, FisherBand(), Game.gm.StaffRadarRange) : null;
                if (chaseTarget != null) state = S.MoveToWork;
                else { timer = 2f; state = S.Working; }
                break;

            case S.MoveToWork:
                if (chaseTarget == null || chaseTarget.state != Fish.State.Wild) { state = S.Idle; break; }
                if (MoveTo(chaseTarget.transform.position, dt)) { timer = 1.25f * Game.gm.StaffWorkTimeMultiplier(1); state = S.Working; }
                break;

            case S.Working:
                timer -= dt;
                if (chaseTarget != null && chaseTarget.state == Fish.State.Wild &&
                    Vector3.Distance(transform.position, chaseTarget.transform.position) > 2.5f)
                { state = S.MoveToWork; break; }
                if (timer <= 0f)
                {
                    if (chaseTarget != null && chaseTarget.state == Fish.State.Wild && Game.gm.IsUnlocked(chaseTarget.species))
                    {
                        Game.sea.Remove(chaseTarget);
                        chaseTarget.SetCarried(stackAnchor, carriedFish.Count);
                        carriedFish.Add(chaseTarget);
                        chaseTarget = null;
                    }
                    else chaseTarget = null;
                    state = carriedFish.Count >= Game.gm.StaffCapacity(1) ? S.MoveToDeliver : S.Idle;
                }
                break;

            case S.MoveToDeliver:
                if (MoveTo(DeliveryPoint(), dt)) { timer = 0f; state = S.Delivering; }
                break;

            case S.Delivering:
                if (carriedFish.Count == 0) { state = S.Idle; break; }
                timer -= dt;
                if (timer <= 0f)
                {
                    timer = 0.18f;
                    Fish f = carriedFish[carriedFish.Count - 1];
                    carriedFish.RemoveAt(carriedFish.Count - 1);
                    for (int i = 0; i < carriedFish.Count; i++) carriedFish[i].SetCarryIndex(i);
                    DeliverFish(f);
                }
                break;
        }
    }

    Vector3 RandomSeaPoint()
    {
        Rect a = Game.sea.area;
        return new Vector3(Random.Range(a.xMin, a.xMin + a.width * 0.5f), 0f, Random.Range(a.yMin, a.yMax));
    }

    int FisherBand()
    {
        List<Staff> fishers = new List<Staff>();
        for (int i = 0; i < Game.staff.Count; i++)
            if (Game.staff[i] != null && Game.staff[i].role == 1) fishers.Add(Game.staff[i]);
        fishers.Sort(delegate (Staff a, Staff b) { return a.GetInstanceID().CompareTo(b.GetInstanceID()); });
        int index = Mathf.Max(0, fishers.IndexOf(this));
        int count = Mathf.Max(1, fishers.Count);
        if (count == 1) return Random.Range(0, 3);
        if (count == 2) return index == 0 ? 0 : 2;
        if (count == 3) return index == 0 ? 0 : 2;
        if (count == 4) return index == 0 ? 0 : index == 1 ? 1 : 2;
        // Five and six workers use a balanced 2 near / 1 middle / rest far split.
        if (index < 2) return 0;
        if (index == 2) return 1;
        return 2;
    }

    Vector3 DeliveryPoint()
    {
        Depot openDepot = Game.DepotWithSpace(transform.position);
        if (openDepot != null)
            return openDepot.transform.position + new Vector3(0f, 0f, -3f);
        Tank best = null;
        for (int i = carriedFish.Count - 1; i >= 0; i--)
        {
            Tank t = Game.TankOf(carriedFish[i].species);
            if (t != null && t.HasSpace) { best = t; break; }
        }
        return best != null ? best.FrontPoint : new Vector3(-12f, 0f, 12f);
    }

    void DeliverFish(Fish f)
    {
        Depot nearbyDepot = Game.NearbyDepot(transform.position, 5f);
        if (nearbyDepot != null && nearbyDepot.HasSpace)
        {
            int sp = f.species;
            Depot targetDepot = nearbyDepot;
            f.FlyTo(targetDepot.DropPoint(), delegate { targetDepot.StoreVisualArrived(sp); if (f != null) Destroy(f.gameObject); }, 0.5f);
            return;
        }
        Tank t = Game.TankOf(f.species);
        if (t != null && t.HasSpace && Vector3.Distance(transform.position, t.transform.position) < 6f)
        {
            if (!t.ReserveSlot())
            {
                carriedFish.Insert(0, f);
                return;
            }
            Tank tank = t;
            Fish captured = f;
            f.FlyTo(t.RandomWaterPoint(), delegate { if (captured != null) tank.Receive(captured); }, 0.5f);
            return;
        }
        carriedFish.Insert(0, f);
        for (int i = 0; i < carriedFish.Count; i++) carriedFish[i].SetCarryIndex(i);
        state = S.MoveToDeliver;
    }

    // ---- carrier ----
    void TickCarrier(float dt)
    {
        switch (state)
        {
            case S.Idle:
                Depot stockDepot = Game.DepotWithStock(transform.position);
                if (stockDepot == null) { timer = 2f; break; }
                int sp;
                if (carriedCrates < Game.gm.StaffCapacity(2) &&
                    Vector3.Distance(transform.position, stockDepot.transform.position) < 4f &&
                    stockDepot.TryTakeForTank(out sp))
                {
                    crateSpecies = sp;
                    carriedCrates++;
                    AddCrateVisual();
                    if (carriedCrates >= Game.gm.StaffCapacity(2)) state = S.MoveToDeliver;
                }
                else if (carriedCrates > 0) state = S.MoveToDeliver;
                else MoveTo(stockDepot.transform.position + new Vector3(0f, 0f, -3f), dt);
                break;

            case S.MoveToDeliver:
                targetTank = Game.TankOf(crateSpecies);
                if (targetTank == null)
                {
                    Depot returnDepot = Game.DepotWithSpace(transform.position);
                    if (returnDepot != null) for (int i = 0; i < carriedCrates; i++) returnDepot.Store(crateSpecies);
                    ClearCrates();
                    state = S.Idle;
                    break;
                }
                // Keep the load and wait beside the aquarium until a customer
                // creates room. Do not bounce a full load back into the depot.
                if (!targetTank.HasSpace)
                {
                    MoveTo(targetTank.FrontPoint, dt);
                    break;
                }
                if (MoveTo(targetTank.FrontPoint, dt)) { timer = 0f; state = S.Delivering; }
                break;

            case S.Delivering:
                if (carriedCrates == 0) { state = S.Idle; break; }
                timer -= dt;
                if (timer <= 0f)
                {
                    timer = 0.2f;
                    if (targetTank != null && targetTank.HasSpace && targetTank.ReserveSlot())
                    {
                        carriedCrates--;
                        RemoveCrateVisual();
                        Fish f = Fish.Create(crateSpecies, stackAnchor.position);
                        Tank tank = targetTank;
                        f.FlyTo(tank.RandomWaterPoint(), delegate { if (f != null) tank.Receive(f); }, 0.5f);
                    }
                    else timer = 0.35f; // full: visibly wait with the remaining crates
                }
                break;
        }
    }

    void AddCrateVisual()
    {
        GameObject c = B.Prim(PrimitiveType.Cube, "Crate", stackAnchor,
            new Vector3(0f, crateVis.Count * 0.45f, 0f), Vector3.zero, new Vector3(0.6f, 0.4f, 0.6f),
            MatLib.Get(new Color(0.85f, 0.65f, 0.35f)));
        crateVis.Add(c);
    }

    void RemoveCrateVisual()
    {
        if (crateVis.Count == 0) return;
        GameObject c = crateVis[crateVis.Count - 1];
        crateVis.RemoveAt(crateVis.Count - 1);
        Destroy(c);
    }

    void ClearCrates()
    {
        for (int i = 0; i < crateVis.Count; i++) Destroy(crateVis[i]);
        crateVis.Clear();
        carriedCrates = 0;
    }

    // ---- janitor (land) / sea cleaner ----
    void TickJanitor(float dt) { TickCleaner(dt, false, Game.gm.StaffCapacity(3)); }
    void TickSeaCleaner(float dt) { TickCleaner(dt, true, Game.gm.StaffCapacity(4)); }

    // Smarter routing: always chases the trash NEAREST to itself, keeps
    // collecting until full or the area is clean, then returns to the bin.
    void TickCleaner(float dt, bool sea, int cap)
    {
        switch (state)
        {
            case S.Idle:
                TrashItem t = Game.trash != null ? Game.trash.FindNearestTo(transform.position, sea) : null;
                if (t != null && carriedTrash.Count < cap)
                {
                    moveTarget = t.transform.position;
                    state = S.MoveToWork;
                }
                else if (carriedTrash.Count > 0) state = S.MoveToDeliver;
                break;

            case S.MoveToWork:
                // retarget live: trash may have been picked up by someone else
                TrashItem cur = Game.trash.FindNearestTo(transform.position, sea);
                if (cur == null) { state = carriedTrash.Count > 0 ? S.MoveToDeliver : S.Idle; break; }
                moveTarget = cur.transform.position;
                if (MoveTo(moveTarget, dt))
                {
                    TrashItem near = Game.trash.FindNear(transform.position, 2f, sea);
                    if (near != null)
                    {
                        Game.trash.PickUp(near);
                        near.AttachTo(stackAnchor, carriedTrash.Count);
                        carriedTrash.Add(near);
                    }
                    state = carriedTrash.Count >= cap ? S.MoveToDeliver : S.Idle;
                }
                break;

            case S.MoveToDeliver:
                if (MoveTo(Game.trash.BinPos + new Vector3(0f, 0f, -1.5f), dt))
                {
                    for (int i = 0; i < carriedTrash.Count; i++)
                        if (carriedTrash[i] != null) carriedTrash[i].DumpInto(Game.trash.BinPos);
                    carriedTrash.Clear();
                    state = S.Idle;
                }
                break;
        }
    }

    // ---- toilet cleaner ----
    void TickToiletCleaner(float dt)
    {
        if (Game.toilets == null) return;
        switch (state)
        {
            case S.Idle:
                int dirty = Game.toilets.DirtiestUnit();
                if (dirty >= 0)
                {
                    moveTarget = Game.toilets.UnitPos(dirty);
                    state = S.MoveToWork;
                }
                break;

            case S.MoveToWork:
                if (MoveTo(moveTarget, dt)) { timer = 2.4f * Game.gm.StaffWorkTimeMultiplier(5); state = S.Working; }
                break;

            case S.Working:
                timer -= dt;
                if (timer <= 0f)
                {
                    Game.toilets.CleanNearest(transform.position);
                    state = S.Idle;
                }
                break;
        }
    }
}
