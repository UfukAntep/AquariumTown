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
        if (Game.player == null || !Game.player.Swimming) { TriggerGoldenFish(); return; }
        Shark.Spawn(Game.player.transform.position);
    }

    public void TriggerQuake()
    {
        // never before level 5, and stays rare afterwards
        if (Game.gm.Level < 5 || Game.tanks.Count == 0) { TriggerGoldenFish(); return; }
        StartCoroutine(QuakeRoutine());
    }

    System.Collections.IEnumerator QuakeRoutine()
    {
        Sfx.Play(Snd.Crash, 0.8f);
        if (Game.ui != null) Game.ui.Toast("DEPREM!!! Tanklari kontrol et!");
        if (Game.cam != null) Game.cam.Shake(1.2f, 0.5f);
        yield return new WaitForSeconds(0.8f);
        // break 1-2 random tanks with stock
        int breaks = Random.value < 0.4f ? 2 : 1;
        for (int b = 0; b < breaks; b++)
        {
            Tank t = Game.tanks[Random.Range(0, Game.tanks.Count)];
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
    enum TState { Enter, ToTarget, Grab, Flee, Escaped }
    TState state = TState.Enter;

    Transform visual;
    float speed = 5.2f;
    Vector3 moveTarget;
    int stolenMoney;
    int stolenSpecies = -1;
    Tank stolenFrom;
    GameObject loot;
    public float fleeTimer = 10f;
    bool revealed;

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
        Sfx.Play(Snd.Alarm, 0.7f);
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
        switch (state)
        {
            case TState.Enter:
                if (MoveTo(moveTarget, dt))
                {
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
                    if (Game.register.PileAmount > 0)
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
                    fleeTimer = 10f;
                    moveTarget = FleePoint();
                    speed = 5.6f;
                }
                break;

            case TState.Flee:
                fleeTimer -= dt;
                if (Game.ui != null) Game.ui.ShowThiefTimer(fleeTimer);
                MoveTo(moveTarget, dt);
                // caught?
                if (Game.player != null && Vector3.Distance(Game.player.transform.position, transform.position) < 1.4f)
                {
                    Caught();
                    break;
                }
                if (fleeTimer <= 0f || Vector3.Distance(transform.position, moveTarget) < 1f)
                {
                    Escaped();
                }
                break;
        }
    }

    Vector3 FleePoint() { return Customer.DoorPos + new Vector3(20f, 0f, Random.Range(-8f, 8f)); }

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
        hits++;
        if (hits < 3) { speed = 3f; return false; } // stagger from the beating
        ReturnLoot("Guvenlik hirsizi dovdu ve mali geri aldi!");
        // run away empty-handed
        stolenMoney = 0; stolenSpecies = -1;
        state = TState.Flee;
        fleeTimer = 3f;
        moveTarget = FleePoint();
        speed = 6.5f;
        if (loot != null) { Destroy(loot); loot = null; }
        if (Game.ui != null) Game.ui.HideThiefTimer();
        return true;
    }

    void ReturnLoot(string msg)
    {
        if (stolenMoney > 0)
        {
            Game.register.Pay(stolenMoney, false);
            if (Game.ui != null) Game.ui.Toast(msg + " +$" + B.Money(stolenMoney));
        }
        else if (stolenSpecies >= 0)
        {
            Tank t = Game.TankOf(stolenSpecies);
            if (t != null) t.AddCount(1);
            if (Game.ui != null) Game.ui.Toast(msg);
        }
        Sfx.Play(Snd.Collect, 0.7f);
    }

    void Caught()
    {
        if (stolenMoney > 0)
        {
            Game.gm.AddMoney(stolenMoney);
            if (Game.ui != null) Game.ui.Toast("Hirsizi yakaladin! +" + B.Money(stolenMoney) + "$ geri alindi!");
        }
        else if (stolenSpecies >= 0)
        {
            Tank t = Game.TankOf(stolenSpecies);
            if (t != null) t.AddCount(1);
            if (Game.ui != null) Game.ui.Toast("Hirsizi yakaladin! Balik geri alindi!");
        }
        else if (Game.ui != null) Game.ui.Toast("Hirsiz eli bos yakalandi!");
        Sfx.Play(Snd.Collect, 0.8f);
        if (Game.ui != null) Game.ui.HideThiefTimer();
        Destroy(gameObject);
    }

    void Escaped()
    {
        if (stolenMoney > 0 || stolenSpecies >= 0)
        {
            Sfx.Play(Snd.Laugh, 0.9f);
            if (Game.ui != null) Game.ui.Toast("Hirsiz kacti ve arkasindan guluyor... hahaha!");
            Game.gm.AddSatisfaction(-3f);
        }
        if (Game.ui != null) Game.ui.HideThiefTimer();
        Destroy(gameObject);
    }

    void OnDestroy() { if (Game.ui != null) Game.ui.HideThiefTimer(); }
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
        go.transform.position = new Vector3(a.xMax - 5f, 0.3f, Mathf.Clamp(nearPlayer.z + Random.Range(-15f, 15f), a.yMin, a.yMax));
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
            Game.player.SharkHit();
            Sfx.Play(Snd.Crash, 1f);
            Destroy(gameObject, 3f);
        }
    }
}
