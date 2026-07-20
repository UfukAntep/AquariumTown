using UnityEngine;

public class Fish : MonoBehaviour
{
    public enum State { Wild, Fly, Carried, InTank, Flop }

    public int species;
    public State state = State.Wild;
    public bool decorative;
    public bool golden;
    public Transform scanner;

    Transform visual;
    Bounds area;
    Vector3 wanderTarget;
    float wanderTimer;
    float swimSpeed = 1.3f;

    Transform carrierAnchor;
    int carryIndex;

    Vector3 flyFrom, flyTo;
    float flyT, flyDur = 0.45f;
    System.Action flyDone;
    float wigglePhase;
    public float flopTimer;
    GameObject badge;

    public static Fish Create(int sp, Vector3 pos, float scale = 1f)
    {
        GameObject go = new GameObject("Fish_" + sp);
        go.transform.position = pos;
        Fish f = go.AddComponent<Fish>();
        f.species = sp;
        // prefer a Quirky Series animal model (animated); fallback: procedural
        GameObject quirky = AssetLib.SpawnSeaAnimal(sp, go.transform, 1.15f * scale);
        f.visual = quirky != null ? quirky.transform : SpeciesInfo.Build(sp, go.transform, scale);
        f.wigglePhase = Random.value * 10f;
        return f;
    }

    public void MakeGolden()
    {
        golden = true;
        Material gold = MatLib.Get(new Color(1f, 0.85f, 0.1f));
        foreach (MeshRenderer mr in visual.GetComponentsInChildren<MeshRenderer>())
            mr.sharedMaterial = gold;
        B.Text3D("ALTIN!", transform, new Vector3(0f, 1.4f, 0f), 0.1f, new Color(1f, 0.9f, 0.2f));
    }

    // Level requirement badge — only shown while the radar is on a locked fish.
    public void SetBadgeVisible(bool show, int reqLevel = 0)
    {
        if (show)
        {
            if (badge == null)
            {
                badge = new GameObject("Badge");
                badge.transform.SetParent(transform, false);
                badge.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                B.Prim(PrimitiveType.Sphere, "Bg", badge.transform, Vector3.zero, Vector3.zero,
                    new Vector3(1.65f, 0.95f, 0.15f), MatLib.Get(Color.white));
                TextMesh badgeText = B.Text3D("Sv " + reqLevel + "\ngerekli", badge.transform,
                    new Vector3(0f, 0f, -0.12f), 0.052f, new Color(0.95f, 0.5f, 0.1f), false);
                badgeText.lineSpacing = 0.78f;
                badge.AddComponent<Billboard>();
            }
            badge.SetActive(true);
        }
        else if (badge != null) badge.SetActive(false);
    }

    public void SetWild(Bounds b, bool decor = false)
    {
        state = State.Wild;
        decorative = decor;
        area = b;
        swimSpeed = 1.0f + Random.value * 0.6f;
        PickWander();
    }

    public void SetInTank(Bounds b)
    {
        state = State.InTank;
        scanner = null;
        area = b;
        swimSpeed = 0.45f + Random.value * 0.3f;
        PickWander();
    }

    public void SetCarried(Transform anchor, int index)
    {
        state = State.Carried;
        scanner = null;
        carrierAnchor = anchor;
        carryIndex = index;
    }

    public void SetCarryIndex(int index) { carryIndex = index; }

    public void SetFlop(Vector3 groundPos)
    {
        state = State.Flop;
        flopTimer = 0f;
        transform.position = groundPos;
    }

    public void FlyTo(Vector3 target, System.Action done, float dur = 0.45f)
    {
        state = State.Fly;
        scanner = null;
        flyFrom = transform.position;
        flyTo = target;
        flyT = 0f;
        flyDur = dur;
        flyDone = done;
    }

    public void Die()
    {
        // little poof
        GameObject poof = B.Prim(PrimitiveType.Sphere, "Poof", null, transform.position, Vector3.zero, Vector3.one * 0.4f,
            MatLib.Glass(new Color(0.6f, 0.6f, 0.6f, 0.5f)));
        poof.AddComponent<Poof>();
        Destroy(gameObject);
    }

    void PickWander()
    {
        int modelIdx = species % 24;
        bool isBottomFeeder = (modelIdx == 1 || modelIdx == 3 || modelIdx == 9 || modelIdx == 10 || modelIdx == 12);
        float targetY = isBottomFeeder ? area.min.y + 0.15f : Random.Range(area.min.y, area.max.y);
        wanderTarget = new Vector3(
            Random.Range(area.min.x, area.max.x),
            targetY,
            Random.Range(area.min.z, area.max.z));
        wanderTimer = Random.Range(2f, 5f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        switch (state)
        {
            case State.Wild:
            case State.InTank:
                Vector3 dir;
                if (state == State.Wild && scanner != null)
                {
                    Vector3 away = transform.position - scanner.position;
                    away.y = 0f;
                    dir = away.sqrMagnitude > 0.01f ? away.normalized : Random.insideUnitSphere;
                    Vector3 next = transform.position + dir * swimSpeed * 1.4f * dt;
                    next.x = Mathf.Clamp(next.x, area.min.x, area.max.x);
                    next.z = Mathf.Clamp(next.z, area.min.z, area.max.z);
                    transform.position = next;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * dt);
                }
                else
                {
                    wanderTimer -= dt;
                    Vector3 to = wanderTarget - transform.position;
                    if (to.magnitude < 0.25f || wanderTimer <= 0f) PickWander();
                    else
                    {
                        dir = to.normalized;
                        transform.position += dir * swimSpeed * dt;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 4f * dt);
                    }
                }
                break;

            case State.Carried:
                if (carrierAnchor == null) break;
                Vector3 slot = carrierAnchor.position + Vector3.up * (carryIndex * 0.32f);
                transform.position = Vector3.Lerp(transform.position, slot, 12f * dt);
                transform.rotation = Quaternion.Slerp(transform.rotation, carrierAnchor.rotation, 8f * dt);
                break;

            case State.Fly:
                flyT += dt / flyDur;
                if (flyT >= 1f)
                {
                    transform.position = flyTo;
                    System.Action cb = flyDone;
                    flyDone = null;
                    if (cb != null) cb();
                }
                else transform.position = B.Parabola(flyFrom, flyTo, 2f, flyT);
                break;

            case State.Flop:
                flopTimer += dt;
                // desperate flopping on the ground
                transform.position += new Vector3(Mathf.Sin(Time.time * 9f + wigglePhase), 0f, Mathf.Cos(Time.time * 7f + wigglePhase)) * 0.4f * dt;
                transform.rotation = Quaternion.Euler(0f, Time.time * 200f % 360f, Mathf.Sin(Time.time * 10f) * 40f);
                break;
        }

        if (visual != null && state != State.Fly && state != State.Flop)
            visual.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 6f + wigglePhase) * 10f, 0f);

        // hide the level badge as soon as the radar leaves us
        if (badge != null && badge.activeSelf && scanner == null)
            badge.SetActive(false);
    }
}

public class Poof : MonoBehaviour
{
    float t;
    void Update()
    {
        t += Time.deltaTime;
        transform.localScale = Vector3.one * (0.4f + t * 2f);
        if (t > 0.4f) Destroy(gameObject);
    }
}
