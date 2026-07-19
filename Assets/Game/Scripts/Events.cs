using UnityEngine;

// Random events: thief, shark attack, earthquake, golden fish, money rain.
public class EventManager : MonoBehaviour
{
    float eventTimer;

    public static EventManager Create(Transform parent)
    {
        GameObject go = new GameObject("EventManager");
        if (parent != null) go.transform.SetParent(parent, false);
        EventManager e = go.AddComponent<EventManager>();
        Game.events = e;
        e.eventTimer = Random.Range(60f, 120f);
        return e;
    }

    void Update()
    {
        if (Game.gm == null) return; // scene restarting
        eventTimer -= Time.deltaTime;
        if (eventTimer > 0f) return;
        eventTimer = Random.Range(90f, 180f);
        TriggerRandom();
    }

    void TriggerRandom()
    {
        float r = Random.value;
        if (r < 0.32f) SpawnThief();
        else if (r < 0.55f) TriggerShark();
        else if (r < 0.65f) TriggerQuake(); // rare
        else if (r < 0.85f) TriggerGoldenFish();
        else TriggerMoneyRain();
    }

    public void SpawnThief()
    {
        if (!Game.gm.shopOpen) return;
        Thief.Spawn();
    }

    public void TriggerShark()
    {
        if (Game.player == null || Game.sea == null) return;
        int count = Game.player.jetski != null ? 5 : 1;
        for (int i = 0; i < count; i++) Shark.Spawn(Game.player.transform.position, i == 0);
    }

    public void TriggerQuake()
    {
        // never before level 5, and stays rare afterwards
        if (Game.gm.Level < 5 || Game.tanks.Count == 0) { TriggerGoldenFish(); return; }
        StartCoroutine(QuakeRoutine());
    }

    System.Collections.IEnumerator QuakeRoutine()
    {
        Sfx.Play(Snd.Quake, 0.9f);
        Sfx.DangerFor(5f);
        if (Game.ui != null) Game.ui.Toast("DEPREM!!! Tanklari kontrol et!");
        if (Game.cam != null) Game.cam.Shake(1.2f, 0.5f);
        yield return new WaitForSeconds(0.8f);
        // A major quake leaves only one or two aquariums standing.
        int survivors = Mathf.Min(Game.tanks.Count, Random.value < 0.5f ? 1 : 2);
        System.Collections.Generic.List<Tank> targets = new System.Collections.Generic.List<Tank>(Game.tanks);
        for (int i = 0; i < targets.Count; i++)
        {
            int swap = Random.Range(i, targets.Count);
            Tank tmp = targets[i]; targets[i] = targets[swap]; targets[swap] = tmp;
        }
        for (int b = survivors; b < targets.Count; b++)
        {
            Tank t = targets[b];
            if (t != null && !t.broken)
            {
                t.Break();
                Sfx.Play(Snd.Crash, 0.6f);
            }
        }
    }

    public void TriggerGoldenFish()
    {
        if (Game.sea == null) return;
        Game.sea.SpawnGolden();
        if (Game.ui != null) Game.ui.Toast("Kiyida ALTIN BALIK belirdi! Cok degerli, kacirma!");
        Sfx.Play(Snd.Collect, 0.7f);
    }

    public void TriggerMoneyRain()
    {
        // donations only make sense while a customer is actually AT the register
        if (Game.register == null || Game.register.QueueCount == 0) { TriggerGoldenFish(); return; }
        int bonus = 20 + Game.gm.Level * 10;
        Game.register.Pay(bonus, false);
        if (Game.ui != null) Game.ui.Toast("Comert musteri! Kasaya +" + bonus + "$ bagis birakti!");
    }
}

// ---------- Thief ----------
public class Thief : MonoBehaviour
{
    enum Weapon { None, Baton, Gun }
    enum TState { Enter, ToTarget, Grab, Flee, Escaped }
    TState state = TState.Enter;

    Transform visual;
    float speed = 5.2f;
    Vector3 moveTarget;
    int stolenMoney;
    int stolenSpecies = -1;
    Tank stolenFrom;
    GameObject loot;
    public float fleeTimer = 20f;
    bool revealed;
    bool toiletThief;
    bool stolenToilet;
    Weapon weapon;
    float attackTimer;
    float knockTime;
    Vector3 knockVelocity;

    public static Thief Spawn()
    {
        GameObject go = new GameObject("Thief");
        go.transform.position = Customer.DoorPos;
        Thief t = go.AddComponent<Thief>();
        // disguised as a normal customer
        GameObject vroot = new GameObject("Visual");
        vroot.transform.SetParent(go.transform, false);
        t.visual = vroot.transform;
        B.Stickman(t.visual, new Color(0.35f, 0.5f, 0.95f));
        t.moveTarget = Customer.GateInside;
        t.toiletThief = Game.toilets != null && Game.toilets.CanStealToilet && Random.value < 0.35f;
        return t;
    }

    void Reveal()
    {
        revealed = true;
        Material black = MatLib.Get(new Color(0.1f, 0.1f, 0.12f));
        foreach (MeshRenderer mr in visual.GetComponentsInChildren<MeshRenderer>())
            mr.sharedMaterial = black;
        B.Prim(PrimitiveType.Cube, "Mask", visual, new Vector3(0f, 1.8f, 0.22f), Vector3.zero, new Vector3(0.5f, 0.15f, 0.2f), MatLib.Get(new Color(0.9f, 0.2f, 0.2f)));
        B.Text3D("HIRSIZ!", transform, new Vector3(0f, 2.7f, 0f), 0.1f, new Color(1f, 0.3f, 0.3f));
        weapon = Game.gm.Level >= 25 ? Weapon.Gun : Game.gm.Level >= 10 ? Weapon.Baton : Weapon.None;
        if (weapon == Weapon.Baton)
            B.Prim(PrimitiveType.Cube, "Sopa", visual, new Vector3(0.65f, 1.1f, 0.15f), new Vector3(15f, 0f, -25f),
                new Vector3(0.16f, 1.5f, 0.16f), MatLib.Get(new Color(0.42f, 0.23f, 0.1f)));
        else if (weapon == Weapon.Gun)
        {
            B.Prim(PrimitiveType.Cube, "Silah", visual, new Vector3(0.55f, 1.15f, 0.35f), Vector3.zero,
                new Vector3(0.2f, 0.28f, 0.85f), MatLib.Get(new Color(0.14f, 0.16f, 0.2f)));
            B.Text3D("TEHLIKELI!", transform, new Vector3(0f, 3.2f, 0f), 0.075f, new Color(1f, 0.65f, 0.2f));
        }
        Sfx.Play(Snd.Alarm, 0.7f);
        Sfx.Play(Snd.Thief, 0.75f);
        Sfx.BeginDanger();
        if (Game.ui != null) Game.ui.Toast("HIRSIZ! Yakala, esyalarini geri al!");
    }

    bool MoveTo(Vector3 t, float dt)
    {
        t.y = 0f;
        Vector3 pos = transform.position; pos.y = 0f;
        Vector3 to = t - pos;
        if (to.magnitude < 0.3f) return true;
        Vector3 dir = to.normalized;
        transform.position += dir * speed * dt;
        visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(dir), 10f * dt);
        return false;
    }

    void Update()
    {
        if (Game.gm == null || Game.register == null) { Destroy(gameObject); return; } // scene restarting
        float dt = Time.deltaTime;
        if (knockTime > 0f)
        {
            knockTime -= dt;
            transform.position += knockVelocity * dt + Vector3.up * Mathf.Sin((0.45f - knockTime) / 0.45f * Mathf.PI) * 3f * dt;
            knockVelocity = Vector3.Lerp(knockVelocity, Vector3.zero, dt * 4f);
            visual.Rotate(Vector3.right, 600f * dt);
            return;
        }
        if (revealed) UpdateWeapon(dt);
        switch (state)
        {
            case TState.Enter:
                if (MoveTo(moveTarget, dt))
                {
                    if (toiletThief)
                    {
                        moveTarget = Game.toilets != null ? Game.toilets.EntrancePos : Customer.GateInside;
                        state = TState.ToTarget;
                        break;
                    }
                    Reveal();
                    // target: cash pile if money, else a stocked tank
                    if (Game.register.PileAmount > 0)
                        moveTarget = Game.register.transform.position + new Vector3(-3.6f, 0f, -1.2f);
                    else
                    {
                        stolenFrom = PickStockedTank();
                        if (stolenFrom == null) { state = TState.Flee; moveTarget = FleePoint(); break; }
                        moveTarget = stolenFrom.FrontPoint;
                    }
                    state = TState.ToTarget;
                }
                break;

            case TState.ToTarget:
                if (MoveTo(moveTarget, dt))
                {
                    if (toiletThief)
                    {
                        Reveal();
                        stolenToilet = Game.toilets != null && Game.toilets.TryStealToilet();
                        if (stolenToilet)
                        {
                            loot = new GameObject("StolenToilet");
                            loot.transform.SetParent(transform, false);
                            loot.transform.localPosition = new Vector3(0f, 1.7f, -0.65f);
                            Material white = MatLib.Get(Color.white);
                            B.Prim(PrimitiveType.Cube, "Tank", loot.transform, new Vector3(0f, 0.35f, 0f), Vector3.zero, new Vector3(0.65f, 0.7f, 0.35f), white);
                            B.Prim(PrimitiveType.Cylinder, "Bowl", loot.transform, new Vector3(0f, -0.15f, 0.2f), Vector3.zero, new Vector3(0.6f, 0.25f, 0.6f), white);
                            if (Game.ui != null) Game.ui.Toast("Hirsiz tuvaleti sirtina aldi! Yirmi saniye icinde yakala!");
                        }
                    }
                    else if (Game.register.PileAmount > 0)
                    {
                        stolenMoney = Game.register.StealAll();
                        loot = B.Prim(PrimitiveType.Cube, "LootBag", transform, new Vector3(0f, 2.4f, 0f), Vector3.zero,
                            new Vector3(0.6f, 0.6f, 0.6f), MatLib.Get(new Color(0.3f, 0.7f, 0.35f)));
                    }
                    else if (stolenFrom != null && stolenFrom.HasStock)
                    {
                        stolenFrom.TakeForCustomer();
                        stolenSpecies = stolenFrom.species;
                        loot = new GameObject("LootFish");
                        loot.transform.SetParent(transform, false);
                        loot.transform.localPosition = new Vector3(0f, 2.4f, 0f);
                        SpeciesInfo.Build(stolenSpecies, loot.transform, 0.5f);
                    }
                    state = TState.Flee;
                    fleeTimer = 20f;
                    moveTarget = FleePoint();
                    speed = 5.6f;
                }
                break;

            case TState.Flee:
                fleeTimer -= dt;
                if (Game.ui != null) Game.ui.ShowThiefTimer(fleeTimer);
                MoveTo(moveTarget, dt);
                if (fleeTimer <= 0f) Escaped();
                else if (Vector3.Distance(transform.position, moveTarget) < 1f) moveTarget = FleePoint();
                break;
        }
        // Never disappear below the water surface; thieves keep running on the
        // beach/grass escape corridor until their timer expires.
        if (Game.sea != null && Game.sea.Contains(transform.position))
        {
            Vector3 safe = transform.position;
            safe.x = Game.sea.area.xMin - 2f;
            transform.position = safe;
            moveTarget = FleePoint();
        }
    }

    Vector3 FleePoint()
    {
        float dz = Random.value < 0.5f ? -Random.Range(18f, 34f) : Random.Range(18f, 34f);
        return new Vector3(Random.Range(16f, 23f), 0f, Mathf.Clamp(transform.position.z + dz, -55f, 130f));
    }

    void UpdateWeapon(float dt)
    {
        if (weapon == Weapon.None) return;
        attackTimer -= dt;
        if (attackTimer > 0f) return;
        if (weapon == Weapon.Baton)
        {
            if (Game.player != null && Vector3.Distance(transform.position, Game.player.transform.position) < 2.5f)
            {
                Vector3 away = Game.player.transform.position - transform.position; away.y = 0f;
                Game.player.LaunchTo(Game.player.transform.position + away.normalized * 5f);
                Sfx.Play(Snd.Punch, 0.9f);
                attackTimer = 1.2f;
                return;
            }
            Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
            for (int i = 0; i < customers.Length; i++)
                if (Vector3.Distance(transform.position, customers[i].transform.position) < 2.5f)
                {
                    customers[i].HitByThief(transform.position);
                    attackTimer = 1.2f;
                    return;
                }
        }
        else
        {
            Vector3 target = transform.position + visual.forward * 20f;
            float pick = Random.value;
            if (pick < 0.55f && Game.tanks.Count > 0)
                target = Game.tanks[Random.Range(0, Game.tanks.Count)].transform.position;
            else if (pick < 0.78f && Game.player != null)
                target = Game.player.transform.position;
            else
            {
                Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
                if (customers.Length > 0) target = customers[Random.Range(0, customers.Length)].transform.position;
            }
            ThiefBullet.Spawn(transform.position + Vector3.up * 1.3f, target, this);
            attackTimer = 1.15f;
        }
    }

    Tank PickStockedTank()
    {
        for (int i = 0; i < Game.tanks.Count; i++)
            if (Game.tanks[i].HasStock) return Game.tanks[i];
        return null;
    }

    // ---- security guard support ----
    public static Thief Nearest(Vector3 pos)
    {
        Thief best = null;
        float bd = 60f;
        Thief[] all = FindObjectsByType<Thief>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (!all[i].revealed) continue;
            float d = Vector3.Distance(pos, all[i].transform.position);
            if (d < bd) { bd = d; best = all[i]; }
        }
        return best;
    }

    int hits;
    // returns true when the thief is subdued (loot returned, thief runs off)
    public bool TakeHit()
    {
        return PlayerHit(transform.position - visual.forward);
    }

    public bool PlayerHit(Vector3 attackerPos)
    {
        if (!revealed) Reveal();
        hits++;
        Vector3 away = transform.position - attackerPos; away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = -visual.forward;
        knockVelocity = away.normalized * 8f;
        knockTime = 0.45f;
        Sfx.Play(Snd.Punch, 0.85f);
        if (hits < 3) { speed = 3.5f; return false; }
        Caught();
        return true;
    }

    void ReturnLoot(string msg)
    {
        if (stolenToilet)
        {
            if (Game.toilets != null) Game.toilets.RestoreStolenToilet();
            stolenToilet = false;
            if (Game.ui != null) Game.ui.Toast(msg + " Tuvalet geri yerlestirildi!");
        }
        else if (stolenMoney > 0)
        {
            Game.register.Pay(stolenMoney, false);
            if (Game.ui != null) Game.ui.Toast(msg + " +$" + B.Money(stolenMoney));
        }
        else if (stolenSpecies >= 0)
        {
            Game.ReturnFishToStorage(stolenSpecies);
            if (Game.ui != null) Game.ui.Toast(msg);
        }
        Sfx.Play(Snd.Collect, 0.7f);
    }

    void Caught()
    {
        if (stolenToilet)
        {
            if (Game.toilets != null) Game.toilets.RestoreStolenToilet();
            stolenToilet = false;
            if (Game.ui != null) Game.ui.Toast("Hirsizi yakaladin! Tuvalet geri alindi!");
        }
        else if (stolenMoney > 0)
        {
            Game.gm.AddMoney(stolenMoney);
            if (Game.ui != null) Game.ui.Toast("Hirsizi yakaladin! +" + B.Money(stolenMoney) + "$ geri alindi!");
        }
        else if (stolenSpecies >= 0)
        {
            Game.ReturnFishToStorage(stolenSpecies);
            if (Game.ui != null) Game.ui.Toast("Hirsizi yakaladin! Balik geri alindi!");
        }
        else if (Game.ui != null) Game.ui.Toast("Hirsiz eli bos yakalandi!");
        Sfx.Play(Snd.Collect, 0.8f);
        if (Game.ui != null) Game.ui.HideThiefTimer();
        Destroy(gameObject);
    }

    void Escaped()
    {
        if (stolenMoney > 0 || stolenSpecies >= 0 || stolenToilet)
        {
            Sfx.Play(Snd.Laugh, 0.9f);
            if (Game.ui != null) Game.ui.Toast("Hirsiz kacti ve arkasindan guluyor... hahaha!");
            Game.gm.AddSatisfaction(-3f);
        }
        if (Game.ui != null) Game.ui.HideThiefTimer();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (Game.ui != null) Game.ui.HideThiefTimer();
        Sfx.EndDanger();
    }
}

public class ThiefBullet : MonoBehaviour
{
    Vector3 velocity;
    float life = 4f;
    Thief owner;

    public static void Spawn(Vector3 from, Vector3 target, Thief owner)
    {
        GameObject go = B.Prim(PrimitiveType.Sphere, "ThiefBullet", null, from, Vector3.zero, Vector3.one * 0.22f,
            MatLib.Get(new Color(1f, 0.72f, 0.12f)));
        ThiefBullet b = go.AddComponent<ThiefBullet>();
        Vector3 dir = target + Vector3.up * 1f - from;
        b.velocity = dir.normalized * 18f;
        b.owner = owner;
        Sfx.Play(Snd.Alarm, 0.28f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        transform.position += velocity * dt;
        life -= dt;
        for (int i = 0; i < Game.tanks.Count; i++)
        {
            Tank tank = Game.tanks[i];
            if (tank != null && !tank.broken && Vector3.Distance(transform.position, tank.transform.position + Vector3.up) < 2.1f)
            {
                tank.Break();
                int damage = Mathf.Min(Game.gm.Money, 100 + SpeciesInfo.Price(tank.species) / 2);
                if (damage > 0) Game.gm.SpendTick(damage);
                if (Game.ui != null) Game.ui.Toast("Mermi akvaryumu kirdi! -$" + B.Money(damage) + " hasar");
                Sfx.Play(Snd.GlassBreak, 0.8f);
                Destroy(gameObject);
                return;
            }
        }
        if (Game.player != null && Vector3.Distance(transform.position, Game.player.transform.position + Vector3.up) < 1.2f)
        {
            Vector3 away = velocity.normalized * 5f;
            Game.player.LaunchTo(Game.player.transform.position + new Vector3(away.x, 0f, away.z));
            Destroy(gameObject);
            return;
        }
        Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
        for (int i = 0; i < customers.Length; i++)
            if (Vector3.Distance(transform.position, customers[i].transform.position + Vector3.up) < 1.1f)
            {
                customers[i].HitByThief(owner != null ? owner.transform.position : transform.position - velocity);
                Destroy(gameObject);
                return;
            }
        if (life <= 0f) Destroy(gameObject);
    }
}

// ---------- Shark ----------
public class Shark : MonoBehaviour
{
    Vector3 targetPos;
    float speed = 9f;
    bool hit;
    Transform visual;

    public static Shark Spawn(Vector3 nearPlayer, bool announce = true)
    {
        GameObject go = new GameObject("Shark");
        Rect a = Game.sea.area;
        int edge = Random.Range(0, 4);
        if (edge == 0) go.transform.position = new Vector3(a.xMin + 4f, 0.3f, Random.Range(a.yMin + 3f, a.yMax - 3f));
        else if (edge == 1) go.transform.position = new Vector3(a.xMax - 4f, 0.3f, Random.Range(a.yMin + 3f, a.yMax - 3f));
        else if (edge == 2) go.transform.position = new Vector3(Random.Range(a.xMin + 3f, a.xMax - 3f), 0.3f, a.yMin + 4f);
        else go.transform.position = new Vector3(Random.Range(a.xMin + 3f, a.xMax - 3f), 0.3f, a.yMax - 4f);
        Shark s = go.AddComponent<Shark>();
        s.Build();
        if (announce)
        {
            Sfx.Play(Snd.Alarm, 0.8f);
            if (Game.ui != null) Game.ui.Toast("KOPEKBALIGI!!! Sudan cik, KAC!!!");
        }
        return s;
    }

    void Build()
    {
        GameObject v = new GameObject("Visual");
        v.transform.SetParent(transform, false);
        visual = v.transform;
        Material gray = MatLib.Get(new Color(0.35f, 0.4f, 0.48f));
        Material white = MatLib.Get(new Color(0.9f, 0.9f, 0.9f));
        B.Prim(PrimitiveType.Sphere, "Body", visual, Vector3.zero, Vector3.zero, new Vector3(1.4f, 1.1f, 3.6f), gray);
        B.Prim(PrimitiveType.Sphere, "Belly", visual, new Vector3(0f, -0.3f, 0.3f), Vector3.zero, new Vector3(1.2f, 0.7f, 2.6f), white);
        B.Prim(PrimitiveType.Cube, "Fin", visual, new Vector3(0f, 0.9f, -0.3f), new Vector3(-25f, 0f, 0f), new Vector3(0.15f, 1f, 0.9f), gray);
        B.Prim(PrimitiveType.Cube, "Tail", visual, new Vector3(0f, 0.2f, -2f), new Vector3(-35f, 0f, 0f), new Vector3(0.15f, 1.2f, 0.7f), gray);
        Destroy(gameObject, 14f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (Game.player == null || Game.gm == null) { Destroy(gameObject); return; }
        targetPos = Game.player.transform.position;

        // player left the water -> shark gives up
        if (!Game.player.Swimming && !hit)
        {
            transform.position += visual.forward * speed * 0.5f * dt;
            Destroy(gameObject, 2f);
            return;
        }

        Vector3 to = targetPos - transform.position;
        to.y = 0f;
        if (to.magnitude > 0.5f)
        {
            Vector3 dir = to.normalized;
            transform.position += dir * speed * dt;
            visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(dir), 6f * dt);
        }

        if (!hit && to.magnitude < 1.6f && Game.player.Swimming)
        {
            hit = true;
            Sfx.Play(Snd.Shark, 0.9f);
            Game.player.SharkHit();
            Sfx.Play(Snd.Crash, 1f);
            Destroy(gameObject, 3f);
        }
    }
}
