using System.Collections.Generic;
using UnityEngine;

// Random events and first-two-hour mechanic introductions.
public class EventManager : MonoBehaviour
{
    float eventTimer;
    public bool PowerOutageActive { get; private set; }
    Coroutine outageRoutine;
    readonly int[] lastEventRoll = new int[10];
    int eventRoll;

    public static EventManager Create(Transform parent)
    {
        GameObject go = new GameObject("EventManager");
        if (parent != null) go.transform.SetParent(parent, false);
        EventManager e = go.AddComponent<EventManager>();
        Game.events = e;
        e.eventTimer = Random.Range(60f, 120f);
        for (int i = 0; i < e.lastEventRoll.Length; i++) e.lastEventRoll[i] = -Random.Range(0, 5);
        return e;
    }

    void Update()
    {
        if (Game.gm == null) return; // scene restarting
        if (PowerOutageActive) return;
        eventTimer -= Time.deltaTime;
        if (eventTimer > 0f) return;
        eventTimer = Random.Range(90f, 180f);
        TriggerRandom();
    }

    void TriggerRandom()
    {
        eventRoll++;
        List<int> eligible = new List<int> { 0, 1, 2, 3, 4, 5 };
        if (Game.gm.Level >= 16) eligible.Add(6);
        if (Game.gm.Level >= 20) eligible.Add(7);
        if (Game.gm.Level >= 23) eligible.Add(8);
        if (Game.gm.Level >= 35) eligible.Add(9);

        // Fair-event protection: once an eligible event has missed roughly a
        // full rotation, the oldest one is selected instead of allowing thief
        // rolls to repeat indefinitely.
        int selected = -1;
        int oldestAge = -1;
        for (int i = 0; i < eligible.Count; i++)
        {
            int id = eligible[i];
            int age = eventRoll - lastEventRoll[id];
            if (age >= Mathf.Max(4, eligible.Count - 1) && age > oldestAge)
            {
                selected = id;
                oldestAge = age;
            }
        }
        if (selected < 0)
        {
            float r = Random.value;
            if (r < 0.20f) selected = 0;
            else if (r < 0.37f) selected = 1;
            else if (r < 0.48f) selected = 2;
            else if (r < 0.53f) selected = 3;
            else if (r < 0.64f) selected = 4;
            else if (r < 0.75f) selected = 5;
            else if (r < 0.84f && eligible.Contains(6)) selected = 6;
            else if (r < 0.93f && eligible.Contains(7)) selected = 7;
            else if (r < 0.97f && eligible.Contains(8)) selected = 8;
            else if (eligible.Contains(9)) selected = 9;
            else if (eligible.Contains(8)) selected = 8;
            else selected = eligible[Random.Range(0, eligible.Count)];
        }
        lastEventRoll[selected] = eventRoll;
        switch (selected)
        {
            case 0: SpawnThief(); break;
            case 1: TriggerShark(); break;
            case 2: TriggerQuake(); break;
            case 3: TriggerGoldenFish(); break;
            case 4: TriggerMoneyAid(); break;
            case 5: TriggerMoneyRain(); break;
            case 6: TriggerPowerOutage(); break;
            case 7: TriggerSchoolTrip(); break;
            case 8: TriggerStorm(); break;
            case 9: TriggerTerrorist(); break;
        }
        /* Legacy probability table retained below only in version history. */
        return;
#if false
        float r = Random.value;
        // Fixed, readable chances per event roll. Locked event slices fall
        // back to a harmless bonus instead of inflating another danger rate.
        if (r < 0.24f) SpawnThief();                         // 24%
        else if (r < 0.42f) TriggerShark();                  // 18%
        else if (r < 0.50f) TriggerQuake();                  // 8%
        else if (r < 0.64f) TriggerGoldenFish();             // 14%
        else if (r < 0.79f) TriggerMoneyRain();              // 15%
        else if (r < 0.87f)                                 // 8%
        {
            if (Game.gm.Level >= 16) TriggerPowerOutage(); else TriggerGoldenFish();
        }
        else if (r < 0.94f)                                 // 7%
        {
            if (Game.gm.Level >= 20) TriggerSchoolTrip(); else TriggerMoneyRain();
        }
        else                                                 // 6%
        {
            if (Game.gm.Level >= 23) TriggerStorm(); else TriggerGoldenFish();
        }
#endif
    }

    public void SpawnThief(bool force = false)
    {
        if (!force && !Game.gm.shopOpen) return;
        if (Game.gm.Level >= 30)
        {
            int count = Random.Range(2, 6);
            int guaranteedGun = Random.Range(0, count);
            for (int i = 0; i < count; i++)
                Thief.Spawn(force, i == guaranteedGun ? 2 : Random.Range(1, 3));
        }
        else Thief.Spawn(force);
    }

    public void TriggerTerrorist(bool force = false)
    {
        if (Game.gm == null || (!force && Game.gm.Level < 35)) return;
        if (FindFirstObjectByType<Terrorist>() != null) return;
        PlayerPrefs.SetInt("AT3_TerroristOccurred", 1);
        PlayerPrefs.Save();
        Terrorist.Spawn(force);
    }

    public void TriggerShark()
    {
        if (Game.player == null || Game.sea == null) return;
        Sfx.Play(Snd.Shark, 0.95f);
        Sfx.DangerFor(6f);
        if (!Game.player.Swimming)
        {
            if (Game.ui != null) Game.ui.Toast("KOPEKBALIGI TEHLIKESI! 10 saniye icinde denize girersen seni bulur.", 6f);
            StartCoroutine(SharkEntryWindow());
            return;
        }
        SpawnSharkAttack();
    }

    System.Collections.IEnumerator SharkEntryWindow()
    {
        float until = Time.time + 10f;
        while (Time.time < until)
        {
            if (Game.player != null && Game.player.Swimming)
            {
                SpawnSharkAttack();
                yield break;
            }
            yield return null;
        }
    }

    void SpawnSharkAttack()
    {
        if (Game.player == null) return;
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
                if (Game.gm != null && Random.value < Game.gm.QuakeResistance)
                {
                    if (Game.ui != null) Game.ui.Toast("Guclendirilmis bir akvaryum depreme dayandi!");
                    continue;
                }
                t.Break();
                Sfx.Play(Snd.Crash, 0.6f);
            }
        }
    }

    public void TriggerGoldenFish()
    {
        if (Game.sea == null) return;
        Fish golden = Game.sea.SpawnGolden();
        if (Game.gm.techOwned.Length > 8 && Game.gm.TechActive(8))
            GoldenFishGuideArrow.Create(golden);
        if (Game.ui != null) Game.ui.Toast("Kiyida ALTIN BALIK belirdi! Cok degerli, kacirma!");
        Sfx.Play(Snd.Collect, 0.7f);
    }

    public void TriggerMoneyAid(bool force = false)
    {
        if (Game.register == null) return;
        // Random donations require a customer; the one-time level lesson is
        // allowed to demonstrate the mechanic even if the queue is empty.
        if (!force && Game.register.QueueCount == 0) { TriggerGoldenFish(); return; }
        int bonus = 20 + Game.gm.Level * 10;
        Game.register.Pay(bonus, false);
        PlayerPrefs.SetInt("AT3_MoneyAidOccurred", 1);
        PlayerPrefs.Save();
        if (Game.ui != null) Game.ui.Toast("PARA YARDIMI! Kasaya $" + bonus + " birakildi; yerden topla!", 5f);
        Sfx.Play(Snd.MoneyPickup, 1f);
    }

    public void TriggerMoneyRain(bool force = false)
    {
        if (Game.gm == null) return;
        int count = Mathf.Clamp(18 + Game.gm.Level, 20, 55);
        int value = Mathf.Clamp(1 + Game.gm.Level / 5, 1, 20);
        for (int i = 0; i < count; i++) MoneyRainPickup.Spawn(value, i * 0.055f);
        PlayerPrefs.SetInt("AT3_MoneyRainOccurred", 1);
        PlayerPrefs.Save();
        if (Game.ui != null) Game.ui.Toast("PARA YAGMURU! Haritaya dusen paralari senden once baskalari da toplayabilir!", 6f);
        Sfx.Play(Snd.MoneyPickup, 1f);
    }

    public void TriggerPowerOutage(bool force = false)
    {
        if (PowerOutageActive || Game.gm == null || (!force && Game.gm.Level < 16)) return;
        PlayerPrefs.SetInt("AT3_PowerOutageOccurred", 1);
        PlayerPrefs.Save();
        PowerOutageActive = true;
        GameBootstrap.SetPowerOutage(true);
        for (int i = 0; i < Game.tanks.Count; i++) if (Game.tanks[i] != null) Game.tanks[i].SetFilterRunning(false);
        GeneratorUnit.Ensure();
        if (Game.ui != null) Game.ui.Toast("ELEKTRIK KESILDI! Tank filtreleri durdu; jeneratoru calistir!", 6f);
        Sfx.Play(Snd.PowerOut, 1f);
        outageRoutine = StartCoroutine(PowerOutageRoutine());
    }

    System.Collections.IEnumerator PowerOutageRoutine()
    {
        float elapsed = 0f;
        float nextDamage = 30f;
        while (PowerOutageActive && elapsed < 70f)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= nextDamage && Game.gm != null)
            {
                nextDamage += 12f;
                Game.gm.AddSatisfaction(-2f);
                if (Game.tanks.Count > 0)
                    Game.tanks[Random.Range(0, Game.tanks.Count)].LoseFishFromFilterFailure();
            }
            yield return null;
        }
        if (PowerOutageActive) RestorePower(false);
    }

    public void RestorePower(bool generatorUsed = true)
    {
        if (!PowerOutageActive) return;
        PowerOutageActive = false;
        if (outageRoutine != null) { StopCoroutine(outageRoutine); outageRoutine = null; }
        GameBootstrap.SetPowerOutage(false);
        for (int i = 0; i < Game.tanks.Count; i++) if (Game.tanks[i] != null) Game.tanks[i].SetFilterRunning(true);
        if (Game.ui != null) Game.ui.Toast(generatorUsed ? "Jenerator calisti! Elektrik ve tank filtreleri geri geldi." : "Sebeke elektrigi geri geldi.", 5f);
        Sfx.Play(Snd.PowerOn, 0.9f);
    }

    public void TriggerSchoolTrip(bool force = false)
    {
        if (Game.gm == null || (!force && Game.gm.Level < 20)) return;
        PlayerPrefs.SetInt("AT3_SchoolTripOccurred", 1);
        PlayerPrefs.Save();
        StartCoroutine(SchoolTripRoutine());
    }

    System.Collections.IEnumerator SchoolTripRoutine()
    {
        SchoolBusVisual.Spawn();
        Sfx.Play(Snd.SchoolBus, 1f);
        yield return new WaitForSeconds(0.65f);
        Sfx.Play(Snd.Children, 1f);
        if (Game.ui != null) Game.ui.Toast("OKUL GEZISI GELDI! Stoklari ve kasayi hazirla!", 6f);
        const int groupSize = 13;
        for (int i = 0; i < groupSize; i++)
        {
            Customer.SpawnSchool(i == groupSize - 1);
            yield return new WaitForSeconds(0.22f);
        }
    }

    public void TriggerStorm(bool force = false)
    {
        if (Game.gm == null || (!force && Game.gm.Level < 23)) return;
        PlayerPrefs.SetInt("AT3_StormOccurred", 1);
        PlayerPrefs.Save();
        StartCoroutine(StormRoutine());
    }

    System.Collections.IEnumerator StormRoutine()
    {
        if (Game.ui != null) Game.ui.Toast("FIRTINA BASLADI! Sahili, denizi ve ekipmanlari kontrol et!", 6f);
        Sfx.Play(Snd.Storm, 1f);
        Sfx.DangerFor(9f);
        for (int wave = 0; wave < 7; wave++)
        {
            if (Game.cam != null) Game.cam.Shake(0.32f, 0.45f);
            StormWave.Spawn(Random.Range(-25f, 72f));
            if (Game.trash != null)
            {
                Game.trash.ScatterIntoSea(new Vector3(42f, 0.5f, Random.Range(-20f, 65f)), 2, false);
                Game.trash.SpawnLandTrash(new Vector3(Random.Range(11f, 23f), 0f, Random.Range(-20f, 70f)));
            }
            yield return new WaitForSeconds(0.85f);
        }
        if (Game.jetski != null && Random.value < 0.75f) Game.jetski.BreakFromStorm();
        if (Game.ramp != null && Random.value < 0.75f) Game.ramp.BreakFromStorm();
        if (Game.ui != null) Game.ui.Toast("Firtina dindi. Sahili, rampayi ve jetskiyi kontrol et!", 6f);
    }
}

// ---------- Thief ----------
public class Thief : MonoBehaviour
{
    enum Weapon { None, Baton, Gun }
    enum TState { Enter, Assault, ToTarget, Grab, Flee, Escaped }
    TState state = TState.Enter;

    Transform visual;
    CharacterController controller;
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
    int fleeStage;
    int health, maxHealth;
    bool caughtPending;
    float assaultTimer;
    GameObject healthRoot;
    Transform healthFill;
    TextMesh healthText;
    Transform batonArm;
    Transform batonTarget;
    float batonSwingTime;
    bool batonHitPending;

    public static Thief Spawn(bool forceInside = false, int forcedWeapon = -1)
    {
        GameObject go = new GameObject("Thief");
        // Milestone thieves must work even while the shop gate is closed, so
        // they begin just inside the entrance instead of colliding with it.
        go.transform.position = (forceInside ? Customer.GateInside : Customer.DoorPos) +
            new Vector3(Random.Range(-1.4f, 1.4f), 0f, Random.Range(-1.2f, 1.2f));
        Thief t = go.AddComponent<Thief>();
        t.controller = go.AddComponent<CharacterController>();
        t.controller.radius = 0.48f;
        t.controller.height = 2.1f;
        t.controller.center = new Vector3(0f, 1.05f, 0f);
        t.controller.stepOffset = 0.35f;
        // disguised as a normal customer
        GameObject vroot = new GameObject("Visual");
        vroot.transform.SetParent(go.transform, false);
        t.visual = vroot.transform;
        B.Stickman(t.visual, new Color(0.35f, 0.5f, 0.95f));
        t.moveTarget = Customer.GateInside;
        if (Game.gm != null) t.fleeTimer = 20f + Game.gm.ThiefTimeBonus;
        t.toiletThief = Game.toilets != null && Game.toilets.CanStealToilet && Random.value < 0.35f;
        t.forcedWeapon = forcedWeapon;
        return t;
    }

    int forcedWeapon = -1;
    public bool IsRevealed { get { return revealed; } }

    void Reveal()
    {
        if (revealed) return;
        revealed = true;
        Material black = MatLib.Get(new Color(0.1f, 0.1f, 0.12f));
        foreach (MeshRenderer mr in visual.GetComponentsInChildren<MeshRenderer>())
            mr.sharedMaterial = black;
        B.Prim(PrimitiveType.Cube, "Mask", visual, new Vector3(0f, 1.8f, 0.22f), Vector3.zero, new Vector3(0.5f, 0.15f, 0.2f), MatLib.Get(new Color(0.9f, 0.2f, 0.2f)));
        B.Text3D("HIRSIZ!", transform, new Vector3(0f, 2.7f, 0f), 0.1f, new Color(1f, 0.3f, 0.3f));
        weapon = forcedWeapon >= 0 ? (Weapon)Mathf.Clamp(forcedWeapon, 0, 2) :
            Game.gm.Level >= 25 ? Weapon.Gun : Game.gm.Level >= 13 ? Weapon.Baton : Weapon.None;
        maxHealth = weapon == Weapon.Gun ? 10 : weapon == Weapon.Baton ? 5 : 1;
        health = maxHealth;
        BuildHealthBar();
        if (weapon == Weapon.Baton)
        {
            GameObject armPivot = new GameObject("SopaKolu");
            armPivot.transform.SetParent(visual, false);
            armPivot.transform.localPosition = new Vector3(0.48f, 1.35f, 0f);
            batonArm = armPivot.transform;
            Material body = MatLib.Get(new Color(0.1f, 0.1f, 0.12f));
            Material wood = MatLib.Get(new Color(0.42f, 0.23f, 0.1f));
            B.Prim(PrimitiveType.Capsule, "Kol", batonArm, new Vector3(0f, -0.3f, 0f), Vector3.zero,
                new Vector3(0.2f, 0.55f, 0.2f), body);
            B.Prim(PrimitiveType.Cube, "Sopa", batonArm, new Vector3(0f, -1.02f, 0.02f), new Vector3(0f, 0f, -5f),
                new Vector3(0.16f, 1.25f, 0.16f), wood);
            batonArm.localRotation = Quaternion.Euler(10f, 0f, -12f);
        }
        else if (weapon == Weapon.Gun)
        {
            B.Prim(PrimitiveType.Cube, "Silah", visual, new Vector3(0.55f, 1.15f, 0.35f), Vector3.zero,
                new Vector3(0.2f, 0.28f, 0.85f), MatLib.Get(new Color(0.14f, 0.16f, 0.2f)));
            B.Text3D("TEHLIKELI!", transform, new Vector3(0f, 3.2f, 0f), 0.075f, new Color(1f, 0.65f, 0.2f));
            Customer[] frightened = FindObjectsByType<Customer>(FindObjectsSortMode.None);
            for (int i = 0; i < frightened.Length; i++) frightened[i].FleeFromGunman(transform.position);
        }
        Sfx.Play(Snd.Alarm, 0.7f);
        Sfx.Play(Snd.Thief, 0.75f);
        Sfx.BeginDanger();
        if (Game.ui != null)
        {
            if (PlayerPrefs.GetInt("AT3_ThiefTutorialDone", 0) == 0)
            {
                PlayerPrefs.SetInt("AT3_ThiefTutorialDone", 1);
                PlayerPrefs.Save();
                Game.ui.ShowPausedInfo("HIRSIZ!",
                    "Hirsiz baliklarinizi veya paralarinizi calabilir.\n\n" +
                    "Onu kovala ve farenin SAG TIK tusuyla veya SPACE ile vur!");
            }
            else Game.ui.Toast("HIRSIZ! Yakala, esyalarini geri al!");
        }
    }

    void BuildHealthBar()
    {
        healthRoot = new GameObject("ThiefHealthBar");
        healthRoot.transform.SetParent(transform, false);
        Material back = MatLib.Get(new Color(0.16f, 0.08f, 0.08f));
        Material red = MatLib.Get(new Color(0.95f, 0.16f, 0.12f));
        B.Prim(PrimitiveType.Cube, "Back", healthRoot.transform, new Vector3(0f, 3.55f, 0f), Vector3.zero,
            new Vector3(2.15f, 0.13f, 0.28f), back);
        GameObject bar = B.Prim(PrimitiveType.Cube, "Health", healthRoot.transform, new Vector3(0f, 3.58f, -0.16f), Vector3.zero,
            new Vector3(2f, 0.15f, 0.3f), red);
        healthFill = bar.transform;
        healthText = B.Text3D("", healthRoot.transform, new Vector3(0f, 3.95f, 0f), 0.065f, Color.white);
        healthRoot.SetActive(false);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthFill == null) return;
        float fraction = Mathf.Clamp01((float)health / Mathf.Max(1, maxHealth));
        healthFill.localScale = new Vector3(2f * fraction, 0.15f, 0.3f);
        healthFill.localPosition = new Vector3(-1f + fraction, 3.58f, -0.16f);
        if (healthText != null) healthText.text = "CAN  " + Mathf.Max(0, health) + " / " + maxHealth;
    }

    void ChooseLootTarget()
    {
        if (Game.register.PileAmount > 0)
            moveTarget = Game.register.transform.position + new Vector3(-3.6f, 0f, -1.2f);
        else
        {
            stolenFrom = PickStockedTank();
            if (stolenFrom == null) { BeginFlee(); return; }
            moveTarget = stolenFrom.FrontPoint;
        }
        state = TState.ToTarget;
    }

    bool MoveTo(Vector3 t, float dt)
    {
        t.y = 0f;
        Vector3 pos = transform.position; pos.y = 0f;
        Vector3 to = t - pos;
        if (to.magnitude < 0.3f) return true;
        Vector3 dir = to.normalized;
        Vector3 motion = dir * speed * dt;
        if (controller != null && controller.enabled) controller.Move(motion);
        else transform.position += motion;
        visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(dir), 10f * dt);
        return false;
    }

    void BeginFlee()
    {
        state = TState.Flee;
        fleeStage = 0;
        moveTarget = Customer.GateInside;
        fleeTimer = 20f + (Game.gm != null ? Game.gm.ThiefTimeBonus : 0f);
        speed = 5.6f;
    }

    void Update()
    {
        if (Game.gm == null || Game.register == null) { Destroy(gameObject); return; } // scene restarting
        float dt = Time.deltaTime;
        if (knockTime > 0f)
        {
            knockTime -= dt;
            Vector3 motion = knockVelocity * dt;
            if (controller != null && controller.enabled) controller.Move(motion);
            else transform.position += motion;
            knockVelocity.x = Mathf.Lerp(knockVelocity.x, 0f, dt * 2.2f);
            knockVelocity.z = Mathf.Lerp(knockVelocity.z, 0f, dt * 2.2f);
            knockVelocity.y -= 21f * dt;
            visual.Rotate(new Vector3(720f, 260f, 480f) * dt, Space.Self);
            if (knockTime <= 0f)
            {
                Vector3 grounded = transform.position;
                grounded.y = 0f;
                transform.position = grounded;
                visual.localRotation = Quaternion.identity;
                if (caughtPending) { Caught(); return; }
            }
            return;
        }
        bool batonEngaged = revealed && weapon == Weapon.Baton && HandleBatonCombat(dt);
        if (revealed && weapon == Weapon.Gun) UpdateWeapon(dt);
        if (!batonEngaged) switch (state)
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
                    ChooseLootTarget();
                }
                break;

            case TState.Assault:
                assaultTimer -= dt;
                // Advance on the player while firing, but keep enough distance
                // for the gunfight to remain readable.
                if (Game.player != null && Vector3.Distance(transform.position, Game.player.transform.position) > 7f)
                    MoveTo(Game.player.transform.position, dt);
                if (assaultTimer <= 0f) ChooseLootTarget();
                break;

            case TState.ToTarget:
                if (MoveTo(moveTarget, dt))
                {
                    if (toiletThief)
                    {
                        stolenToilet = Game.toilets != null && Game.toilets.TryStealToilet();
                        if (stolenToilet)
                        {
                            Reveal();
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
                        if (stolenMoney > 0)
                        {
                            Reveal();
                            loot = B.Prim(PrimitiveType.Cube, "LootBag", transform, new Vector3(0f, 2.4f, 0f), Vector3.zero,
                                new Vector3(0.6f, 0.6f, 0.6f), MatLib.Get(new Color(0.3f, 0.7f, 0.35f)));
                        }
                    }
                    else if (stolenFrom != null && stolenFrom.HasStock)
                    {
                        stolenFrom.TakeForCustomer();
                        stolenSpecies = stolenFrom.species;
                        Reveal();
                        loot = new GameObject("LootFish");
                        loot.transform.SetParent(transform, false);
                        loot.transform.localPosition = new Vector3(0f, 2.4f, 0f);
                        SpeciesInfo.Build(stolenSpecies, loot.transform, 0.5f);
                    }
                    BeginFlee();
                }
                break;

            case TState.Flee:
                fleeTimer -= dt;
                if (Game.ui != null) Game.ui.ShowThiefTimer(fleeTimer);
                if (MoveTo(moveTarget, dt))
                {
                    if (fleeStage == 0) { fleeStage = 1; moveTarget = Customer.DoorPos; }
                    else if (fleeStage == 1) { fleeStage = 2; moveTarget = FleePoint(); }
                    else moveTarget = FleePoint();
                }
                if (fleeTimer <= 0f) Escaped();
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
        if (weapon == Weapon.Gun)
        {
            Vector3 target = transform.position + visual.forward * 20f;
            float pick = Random.value;
            Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
            System.Collections.Generic.List<Customer> living = new System.Collections.Generic.List<Customer>();
            for (int i = 0; i < customers.Length; i++) if (!customers[i].IsDead) living.Add(customers[i]);
            if (pick < 0.46f && living.Count > 0)
                target = living[Random.Range(0, living.Count)].transform.position;
            else if (pick < 0.72f && Game.player != null)
                target = Game.player.transform.position;
            else if (Game.tanks.Count > 0)
                target = Game.tanks[Random.Range(0, Game.tanks.Count)].transform.position;
            else if (living.Count > 0)
            {
                target = living[Random.Range(0, living.Count)].transform.position;
            }
            ThiefBullet.Spawn(transform.position + Vector3.up * 1.3f, target, this);
            attackTimer = 0.85f;
        }
    }

    // A baton thief interrupts stealing or escaping whenever the player gets
    // close. It turns, pursues, visibly swings its arm, then deals damage at
    // the middle of the swing instead of hitting instantly from a run cycle.
    bool HandleBatonCombat(float dt)
    {
        attackTimer -= dt;

        if (batonSwingTime > 0f)
        {
            batonSwingTime -= dt;
            AnimateBatonSwing();
            FaceBatonTarget(dt);
            if (batonHitPending && batonSwingTime <= 0.24f)
            {
                batonHitPending = false;
                ApplyBatonHit();
            }
            if (batonSwingTime <= 0f)
            {
                batonTarget = null;
                if (batonArm != null) batonArm.localRotation = Quaternion.Euler(10f, 0f, -12f);
            }
            return true;
        }

        Transform playerTarget = Game.player != null ? Game.player.transform : null;
        float playerDistance = playerTarget != null ? Vector3.Distance(transform.position, playerTarget.position) : float.MaxValue;

        // The larger awareness radius is intentional: approaching the thief
        // should make it defend itself instead of continuing to run away.
        if (playerTarget != null && playerDistance <= 8.5f)
        {
            batonTarget = playerTarget;
            FaceBatonTarget(dt);
            if (playerDistance > 2.15f)
                MoveTo(playerTarget.position, dt);
            else if (attackTimer <= 0f)
                StartBatonSwing(playerTarget);
            return true;
        }

        // While no player is threatening it, a baton thief still strikes a
        // customer who happens to block its path.
        Customer nearest = null;
        float nearestDistance = 2.35f;
        Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
        for (int i = 0; i < customers.Length; i++)
        {
            if (customers[i].IsDead) continue;
            float distance = Vector3.Distance(transform.position, customers[i].transform.position);
            if (distance < nearestDistance) { nearest = customers[i]; nearestDistance = distance; }
        }
        if (nearest != null && attackTimer <= 0f)
        {
            StartBatonSwing(nearest.transform);
            return true;
        }
        return false;
    }

    void StartBatonSwing(Transform target)
    {
        batonTarget = target;
        batonSwingTime = 0.52f;
        batonHitPending = true;
        attackTimer = 1.15f;
        FaceBatonTarget(1f);
    }

    void AnimateBatonSwing()
    {
        if (batonArm == null) return;
        float progress = 1f - Mathf.Clamp01(batonSwingTime / 0.52f);
        float angle;
        if (progress < 0.28f)
            angle = Mathf.Lerp(-75f, -105f, progress / 0.28f);
        else if (progress < 0.72f)
            angle = Mathf.Lerp(-105f, 82f, Mathf.SmoothStep(0f, 1f, (progress - 0.28f) / 0.44f));
        else
            angle = Mathf.Lerp(82f, 10f, (progress - 0.72f) / 0.28f);
        batonArm.localRotation = Quaternion.Euler(angle, 0f, -12f);
    }

    void FaceBatonTarget(float dt)
    {
        if (batonTarget == null) return;
        Vector3 direction = batonTarget.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
            visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(direction), Mathf.Clamp01(14f * dt));
    }

    void ApplyBatonHit()
    {
        if (batonTarget == null) return;
        if (Vector3.Distance(transform.position, batonTarget.position) > 2.7f) return;

        if (Game.player != null && batonTarget == Game.player.transform)
        {
            Vector3 away = Game.player.transform.position - transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = visual.forward;
            Game.player.LaunchTo(Game.player.transform.position + away.normalized * 5f);
            Sfx.Play(Snd.Punch, 0.9f);
            return;
        }

        Customer customer = batonTarget.GetComponent<Customer>();
        if (customer != null && !customer.IsDead)
        {
            customer.HitByThief(transform.position);
            Sfx.Play(Snd.Punch, 0.9f);
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

    // returns true when the thief is subdued (loot returned, thief runs off)
    public bool TakeHit(int damage = 1)
    {
        return PlayerHit(transform.position - visual.forward, damage);
    }

    public bool PlayerHit(Vector3 attackerPos, int damage = 1)
    {
        if (!revealed) return false;
        if (caughtPending) return true;
        health = Mathf.Max(0, health - Mathf.Max(1, damage));
        if (healthRoot != null) healthRoot.SetActive(true);
        UpdateHealthBar();
        Vector3 away = transform.position - attackerPos; away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = -visual.forward;
        knockVelocity = away.normalized * 9.5f + Vector3.up * 8.5f;
        knockTime = 0.78f;
        Sfx.Play(Snd.Punch, 0.85f);
        if (health > 0) { speed = Mathf.Max(3.5f, speed - 0.18f); return false; }
        caughtPending = true;
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
                bool broke = tank.HitByBullet();
                if (broke)
                {
                    int damage = Mathf.Min(Game.gm.Money, 100 + SpeciesInfo.Price(tank.species) / 2);
                    if (damage > 0) Game.gm.SpendTick(damage);
                    if (Game.ui != null) Game.ui.Toast("Mermi akvaryumu kirdi! -$" + B.Money(damage) + " hasar");
                    Sfx.Play(Snd.GlassBreak, 0.8f);
                }
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

// A real map-wide money rain. Bills fall visibly, remain on the floor for a
// short time, and can be collected by the player or taken by nearby NPCs.
public class MoneyRainPickup : MonoBehaviour
{
    int value;
    float delay;
    float life = 24f;
    float groundY;
    bool landed;

    public static void Spawn(int value, float delay)
    {
        GameObject go = new GameObject("MoneyRainDollar");
        MoneyRainPickup pickup = go.AddComponent<MoneyRainPickup>();
        pickup.value = value;
        pickup.delay = delay;
        float x = Random.Range(-43f, 95f);
        float z = Random.Range(-8f, 54f);
        pickup.groundY = 0.42f;
        go.transform.position = new Vector3(x, Random.Range(13f, 22f), z);
        B.Prim(PrimitiveType.Cube, "OneDollar", go.transform, Vector3.zero, new Vector3(0f, Random.Range(0f, 360f), 0f),
            new Vector3(0.62f, 0.05f, 0.3f), MatLib.Get(new Color(0.18f, 0.75f, 0.25f)));
    }

    void Update()
    {
        if (delay > 0f) { delay -= Time.deltaTime; return; }
        life -= Time.deltaTime;
        if (life <= 0f) { Destroy(gameObject); return; }
        if (!landed)
        {
            transform.position += Vector3.down * (6f + (22f - transform.position.y) * 0.15f) * Time.deltaTime;
            transform.Rotate(90f * Time.deltaTime, 170f * Time.deltaTime, 60f * Time.deltaTime);
            if (transform.position.y <= groundY)
            {
                Vector3 p = transform.position; p.y = groundY; transform.position = p;
                transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                landed = true;
            }
            return;
        }
        if (Game.player != null && FlatDistance(transform.position, Game.player.transform.position) < 1.25f)
        {
            Game.gm.AddMoney(value);
            if (Game.ui != null) Game.ui.MoneyPunch();
            Sfx.Play(Snd.MoneyPickup, 0.35f);
            Destroy(gameObject);
            return;
        }
        Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
        for (int i = 0; i < customers.Length; i++) if (FlatDistance(transform.position, customers[i].transform.position) < 0.8f) { Destroy(gameObject); return; }
        BeachVisitor[] visitors = FindObjectsByType<BeachVisitor>(FindObjectsSortMode.None);
        for (int i = 0; i < visitors.Length; i++) if (FlatDistance(transform.position, visitors[i].transform.position) < 0.8f) { Destroy(gameObject); return; }
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
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
        GameObject sharkModel = AssetLib.SpawnShark(visual, 4.8f);
        if (sharkModel != null)
        {
            Destroy(gameObject, 14f);
            return;
        }
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
