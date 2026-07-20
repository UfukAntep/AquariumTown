using UnityEngine;

public class Customer : MonoBehaviour
{
    public enum CState { Enter, PickTank, WalkToTank, Browse, ToQueue, Paying, ToToilet, InToilet, ReturnFish, Exit, ExitSad }

    public CState state = CState.Enter;
    Tank targetTank;
    int holdingPrice;
    int holdingSpecies;
    float timer;
    float queueWait;
    float speed;
    Vector3 moveTarget;
    Transform visual;
    GameObject bowl;
    TextMesh bubble;

    public static Vector3 DoorPos = new Vector3(12f, 0f, 26f);   // outside the gate
    public static Vector3 GateInside = new Vector3(4f, 0f, 26f); // just inside

    public static Customer Spawn()
    {
        return SpawnInternal(false, false);
    }

    public static Customer SpawnSchool(bool teacher)
    {
        return SpawnInternal(true, teacher);
    }

    static Customer SpawnInternal(bool school, bool teacher)
    {
        GameObject go = new GameObject("Customer");
        go.transform.position = DoorPos + new Vector3(Random.Range(-1f, 3f), 0f, Random.Range(-1.5f, 1.5f));
        Customer c = go.AddComponent<Customer>();
        c.schoolTrip = school;
        c.schoolTeacher = teacher;
        if (Game.gm != null) Game.gm.RegisterCustomerArrival();
        c.Build();
        return c;
    }

    bool vip;
    bool bought;        // successfully purchased -> happy review
    bool reviewed;
    bool toiletChecked;
    bool attacked;
    bool dead;
    bool schoolTrip;
    bool schoolTeacher;
    float deathTimer;
    float knockTime;
    float attackedRecovery;
    Vector3 knockVelocity;
    public bool IsDead { get { return dead; } }

    void LeaveReview(bool happy)
    {
        if (reviewed || Game.gm == null) return;
        reviewed = true;
        Game.gm.AddReview(Game.gm.Satisfaction, happy);
    }

    void Build()
    {
        speed = Random.Range(2.6f, 3.4f) * Game.gm.CustSpeedMult;
        // cute stickman customers (mostly blue); rare GOLDEN VIPs pay 3x!
        GameObject vroot = new GameObject("Visual");
        vroot.transform.SetParent(transform, false);
        visual = vroot.transform;
        vip = Random.value < 0.05f;
        Color col = vip ? new Color(1f, 0.82f, 0.2f)
            : Random.value < 0.85f
                ? new Color(0.35f, 0.5f, Random.Range(0.85f, 1f))
                : Color.HSVToRGB(Random.value, 0.5f, 0.95f);
        float characterScale = schoolTrip && !schoolTeacher ? 0.76f : vip ? 1.15f : 1f;
        B.Stickman(visual, schoolTrip ? (schoolTeacher ? new Color(0.25f, 0.4f, 0.78f) : new Color(0.95f, 0.55f, 0.2f)) : col, characterScale);
        if (vip) B.Text3D("VIP", transform, new Vector3(0f, 3.1f, 0f), 0.1f, new Color(1f, 0.85f, 0.2f));
        if (schoolTrip) B.Text3D(schoolTeacher ? "OGRETMEN" : "OGRENCI", transform,
            new Vector3(0f, schoolTeacher ? 3.05f : 2.55f, 0f), 0.06f, schoolTeacher ? UIKit.Blue : UIKit.Orange);
        bubble = B.Text3D("", transform, new Vector3(0f, 2.6f, 0f), 0.09f, Color.white);
        moveTarget = GateInside;

        // dirty shop? some customers complain and leave right away
        if (Game.trash != null && Game.trash.ShopCount >= 3 && Random.value < 0.5f)
        {
            state = CState.ExitSad;
            bubble.text = "Cok pis burasi!";
            bubble.color = new Color(1f, 0.5f, 0.4f);
            moveTarget = DoorPos;
            Game.gm.AddSatisfaction(-3f);
        }
    }

    bool MoveTo(Vector3 t, float dt)
    {
        t.y = 0f;
        Vector3 pos = transform.position; pos.y = 0f;
        Vector3 to = t - pos;
        if (to.magnitude < 0.25f) return true;
        Vector3 dir = to.normalized;
        transform.position += dir * speed * dt;
        if (visual != null)
            visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(dir), 10f * dt);
        return false;
    }

    public void HitByPlayer(Vector3 attackerPos)
    {
        if (attacked || state == CState.Exit || state == CState.ExitSad) return;
        attacked = true;
        ReturnHeldFish();
        if (Game.register != null) Game.register.Leave(this);
        if (Game.gm != null)
        {
            Game.gm.AddSatisfaction(-12f);
            Game.gm.AddOneStarReview();
        }
        reviewed = true;
        bubble.text = "Bana vurdu! Bir daha gelmem!";
        bubble.color = new Color(1f, 0.35f, 0.3f);
        LaunchAway(attackerPos, 7f);
        speed = 6f;
        state = CState.ExitSad;
        moveTarget = DoorPos;
    }

    public void HitByThief(Vector3 attackerPos)
    {
        if (dead || state == CState.Exit) return;
        // A customer killed by a thief loses the fish they were carrying; it
        // does not magically return to shop stock.
        holdingPrice = 0;
        HideBowl();
        if (Game.register != null) Game.register.Leave(this);
        dead = true;
        reviewed = true;
        deathTimer = 8f;
        bubble.text = "IMDAT!";
        bubble.color = new Color(1f, 0.45f, 0.3f);
        LaunchAway(attackerPos, 7f);
        if (Game.gm != null) Game.gm.AddSatisfaction(-8f);
    }

    public void FleeFromGunman(Vector3 dangerPos)
    {
        if (dead || state == CState.Exit || state == CState.ExitSad) return;
        ReturnHeldFish();
        if (Game.register != null) Game.register.Leave(this);
        bubble.text = "SILAH! KACIN!";
        bubble.color = new Color(1f, 0.35f, 0.25f);
        speed = 7f;
        state = CState.ExitSad;
        moveTarget = DoorPos;
    }

    void ReturnHeldFish()
    {
        if (holdingPrice > 0)
        {
            Game.ReturnFishToStorage(holdingSpecies);
            holdingPrice = 0;
        }
        HideBowl();
    }

    void LaunchAway(Vector3 from, float force)
    {
        Vector3 away = transform.position - from;
        away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = Vector3.back;
        knockVelocity = away.normalized * force;
        knockTime = 0.55f;
        Sfx.Play(Snd.Punch, 0.75f);
    }

    void ShowBowl(int sp)
    {
        bowl = new GameObject("Bowl");
        bowl.transform.SetParent(transform, false);
        bowl.transform.localPosition = new Vector3(0f, 2.35f, 0f);
        B.Prim(PrimitiveType.Sphere, "Glass", bowl.transform, Vector3.zero, Vector3.zero, Vector3.one * 0.65f, MatLib.Glass(new Color(0.7f, 0.9f, 1f, 0.4f)));
        B.Prim(PrimitiveType.Sphere, "Water", bowl.transform, new Vector3(0f, -0.08f, 0f), Vector3.zero, Vector3.one * 0.55f, MatLib.Glass(new Color(0.3f, 0.7f, 0.95f, 0.5f)));
        Transform mini = SpeciesInfo.Build(sp, bowl.transform, 0.3f);
        mini.localPosition = new Vector3(0f, -0.05f, 0f);
    }

    void HideBowl() { if (bowl != null) Destroy(bowl); }

    void Update()
    {
        if (Game.gm == null || Game.register == null) return; // scene restarting
        float dt = Time.deltaTime;
        if (knockTime > 0f)
        {
            knockTime -= dt;
            transform.position += knockVelocity * dt + Vector3.up * Mathf.Sin((0.55f - knockTime) / 0.55f * Mathf.PI) * 3f * dt;
            knockVelocity = Vector3.Lerp(knockVelocity, Vector3.zero, dt * 3f);
            visual.Rotate(Vector3.right, 520f * dt);
            if (knockTime <= 0f)
            {
                Vector3 grounded = transform.position; grounded.y = 0f; transform.position = grounded;
                if (dead) visual.rotation = Quaternion.Euler(0f, visual.eulerAngles.y, 90f);
                else if (attacked)
                {
                    visual.rotation = Quaternion.Euler(0f, visual.eulerAngles.y, 90f);
                    attackedRecovery = 0.7f;
                }
            }
            return;
        }
        if (dead)
        {
            deathTimer -= dt;
            if (deathTimer <= 0f) Destroy(gameObject);
            return;
        }
        if (attackedRecovery > 0f)
        {
            attackedRecovery -= dt;
            if (attackedRecovery <= 0f)
                visual.rotation = Quaternion.Euler(0f, visual.eulerAngles.y, 0f);
            return;
        }
        switch (state)
        {
            case CState.Enter:
                if (MoveTo(moveTarget, dt))
                {
                    // maybe visit the toilet first
                    if (Game.toilets != null && Game.toilets.HasCleanUnit && Random.value < 0.58f)
                    {
                        moveTarget = Game.toilets.EntrancePos;
                        state = CState.ToToilet;
                    }
                    else state = CState.PickTank;
                }
                break;

            case CState.ToToilet:
                if (MoveTo(moveTarget, dt))
                {
                    Game.toilets.Use();
                    timer = 2.5f;
                    state = CState.InToilet;
                    bubble.text = "~";
                }
                break;

            case CState.InToilet:
                timer -= dt;
                if (timer <= 0f) { bubble.text = ""; state = CState.PickTank; }
                break;

            case CState.PickTank:
                // from mid-game customers demand a toilet; complain if there's none
                if (!toiletChecked)
                {
                    toiletChecked = true;
                    if (Game.gm.NeedsToilet && Random.value < 0.4f)
                    {
                        bubble.text = "Tuvalet yok mu?!";
                        bubble.color = new Color(1f, 0.5f, 0.4f);
                        Game.gm.AddSatisfaction(-4f);
                        LeaveReview(false);
                        state = CState.ExitSad;
                        moveTarget = DoorPos;
                        break;
                    }
                }
                targetTank = PickStockedTank();
                if (targetTank != null)
                {
                    moveTarget = targetTank.FrontPoint + new Vector3(Random.Range(-0.8f, 0.8f), 0f, 0f);
                    state = CState.WalkToTank;
                    bubble.text = "";
                }
                else
                {
                    timer += dt;
                    bubble.text = "Balik yok!";
                    bubble.color = new Color(1f, 0.55f, 0.45f);
                    if (timer > 6f)
                    {
                        Game.gm.AddSatisfaction(-5f);
                        state = CState.ExitSad;
                        moveTarget = DoorPos;
                    }
                }
                break;

            case CState.WalkToTank:
                if (targetTank == null) { state = CState.PickTank; break; }
                if (MoveTo(moveTarget, dt)) { state = CState.Browse; timer = Random.Range(0.7f, 1.4f); }
                break;

            case CState.Browse:
                timer -= dt;
                if (timer <= 0f)
                {
                    holdingPrice = targetTank != null ? targetTank.TakeForCustomer() : 0;
                    if (vip) holdingPrice *= 3;
                    if (holdingPrice > 0)
                    {
                        holdingSpecies = targetTank.species;
                        ShowBowl(holdingSpecies);
                        if (Random.value < Game.gm.TipChance)
                        {
                            holdingPrice = Mathf.RoundToInt(holdingPrice * 1.5f);
                            bubble.text = "Bahsis!";
                            bubble.color = new Color(1f, 0.9f, 0.4f);
                        }
                        else
                        {
                            bubble.text = "";
                        }
                        Game.register.Join(this);
                        queueWait = 0f;
                        state = CState.ToQueue;
                    }
                    else state = CState.PickTank;
                }
                break;

            case CState.ToQueue:
                queueWait += dt;
                int qi = Game.register.IndexOf(this);
                if (qi >= 0) moveTarget = Game.register.QueueSlot(qi);
                if (queueWait > 20f)
                {
                    // waited too long -> puts the fish back and leaves angry
                    Game.register.Leave(this);
                    Game.ReturnFishToStorage(holdingSpecies);
                    HideBowl();
                    bubble.text = "Ilgilenen yok!";
                    bubble.color = new Color(1f, 0.45f, 0.35f);
                    Game.gm.AddSatisfaction(-8f);
                    state = CState.ExitSad;
                    moveTarget = DoorPos;
                    break;
                }
                if (MoveTo(moveTarget, dt) && qi == 0 && Game.register.HasOperator)
                {
                    state = CState.Paying;
                    timer = Game.register.CashierPresent ? 0.9f * Game.gm.StaffWorkTimeMultiplier(0) : 0.7f;
                }
                break;

            case CState.Paying:
                queueWait += dt;
                if (!Game.register.HasOperator) { state = CState.ToQueue; break; }
                timer -= dt;
                if (timer <= 0f)
                {
                    Game.register.Pay(holdingPrice);
                    Game.register.Leave(this);
                    Game.gm.AddSatisfaction(vip ? +3f : +1f);
                    bought = true;
                    bubble.text = "";
                    state = CState.Exit;
                    moveTarget = DoorPos;
                }
                break;

            case CState.Exit:
            case CState.ExitSad:
                if (MoveTo(moveTarget, dt)) Destroy(gameObject);
                break;
        }
    }

    Tank PickStockedTank()
    {
        int stocked = 0;
        for (int i = 0; i < Game.tanks.Count; i++) if (Game.tanks[i].HasStock) stocked++;
        if (stocked == 0) return null;
        int pick = Random.Range(0, stocked);
        for (int i = 0; i < Game.tanks.Count; i++)
        {
            if (!Game.tanks[i].HasStock) continue;
            if (pick == 0) return Game.tanks[i];
            pick--;
        }
        return null;
    }

    void OnDestroy()
    {
        if (Game.register != null) Game.register.Leave(this);
        // leave a star review on the way out (only ~35% do, to avoid spam)
        if (!reviewed && Game.gm != null)
        {
            if (schoolTrip && !bought)
            {
                // An unprepared school visit produces a visible wave of bad
                // feedback: every disappointed child reviews, teacher counts twice.
                Game.gm.AddOneStarReview();
                if (schoolTeacher) Game.gm.AddOneStarReview();
                reviewed = true;
            }
            else if (Random.value < 0.35f) LeaveReview(bought);
        }
    }
}

public class CustomerManager : MonoBehaviour
{
    float timer = 2f;

    void Update()
    {
        if (Game.gm == null) return; // scene restarting
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        // Shop must be open and customers visit from 06:00 until 22:00.
        if (!Game.gm.shopOpen || !Game.gm.CustomersAllowed) { timer = 2f; return; }

        int stockedTanks = Game.StockedTankCount();
        int alive = FindObjectsByType<Customer>(FindObjectsSortMode.None).Length;

        // Customers still enter an empty shop and discover that there is no
        // stock; otherwise an open shop can look broken before the first catch.
        if (alive < 10)
        {
            Customer.Spawn();
            float interval = Mathf.Clamp(7f - stockedTanks * 0.5f, 1.8f, 7f);
            if (Game.trash != null && Game.trash.Count >= 5) interval *= 1.8f;
            timer = interval;
        }
        else timer = 1.5f;
    }
}
