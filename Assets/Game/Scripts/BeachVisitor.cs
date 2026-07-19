using UnityEngine;

// From level 40 on, tourists come to swim at the beach and leave a mess
// (toppled loungers, food litter) that raises pollution. Player or the
// beach-cleaner staff must tidy it up.
public class BeachManager : MonoBehaviour
{
    public const int UnlockLevel = 40;
    float timer = 8f;

    public static BeachManager Create(Transform parent)
    {
        GameObject go = new GameObject("BeachManager");
        if (parent != null) go.transform.SetParent(parent, false);
        return go.AddComponent<BeachManager>();
    }

    void Update()
    {
        if (Game.gm == null || Game.gm.Level < UnlockLevel) return;
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = Random.Range(14f, 26f);
        int alive = FindObjectsByType<BeachVisitor>(FindObjectsSortMode.None).Length;
        if (alive < 5) BeachVisitor.Spawn();
    }
}

public class BeachVisitor : MonoBehaviour
{
    enum S { ToBeach, Relax, ToSea, Swim, Leave }
    S state = S.ToBeach;
    Transform visual;
    float speed, timer;
    Vector3 moveTarget, loungerSpot;
    GameObject lounger;

    public static BeachVisitor Spawn()
    {
        GameObject go = new GameObject("BeachVisitor");
        go.transform.position = new Vector3(20f, 0f, -6f); // from the south beach
        BeachVisitor v = go.AddComponent<BeachVisitor>();
        v.Build();
        return v;
    }

    void Build()
    {
        speed = Random.Range(2.4f, 3.2f);
        GameObject vroot = new GameObject("Visual");
        vroot.transform.SetParent(transform, false);
        visual = vroot.transform;
        B.Stickman(visual, Color.HSVToRGB(Random.value, 0.5f, 0.95f));
        loungerSpot = new Vector3(Random.Range(21f, 25f), 0f, Random.Range(2f, 34f));
        moveTarget = loungerSpot;
    }

    bool MoveTo(Vector3 t, float dt)
    {
        t.y = 0f;
        Vector3 p = transform.position; p.y = 0f;
        Vector3 to = t - p;
        if (to.magnitude < 0.3f) return true;
        transform.position += to.normalized * speed * dt;
        if (visual != null) visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(to.normalized), 10f * dt);
        return false;
    }

    void PlaceLounger()
    {
        lounger = new GameObject("Lounger");
        lounger.transform.SetParent(transform.parent, false);
        lounger.transform.position = loungerSpot;
        Material wood = MatLib.Get(new Color(0.85f, 0.8f, 0.5f));
        B.Prim(PrimitiveType.Cube, "Seat", lounger.transform, new Vector3(0f, 0.3f, 0f), Vector3.zero, new Vector3(0.9f, 0.1f, 2f), wood);
        for (int i = 0; i < 4; i++)
            B.Prim(PrimitiveType.Cube, "Leg", lounger.transform, new Vector3(((i % 2) * 2 - 1) * 0.4f, 0.15f, ((i / 2) * 2 - 1) * 0.9f), Vector3.zero, new Vector3(0.08f, 0.3f, 0.08f), wood);
    }

    void Update()
    {
        if (Game.gm == null) { Destroy(gameObject); return; }
        float dt = Time.deltaTime;
        switch (state)
        {
            case S.ToBeach:
                if (MoveTo(moveTarget, dt)) { PlaceLounger(); timer = Random.Range(6f, 12f); state = S.Relax; }
                break;
            case S.Relax:
                timer -= dt;
                if (timer <= 0f) { moveTarget = new Vector3(30f, 0f, loungerSpot.z); state = S.ToSea; }
                break;
            case S.ToSea:
                if (MoveTo(moveTarget, dt)) { timer = Random.Range(5f, 10f); state = S.Swim; }
                break;
            case S.Swim:
                timer -= dt;
                // bob in the shallows
                transform.position += new Vector3(Mathf.Sin(Time.time * 1.5f), 0f, Mathf.Cos(Time.time * 1.3f)) * 0.4f * dt;
                if (timer <= 0f) { moveTarget = new Vector3(20f, 0f, -8f); state = S.Leave; LeaveMess(); }
                break;
            case S.Leave:
                if (MoveTo(moveTarget, dt)) Destroy(gameObject);
                break;
        }
    }

    // topple the lounger + drop food litter on the beach (counts as pollution)
    void LeaveMess()
    {
        if (lounger != null && Random.value < 0.7f)
            lounger.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 75f); // toppled
        if (Game.trash != null)
        {
            int n = Random.Range(1, 3);
            for (int i = 0; i < n; i++)
                Game.trash.SpawnLandTrash(loungerSpot + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f)));
        }
    }
}
