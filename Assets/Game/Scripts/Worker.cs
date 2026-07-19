using System.Collections.Generic;
using UnityEngine;

// Hired staff. Roles: 0 Cashier, 1 Fisher, 2 Carrier, 3 Janitor, 4 SeaCleaner, 5 ToiletCleaner.
public class Staff : MonoBehaviour
{
    public int role;

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
    const float Speed = 4.2f;

    public static Staff Create(int role, Vector3 pos)
    {
        GameObject go = new GameObject("Staff_" + StaffInfo.Names[role]);
        go.transform.position = pos;
        Staff w = go.AddComponent<Staff>();
        w.role = role;
        w.Build();
        Game.staff.Add(w);
        return w;
    }

    void Build()
    {
        Color[] cols = {
            new Color(0.7f, 0.4f, 0.9f), new Color(0.25f, 0.7f, 0.6f), new Color(0.9f, 0.6f, 0.2f),
            new Color(0.5f, 0.65f, 0.5f), new Color(0.2f, 0.6f, 0.85f), new Color(0.8f, 0.75f, 0.5f),
            new Color(0.2f, 0.28f, 0.42f), new Color(0.25f, 0.75f, 0.72f) };
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
        transform.position += dir * Speed * dt;
        visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(dir), 10f * dt);
        return false;
    }

    void Update()
    {
        if (Game.gm == null || Game.register == null) return; // scene restarting
        float dt = Time.deltaTime;
        if (!Game.gm.StaffOnShift)
        {
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
        }
    }

    void SetShiftVisible(bool visible)
    {
        shiftVisible = visible;
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = visible;
    }

    void OnDestroy()
    {
        Game.staff.Remove(this);
    }

    // ---- beach cleaner: clears shore/beach litter ----
    void TickBeachCleaner(float dt) { TickCleaner(dt, false, 4); }

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
                // patrol near the entrance
                if (Vector3.Distance(transform.position, patrolTarget) < 1.5f || patrolTarget == Vector3.zero)
                    patrolTarget = Customer.GateInside + new Vector3(Random.Range(-6f, 4f), 0f, Random.Range(-4f, 4f));
                MoveTo(patrolTarget, dt);
                return;
            }
        }
        if (chaseThief == null) return;
        float d = Vector3.Distance(transform.position, chaseThief.transform.position);
        if (d > 1.6f) MoveTo(chaseThief.transform.position, dt);
        else
        {
            // beat it up!
            punchTimer -= dt;
            visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 25f) * 20f);
            if (punchTimer <= 0f)
            {
                punchTimer = 0.35f;
                Sfx.Play(Snd.Punch, 0.7f);
                if (chaseThief.TakeHit())
                    chaseThief = null; // caught: thief returned loot & fled
            }
        }
    }

    // ---- cashier ----
    void TickCashier(float dt)
    {
        Vector3 spot = Game.register.OperatorSpot;
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
                if (carriedFish.Count >= 4) { state = S.MoveToDeliver; break; }
                chaseTarget = Game.sea != null ? Game.sea.FindTarget(RandomSeaPoint(), 40f) : null;
                if (chaseTarget != null) state = S.MoveToWork;
                else { timer = 2f; state = S.Working; }
                break;

            case S.MoveToWork:
                if (chaseTarget == null || chaseTarget.state != Fish.State.Wild) { state = S.Idle; break; }
                if (MoveTo(chaseTarget.transform.position, dt)) { timer = 1.1f; state = S.Working; }
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
                    state = carriedFish.Count >= 4 ? S.MoveToDeliver : S.Idle;
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

    Vector3 DeliveryPoint()
    {
        if (Game.depot != null && Game.depot.HasSpace)
            return Game.depot.transform.position + new Vector3(0f, 0f, -3f);
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
        if (Game.depot != null && Game.depot.HasSpace &&
            Vector3.Distance(transform.position, Game.depot.transform.position) < 5f)
        {
            int sp = f.species;
            f.FlyTo(Game.depot.DropPoint(), delegate { Game.depot.StoreVisualArrived(sp); if (f != null) Destroy(f.gameObject); }, 0.5f);
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
                if (Game.depot == null) { timer = 2f; break; }
                int sp;
                if (carriedCrates < 5 &&
                    Vector3.Distance(transform.position, Game.depot.transform.position) < 4f &&
                    Game.depot.TryTakeForTank(out sp))
                {
                    crateSpecies = sp;
                    carriedCrates++;
                    AddCrateVisual();
                    if (carriedCrates >= 5) state = S.MoveToDeliver;
                }
                else if (carriedCrates > 0) state = S.MoveToDeliver;
                else MoveTo(Game.depot.transform.position + new Vector3(0f, 0f, -3f), dt);
                break;

            case S.MoveToDeliver:
                targetTank = Game.TankOf(crateSpecies);
                if (targetTank == null)
                {
                    for (int i = 0; i < carriedCrates; i++) Game.depot.Store(crateSpecies);
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
    void TickJanitor(float dt) { TickCleaner(dt, false, 3); }
    void TickSeaCleaner(float dt) { TickCleaner(dt, true, 5); }

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
                if (MoveTo(moveTarget, dt)) { timer = 2f; state = S.Working; }
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
